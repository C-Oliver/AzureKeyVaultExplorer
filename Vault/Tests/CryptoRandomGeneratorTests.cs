using Microsoft.Vault.Core;

namespace VaultExplorer.Tests;

public class CryptoRandomGeneratorTests
{
    [Fact]
    public void Next_ReturnsValueInRange()
    {
        using var rng = new CryptoRandomGenerator();
        for (int i = 0; i < 1000; i++)
        {
            int val = rng.Next(10, 20);
            Assert.InRange(val, 10, 19);
        }
    }

    [Fact]
    public void Next_MinEqualsMaxMinusOne_ReturnsMin()
    {
        using var rng = new CryptoRandomGenerator();
        int val = rng.Next(5, 6);
        Assert.Equal(5, val);
    }

    [Fact]
    public void Next_MinGreaterThanMax_ThrowsArgumentOutOfRange()
    {
        using var rng = new CryptoRandomGenerator();
        Assert.Throws<ArgumentOutOfRangeException>(() => rng.Next(20, 10));
    }

    [Fact]
    public void Next_MinEqualsMax_ThrowsArgumentOutOfRange()
    {
        using var rng = new CryptoRandomGenerator();
        Assert.Throws<ArgumentOutOfRangeException>(() => rng.Next(10, 10));
    }

    [Fact]
    public void Next_SingleArg_ReturnsValueInRange()
    {
        using var rng = new CryptoRandomGenerator();
        for (int i = 0; i < 100; i++)
        {
            int val = rng.Next(100);
            Assert.InRange(val, 0, 99);
        }
    }

    [Fact]
    public void Next_ProducesVariedResults()
    {
        using var rng = new CryptoRandomGenerator();
        var values = Enumerable.Range(0, 100).Select(_ => rng.Next(0, 1000)).ToHashSet();
        // With 100 random values in [0,1000), we should get at least 50 unique values
        Assert.True(values.Count > 50, $"Expected diverse values but only got {values.Count} unique");
    }
}
