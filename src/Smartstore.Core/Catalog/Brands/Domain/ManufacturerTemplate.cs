﻿using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Smartstore.Data.Caching;
using Smartstore.Domain;

namespace Smartstore.Core.Catalog.Brands
{
    /// <summary>
    /// Represents a manufacturer template.
    /// </summary>
    [CacheableEntity]
    public partial class ManufacturerTemplate : BaseEntity, IDisplayOrder
    {
        /// <summary>
        /// Gets or sets the template name.
        /// </summary>
        [Required, StringLength(400)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the view path.
        /// </summary>
        [Required, StringLength(400)]
        public string ViewPath { get; set; }

        /// <summary>
        /// Gets or sets the display order.
        /// </summary>
        public int DisplayOrder { get; set; }
    }
}
