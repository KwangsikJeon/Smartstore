﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Smartstore.ComponentModel;
using Smartstore.Data;
using Smartstore.Domain;
using Smartstore.Utilities;
using EfState = Microsoft.EntityFrameworkCore.EntityState;

namespace Smartstore
{
    public static class DbContextExtensions
    {
        #region Connection

        /// <summary>
        /// Opens and retains connection until end of scope. Call this method in long running 
        /// processes to gain slightly faster database interaction.
        /// </summary>
        /// <param name="ctx">The database context</param>
        public static IDisposable OpenConnection(this DbContext ctx)
        {
            bool wasOpened = false;
            var db = ctx.Database;

            if (db.GetDbConnection().State != ConnectionState.Open)
            {
                db.OpenConnection();
                wasOpened = true;
            }

            return new ActionDisposable(() => 
            {
                if (wasOpened)
                    db.CloseConnection();
            });
        }

        /// <summary>
        /// Opens and retains connection until end of scope. Call this method in long running 
        /// processes to gain slightly faster database interaction.
        /// </summary>
        /// <param name="ctx">The database context</param>
        public static async Task<IAsyncDisposable> OpenConnectionAsync(this DbContext ctx)
        {
            bool wasOpened = false;
            var db = ctx.Database;

            if (db.GetDbConnection().State != ConnectionState.Open)
            {
                await db.OpenConnectionAsync();
                wasOpened = true;
            }

            return new AsyncActionDisposable(async () =>
            {
                if (wasOpened)
                    await db.CloseConnectionAsync();
            });
        }

        #endregion

        #region Entity states, detaching

        /// <summary>
        /// Checks whether at least one entity in the change tracker is in <see cref="EfState.Added"/>, 
        /// <see cref="EfState.Deleted"/> or <see cref="EfState.Modified"/> state.
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public static bool HasChanges(this HookingDbContext ctx)
        {
            return ctx.ChangeTracker.Entries().Where(x => x.State > EfState.Unchanged).Any();
        }

        /// <summary>
        /// Change the state of an entity object
        /// </summary>
        /// <typeparam name="TEntity">Type of entity</typeparam>
        /// <param name="entity">The entity instance</param>
        /// <param name="requestedState">The requested new state</param>
        public static void ChangeState<TEntity>(this HookingDbContext ctx, TEntity entity, EfState requestedState) where TEntity : BaseEntity
        {
            //Console.WriteLine("ChangeState ORIGINAL");
            var entry = ctx.Entry(entity);

            if (entry.State != requestedState)
            {
                // Only change state when requested state differs,
                // because EF internally sets all properties to modified
                // if necessary, even when requested state equals current state.
                entry.State = requestedState;
            }
        }

        /// <summary>
        /// Determines whether an entity property has changed since it was attached.
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <param name="propertyName">The property name to check</param>
        /// <param name="originalValue">The previous/original property value if change was detected</param>
        /// <returns><c>true</c> if property has changed, <c>false</c> otherwise</returns>
        public static bool TryGetModifiedProperty(this HookingDbContext ctx, BaseEntity entity, string propertyName, out object originalValue)
        {
            Guard.NotNull(entity, nameof(entity));

            if (entity.IsTransientRecord())
            {
                originalValue = null;
                return false;
            }

            var entry = ctx.Entry((object)entity);
            return entry.TryGetModifiedProperty(propertyName, out originalValue);
        }

        /// <summary>
        /// Gets a list of modified properties for the specified entity
        /// </summary>
        /// <param name="entity">The entity instance for which to get modified properties for</param>
        /// <returns>
        /// A dictionary, where the key is the name of the modified property
        /// and the value is its ORIGINAL value (which was tracked when the entity
        /// was attached to the context the first time)
        /// Returns an empty dictionary if no modification could be detected.
        /// </returns>
        public static IDictionary<string, object> GetModifiedProperties(this HookingDbContext ctx, BaseEntity entity)
        {
            return ctx.Entry((object)entity).GetModifiedProperties();
        }

        /// <summary>
        /// Reloads the entity from the database overwriting any property values with values from the database. 
        /// The entity will be in the Unchanged state after calling this method. 
        /// </summary>
        /// <typeparam name="TEntity">Type of entity</typeparam>
        /// <param name="entity">The entity instance</param>
        public static void ReloadEntity<TEntity>(this HookingDbContext ctx, TEntity entity) where TEntity : BaseEntity
        {
            ctx.Entry((object)entity).ReloadEntity();
        }

        /// <summary>
        /// Detaches an entity from the current context if it's attached.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity</typeparam>
        /// <param name="entity">The entity instance to detach</param>
        /// <param name="deep">Whether to scan all navigation properties and detach them recursively also.</param>
        /// <returns>The count of detached entities</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int DetachEntity<TEntity>(this HookingDbContext ctx, TEntity entity, bool deep = false) where TEntity : BaseEntity
        {
            return ctx.DetachInternal(entity, deep ? new HashSet<BaseEntity>() : null, deep);
        }

        /// <summary>
        /// Detaches all passed entities from the current context.
        /// </summary>
        /// <param name="unchangedEntitiesOnly">When <c>true</c>, only entities in unchanged state get detached.</param>
        /// <param name="deep">Whether to scan all navigation properties and detach them recursively also. LazyLoading should be turned off when <c>true</c>.</param>
        /// <returns>The count of detached entities</returns>
        public static void DetachEntities<TEntity>(this HookingDbContext ctx, IEnumerable<TEntity> entities, bool deep = false) where TEntity : BaseEntity
        {
            Guard.NotNull(ctx, nameof(ctx));

            using (new DbContextScope(ctx, autoDetectChanges: false, lazyLoading: false))
            {
                entities.Each(x => ctx.DetachEntity(x, deep));
            }
        }

        /// <summary>
        /// Detaches all entities of type <c>TEntity</c> from the current object context
        /// </summary>
        /// <param name="unchangedEntitiesOnly">When <c>true</c>, only entities in unchanged state get detached.</param>
        /// <param name="deep">Whether to scan all navigation properties and detach them recursively also. LazyLoading should be turned off when <c>true</c>.</param>
        /// <returns>The count of detached entities</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int DetachEntities<TEntity>(this HookingDbContext ctx, bool unchangedEntitiesOnly = true, bool deep = false) where TEntity : BaseEntity
        {
            return ctx.DetachEntities(o => o is TEntity, unchangedEntitiesOnly, deep);
        }

        /// <summary>
        /// Detaches all entities matching the passed <paramref name="predicate"/> from the current object context
        /// </summary>
        /// <param name="unchangedEntitiesOnly">When <c>true</c>, only entities in unchanged state will be detached.</param>
        /// <param name="deep">Whether to scan all navigation properties and detach them recursively also.</param>
        /// <returns>The count of detached entities</returns>
        public static int DetachEntities(this HookingDbContext ctx, Func<BaseEntity, bool> predicate, bool unchangedEntitiesOnly = true, bool deep = false)
        {
            Guard.NotNull(predicate, nameof(predicate));

            var numDetached = 0;

            using (new DbContextScope(ctx, autoDetectChanges: false, lazyLoading: false))
            {
                var entries = ctx.ChangeTracker.Entries<BaseEntity>().Where(Match).ToList();

                HashSet<BaseEntity> objSet = deep ? new HashSet<BaseEntity>() : null;

                foreach (var entry in entries)
                {
                    numDetached += ctx.DetachInternal(entry, objSet, deep);
                }

                return numDetached;
            }

            bool Match(EntityEntry<BaseEntity> entry)
            {
                if (entry.State > EfState.Detached && predicate(entry.Entity))
                {
                    return unchangedEntitiesOnly
                        ? entry.State == EfState.Unchanged
                        : true;
                }

                return false;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int DetachInternal(this HookingDbContext ctx, BaseEntity obj, ISet<BaseEntity> objSet, bool deep)
        {
            if (obj == null)
                return 0;

            return ctx.DetachInternal(ctx.Entry(obj), objSet, deep);
        }

        private static int DetachInternal(this HookingDbContext ctx, EntityEntry<BaseEntity> entry, ISet<BaseEntity> objSet, bool deep)
        {
            var obj = entry.Entity;
            int numDetached = 0;

            if (deep)
            {
                // This is to prevent an infinite recursion when the child object has a navigation property
                // that points back to the parent
                if (objSet != null && !objSet.Add(obj))
                    return 0;

                // Recursively detach all navigation properties
                foreach (var prop in FastProperty.GetProperties(obj.GetType()).Values)
                {
                    if (typeof(BaseEntity).IsAssignableFrom(prop.Property.PropertyType))
                    {
                        numDetached += ctx.DetachInternal(prop.GetValue(obj) as BaseEntity, objSet, deep);
                    }
                    else if (typeof(IEnumerable<BaseEntity>).IsAssignableFrom(prop.Property.PropertyType))
                    {
                        var val = prop.GetValue(obj);
                        if (val is IEnumerable<BaseEntity> list)
                        {
                            foreach (var item in list.ToList())
                            {
                                numDetached += ctx.DetachInternal(item, objSet, deep);
                            }
                        }
                    }
                }
            }

            entry.State = EfState.Detached;
            numDetached++;

            return numDetached;
        }

        #endregion

        #region Load collection / reference

        public static bool IsCollectionLoaded<TEntity, TCollection>(
            this HookingDbContext ctx,
            TEntity entity,
            Expression<Func<TEntity, IEnumerable<TCollection>>> navigationProperty)
            where TEntity : BaseEntity
            where TCollection : BaseEntity
        {
            Guard.NotNull(entity, nameof(entity));
            Guard.NotNull(navigationProperty, nameof(navigationProperty));

            var entry = ctx.Entry(entity);
            var collection = entry.Collection(navigationProperty);

            // Avoid System.InvalidOperationException: Member 'IsLoaded' cannot be called for property...
            if (entry.State == EfState.Detached)
            {
                return false;
            }

            return collection.IsLoaded;
        }

        public static bool IsReferenceLoaded<TEntity, TProperty>(
            this HookingDbContext ctx,
            TEntity entity,
            Expression<Func<TEntity, TProperty>> navigationProperty)
            where TEntity : BaseEntity
            where TProperty : BaseEntity
        {
            Guard.NotNull(entity, nameof(entity));
            Guard.NotNull(navigationProperty, nameof(navigationProperty));

            var entry = ctx.Entry(entity);
            var reference = entry.Reference(navigationProperty);

            // Avoid System.InvalidOperationException: Member 'IsLoaded' cannot be called for property...
            if (entry.State == EfState.Detached)
            {
                return false;
            }

            return reference.IsLoaded;
        }

        public static async Task LoadCollectionAsync<TEntity, TCollection>(
            this HookingDbContext ctx,
            TEntity entity,
            Expression<Func<TEntity, IEnumerable<TCollection>>> navigationProperty,
            bool force = false,
            Func<IQueryable<TCollection>, IQueryable<TCollection>> queryAction = null)
            where TEntity : BaseEntity
            where TCollection : BaseEntity
        {
            Guard.NotNull(entity, nameof(entity));
            Guard.NotNull(navigationProperty, nameof(navigationProperty));

            var entry = ctx.Entry(entity);
            var collection = entry.Collection(navigationProperty);

            // Avoid System.InvalidOperationException: Member 'IsLoaded' cannot be called for property...
            if (entry.State == EfState.Detached)
            {
                ctx.Attach(entity);
            }

            if (force)
            {
                collection.IsLoaded = false;
            }

            if (!collection.IsLoaded)
            {
                if (queryAction != null)
                {
                    var query = collection.Query();

                    var myQuery = queryAction != null
                        ? queryAction(query)
                        : query;

                    collection.CurrentValue = await myQuery.ToListAsync();
                }
                else
                {
                    await collection.LoadAsync();
                }

                collection.IsLoaded = true;
            }
        }

        public static async Task LoadReferenceAsync<TEntity, TProperty>(
            this HookingDbContext ctx,
            TEntity entity,
            Expression<Func<TEntity, TProperty>> navigationProperty,
            bool force = false,
            Func<IQueryable<TProperty>, IQueryable<TProperty>> queryAction = null)
            where TEntity : BaseEntity
            where TProperty : BaseEntity
        {
            Guard.NotNull(entity, nameof(entity));
            Guard.NotNull(navigationProperty, nameof(navigationProperty));

            var entry = ctx.Entry(entity);
            var reference = entry.Reference(navigationProperty);

            // Avoid System.InvalidOperationException: Member 'IsLoaded' cannot be called for property...
            if (entry.State == EfState.Detached)
            {
                ctx.Attach(entity);
            }

            if (force)
            {
                reference.IsLoaded = false;
            }

            if (!reference.IsLoaded)
            {
                if (queryAction != null)
                {
                    var query = reference.Query();

                    var myQuery = queryAction != null
                        ? queryAction(query)
                        : query;

                    reference.CurrentValue = await myQuery.FirstOrDefaultAsync();
                }
                else
                {
                    await reference.LoadAsync();
                }

                reference.IsLoaded = true;
            }
        }

        #endregion
    }
}
