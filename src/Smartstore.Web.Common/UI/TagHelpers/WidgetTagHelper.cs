﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Smartstore.Web.UI.TagHelpers
{
    [HtmlTargetElement("widget", Attributes = "target")]
    public class WidgetTagHelper : SmartTagHelper
    {
        private const string UniqueKeysKey = "WidgetTagHelper.UniqueKeys";

        private readonly IWidgetProvider _widgetProvider;

        public WidgetTagHelper(IWidgetProvider widgetProvider)
        {
            _widgetProvider = widgetProvider;
        }

        /// <summary>
        /// The target zone name to inject this widget to.
        /// </summary>
        public string Target { get; set; }

        /// <summary>
        /// The order within the target zone.
        /// </summary>
        [HtmlAttributeName("order")]
        public int Ordinal { get; set; }

        /// <summary>
        /// When set, ensures uniqueness within a request
        /// </summary>
        public string Key { get; set; }

        protected override string GenerateTagId(TagHelperContext context) => null;

        protected override async Task ProcessCoreAsync(TagHelperContext context, TagHelperOutput output)
        {
            output.SuppressOutput();

            if (Target.IsEmpty())
            {
                return;
            }

            if (Key.HasValue())
            {
                var uniqueKeys = GetUniqueKeys();
                if (uniqueKeys.Contains(Key))
                {
                    return;
                }
                uniqueKeys.Add(Key);
            }
            
            var childContent = await output.GetChildContentAsync();
            var widget = new HtmlWidgetInvoker(childContent) { Order = Ordinal };

            _widgetProvider.RegisterWidget(Target, widget);
        }

        private HashSet<string> GetUniqueKeys()
        {
            return ViewContext.HttpContext.GetItem(UniqueKeysKey, () => new HashSet<string>(StringComparer.OrdinalIgnoreCase));
        }
    }
}
