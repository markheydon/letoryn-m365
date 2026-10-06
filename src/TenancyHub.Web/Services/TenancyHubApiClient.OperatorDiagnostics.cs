namespace TenancyHub.Web.Services;

/// <summary>Operator cross-agency diagnostics API calls (FR-009, US4).</summary>
public sealed partial class TenancyHubApiClient
{
    /// <summary>Loads read-only agency summary for an assigned agency.</summary>
    public async Task<OperatorAgencySummaryLoadResult> GetOperatorAgencySummaryAsync(
        Guid agencyId,
        CancellationToken cancellationToken = default)
    {
        using var response = await SendWithSessionRecoveryAsync(
            () => new HttpRequestMessage(
                HttpMethod.Get,
                $"/api/v1/operator/agencies/{agencyId:D}/summary"),
            cancellationToken);

        if (response.StatusCode is System.Net.HttpStatusCode.Forbidden
            or System.Net.HttpStatusCode.NotFound)
        {
            return OperatorAgencySummaryLoadResult.Denied();
        }

        if (!response.IsSuccessStatusCode)
        {
            return OperatorAgencySummaryLoadResult.Failed();
        }

        var summary = await response.Content.ReadFromJsonAsync<OperatorAgencySummaryDto>(cancellationToken);
        return summary is null
            ? OperatorAgencySummaryLoadResult.Failed()
            : OperatorAgencySummaryLoadResult.Success(summary);
    }

    /// <summary>Loads a cursor page of audit events via the operator cross-agency route.</summary>
    public async Task<AgencyAuditLoadResult> GetOperatorAgencyAuditAsync(
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
                $"/api/v1/operator/agencies/{agencyId:D}/audit{query}"),
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
