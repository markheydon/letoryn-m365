using TenancyHub.Application.Abstractions.Memberships;
using TenancyHub.Domain.Agencies;

namespace TenancyHub.Application.Abstractions.Agencies;

/// <summary>Agency settings fields exposed to authorized callers.</summary>
public sealed record AgencySettingsSnapshot(
    string DisplayName,
    string PrimaryContactEmail,
    string PrimaryContactPhone,
    AgencyLifecycleStatus LifecycleStatus);

/// <summary>Outcome of reading agency settings.</summary>
public sealed record AgencySettingsReadResult(
    MembershipOperationStatus Status,
    string? UserMessage = null,
    AgencySettingsSnapshot? Settings = null);
