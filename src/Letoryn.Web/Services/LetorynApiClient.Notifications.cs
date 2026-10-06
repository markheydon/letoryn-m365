namespace Letoryn.Web.Services;

/// <summary>Agency notification list API calls (FR-010).</summary>
public sealed partial class LetorynApiClient
{
    /// <summary>Lists notifications for the active agency context.</summary>
    public async Task<NotificationListResult> GetAgencyNotificationsAsync(
        Guid agencyId,
        CancellationToken cancellationToken = default)
    {
        using var response = await SendWithSessionRecoveryAsync(
            () => new HttpRequestMessage(
                HttpMethod.Get,
                $"/api/v1/agencies/{agencyId:D}/notifications"),
            cancellationToken);

        if (response.StatusCode is System.Net.HttpStatusCode.Forbidden
            or System.Net.HttpStatusCode.NotFound)
        {
            return NotificationListResult.Denied();
        }

        if (!response.IsSuccessStatusCode)
        {
            return NotificationListResult.Failed();
        }

        var items = await response.Content.ReadFromJsonAsync<List<NotificationListItemDto>>(cancellationToken)
            ?? [];
        return NotificationListResult.Success(items);
    }

    /// <summary>Marks a notification read (standard members only).</summary>
    public async Task<NotificationMarkReadResult> MarkAgencyNotificationReadAsync(
        Guid agencyId,
        Guid notificationId,
        CancellationToken cancellationToken = default)
    {
        using var response = await SendWithSessionRecoveryAsync(
            () => new HttpRequestMessage(
                HttpMethod.Post,
                $"/api/v1/agencies/{agencyId:D}/notifications/{notificationId:D}/read"),
            cancellationToken);

        if (response.StatusCode is System.Net.HttpStatusCode.Forbidden)
        {
            return NotificationMarkReadResult.Denied();
        }

        if (!response.IsSuccessStatusCode)
        {
            var (message, _) = await ReadProblemAsync(response, cancellationToken);
            return NotificationMarkReadResult.Failed(message);
        }

        return NotificationMarkReadResult.Ok;
    }
}

/// <summary>Agency-scoped notification row from the API.</summary>
public sealed record NotificationListItemDto(
    Guid Id,
    DateTimeOffset CreatedAt,
    string Category,
    string Title,
    string Body,
    bool IsRead,
    DateTimeOffset? ReadAt);

/// <summary>Outcome of loading the agency notification list.</summary>
public sealed record NotificationListResult(
    bool Succeeded,
    bool AccessDenied,
    IReadOnlyList<NotificationListItemDto> Items,
    string? ErrorMessage)
{
    /// <summary>Successful list load.</summary>
    public static NotificationListResult Success(IReadOnlyList<NotificationListItemDto> items) =>
        new(true, false, items, null);

    /// <summary>Caller lacks permission or agency context.</summary>
    public static NotificationListResult Denied() =>
        new(false, true, [], "You do not have permission to view notifications for this agency.");

    /// <summary>Non-success HTTP response.</summary>
    public static NotificationListResult Failed() =>
        new(false, false, [], "We could not load notifications. Try again.");
}

/// <summary>Outcome of marking a notification read.</summary>
public sealed record NotificationMarkReadResult(bool Succeeded, bool AccessDenied, string? ErrorMessage)
{
    /// <summary>Notification marked read.</summary>
    public static NotificationMarkReadResult Ok { get; } = new(true, false, null);

    /// <summary>Read-only members and other denied callers.</summary>
    public static NotificationMarkReadResult Denied() =>
        new(false, true, "You do not have permission to mark notifications as read.");

    /// <summary>Non-success HTTP response.</summary>
    public static NotificationMarkReadResult Failed(string? message) =>
        new(false, false, message ?? "We could not update that notification. Try again.");
}
