﻿using HomeSeer.PluginSdk;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Globalization;
using System.IO;

#nullable enable

namespace Hspi
{
    // uses Serlog as underlying framework
    internal static class Logger
    {
        public static void ConfigureLogging(bool enableDebugLogging,
                                            bool logToFile,
                                            IHsController? hsController = null)
        {
            var config = new LoggerConfiguration().WriteTo.Console();

            config = enableDebugLogging ? config.MinimumLevel.Debug() : config.MinimumLevel.Information();

            if (hsController != null)
            {
                var hsTarget = new HomeSeerTarget(hsController);
                config = config.WriteTo.Sink(hsTarget);
            }

            if (logToFile)
            {
                using var process = System.Diagnostics.Process.GetCurrentProcess();
                string mainExeFile = process.MainModule.FileName;
                string hsDir = Path.GetDirectoryName(mainExeFile);
                string logFile = Path.Combine(hsDir, "logs", PlugInData.PlugInId, "file.log");

                config = config.WriteTo.File(logFile, fileSizeLimitBytes: 10 * 1024 * 1024);
            }
            Log.Logger = config.CreateLogger();
        }

        public sealed class HomeSeerTarget : ILogEventSink
        {
            public HomeSeerTarget(IHsController hsController)
            {
                this.loggerWeakReference = new WeakReference<IHsController>(hsController);
            }

            public void Emit(LogEvent logEvent)
            {
                if (loggerWeakReference.TryGetTarget(out var logger))
                {
                    string message = logEvent.RenderMessage(CultureInfo.InvariantCulture);
                    if (logEvent.Level.Equals(LogEventLevel.Debug))
                    {
                        logger?.WriteLog(HomeSeer.PluginSdk.Logging.ELogType.Debug, message, PlugInData.PlugInName);
                    }
                    else if (logEvent.Level.Equals(LogEventLevel.Information))
                    {
                        logger?.WriteLog(HomeSeer.PluginSdk.Logging.ELogType.Info, message, PlugInData.PlugInName);
                    }
                    else if (logEvent.Level.Equals(LogEventLevel.Warning))
                    {
                        logger?.WriteLog(HomeSeer.PluginSdk.Logging.ELogType.Warning, message, PlugInData.PlugInName, "#D58000");
                    }
                    else if (logEvent.Level >= LogEventLevel.Error)
                    {
                        logger?.WriteLog(HomeSeer.PluginSdk.Logging.ELogType.Error, message, PlugInData.PlugInName, "#FF0000");
                    }
                }
            }

            private readonly WeakReference<IHsController> loggerWeakReference;
        }
    }
}