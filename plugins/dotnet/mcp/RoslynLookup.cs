using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

/// <summary>
/// On-machine Roslyn lookups. Three read-only tools. No write or code-action tools.
/// </summary>
internal static class RoslynLookup
{
    public static readonly IReadOnlyList<string> ReadOnlyTools =
        ["list_symbols", "find_references", "list_diagnostics"];

    public static IReadOnlyList<string> ListSymbols(string source)
    {
        return CSharpSyntaxTree.ParseText(source).GetCompilationUnitRoot().DescendantNodes()
            .Select(node => node switch
            {
                TypeDeclarationSyntax type => type.Identifier.ValueText,
                MethodDeclarationSyntax method => method.Identifier.ValueText,
                _ => null
            })
            .Where(name => name is not null)
            .Cast<string>()
            .Distinct(StringComparer.Ordinal)
            .ToList();
    }

    public static IReadOnlyList<string> FindReferences(string source, string symbol)
    {
        return CSharpSyntaxTree.ParseText(source).GetCompilationUnitRoot().DescendantTokens()
            .Where(token => token.IsKind(SyntaxKind.IdentifierToken) &&
                            string.Equals(token.ValueText, symbol, StringComparison.Ordinal))
            .Select(token => (token.GetLocation().GetLineSpan().StartLinePosition.Line + 1).ToString())
            .ToList();
    }

    public static IReadOnlyList<string> ListDiagnostics(string source)
    {
        var tree = CSharpSyntaxTree.ParseText(source);
        var references = (AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string ?? "")
            .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
            .Select(path => MetadataReference.CreateFromFile(path));
        var compilation = CSharpCompilation.Create(
            "lookup",
            [tree],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        return compilation.GetDiagnostics()
            .Where(diagnostic => diagnostic.Severity >= DiagnosticSeverity.Warning)
            .Select(diagnostic => $"{diagnostic.Id}: {diagnostic.GetMessage()}")
            .ToList();
    }
}
