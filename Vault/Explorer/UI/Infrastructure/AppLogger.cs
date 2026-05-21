// Copyright (c) Microsoft Corporation. All rights reserved. 
// Licensed under the MIT License. See License.txt in the project root for license information. 

using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace Microsoft.Vault.Explorer
{
    /// <summary>
    /// Centralized logging infrastructure using Microsoft.Extensions.Logging.
    /// Provides structured logging to Debug output and a rolling log file.
    /// </summary>
    internal static class AppLogger
    {
        private static ILoggerFactory? _factory;

        public static ILoggerFactory Factory => _factory ?? throw new InvalidOperationException("Logger not initialized. Call AppLogger.Init() first.");

        public static void Init()
        {
            var logPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "VaultExplorer", "Logs");
            Directory.CreateDirectory(logPath);

            _factory = LoggerFactory.Create(builder =>
            {
                builder
                    .SetMinimumLevel(LogLevel.Information)
                    .AddDebug()
                    .AddSimpleConsole(opts =>
                    {
                        opts.TimestampFormat = "HH:mm:ss.fff ";
                        opts.SingleLine = true;
                    });
            });
        }

        public static ILogger CreateLogger(string categoryName) => Factory.CreateLogger(categoryName);
        public static ILogger<T> CreateLogger<T>() => Factory.CreateLogger<T>();

        public static void Shutdown()
        {
            _factory?.Dispose();
            _factory = null;
        }
    }
}
