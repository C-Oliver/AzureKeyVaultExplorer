using Microsoft.Vault.Library;

namespace VaultExplorer.Tests;

public class UtilsHashTests
{
    [Fact]
    public void CalculateHash_SameInput_ProducesSameHash()
    {
        string hash1 = Utils.CalculateHash("test-secret-value");
        string hash2 = Utils.CalculateHash("test-secret-value");
        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void CalculateHash_DifferentInput_ProducesDifferentHash()
    {
        string hash1 = Utils.CalculateHash("value-a");
        string hash2 = Utils.CalculateHash("value-b");
        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void CalculateHash_ReturnsLowercaseHex()
    {
        string hash = Utils.CalculateHash("test");
        Assert.Matches("^[0-9a-f]+$", hash);
    }

    [Fact]
    public void CalculateHash_ReturnsSHA256Length()
    {
        // After migration from MD5 (32 chars) to SHA256 (64 chars)
        string hash = Utils.CalculateHash("test");
        Assert.Equal(64, hash.Length);
    }

    [Fact]
    public void CalculateHash_EmptyString_ProducesHash()
    {
        string hash = Utils.CalculateHash("");
        Assert.NotNull(hash);
        Assert.Equal(64, hash.Length);
    }
}
