using System.Net;
using TenancyHub.Application.Abstractions.Operators;

namespace TenancyHub.Web.Services;

/// <summary>Platform operator grant, revoke, and assignment API calls (FR-006).</summary>
public sealed partial class TenancyHubApiClient
{
    /// <summary>Lists platform operators and their agency assignments.</summary>
    public async Task<IReadOnlyList<PlatformOperatorDto>> GetPlatformOperatorsAsync(
        CancellationToken cancellationToken = default)
    {
        using var response = await SendWithSessionRecoveryAsync(
            () => new HttpRequestMessage(HttpMethod.Get, "/api/v1/operator/platform/operators"),
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return [];
        }

        return await response.Content.ReadFromJsonAsync<List<PlatformOperatorDto>>(cancellationToken) ?? [];
    }

    /// <summary>Grants platform operator status to a work-account identity.</summary>
    public async Task<PlatformOperatorActionResult> GrantPlatformOperatorAsync(
        Guid userId,
        CancellationToken cancellationToken = default) =>
        await SendPlatformOperatorActionAsync(
            HttpMethod.Post,
            $"/api/v1/operator/platform/operators/{userId:D}/grant",
            cancellationToken);

    /// <summary>Revokes platform operator status from a work-account identity.</summary>
    public async Task<PlatformOperatorActionResult> RevokePlatformOperatorAsync(
        Guid userId,
        CancellationToken cancellationToken = default) =>
        await SendPlatformOperatorActionAsync(
            HttpMethod.Post,
            $"/api/v1/operator/platform/operators/{userId:D}/revoke",
            cancellationToken);

    /// <summary>Replaces agency assignments for a platform operator.</summary>
    public async Task<PlatformOperatorActionResult> SetPlatformOperatorAssignmentsAsync(
        Guid userId,
        IReadOnlyList<Guid> agencyIds,
        CancellationToken cancellationToken = default)
    {
        using var response = await SendWithSessionRecoveryAsync(
            () => new HttpRequestMessage(
                HttpMethod.Put,
                $"/api/v1/operator/platform/operators/{userId:D}/assignments")
            {
                Content = JsonContent.Create(new SetPlatformOperatorAssignmentsRequest(agencyIds)),
            },
            cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return PlatformOperatorActionResult.Success;
        }

        var (message, _) = await ReadProblemAsync(response, cancellationToken);
        return PlatformOperatorActionResult.Failed(message);
    }

    private async Task<PlatformOperatorActionResult> SendPlatformOperatorActionAsync(
        HttpMethod method,
        string path,
        CancellationToken cancellationToken)
    {
        using var response = await SendWithSessionRecoveryAsync(
            () => new HttpRequestMessage(method, path),
            cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return PlatformOperatorActionResult.Success;
        }

        if (response.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.Forbidden)
        {
            var (message, _) = await ReadProblemAsync(response, cancellationToken);
            return PlatformOperatorActionResult.Failed(message);
        }

        var (detail, _) = await ReadProblemAsync(response, cancellationToken);
        return PlatformOperatorActionResult.Failed(detail);
    }
}
