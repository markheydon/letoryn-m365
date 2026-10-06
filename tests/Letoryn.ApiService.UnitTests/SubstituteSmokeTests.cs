namespace Letoryn.ApiService.UnitTests;

public sealed class SubstituteSmokeTests
{
    [Fact]
    public void NSubstitute_can_configure_a_substitute()
    {
        var clock = Substitute.For<IClock>();
        var expected = new DateTimeOffset(2026, 9, 30, 12, 0, 0, TimeSpan.Zero);
        clock.UtcNow.Returns(expected);

        Assert.Equal(expected, clock.UtcNow);
    }

    public interface IClock
    {
        DateTimeOffset UtcNow { get; }
    }
}
