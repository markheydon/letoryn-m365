namespace TenancyHub.ApiService.Validation.Operator;

/// <summary>
/// Request body for POST <c>/api/v1/operator/agencies</c> (platform operator agency create).
/// </summary>
public sealed record CreateOperatorAgencyRequest(
    string DisplayName,
    string PrimaryContactEmail,
    string? PrimaryContactPhone);
