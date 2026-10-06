using Letoryn.Application.Abstractions.Me;

namespace Letoryn.Application.Me;

/// <summary>Outcome of selecting an active agency context.</summary>
public sealed record SetActiveAgencyOutcome(SetActiveAgencyResult Result, string? UserMessage = null);
