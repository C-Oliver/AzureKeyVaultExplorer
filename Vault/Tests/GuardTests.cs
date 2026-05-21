using Microsoft.Vault.Core;

namespace VaultExplorer.Tests;

public class GuardTests
{
    [Fact]
    public void ArgumentNotNull_WithNull_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => Guard.ArgumentNotNull<object>(null!, "param"));
    }

    [Fact]
    public void ArgumentNotNull_WithValue_DoesNotThrow()
    {
        Guard.ArgumentNotNull("value", "param");
    }

    [Fact]
    public void ArgumentNotNullOrEmptyString_WithEmpty_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => Guard.ArgumentNotNullOrEmptyString("", "param"));
    }

    [Fact]
    public void ArgumentNotNullOrEmptyString_WithNull_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => Guard.ArgumentNotNullOrEmptyString(null!, "param"));
    }

    [Fact]
    public void ArgumentNotNullOrEmptyString_WithValue_DoesNotThrow()
    {
        Guard.ArgumentNotNullOrEmptyString("hello", "param");
    }

    [Fact]
    public void ArgumentIsEqual_WhenEqual_DoesNotThrow()
    {
        Guard.ArgumentIsEqual(42, 42, "param");
    }

    [Fact]
    public void ArgumentIsEqual_WhenNotEqual_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => Guard.ArgumentIsEqual(1, 2, "param"));
    }

    [Fact]
    public void ArgumentInRange_WithinRange_DoesNotThrow()
    {
        Guard.ArgumentInRange(5, 0, 10, "param");
    }

    [Fact]
    public void ArgumentInRange_BelowRange_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Guard.ArgumentInRange(-1, 0, 10, "param"));
    }

    [Fact]
    public void ArgumentInRange_AboveRange_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Guard.ArgumentInRange(11, 0, 10, "param"));
    }
}
