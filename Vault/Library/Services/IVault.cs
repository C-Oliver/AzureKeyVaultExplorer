// Copyright (c) Microsoft Corporation. All rights reserved. 
// Licensed under the MIT License. See License.txt in the project root for license information. 

using Azure.Security.KeyVault.Certificates;
using Azure.Security.KeyVault.Secrets;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Vault.Library
{
    /// <summary>
    /// Abstraction for Azure Key Vault operations, enabling dependency injection and testability.
    /// </summary>
    public interface IVault
    {
        string? AuthenticatedUserName { get; }
        string[] VaultNames { get; }

        Task<KeyVaultSecret> GetSecretAsync(string secretName, string? secretVersion = null, CancellationToken cancellationToken = default);
        Task<KeyVaultSecret> SetSecretAsync(string secretName, string value, SecretProperties secretProperties, CancellationToken cancellationToken = default);
        Task<SecretProperties> UpdateSecretAsync(SecretProperties secretProperties, CancellationToken cancellationToken = default);
        Task<IEnumerable<SecretProperties>> ListSecretsAsync(int regionIndex = 0, Vault.ListOperationProgressUpdate? listSecretsProgressUpdate = null, CancellationToken cancellationToken = default);
        Task<IEnumerable<SecretProperties>> GetSecretVersionsAsync(string secretName, int regionIndex = 0, CancellationToken cancellationToken = default);
        Task<DeleteSecretOperation> DeleteSecretAsync(string secretName, CancellationToken cancellationToken = default);

        Task<X509Certificate2> GetCertificateWithExportableKeysAsync(string certificateName, string? certificateVersion = null, CancellationToken cancellationToken = default);
        Task<KeyVaultCertificateWithPolicy> GetCertificateAsync(string certificateName, CancellationToken cancellationToken = default);
        Task<IEnumerable<CertificateProperties>> ListCertificatesAsync(int regionIndex = 0, Vault.ListOperationProgressUpdate? listCertificatesProgressUpdate = null, CancellationToken cancellationToken = default);
        Task<KeyVaultCertificateWithPolicy> ImportCertificateAsync(ImportCertificateOptions importCertificateOptions, CancellationToken cancellationToken = default);
        Task<DeleteCertificateOperation> DeleteCertificateAsync(string certificateName, CancellationToken cancellationToken = default);
    }
}
