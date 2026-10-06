using TenancyHub.ApiService.UnitTests.Infrastructure;
using TenancyHub.ApiService.Validation;
using TenancyHub.ApiService.Validation.Operator;

namespace TenancyHub.ApiService.UnitTests.Validation;

public sealed class ValidationProblemResultsTests
{
    [Fact]
    public async Task FromFailures_Returns400ProblemDetailsWithGroupedErrors()
    {
        var failures = new List<ValidationFailure>
        {
            new(nameof(CreateOperatorAgencyRequest.DisplayName), "Display name is required."),
            new(nameof(CreateOperatorAgencyRequest.PrimaryContactEmail), "Email is invalid."),
        };

        var (statusCode, contentType, body) = await HttpResultTestHelper.ExecuteAsync(
            ValidationProblemResults.FromFailures(failures));

        Assert.Equal(StatusCodes.Status400BadRequest, statusCode);
        Assert.Contains("problem+json", contentType, StringComparison.OrdinalIgnoreCase);

        using var json = HttpResultTestHelper.ParseJsonBody(body);
        Assert.Equal("Validation failed", json.RootElement.GetProperty("title").GetString());
        Assert.Equal(StatusCodes.Status400BadRequest, json.RootElement.GetProperty("status").GetInt32());

        var errors = json.RootElement.GetProperty("errors");
        Assert.True(errors.TryGetProperty(nameof(CreateOperatorAgencyRequest.DisplayName), out var displayNameErrors));
        Assert.Contains("Display name is required.", displayNameErrors[0].GetString());
    }

    [Fact]
    public void FromResult_WhenValid_ReturnsNull()
    {
        var problem = ValidationProblemResults.FromResult(RequestValidationResult.Success());

        Assert.Null(problem);
    }

    [Fact]
    public void FromResult_WhenInvalid_ReturnsProblem()
    {
        var problem = ValidationProblemResults.FromResult(
            RequestValidationResult.Failure(new ValidationFailure("Field", "Message")));

        Assert.NotNull(problem);
    }
}
