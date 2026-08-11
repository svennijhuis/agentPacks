using System.Text;

namespace AgentPacks.Cli.Validation;

/// <summary>
/// Why a finding matters. The specification isolates failures and tolerates some violations;
/// this repository is stricter. Tagging the difference lets a developer tell "this breaks
/// clients" from "this breaks our rules".
/// </summary>
internal enum DiagnosticKind
{
    /// <summary>A client would reject the plugin or component outright.</summary>
    SpecFatal,

    /// <summary>A spec violation clients report and ignore. We still fail the build.</summary>
    SpecTolerated,

    /// <summary>Valid per the specification, rejected by company policy.</summary>
    CompanyPolicy,

    /// <summary>Advisory only. Does not fail the build.</summary>
    Warning
}

internal sealed record Diagnostic(string Path, string Message, DiagnosticKind Kind)
{
    public bool IsError => Kind != DiagnosticKind.Warning;

    public string Label => Kind switch
    {
        DiagnosticKind.SpecFatal => "spec",
        DiagnosticKind.SpecTolerated => "spec(tolerated)",
        DiagnosticKind.CompanyPolicy => "policy",
        DiagnosticKind.Warning => "warning",
        _ => "error"
    };
}

/// <summary>
/// Collects every finding instead of throwing on the first, so one run reports one round of fixes.
/// </summary>
internal sealed class DiagnosticCollector
{
    private readonly List<Diagnostic> _diagnostics = [];

    public IReadOnlyList<Diagnostic> Diagnostics => _diagnostics;

    public bool HasErrors => _diagnostics.Any(d => d.IsError);

    public int ErrorCount => _diagnostics.Count(d => d.IsError);

    public int WarningCount => _diagnostics.Count(d => !d.IsError);

    public void Add(string path, string message, DiagnosticKind kind) =>
        _diagnostics.Add(new Diagnostic(path, message, kind));

    public void SpecFatal(string path, string message) => Add(path, message, DiagnosticKind.SpecFatal);

    public void SpecTolerated(string path, string message) => Add(path, message, DiagnosticKind.SpecTolerated);

    public void Policy(string path, string message) => Add(path, message, DiagnosticKind.CompanyPolicy);

    public void Warning(string path, string message) => Add(path, message, DiagnosticKind.Warning);

    /// <summary>Deterministic rendering: ordered by path, then message.</summary>
    public string Render()
    {
        var builder = new StringBuilder();

        foreach (var diagnostic in _diagnostics
                     .OrderBy(d => d.Path, StringComparer.Ordinal)
                     .ThenBy(d => d.Message, StringComparer.Ordinal))
        {
            builder.Append(diagnostic.Path)
                .Append(": [")
                .Append(diagnostic.Label)
                .Append("] ")
                .AppendLine(diagnostic.Message);
        }

        if (_diagnostics.Count > 0)
        {
            builder.Append(ErrorCount).Append(" error(s)");

            if (WarningCount > 0)
            {
                builder.Append(", ").Append(WarningCount).Append(" warning(s)");
            }

            builder.AppendLine(".");
        }

        return builder.ToString();
    }
}
