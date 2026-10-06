namespace TenancyHub.Web.Resources;

/// <summary>
/// Central UK English user-facing copy for the web UI (R1: English only, no language selector).
/// </summary>
public static class UiStrings
{
    /// <summary>Product name shown in the shell and marketing surfaces.</summary>
    public const string ProductName = "Tenancy Hub";

    /// <summary>Signed-in application shell chrome.</summary>
    public static class Shell
    {
        public const string SignOut = "Sign out";

        public const string PendingInvitationsBanner = "You have pending invitations.";

        public const string ReviewInvitationsLink = "Review invitations";
    }

    /// <summary>Blazor platform error bar (not Fluent UI).</summary>
    public static class BlazorError
    {
        public const string UnhandledError = "An unhandled error has occurred.";

        public const string Reload = "Reload";
    }

    /// <summary>Route shown when the user is signed in but has no agency or operator access.</summary>
    public static class AccessNotConfigured
    {
        public const string Title = "Access not configured";

        public const string Body =
            "Your work account is signed in, but you do not have an active agency membership or platform operator access yet. "
            + "Contact your administrator or a platform operator to be invited or provisioned.";
    }
}
