﻿using Autofac;

namespace Smartstore.Core.Configuration.DependencyInjection
{
    public sealed class SettingsModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterSource(new SettingsSource());

            builder.RegisterType<SettingService>()
                .As<ISettingService>()
                .SingleInstance();
        }
    }
}