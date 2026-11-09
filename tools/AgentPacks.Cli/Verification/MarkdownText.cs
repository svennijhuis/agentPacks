namespace AgentPacks.Cli.Verification;

/// <summary>Shared markdown cell helpers for plan and verifier parsers.</summary>
internal static class MarkdownText
{
    /// <summary>Strips one pair of wrapping backticks from a table cell or inline code span.</summary>
    public static string Unwrap(string value)
    {
        var trimmed = value.Trim();

        if (trimmed.Length >= 2 && trimmed[0] == '`' && trimmed[^1] == '`')
        {
            return trimmed[1..^1].Trim();
        }

        return trimmed;
    }
}
