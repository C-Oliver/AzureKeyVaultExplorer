using Microsoft.Vault.Explorer;

namespace VaultExplorer.Tests;

public class VaultAliasTests
{
    [Fact]
    public void Constructor_SetsProperties()
    {
        var alias = new VaultAlias("test-alias", new[] { "vault1", "vault2" }, new[] { "Tag1" });
        Assert.Equal("test-alias", alias.Alias);
        Assert.Equal(new[] { "vault1", "vault2" }, alias.VaultNames);
    }

    [Fact]
    public void Equals_SameAlias_ReturnsTrue()
    {
        var a = new VaultAlias("alias", new[] { "vault1" }, new[] { "Tag1" });
        var b = new VaultAlias("alias", new[] { "vault1" }, new[] { "Tag1" });
        Assert.Equal(a, b);
    }

    [Fact]
    public void Equals_DifferentAlias_ReturnsFalse()
    {
        var a = new VaultAlias("alias-a", new[] { "vault1" }, new[] { "Tag1" });
        var b = new VaultAlias("alias-b", new[] { "vault1" }, new[] { "Tag1" });
        Assert.NotEqual(a, b);
    }

    [Fact]
    public void ToString_ReturnsAlias()
    {
        var alias = new VaultAlias("my-vault", new[] { "vault1" }, new[] { "Tag1" });
        Assert.Equal("my-vault", alias.ToString());
    }
}
