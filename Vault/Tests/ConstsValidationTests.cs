using Microsoft.Vault.Library;

namespace VaultExplorer.Tests;

public class ConstsValidationTests
{
    [Theory]
    [InlineData("myvault", true)]
    [InlineData("my-vault-123", true)]
    [InlineData("abc", true)]
    [InlineData("a-very-long-vault-name24", true)]  // Exactly 24 chars = valid
    [InlineData("a-very-long-vault-name-25", false)] // 25 chars = invalid
    [InlineData("ab", false)]           // Too short
    [InlineData("a", false)]            // Too short
    [InlineData("", false)]             // Empty
    [InlineData("vault_name", false)]   // Underscore not allowed
    [InlineData("vault.name", false)]   // Dot not allowed
    [InlineData("vault name", false)]   // Space not allowed
    public void ValidVaultNameRegex_MatchesCorrectly(string name, bool expected)
    {
        Assert.Equal(expected, Consts.ValidVaultNameRegex.IsMatch(name));
    }

    [Theory]
    [InlineData("my-secret", true)]
    [InlineData("a", true)]
    [InlineData("secret-with-dashes-123", true)]
    [InlineData("", false)]
    [InlineData("secret_name", false)]  // Underscore not allowed
    public void ValidSecretNameRegex_MatchesCorrectly(string name, bool expected)
    {
        Assert.Equal(expected, Consts.ValidSecretNameRegex.IsMatch(name));
    }

    [Theory]
    [InlineData("https://myvault.vault.azure.net/secrets/mysecret", true)]
    [InlineData("https://myvault.vault.azure.net/keys/mykey", true)]
    [InlineData("https://myvault.vault.azure.net/certificates/mycert", true)]
    [InlineData("https://myvault.vault.azure.net:443/secrets/mysecret", true)]
    [InlineData("https://myvault.vault.azure.net/secrets/mysecret/abcdef0123456789abcdef0123456789", true)]
    [InlineData("http://myvault.vault.azure.net/secrets/mysecret", false)] // HTTP not allowed
    [InlineData("https://myvault.vault.azure.net/invalid/mysecret", false)] // Bad collection
    public void ValidVaultItemHttpsUriRegex_MatchesCorrectly(string uri, bool expected)
    {
        Assert.Equal(expected, Consts.ValidVaultItemHttpsUriRegex.IsMatch(uri));
    }

    [Fact]
    public void SizeConstants_AreCorrect()
    {
        Assert.Equal(1024L, Consts.KB);
        Assert.Equal(1024L * 1024, Consts.MB);
        Assert.Equal(1024L * 1024 * 1024, Consts.GB);
        Assert.Equal(1024L * 1024 * 1024 * 1024, Consts.TB);
    }
}
