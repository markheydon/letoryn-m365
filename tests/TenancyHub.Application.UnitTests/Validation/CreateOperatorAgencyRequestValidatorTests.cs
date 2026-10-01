using TenancyHub.ApiService.Validation.Operator;

namespace TenancyHub.Application.UnitTests.Validation;

public sealed class CreateOperatorAgencyRequestValidatorTests
{
    private readonly CreateOperatorAgencyRequestValidator _validator = new();

    [Fact]
    public void Validate_MissingDisplayName_ReturnsFailure()
    {
        var result = _validator.Validate(new CreateOperatorAgencyRequest(
            DisplayName: "",
            PrimaryContactEmail: "admin@contoso.com",
            PrimaryContactPhone: null));

        Assert.False(result.IsValid);
        Assert.Contains(result.Failures, f => f.Field == nameof(CreateOperatorAgencyRequest.DisplayName));
    }

    [Fact]
    public void Validate_InvalidEmail_ReturnsFailure()
    {
        var result = _validator.Validate(new CreateOperatorAgencyRequest(
            DisplayName: "Contoso Lettings",
            PrimaryContactEmail: "not-an-email",
            PrimaryContactPhone: null));

        Assert.False(result.IsValid);
        Assert.Contains(result.Failures, f => f.Field == nameof(CreateOperatorAgencyRequest.PrimaryContactEmail));
    }

    [Fact]
    public void Validate_ValidRequest_Succeeds()
    {
        var result = _validator.Validate(new CreateOperatorAgencyRequest(
            DisplayName: "Contoso Lettings",
            PrimaryContactEmail: "admin@contoso.com",
            PrimaryContactPhone: "+441234567890"));

        Assert.True(result.IsValid);
    }
}
