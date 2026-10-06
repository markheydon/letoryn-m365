using Letoryn.Application.Abstractions.Memberships;
using Letoryn.Domain.Agencies;

namespace Letoryn.Application.Abstractions.Agencies;

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
