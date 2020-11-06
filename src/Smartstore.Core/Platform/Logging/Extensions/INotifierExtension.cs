﻿using System;
using System.Runtime.CompilerServices;
using Smartstore.Core.Localization;
using Smartstore.Core.Logging;

namespace Smartstore
{
    public static class INotifierExtension
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Information(this INotifier notifier, LocalizedString message, bool durable = true)
        {
            notifier.Add(NotifyType.Info, message, durable);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Success(this INotifier notifier, LocalizedString message, bool durable = true)
        {
            notifier.Add(NotifyType.Success, message, durable);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Warning(this INotifier notifier, LocalizedString message, bool durable = true)
        {
            notifier.Add(NotifyType.Warning, message, durable);
        }

        public static void Error(this INotifier notifier, Exception exception, bool durable = true)
        {
            if (exception == null)
                return;

            while (exception.InnerException != null)
            {
                exception = exception.InnerException;
            }

            notifier.Add(NotifyType.Error, exception.Message, durable);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Error(this INotifier notifier, LocalizedString message, bool durable = true)
        {
            notifier.Add(NotifyType.Error, message, durable);
        }
    }
}