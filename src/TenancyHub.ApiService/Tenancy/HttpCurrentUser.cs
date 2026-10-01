using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using TenancyHub.Application.Abstractions.Tenancy;
using TenancyHub.Infrastructure.Persistence;

namespace TenancyHub.ApiService.Tenancy;

/// <summary>Resolves <see cref="ICurrentUser"/> from the HTTP context and database.</summary>
public sealed class HttpCurrentUser(
    IHttpContextAccessor httpContextAccessor,
    TenancyHubDbContext dbContext) : ICurrentUser
{
    private UserSnapshot? _snapshot;

    /// <inheritdoc />
    public Guid UserIdentityId => Snapshot.UserIdentityId;

    /// <inheritdoc />
    public string EntraObjectId => Snapshot.EntraObjectId;

    /// <inheritdoc />
    public string Email => Snapshot.Email;

    /// <inheritdoc />
    public bool IsPlatformOperator => Snapshot.IsPlatformOperator;

    private UserSnapshot Snapshot => _snapshot ??= LoadSnapshot();

    private UserSnapshot LoadSnapshot()
    {
        var httpContext = httpContextAccessor.HttpContext;
        var principal = httpContext?.User;
        if (principal?.Identity?.IsAuthenticated != true)
        {
            return UserSnapshot.Anonymous;
        }

        var oid = principal.FindFirstValue("oid")
            ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(oid))
        {
            return UserSnapshot.Anonymous;
        }

        var identity = dbContext.UserIdentities
            .AsNoTracking()
            .FirstOrDefault(u => u.EntraObjectId == oid);

        if (identity is null)
        {
            return new UserSnapshot(Guid.Empty, oid, string.Empty, false);
        }

        return new UserSnapshot(identity.Id, identity.EntraObjectId, identity.Email, identity.IsPlatformOperator);
    }

    private readonly record struct UserSnapshot(Guid UserIdentityId, string EntraObjectId, string Email, bool IsPlatformOperator)
    {
        public static UserSnapshot Anonymous => new(Guid.Empty, string.Empty, string.Empty, false);
    }
}
