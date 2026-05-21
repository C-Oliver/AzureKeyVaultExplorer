// Copyright (c) Microsoft Corporation. All rights reserved. 
// Licensed under the MIT License. See License.txt in the project root for license information. 

using Newtonsoft.Json;
using System;
using System.IO;

namespace Microsoft.Vault.Explorer
{
    /// <summary>
    /// Strongly-typed application configuration loaded from appsettings.json.
    /// Values can be overridden via environment variables at runtime or build-time substitution.
    /// </summary>
    internal sealed class AppConfig
    {
        private static AppConfig? _instance;
        public static AppConfig Current => _instance ?? throw new InvalidOperationException("Call AppConfig.Load() first.");

        public TelemetryConfig Telemetry { get; set; } = new();
        public AzureConfig Azure { get; set; } = new();
        public ApplicationConfig Application { get; set; } = new();

        public static void Load()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
            if (File.Exists(path))
            {
                _instance = JsonConvert.DeserializeObject<AppConfig>(File.ReadAllText(path)) ?? new AppConfig();
            }
            else
            {
                _instance = new AppConfig();
            }

            // Environment variable overrides (highest priority)
            var connStr = Environment.GetEnvironmentVariable("APPINSIGHTS_CONNECTION_STRING");
            if (!string.IsNullOrEmpty(connStr))
                _instance.Telemetry.ConnectionString = connStr;

            var vaultSuffix = Environment.GetEnvironmentVariable("AZURE_KEYVAULT_DNS_SUFFIX");
            if (!string.IsNullOrEmpty(vaultSuffix))
                _instance.Azure.VaultDnsSuffix = vaultSuffix;

            var mgmtEndpoint = Environment.GetEnvironmentVariable("AZURE_MANAGEMENT_ENDPOINT");
            if (!string.IsNullOrEmpty(mgmtEndpoint))
                _instance.Azure.ManagementEndpoint = mgmtEndpoint;
        }

        /// <summary>
        /// Builds a vault URI from a vault name using the configured DNS suffix.
        /// </summary>
        public static Uri BuildVaultUri(string vaultName)
        {
            if (vaultName.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                return new Uri(vaultName);
            return new Uri($"https://{vaultName}.{Current.Azure.VaultDnsSuffix}/");
        }
    }

    internal sealed class TelemetryConfig
    {
        public string ConnectionString { get; set; } = "";
        public bool Enabled { get; set; } = false;
    }

    internal sealed class AzureConfig
    {
        public string ManagementEndpoint { get; set; } = "https://management.azure.com/";
        public string ManagementScope { get; set; } = "https://management.azure.com/.default";
        public string VaultDnsSuffix { get; set; } = "vault.azure.net";
        public string SubscriptionApiVersion { get; set; } = "2022-12-01";
        public string KeyVaultApiVersion { get; set; } = "2023-07-01";
    }

    internal sealed class ApplicationConfig
    {
        public string HelpUrl { get; set; } = "https://aka.ms/vaultexplorer";
        public string ShareLinkBaseUrl { get; set; } = "https://aka.ms/ve";
        public string FeedbackEmail { get; set; } = "vedev@microsoft.com";
        public int IdleTimeoutMinutes { get; set; } = 60;
        public string TokenCacheDirectory { get; set; } = @"%AppData%\Microsoft\Vault";
    }
}
