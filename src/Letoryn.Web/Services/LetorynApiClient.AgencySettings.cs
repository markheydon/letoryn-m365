namespace Letoryn.Web.Services;

/// <summary>Agency settings API calls (FR-015).</summary>
public sealed partial class LetorynApiClient
{
    /// <summary>Loads agency settings for the active agency context.</summary>
    public async Task<AgencySettingsLoadResult> GetAgencySettingsAsync(
        Guid agencyId,
        CancellationToken cancellationToken = default)
    {
        using var response = await SendWithSessionRecoveryAsync(
            () => new HttpRequestMessage(
                HttpMethod.Get,
                $"/api/v1/agencies/{agencyId:D}/settings"),
            cancellationToken);

        if (response.StatusCode is System.Net.HttpStatusCode.Forbidden
            or System.Net.HttpStatusCode.NotFound)
        {
            return AgencySettingsLoadResult.Denied();
        }

        if (!response.IsSuccessStatusCode)
        {
            return AgencySettingsLoadResult.Failed();
        }

        var settings = await response.Content.ReadFromJsonAsync<AgencySettingsApiDto>(cancellationToken);
        return settings is null
            ? AgencySettingsLoadResult.Failed()
            : AgencySettingsLoadResult.Success(settings);
    }

    /// <summary>Updates agency display name and contact fields.</summary>
    public Task<AgencySettingsMutationResult> UpdateAgencySettingsAsync(
        Guid agencyId,
        UpdateAgencySettingsApiRequest request,
        CancellationToken cancellationToken = default) =>
        SendAgencySettingsMutationAsync(
            () => new HttpRequestMessage(
                HttpMethod.Patch,
                $"/api/v1/agencies/{agencyId:D}/settings")
            {
                Content = JsonContent.Create(request),
            },
            cancellationToken);

    private async Task<AgencySettingsMutationResult> SendAgencySettingsMutationAsync(
        Func<HttpRequestMessage> requestFactory,
        CancellationToken cancellationToken)
    {
        using var response = await SendWithSessionRecoveryAsync(requestFactory, cancellationToken);
        return await AgencySettingsMutationResult.FromResponseAsync(response, cancellationToken);
    }
}

/// <summary>Result of a PATCH agency settings call.</summary>
public sealed record AgencySettingsMutationResult(bool Succeeded, string? ErrorMessage)
{
    /// <summary>Maps an HTTP response to a mutation result.</summary>
    public static async Task<AgencySettingsMutationResult> FromResponseAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return new AgencySettingsMutationResult(true, null);
        }

        var message = response.StatusCode switch
        {
            System.Net.HttpStatusCode.Forbidden or System.Net.HttpStatusCode.NotFound =>
                "You do not have permission to update agency settings.",
            System.Net.HttpStatusCode.BadRequest =>
                "The request could not be processed. Check the details and try again.",
            _ => "Something went wrong. Try again later.",
        };

        if (response.Content.Headers.ContentLength is not 0)
        {
            try
            {
                var problem = await response.Content.ReadFromJsonAsync<SettingsApiProblemDetails>(cancellationToken);
                if (!string.IsNullOrWhiteSpace(problem?.Detail))
                {
                    message = problem.Detail;
                }
            }
            catch
            {
                // Use generic message when ProblemDetails cannot be parsed.
            }
        }

        return new AgencySettingsMutationResult(false, message);
    }

    private sealed record SettingsApiProblemDetails(string? Detail);
}
