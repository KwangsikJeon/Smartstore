﻿using System;
using Autofac;
using Smartstore.Core.Customers;

namespace Smartstore.Core.DependencyInjection
{
    public sealed class CustomersModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<CustomerService>().As<ICustomerService>().InstancePerLifetimeScope();
        }
    }
}