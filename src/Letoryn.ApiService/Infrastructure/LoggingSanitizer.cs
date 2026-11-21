namespace Letoryn.ApiService.Infrastructure;

/// <summary>
/// Sanitises untrusted strings before they are written to logs (CWE-117).
/// </summary>
internal static class LoggingSanitizer
{
    /// <summary>
    /// Removes carriage-return and line-feed characters so user-controlled values cannot forge log lines.
    /// </summary>
    public static string ForLog(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        return value
            .Replace("\r", string.Empty, StringComparison.Ordinal)
            .Replace("\n", string.Empty, StringComparison.Ordinal);
    }
}
