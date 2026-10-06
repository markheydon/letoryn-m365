using Letoryn.ApiService.UnitTests.Infrastructure;
using Letoryn.ApiService.Validation;
using Letoryn.ApiService.Validation.Operator;

namespace Letoryn.ApiService.UnitTests.Validation;

public sealed class RequestValidationFilterTests
{
    [Fact]
    public async Task InvokeAsync_InvalidRequest_ReturnsValidationProblemWithoutCallingNext()
    {
        IRequestValidator<CreateOperatorAgencyRequest> validator = new CreateOperatorAgencyRequestValidator();
        var filter = new RequestValidationFilter<CreateOperatorAgencyRequest>(validator);
        var invalidRequest = new CreateOperatorAgencyRequest("", "not-email", null);
        var context = new DefaultEndpointFilterInvocationContext(
            new DefaultHttpContext(),
            invalidRequest);
        var nextCalled = false;

        var result = await filter.InvokeAsync(
            context,
            _ =>
            {
                nextCalled = true;
                return ValueTask.FromResult<object?>(Results.Ok());
            });

        Assert.False(nextCalled);
        Assert.NotNull(result);

        var (statusCode, contentType, body) = await HttpResultTestHelper.ExecuteAsync((IResult)result);
        Assert.Equal(StatusCodes.Status400BadRequest, statusCode);
        Assert.Contains("problem+json", contentType, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("errors", body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task InvokeAsync_ValidRequest_CallsNext()
    {
        IRequestValidator<CreateOperatorAgencyRequest> validator = new CreateOperatorAgencyRequestValidator();
        var filter = new RequestValidationFilter<CreateOperatorAgencyRequest>(validator);
        var validRequest = new CreateOperatorAgencyRequest("Agency", "admin@contoso.com", null);
        var context = new DefaultEndpointFilterInvocationContext(
            new DefaultHttpContext(),
            validRequest);
        var nextCalled = false;

        var result = await filter.InvokeAsync(
            context,
            _ =>
            {
                nextCalled = true;
                return ValueTask.FromResult<object?>(Results.Ok());
            });

        Assert.True(nextCalled);
        Assert.NotNull(result);
    }
}
