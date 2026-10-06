using TenancyHub.Domain.Guards;

namespace TenancyHub.Application.UnitTests.Guards;

public sealed class PlatformOperatorGuardsTests
{
    [Fact]
    public void EnsureAtLeastOnePlatformOperator_WhenZero_Throws()
    {
        var ex = Assert.Throws<InvalidOperationException>(
            () => PlatformOperatorGuards.EnsureAtLeastOnePlatformOperator(0));

        Assert.Contains("platform operator", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void EnsureAtLeastOnePlatformOperator_WhenOne_DoesNotThrow()
    {
        PlatformOperatorGuards.EnsureAtLeastOnePlatformOperator(1);
    }
}
