namespace TenancyHub.Application.Abstractions.Memberships;

/// <summary>Outcome for membership command operations.</summary>
public enum MembershipOperationStatus
{
    /// <summary>Operation completed.</summary>
    Succeeded,

    /// <summary>Validation or business rule failure.</summary>
    ValidationFailed,

    /// <summary>Caller not permitted.</summary>
    Forbidden,

    /// <summary>Resource missing or tenant-safe denial.</summary>
    NotFound,

    /// <summary>Conflict (for example duplicate invite).</summary>
    Conflict,
}

/// <summary>Result of a membership mutation.</summary>
public sealed record MembershipOperationResult(
    MembershipOperationStatus Status,
    string? UserMessage = null,
    Guid? MembershipId = null);
