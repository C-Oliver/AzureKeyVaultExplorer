// Copyright (c) Microsoft Corporation. All rights reserved. 
// Licensed under the MIT License. See License.txt in the project root for license information. 

using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.ApplicationInsights.Channel;
using System.Diagnostics;

namespace Microsoft.Vault.Explorer
{
    public static class Telemetry
    {
        public static TelemetryClient Default { get; private set; }

        public static void Init()
        {
            TelemetryConfiguration config = TelemetryConfiguration.CreateDefault();
            var telemetryConfig = AppConfig.Current.Telemetry;
            if (telemetryConfig.Enabled && !string.IsNullOrEmpty(telemetryConfig.ConnectionString))
            {
                config.ConnectionString = telemetryConfig.ConnectionString;
                config.DisableTelemetry = Settings.Default.DisableTelemetry;
            }
            else
            {
                config.DisableTelemetry = true;
            }
            Default = new TelemetryClient(config);
            config.TelemetryInitializers.Add(new TelemetryInitializer());
        }
    }

    internal class TelemetryInitializer : ITelemetryInitializer
    {
        private static readonly Guid SessionId = Guid.NewGuid();
        private static readonly string UserDomainName = Environment.UserDomainName;
        private static readonly string UserName = Environment.UserName;
        private static readonly string AppVersion = Utils.GetFileVersionString("", Path.GetFileName(Application.ExecutablePath));
        private static readonly string OSVersion = Environment.OSVersion.ToString();

        public void Initialize(ITelemetry telemetry)
        {
            telemetry.Context.Session.Id = SessionId.ToString();

            // Do not send PII — use hashed identifiers only
            telemetry.Context.User.AccountId = ComputeHash(UserDomainName);
            telemetry.Context.User.Id = ComputeHash(UserName);

            telemetry.Context.Component.Version = AppVersion;

            telemetry.Context.Device.OperatingSystem = OSVersion;
        }

        private static string ComputeHash(string value)
        {
            var bytes = System.Security.Cryptography.SHA256.HashData(Encoding.UTF8.GetBytes(value));
            return Convert.ToHexString(bytes)[..16]; // truncated hash, not reversible
        }
    }

    /// <summary>
    /// Base class for forms to track Telemetry data as a PageView event.
    /// Also applies duration to the logged entry
    /// </summary>
    public class FormTelemetry : Form
    {
        private readonly PageViewTelemetry _telemetryData;

        private bool _viewLogged = false;
        private DateTimeOffset _startTime;

        /// <summary>
        /// Initializes a new instance of the <see cref="FormTelemetry"/> class.
        /// </summary>
        public FormTelemetry()
        {
            _telemetryData = new PageViewTelemetry(!string.IsNullOrWhiteSpace(this.Name) ? this.Name : this.GetType().Name);
            // Modern form defaults
            AutoScaleMode = AutoScaleMode.Dpi;
            StartPosition = FormStartPosition.CenterParent;
            Font = new System.Drawing.Font("Segoe UI", 9.75F);
        }

        /// <summary>
        /// Raises the <see cref="E:Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            _startTime = DateTimeOffset.UtcNow;
            ThemeHelper.ApplyTo(this);
            base.OnLoad(e);
        }

        /// <summary>
        /// Raises the <see cref="E:Closed" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected override void OnClosed(EventArgs e)
        {
            LogPageView();
            base.OnClosed(e);
        }

        private void LogPageView()
        {
            if (!this.DesignMode && !_viewLogged)
            {
                _telemetryData.Timestamp = _startTime;
                _telemetryData.Duration = DateTimeOffset.UtcNow - _startTime;
                Telemetry.Default.TrackPageView(_telemetryData);
                _viewLogged = true;
            }
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            LogPageView();
            base.Dispose(disposing);
        }
    }
}
