namespace Letoryn.Web.Services;

/// <summary>Agency audit history API calls (FR-009, US4).</summary>
public sealed partial class LetorynApiClient
{
    private const int DefaultAgencyAuditPageSize = 50;

    /// <summary>Loads a cursor page of agency-scoped audit events.</summary>
    public async Task<AgencyAuditLoadResult> GetAgencyAuditAsync(
        Guid agencyId,
        string? cursor = null,
        int? limit = null,
        CancellationToken cancellationToken = default)
    {
        var pageSize = limit ?? DefaultAgencyAuditPageSize;
        var query = string.IsNullOrWhiteSpace(cursor)
            ? $"?limit={pageSize}"
            : $"?cursor={Uri.EscapeDataString(cursor)}&limit={pageSize}";

        using var response = await SendWithSessionRecoveryAsync(
            () => new HttpRequestMessage(
                HttpMethod.Get,
                $"/api/v1/agencies/{agencyId:D}/audit{query}"),
            cancellationToken);

        if (response.StatusCode is System.Net.HttpStatusCode.Forbidden
            or System.Net.HttpStatusCode.NotFound)
        {
            return AgencyAuditLoadResult.Denied();
        }

        if (!response.IsSuccessStatusCode)
        {
            return AgencyAuditLoadResult.Failed();
        }

        var page = await response.Content.ReadFromJsonAsync<AgencyAuditListDto>(cancellationToken);
        return page is null
            ? AgencyAuditLoadResult.Failed()
            : AgencyAuditLoadResult.Success(page);
    }
}
