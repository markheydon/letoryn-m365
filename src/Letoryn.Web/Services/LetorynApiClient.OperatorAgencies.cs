using System.Net;
using System.Text.Json;

namespace Letoryn.Web.Services;

/// <summary>Operator agency provisioning API calls (FR-016).</summary>
public sealed partial class LetorynApiClient
{
    private static readonly JsonSerializerOptions ProblemJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    /// <summary>Creates a new agency and auto-assigns the caller.</summary>
    public async Task<CreateOperatorAgencyApiResult> CreateOperatorAgencyAsync(
        CreateOperatorAgencyApiRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await SendWithSessionRecoveryAsync(
            () => new HttpRequestMessage(HttpMethod.Post, "/api/v1/operator/agencies")
            {
                Content = JsonContent.Create(request),
            },
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.Created)
        {
            var body = await response.Content.ReadFromJsonAsync<CreateOperatorAgencyApiResponse>(cancellationToken);
            return body is null
                ? CreateOperatorAgencyApiResult.Failed("The agency was created but the response could not be read.")
                : CreateOperatorAgencyApiResult.Succeeded(body.AgencyId);
        }

        var (message, fieldErrors) = await ReadProblemAsync(response, cancellationToken);
        return CreateOperatorAgencyApiResult.Failed(message, fieldErrors);
    }

    /// <summary>Changes lifecycle status for an assigned agency.</summary>
    public async Task<OperatorAgencyLifecycleApiResult> ChangeOperatorAgencyLifecycleAsync(
        Guid agencyId,
        string targetStatus,
        CancellationToken cancellationToken = default)
    {
        using var response = await SendWithSessionRecoveryAsync(
            () => new HttpRequestMessage(
                HttpMethod.Post,
                $"/api/v1/operator/agencies/{agencyId:D}/lifecycle")
            {
                Content = JsonContent.Create(new { targetStatus }),
            },
            cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return OperatorAgencyLifecycleApiResult.Succeeded();
        }

        var (message, fieldErrors) = await ReadProblemAsync(response, cancellationToken);
        return OperatorAgencyLifecycleApiResult.Failed(message, fieldErrors);
    }

    private static async Task<(string? Message, IReadOnlyDictionary<string, string[]> FieldErrors)> ReadProblemAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (response.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.Forbidden)
        {
            return ("You do not have permission to perform this action.", EmptyFieldErrors);
        }

        var problem = await response.Content.ReadFromJsonAsync<ApiProblemDetails>(ProblemJsonOptions, cancellationToken);
        if (problem is null)
        {
            return ($"The request failed ({(int)response.StatusCode}).", EmptyFieldErrors);
        }

        var message = !string.IsNullOrWhiteSpace(problem.Detail)
            ? problem.Detail
            : problem.Title;

        var errors = problem.Errors ?? EmptyFieldErrors;
        return (message, errors);
    }

    private static readonly Dictionary<string, string[]> EmptyFieldErrors = new(StringComparer.Ordinal);

    private sealed class ApiProblemDetails
    {
        public string? Title { get; init; }

        public string? Detail { get; init; }

        public Dictionary<string, string[]>? Errors { get; init; }
    }
}
