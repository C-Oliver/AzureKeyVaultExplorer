# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

## [2.0.0] - 2026-05-21

### Changed
- **BREAKING**: Migrated from .NET Framework to **.NET 10** with C# 14
- **BREAKING**: Replaced legacy Azure SDK (`Microsoft.Azure.KeyVault`) with modern `Azure.Security.KeyVault.Secrets` / `Azure.Security.KeyVault.Certificates` and `Azure.Identity`
- Upgraded all NuGet packages to latest stable versions
- Modernized WinForms startup (`ApplicationConfiguration.Initialize`, `SystemColorMode.System`)
- Replaced ClickOnce deployment (unavailable in .NET 10) with single-file publish
- All hardcoded values (endpoints, API versions, telemetry) moved to `appsettings.json`
- Telemetry is now runtime-configured — no preprocessor directives
- Renamed `CalculateMd5` → `CalculateHash` (implementation was already SHA-256)
- API version for subscription listing updated from 2016-07-01 to 2022-12-01
- Reorganized project files into `Models/`, `UI/Controls/`, `UI/Dialogs/`, `UI/Infrastructure/`, `Configuration/`, `Services/`, `Security/`

### Added
- **Dark mode support** — follows Windows system theme via `Application.SetColorMode`
- **Theme-aware colors** — `ThemeHelper` adapts link, error, and warning colors for dark/light mode
- **PropertyGrid theming** — category headers and values readable in dark mode
- **Structured logging** — `Microsoft.Extensions.Logging` with `AppLogger` centralized factory
- **Loading indicator** — status bar shows "Connecting to vault..." during vault operations
- **Sovereign cloud support** — configurable vault DNS suffix and management endpoint via env vars
- **appsettings.json** — externalized configuration for telemetry, Azure endpoints, and app behavior
- **AppConfig** class — strongly-typed config with environment variable overrides
- **Unit tests** — 65 xUnit tests covering URI building, serialization security, validation, crypto, hashing
- **UI automation tests** — FlaUI-based tests for main window structure, toolbar, dialogs
- **GitHub Actions CI/CD** — build, test with coverage, publish release artifacts
- **Code coverage** — Cobertura reports with PR comment summaries
- **Deserialization security** — `VaultAccessSerializationBinder` whitelist prevents RCE via `TypeNameHandling.Auto`
- **Regex ReDoS protection** — `RegexOptions.NonBacktracking` for user-configurable patterns
- **Resilient Settings** — auto-recovers from corrupt `user.config` files

### Fixed
- `System.UriFormatException` when connecting to vault (bare vault names now produce proper URIs)
- `System.NullReferenceException` in Subscriptions Manager account selection
- `CertificateClient` never initialized in `Vault` constructor
- `Process.Start` crash on .NET 10 (added `UseShellExecute = true` for URL launches)
- `CachePersistence` file handle leaks and broken DPAPI entropy handling
- `AutoScaleDimensions` mismatch (`8F, 16F` font metrics with `AutoScaleMode.Dpi`) causing layout distortion in all dialogs
- Subscriptions Manager layout — restructured with proper Dock-based layout

### Security
- **TypeNameHandling RCE** (CWE-502) — added serialization binder whitelist
- **MD5 → SHA-256** — replaced broken hash algorithm in clipboard verification and secret tagging
- **PII in telemetry** — replaced plaintext username/machine with truncated SHA-256 hashes
- **PowerShell injection** (CWE-78) — switched from string concatenation to `ProcessStartInfo.ArgumentList`
- **Exception info disclosure** (CWE-209) — file paths stripped from stack traces shown to users
- **Insecure temp files** (CWE-377) — switched to random filenames for clipboard temp files
- **DPAPI entropy** — added static entropy to `SecretFile` DPAPI protection (was null)
- **Execution policy** — PowerShell launch changed from `Unrestricted` to `RemoteSigned`
- **HTTP → HTTPS** — all external URLs updated to HTTPS
- **X509Store disposal** — added `using` for proper resource cleanup
- **Obsolete X509Certificate2 constructors** — replaced with `X509CertificateLoader`

### Removed
- Legacy .NET Framework build infrastructure (`init.cmd`, `Environment.props`, `.init/`, `Vault/Build/`)
- ClickOnce deployment files (`ClickOnce.props`, `CreateReleaseAnnotation.ps1`)
- `App.config` files (replaced by `appsettings.json`)
- `packages.config` (replaced by PackageReference)
- Hardcoded Azure subscription IDs from source comments
- `#if ENABLE_TELEMETRY` preprocessor directives
