﻿using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using Smartstore.Engine;

namespace Smartstore.Web.Modelling
{
    public abstract partial class ModelBase
    {
        private readonly static ContextState<Dictionary<ModelBase, IDictionary<string, object>>> _contextState = new("ModelBase.CustomThreadProperties");

        public virtual void BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
        }

        /// <summary>
        /// Gets a custom property value either from the thread local or the static storage (in this particular order)
        /// </summary>
        /// <typeparam name="TProperty">Type of property</typeparam>
        /// <param name="key">Custom property key</param>
        /// <returns>The property value or null</returns>
        public TProperty Get<TProperty>(string key)
        {
            Guard.NotEmpty(key, nameof(key));

            if (TryGetCustomThreadProperties(false, out var dict) && dict.TryGetValue(key, out var value))
            {
                return (TProperty)value;
            }

            if (CustomProperties.TryGetValue(key, out value))
            {
                return (TProperty)value;
            }

            return default;
        }

        /// <summary>
        /// Use this property to store any custom value for your models. 
        /// </summary>
		public CustomPropertiesDictionary CustomProperties { get; set; } = new();

        /// <summary>
        /// A data bag for custom model properties which only
        /// lives during a thread/request lifecycle
        /// </summary>
        /// <remarks>
        /// Use thread properties whenever you need to persist request-scoped data,
        /// but the model is potentially cached statically.
        /// </remarks>
        [JsonIgnore]
        public IDictionary<string, object> CustomThreadProperties
        {
            get
            {
                TryGetCustomThreadProperties(true, out var dict);
                return dict;
            }
        }

        private bool TryGetCustomThreadProperties(bool create, out IDictionary<string, object> dict)
        {
            dict = null;
            var state = _contextState.GetState();

            if (state == null && create)
            {
                state = new Dictionary<ModelBase, IDictionary<string, object>>();
                _contextState.SetState(state);
            }

            if (state != null)
            {
                if (!state.TryGetValue(this, out dict))
                {
                    if (create)
                    {
                        dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                        state[this] = dict;
                    }
                }

                return dict != null;
            }

            return false;
        }
    }

    public abstract partial class EntityModelBase : ModelBase
    {
        // TODO: (core) (Re)implement SmartResourceDisplayName
        //[SmartResourceDisplayName("Admin.Common.Entity.Fields.Id")]
        public virtual int Id { get; set; }

        /// <remarks>
        /// This property is required for serialization JSON data of Telerik grids.
        /// Without a lower case Id property in JSON results its AJAX operations do not work correctly.
        /// Occurs since RouteCollection.LowercaseUrls was set to true in Global.asax.
        /// </remarks>
        [JsonProperty("id")]
        internal int EntityId => Id;
    }

    public abstract partial class TabbableModel : EntityModelBase
    {
        public virtual string[] LoadedTabs { get; set; }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class CustomModelPartAttribute : Attribute
    {
    }

    public sealed class CustomPropertiesDictionary : Dictionary<string, object>
    {
    }
}
