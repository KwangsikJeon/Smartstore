﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Smartstore.Core.Configuration;
using Smartstore.Core.Data;
using Smartstore.Core.Localization;
using Smartstore.Core.Scheduling;
using Smartstore.Core.Stores;

namespace Smartstore.Core.Content.Seo
{
    public class XmlSitemapBuildContext
    {
        private readonly ISettingFactory _settingFactory;
        private readonly bool _isSingleStoreMode;

        public XmlSitemapBuildContext(Store store, Language[] languages, ISettingFactory settingFactory, bool isSingleStoreMode)
        {
            Guard.NotNull(store, nameof(store));
            Guard.NotEmpty(languages, nameof(languages));

            Store = store;
            Languages = languages;
            Protocol = Store.ForceSslForAllPages ? "https" : "http";

            _settingFactory = settingFactory;
            _isSingleStoreMode = isSingleStoreMode;

            RequestStoreId = _isSingleStoreMode ? 0 : store.Id;
        }

        public CancellationToken CancellationToken { get; init; }
        public ProgressCallback ProgressCallback { get; set; }
        public Store Store { get; init; }
        public int RequestStoreId { get; init; }
        public Language[] Languages { get; init; }
        public int MaximumNodeCount { get; init; } = XmlSitemapGenerator.MaximumSiteMapNodeCount;

        public string Protocol { get; init; }

        public Task<T> LoadSettingsAsync<T>() where T : ISettings, new()
        {
            return _settingFactory.LoadSettingsAsync<T>(RequestStoreId);
        }
    }
}