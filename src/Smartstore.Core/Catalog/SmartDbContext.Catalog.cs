﻿using Microsoft.EntityFrameworkCore;
using Smartstore.Core.Catalog.Attributes;
using Smartstore.Core.Catalog.Brands;
using Smartstore.Core.Catalog.Categories;
using Smartstore.Core.Catalog.Discounts;
using Smartstore.Core.Catalog.Pricing;
using Smartstore.Core.Catalog.Products;

namespace Smartstore.Core.Data
{
    public partial class SmartDbContext
    {
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Manufacturer> Manufacturers { get; set; }

        public DbSet<ProductCategory> ProductCategories { get; set; }
        public DbSet<ProductManufacturer> ProductManufacturers { get; set; }

        public DbSet<RelatedProduct> RelatedProducts { get; set; }
        public DbSet<CrossSellProduct> CrossSellProducts { get; set; }
        public DbSet<ProductBundleItem> ProductBundleItem { get; set; }
        public DbSet<ProductMediaFile> ProductMediaFiles { get; set; }

        public DbSet<ProductAttribute> ProductAttributes { get; set; }
        public DbSet<ProductVariantAttribute> ProductVariantAttributes { get; set; }
        public DbSet<ProductVariantAttributeValue> ProductVariantAttributeValues { get; set; }
        public DbSet<ProductVariantAttributeCombination> ProductVariantAttributeCombinations { get; set; }

        public DbSet<ProductAttributeOptionsSet> ProductAttributeOptionsSets { get; set; }
        public DbSet<ProductAttributeOption> ProductAttributeOptions { get; set; }

        public DbSet<SpecificationAttribute> SpecificationAttributes { get; set; }
        public DbSet<SpecificationAttributeOption> SpecificationAttributeOptions { get; set; }
        public DbSet<ProductSpecificationAttribute> ProductSpecificationAttributes { get; set; }

        public DbSet<Discount> Discounts { get; set; }
        public DbSet<TierPrice> TierPrices { get; set; }
    }
}