namespace AgentPacks.Cli.Verification;

/// <summary>Shared markdown table-cell cleanup used by verifier parsers.</summary>
internal static class MarkdownCells
{
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
