﻿using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Smartstore.Core.Localization;
using Smartstore.Data.Caching;
using Smartstore.Domain;

namespace Smartstore.Core.Common
{
    public class MeasureWeightMap : IEntityTypeConfiguration<MeasureWeight>
    {
        public void Configure(EntityTypeBuilder<MeasureWeight> builder)
        {
            builder.Property(c => c.Ratio).HasPrecision(18, 8);
        }
    }

    /// <summary>
    /// Represents a measure weight
    /// </summary>
    [CacheableEntity]
    public partial class MeasureWeight : BaseEntity, ILocalizedEntity, IDisplayOrder
    {
        /// <summary>
        /// Gets or sets the name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the system keyword
        /// </summary>
        public string SystemKeyword { get; set; }

        /// <summary>
        /// Gets or sets the ratio
        /// </summary>
        public decimal Ratio { get; set; }

        /// <summary>
        /// Gets or sets the display order
        /// </summary>
        public int DisplayOrder { get; set; }
    }
}