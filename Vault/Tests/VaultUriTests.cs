using Microsoft.Vault.Library;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace VaultExplorer.Tests;

public class VaultUriTests
{
    [Theory]
    [InlineData("myvault", "https://myvault.vault.azure.net/")]
    [InlineData("my-vault-123", "https://my-vault-123.vault.azure.net/")]
    [InlineData("ABC", "https://abc.vault.azure.net/")]
    public void BuildVaultUri_ShortName_ProducesCorrectUri(string vaultName, string expected)
    {
        var uri = Vault.BuildVaultUri(vaultName);
        Assert.Equal(expected, uri.ToString());
    }

    [Theory]
    [InlineData("https://myvault.vault.azure.net/")]
    [InlineData("https://myvault.vault.azure.net")]
    public void BuildVaultUri_FullUrl_PassesThrough(string fullUrl)
    {
        var uri = Vault.BuildVaultUri(fullUrl);
        Assert.StartsWith("https://myvault.vault.azure.net", uri.ToString());
    }

    [Fact]
    public void BuildVaultUri_EmptyName_ThrowsUriFormatException()
    {
        Assert.Throws<UriFormatException>(() => Vault.BuildVaultUri(""));
    }
}

public class SerializationBinderSecurityTests
{
    [Theory]
    [InlineData("Microsoft.Vault.Library.VaultAccessUserInteractive")]
    [InlineData("Microsoft.Vault.Library.VaultAccessClientCredential")]
    [InlineData("Microsoft.Vault.Library.VaultAccessClientCertificate")]
    [InlineData("Microsoft.Vault.Library.VaultAccessType")]
    [InlineData("Microsoft.Vault.Library.VaultsConfig")]
    public void AllowedTypes_AreResolved(string typeName)
    {
        var binder = new VaultAccessSerializationBinder();
        var resolved = binder.BindToType(null, typeName);
        Assert.NotNull(resolved);
    }

    [Theory]
    [InlineData("System.Diagnostics.Process")]
    [InlineData("System.IO.File")]
    [InlineData("System.Runtime.Remoting.ObjectHandle")]
    [InlineData("System.CodeDom.Compiler.TempFileCollection")]
    public void DisallowedTypes_ThrowJsonSerializationException(string typeName)
    {
        var binder = new VaultAccessSerializationBinder();
        Assert.Throws<JsonSerializationException>(() => binder.BindToType(null, typeName));
    }

    [Fact]
    public void BindToName_ReturnsFullName()
    {
        var binder = new VaultAccessSerializationBinder();
        binder.BindToName(typeof(VaultAccessUserInteractive), out var assembly, out var typeName);
        Assert.Null(assembly);
        Assert.Equal("Microsoft.Vault.Library.VaultAccessUserInteractive", typeName);
    }
}
