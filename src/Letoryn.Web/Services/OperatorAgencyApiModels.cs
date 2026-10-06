namespace Letoryn.Web.Services;

/// <summary>Request body for operator agency create.</summary>
public sealed record CreateOperatorAgencyApiRequest(
    string DisplayName,
    string PrimaryContactEmail,
    string? PrimaryContactPhone);

/// <summary>Successful create response from the API.</summary>
public sealed record CreateOperatorAgencyApiResponse(Guid AgencyId);

/// <summary>Outcome of an operator agency create call.</summary>
public sealed record CreateOperatorAgencyApiResult(
    bool IsSuccess,
    Guid? AgencyId,
    string? ErrorMessage,
    IReadOnlyDictionary<string, string[]> FieldErrors)
{
    /// <summary>Creates a success result.</summary>
    public static CreateOperatorAgencyApiResult Succeeded(Guid agencyId) =>
        new(true, agencyId, null, EmptyErrors);

    /// <summary>Creates a failure result.</summary>
    public static CreateOperatorAgencyApiResult Failed(
        string? errorMessage,
        IReadOnlyDictionary<string, string[]>? fieldErrors = null) =>
        new(false, null, errorMessage, fieldErrors ?? EmptyErrors);

    private static readonly Dictionary<string, string[]> EmptyErrors = new(StringComparer.Ordinal);
}

/// <summary>Outcome of an operator agency lifecycle change.</summary>
public sealed record OperatorAgencyLifecycleApiResult(
    bool IsSuccess,
    string? ErrorMessage,
    IReadOnlyDictionary<string, string[]> FieldErrors)
{
    /// <summary>Creates a success result.</summary>
    public static OperatorAgencyLifecycleApiResult Succeeded() =>
        new(true, null, EmptyErrors);

    /// <summary>Creates a failure result.</summary>
    public static OperatorAgencyLifecycleApiResult Failed(
        string? errorMessage,
        IReadOnlyDictionary<string, string[]>? fieldErrors = null) =>
        new(false, errorMessage, fieldErrors ?? EmptyErrors);

    private static readonly Dictionary<string, string[]> EmptyErrors = new(StringComparer.Ordinal);
}
