﻿using Autofac;
using Smartstore.Core.Catalog.Brands;
using Smartstore.Core.Catalog.Pricing;
using Smartstore.Core.Catalog.Products;
using Smartstore.Core.Content.Seo;

namespace Smartstore.Core.DependencyInjection
{
    public sealed class CatalogModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ProductService>().As<IProductService>().InstancePerLifetimeScope();

            builder.RegisterType<ManufacturerService>()
                .As<IManufacturerService>()
                .As<IXmlSitemapPublisher>()
                //.WithNullCache()  // TODO: (core) Do we really need Autofac registration "WithNullCache"?
                .InstancePerLifetimeScope();

            builder.RegisterType<PriceFormatter>().As<IPriceFormatter>().InstancePerLifetimeScope();
            builder.RegisterType<RecentlyViewedProductsService>().As<IRecentlyViewedProductsService>().InstancePerLifetimeScope();
        }
    }
}
