using TenancyHub.ApiService.Infrastructure;

namespace TenancyHub.ApiService.UnitTests.Infrastructure;

public sealed class TenantSafeResultsTests
{
    [Fact]
    public async Task NotFoundOrForbidden_Returns404WithCanonicalShape()
    {
        var (statusCode, _, body) = await HttpResultTestHelper.ExecuteAsync(TenantSafeResults.NotFoundOrForbidden());

        Assert.Equal(StatusCodes.Status404NotFound, statusCode);

        using var json = HttpResultTestHelper.ParseJsonBody(body);
        Assert.Equal(TenantSafeResults.ResourceNotAvailableTitle, json.RootElement.GetProperty("title").GetString());
        Assert.Equal(TenantSafeResults.ResourceNotAvailableDetail, json.RootElement.GetProperty("detail").GetString());
        Assert.Equal(StatusCodes.Status404NotFound, json.RootElement.GetProperty("status").GetInt32());
    }

    [Fact]
    public async Task Forbidden_Returns403WithPermissionMessage()
    {
        var (statusCode, _, body) = await HttpResultTestHelper.ExecuteAsync(TenantSafeResults.Forbidden());

        Assert.Equal(StatusCodes.Status403Forbidden, statusCode);

        using var json = HttpResultTestHelper.ParseJsonBody(body);
        Assert.Equal("Forbidden", json.RootElement.GetProperty("title").GetString());
        Assert.Equal(StatusCodes.Status403Forbidden, json.RootElement.GetProperty("status").GetInt32());
    }
}
