﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Smartstore.Core.Data;
using Smartstore.Data.Hooks;
using Smartstore.Events;
using Smartstore.Engine;
using Smartstore.Web.Models;
using Smartstore.Core.Configuration;
using Smartstore.Core.Stores;
using Smartstore.Caching;
using Smartstore.Threading;
using System.Threading;
using Smartstore.Web.Theming;
using Microsoft.Extensions.Logging.Abstractions;
using Smartstore.Core;
using Smartstore.Core.Logging;
using LogLevel = Smartstore.Core.Logging.LogLevel;
using System.Text;
using Smartstore.Core.Common;
using Smartstore.Data.Caching;
using Smartstore.Core.Localization;
using Smartstore.Core.Content.Seo;
using Microsoft.AspNetCore.Routing;
using Smartstore.Core.Localization.Routing;
using Smartstore.Core.Checkout.Tax;

namespace Smartstore.Web.Controllers
{
    public class MyProgress
    {
        public int Percent { get; set; }
        public string Message { get; set; }
    }

    public class TestSettings : ISettings
    {
        public string Prop1 { get; set; } = "Prop1";
        public string Prop2 { get; set; } = "Prop2";
        public string Prop3 { get; set; } = "Prop3";
    }

    public class HomeController : Controller
    {
        private static CancellationTokenSource _cancelTokenSource = new();
        
        private readonly SmartDbContext _db;
        private readonly IEventPublisher _eventPublisher;
        private readonly ISettingFactory _settingFactory;
        private readonly IStoreContext _storeContext;
        private readonly ILogger<HomeController> _logger1;
        private readonly ILogger _logger2;
        private readonly ICacheManager _cache;
        private readonly IAsyncState _asyncState;
        private readonly IThemeRegistry _themeRegistry;
        private readonly ICommonServices _services;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILocalizationService _locService;

        public HomeController(
            SmartDbContext db, 
            ILogger<HomeController> logger1,
            ILogger logger2,
            ISettingFactory settingFactory,
            IEventPublisher eventPublisher,
            IDbContextFactory<SmartDbContext> dbContextFactory,
            IStoreContext storeContext,
            IEnumerable<IDbSaveHook> hooks,
            ICacheManager cache,
            IAsyncState asyncState,
            IThemeRegistry themeRegistry,
            TaxSettings taxSettings,
            ICommonServices services,
            ILoggerFactory loggerFactory,
            ILocalizationService locService)
        {
            _db = db;
            _eventPublisher = eventPublisher;
            _settingFactory = settingFactory;
            _storeContext = storeContext;
            _logger1 = logger1;
            _logger2 = logger2;
            _cache = cache;
            _asyncState = asyncState;
            _themeRegistry = themeRegistry;
            _services = services;
            _loggerFactory = loggerFactory;
            _locService = locService;

            var currentStore = _services.StoreContext.CurrentStore;
        }

        public ILogger Logger { get; set; } = NullLogger.Instance;

        public Localizer T { get; set; } = NullLocalizer.Instance;

        [LocalizedRoute("/", Name = "Homepage")]
        public async Task<IActionResult> Index()
        {
            #region Settings Test
            ////var xxx = await _services.Settings.GetSettingByKeyAsync<bool>("CatalogSettings.ShowPopularProductTagsOnHomepage", true, 2, true);

            ////await _services.SettingFactory.SaveSettingsAsync(new TestSettings(), 1);
            ////await _db.SaveChangesAsync();

            ////await _services.Settings.ApplySettingAsync("yodele.gut", "yodele");
            ////await _services.Settings.ApplySettingAsync("yodele.schlecht", "yodele");
            ////await _services.Settings.ApplySettingAsync("yodele.prop3", "yodele");
            ////await _services.Settings.ApplySettingAsync("yodele.prop4", "yodele");
            ////await _db.SaveChangesAsync();

            ////var yodele1 = await _services.Settings.GetSettingByKeyAsync<string>("yodele.gut");
            ////var yodele2 = await _services.Settings.GetSettingByKeyAsync<string>("yodele.schlecht");
            ////var yodele3 = await _services.Settings.GetSettingByKeyAsync<string>("yodele.prop3");
            ////var yodele4 = await _services.Settings.GetSettingByKeyAsync<string>("yodele.prop4");
            //////await _services.Settings.DeleteSettingsAsync("yodele");
            ////yodele1 = await _services.Settings.GetSettingByKeyAsync<string>("yodele.gut");

            ////await _db.SaveChangesAsync();

            //var testSettings = await _services.SettingFactory.LoadSettingsAsync<TestSettings>(1);
            //testSettings.Prop1 = CommonHelper.GenerateRandomDigitCode(10);
            //testSettings.Prop2 = CommonHelper.GenerateRandomDigitCode(10);
            //testSettings.Prop3 = CommonHelper.GenerateRandomDigitCode(10);
            //var numSaved = await _services.SettingFactory.SaveSettingsAsync(testSettings, 1);
            #endregion

            //_cancelTokenSource = new CancellationTokenSource();
            //await _asyncState.CreateAsync(new MyProgress(), cancelTokenSource: _cancelTokenSource);

            //var result = await _services.Resolve<IDbLogService>().ClearLogsAsync(new DateTime(2016, 12, 31), LogLevel.Fatal);

            //var count = await _db.Countries
            //    .AsNoTracking()
            //    .Where(x => x.SubjectToVat)
            //    .AsCaching()
            //    .CountAsync();

            //var langService = _services.Resolve<ILanguageService>();
            //for (var i = 0; i < 50; i++)
            //{
            //    var lid = await langService.GetDefaultLanguageIdAsync();
            //    var storeCache = _storeContext.GetCachedStores();
            //    var anon = await _db.Countries
            //        .AsNoTracking()
            //        .Where(x => x.SubjectToVat == true && x.DisplayOrder > 0)
            //        .AsCaching()
            //        .Select(x => new { x.Id, x.Name, x.TwoLetterIsoCode })
            //        .ToListAsync();
            //}

            var anon = await _db.Countries
                .AsNoTracking()
                .Where(x => x.SubjectToVat == true && x.DisplayOrder > 0)
                .AsCaching()
                .Select(x => new { x.Id, x.Name, x.TwoLetterIsoCode })
                .ToListAsync();

            //var anon2 = _db.Countries
            //    .AsNoTracking()
            //    .Where(x => x.SubjectToVat == true && x.DisplayOrder > 1)
            //    .AsCaching()
            //    //.Select(x => new { x.Id, x.Name, x.TwoLetterIsoCode })
            //    .ToList();

            //var noResult = _db.Countries
            //    .AsNoTracking()
            //    .Where(x => x.Name == "fsdfsdfsdfsfsdfd")
            //    .AsCaching()
            //    //.Select(x => new { x.Id, x.Name, x.TwoLetterIsoCode })
            //    .FirstOrDefault();

            #region MH test area

            // QuantityUnit
            // Get QuantityUnit by Id
            var qu = _db.QuantityUnits.ApplyQuantityUnitFilter(1).FirstOrDefault();

            // Save hook > TODO: BROKEN > Why?
            qu.IsDefault = true;
            _db.SaveChanges();
            // TODO Test: Assert.OnlyOne has Default = true, 

            // Delete hook
            var qu2 = _db.QuantityUnits.ApplyQuantityUnitFilter(22).FirstOrDefault();

            if (qu2 != null)
            {
                _db.QuantityUnits.Remove(qu2);
                await _db.SaveChangesAsync();
            }

            // StateProvince
            var sp = _db.StateProvinces
                .ApplyCountryFilter(1)
                .ApplyAbbreviationFilter("BE")
                .FirstOrDefault();
            // TODO Test: Assert name of entity is Berlin

            // DeliveryTime
            //var dt = _db.DeliveryTimes.GetDeliveryTimeFilter(1);

            var test = "";

            #endregion


            return View();
        }

        [LocalizedRoute("/privacy", Name = "Privacy")]
        public async Task<IActionResult> Privacy()
        {
            #region Settings Test
            //var xxx = await _services.Settings.GetSettingByKeyAsync<bool>("CatalogSettings.ShowPopularProductTagsOnHomepage", true, 2, true);

            //var yodele1 = await _services.Settings.GetSettingByKeyAsync<string>("yodele.gut");
            //var yodele2 = await _services.Settings.GetSettingByKeyAsync<string>("yodele.schlecht");
            //var yodele3 = await _services.Settings.GetSettingByKeyAsync<string>("yodele.prop3");
            //var yodele4 = await _services.Settings.GetSettingByKeyAsync<string>("yodele.prop4");
            ////await _services.Settings.DeleteSettingsAsync("yodele");
            //yodele1 = await _services.Settings.GetSettingByKeyAsync<string>("yodele.gut");

            //var testSettings = await _services.SettingFactory.LoadSettingsAsync<TestSettings>(1);
            #endregion

            //await _asyncState.UpdateAsync<MyProgress>(x =>
            //{
            //    x.Percent++;
            //    x.Message = $"Fortschritt {x.Percent}";
            //});

            //var numDeleted = await _locService.DeleteLocaleStringResourcesAsync("Yodele.Nixda");

            await using var scope = await _db.OpenConnectionAsync();

            var products = await _db.Products.OrderByDescending(x => x.Id).Skip(600).Take(100).ToListAsync();
            var urlService = _services.Resolve<IUrlService>();

            //foreach (var product in products)
            //{
            //    var result = await urlService.ValidateSlugAsync(product, product.Name, true);
            //    //await urlService.ApplySlugAsync(result, false);
            //}

            //using var batchScope = urlService.CreateBatchScope();
            //foreach (var product in products)
            //{
            //    batchScope.ApplySlugs(new ValidateSlugResult { Source = product, Slug = product.BuildSlug() });
            //}
            //await batchScope.CommitAsync();

            //int numSaved = await _db.SaveChangesAsync();

            return View();
        }

        [LocalizedRoute("/logs")]
        public async Task<IActionResult> Logs()
        {
            #region Test

            //var logger = _loggerFactory.CreateLogger("File");
            //logger.Debug("Yodeleeeee");
            //logger.Info("Yodeleeeee");
            //logger.Warn("Yodeleeeee");
            //logger.Error("Yodeleeeee");

            //logger = _loggerFactory.CreateLogger("File/App_Data/Logs/yodele/");
            //logger.Debug("Yodeleeeee");
            //logger.Info("Yodeleeeee");
            //logger.Warn("Yodeleeeee");
            //logger.Error("Yodeleeeee");

            //logger = _loggerFactory.CreateLogger("File/App_Data/Logs/hello");
            //logger.Debug("Yodeleeeee");
            //logger.Info("Yodeleeeee");
            //logger.Warn("Yodeleeeee");
            //logger.Error("Yodeleeeee");

            #endregion

            var query = _db.Logs
                .AsNoTracking()
                .Where(x => x.CustomerId != 399003)
                .OrderByDescending(x => x.CreatedOnUtc)
                .Take(500);

            var logs = await query.ToListAsync();

            var country = await _db.Countries.OrderByDescending(x => x.Id).FirstOrDefaultAsync();
            country.DisplayCookieManager = !country.DisplayCookieManager;
            await _db.SaveChangesAsync();

            return View(logs);
        }

        [LocalizedRoute("/countries")]
        public async Task<IActionResult> Countries()
        {
            #region Test

            var taxSettings = await _settingFactory.LoadSettingsAsync<TaxSettings>(_storeContext.CurrentStore.Id);

            //_cache.Put("a", new CacheEntry { Key = "a", Value = "a" });
            //_cache.Put("b", new CacheEntry { Key = "b", Value = "b", Dependencies = new[] { "a" } });
            //_cache.Put("c", new CacheEntry { Key = "c", Value = "c", Dependencies = new[] { "a", "b" } });
            //_cache.Put("d", new CacheEntry { Key = "d", Value = "d", Dependencies = new[] { "a", "b", "c" } });

            ////_cache.Remove("d");
            ////_cache.Remove("c");
            //_cache.Remove("b");
            ////_cache.Remove("a");

            #endregion

            Logger.Error(new Exception("WTF Exception"), "WTF maaaan");
            Logger.Warn("WTF WARN maaaan");

            Logger.Info("INFO maaaan");
            _logger1.Info("INFO maaaan");
            _logger2.Info("INFO maaaan");
            _logger2.Warn("WARN maaaan");
            //_logger2.Error("WARN maaaan");

            //_asyncState.Cancel<MyProgress>();
            ////_cancelTokenSource.Cancel();
            //_cancelTokenSource = new CancellationTokenSource();

            var query = _db.Countries
                .AsNoTracking()
                .ApplyStoreFilter(1)
                .Include(x => x.StateProvinces)
                .OrderBy(x => x.DisplayOrder)
                .ThenBy(x => x.Name);

            //var countries = await GetCountriesCached();
            //countries = await GetCountriesCached();
            //countries = await GetCountriesCached();
            //countries = await GetCountriesCached();
            //countries = await GetCountriesCached();
            //countries = await GetCountriesCached();

            //var countries = await GetCountriesUncached();
            //countries = await GetCountriesUncached();
            //countries = await GetCountriesUncached();
            //countries = await GetCountriesUncached();
            //countries = await GetCountriesUncached();
            //countries = await GetCountriesUncached();

            //var country = GetCountryCachedSync();
            //country = GetCountryCachedSync();
            //country = GetCountryCachedSync();

            var countries = await GetCountries();
            for (var i = 0; i < 10; i++)
            {
                var country = await GetCountry();
            }

            //_db.SaveChanges();

            return View(countries);
        }

        [Route("settings")]
        public async Task<IActionResult> Settings()
        {
            await _asyncState.RemoveAsync<MyProgress>();

            var settings = await _db.Settings
                .AsNoTracking()
                .ApplySorting()
                .Take(500)
                .ToListAsync();

            #region Test

            var p = _db.DataProvider;

            //await p.BackupDatabaseAsync(@"D:\_Backup\db\yoman.bak");
            //await p.RestoreDatabaseAsync(@"D:\_Backup\db\yoman.bak");

            //var x = p.HasTable("Product");
            //var y = await p.HasTableAsync("xxxxxProduct");
            //var z = p.HasDatabase("yogisan-db");
            //z = await p.HasDatabaseAsync("FelgenOnline");
            //z = p.HasDatabase("yodeleeeeeee");
            //z = p.HasColumn("Discount", "Name");
            //z = await p.HasColumnAsync("Discount", "xxxxxName");

            //var ident = p.GetTableIdent<Store>();
            //ident = await p.GetTableIdentAsync<Country>();
            //ident = p.GetTableIdent<Setting>();

            //var size = p.GetDatabaseSize();
            //////await p.ShrinkDatabaseAsync();
            ////p.ReIndexTables();
            ////p.ShrinkDatabase();
            ////size = p.GetDatabaseSize();

            return View(settings);

            //var attrs = new GenericAttribute[] 
            //{
            //    new GenericAttribute { EntityId = 1, Key = "Yo", KeyGroup = "Man", StoreId = 1, Value = "Wert" },
            //    new GenericAttribute { EntityId = 1, Key = "Yo", KeyGroup = "Man", StoreId = 1, Value = "Wert" },
            //    new GenericAttribute { EntityId = 1, Key = "Yo", KeyGroup = "Man", StoreId = 1, Value = "Wert" },
            //    new GenericAttribute { EntityId = 1, Key = "Yo", KeyGroup = "Man", StoreId = 1, Value = "Wert" }
            //};
            //var maps = new StoreMapping[]
            //{
            //    new StoreMapping { EntityId = 1, EntityName = "Product", StoreId = 1 },
            //    new StoreMapping { EntityId = 1, EntityName = "Product", StoreId = 1 },
            //    new StoreMapping { EntityId = 1, EntityName = "Product", StoreId = 1 },
            //    new StoreMapping { EntityId = 1, EntityName = "Product", StoreId = 1 },
            //    new StoreMapping { EntityId = 1, EntityName = "Product", StoreId = 1 },
            //    new StoreMapping { EntityId = 1, EntityName = "Product", StoreId = 1 }
            //};

            //_db.GenericAttributes.AddRange(attrs);
            //_db.StoreMappings.AddRange(maps);

            //await _db.SaveChangesAsync();

            //_db.GenericAttributes.RemoveRange(attrs);
            //_db.StoreMappings.RemoveRange(maps);

            //_db.SaveChanges();

            #endregion
        }

        private Task<List<Country>> GetCountries()
        {
            var query = _db.Countries
                .AsNoTracking()
                .ApplyStoreFilter(1)
                .Include(x => x.StateProvinces)
                .OrderBy(x => x.DisplayOrder)
                .ThenBy(x => x.Name);

            return query.ToListAsync();
        }

        private Task<Country> GetCountry()
        {
            var query = _db.Countries
                .AsNoTracking()
                .ApplyStoreFilter(1)
                .Include(x => x.StateProvinces)
                .OrderBy(x => x.DisplayOrder)
                .ThenBy(x => x.Name);

            return query.FirstOrDefaultAsync();
        }

        private Country GetCountryCachedSync()
        {
            var query = _db.Countries
                .AsNoTracking()
                .ApplyStoreFilter(1)
                .Include(x => x.StateProvinces)
                .OrderBy(x => x.DisplayOrder)
                .ThenBy(x => x.Name);

            return query.FirstOrDefault();
        }

        private List<Country> GetCountriesUncachedSync()
        {
            var query = _db.Countries
                .AsNoTracking()
                .ApplyStoreFilter(1)
                .Include(x => x.StateProvinces)
                .OrderBy(x => x.DisplayOrder)
                .ThenBy(x => x.Name);

            return query.ToList();
        }

        private List<Country> GetCountriesCachedSync()
        {
            var query = _db.Countries
                .AsNoTracking()
                .ApplyStoreFilter(1)
                .Include(x => x.StateProvinces)
                .OrderBy(x => x.DisplayOrder)
                .ThenBy(x => x.Name);

            return query.ToList();
        }

        public IActionResult Slug()
        {
            var e = (UrlRecord)HttpContext.GetRouteData().Values["entity"];
            return Content($"Slug matched >>> Entity: {e.EntityName} {e.EntityId}, Id: {e.Id}, Language: {e.LanguageId}, Slug: {e.Slug}, IsActive: {e.IsActive}");
        }

        public async Task<IActionResult> MgTest()
        {
            var content = new StringBuilder();

            //var discount = await _db.Discounts.FindByIdAsync(29);
            //_db.Discounts.Remove(discount);
            //await _db.SaveChangesAsync();


            var product = await _db.Products.FindByIdAsync(4366);
            content.AppendLine($"number of applied discounts {product.AppliedDiscounts.Count}: {string.Join(", ", product.AppliedDiscounts.Select(x => x.Id))}. has discounts applied {product.HasDiscountsApplied}.");

            var discount = await _db.Discounts.FirstOrDefaultAsync(x => x.Id == 25);
            product.AppliedDiscounts.Add(discount);
            await _db.SaveChangesAsync();

            product = await _db.Products.FindByIdAsync(4366);
            content.AppendLine($"number of applied discounts {product.AppliedDiscounts.Count}: {string.Join(", ", product.AppliedDiscounts.Select(x => x.Id))}. has discounts applied {product.HasDiscountsApplied}.");


            //var productIds = new int[] { 4317, 1748, 1749, 1750, 4317, 4366 };

            //var query = _db.Discounts
            //    .SelectMany(x => x.AppliedToProducts)
            //    .Where(x => productIds.Contains(x.Id))
            //    .Select(x => x.Id)
            //    .Distinct();

            //content.AppendLine("Query string:");
            //content.AppendLine(query.ToQueryString());
            //content.AppendLine();

            //var ids = await query.ToListAsync();
            //content.AppendLine($"discount applied to products {ids.Count}: {string.Join(", ", ids)}");

            return Content(content.ToString());
        }
    }
}
