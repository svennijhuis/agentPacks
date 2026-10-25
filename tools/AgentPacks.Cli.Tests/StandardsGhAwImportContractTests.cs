using System.Text.Json.Nodes;
using AgentPacks.Cli.Loading;

namespace AgentPacks.Cli.Tests;

/// <summary>
/// gh-aw imports a single Markdown file per entry; nested relative imports resolve from the
/// consumer workflow directory, so authored standards must be plain Markdown at stable paths.
/// </summary>
public sealed class StandardsGhAwImportContractTests
{
    [Fact]
    public void Standards_catalog_documents_exist_and_are_plain_markdown_for_gh_aw()
    {
        var root = TestRepository.SourceRoot();

        foreach (var catalogPath in Directory.GetFiles(
                     Path.Combine(root, "plugins"),
                     PluginLoader.StandardsFileName,
                     SearchOption.AllDirectories))
        {
            var pluginDir = Path.GetDirectoryName(catalogPath)!;
            var catalog = JsonNode.Parse(File.ReadAllText(catalogPath))!;
            var documents = catalog["documents"]!.AsObject();
            var relativeCatalog = Path.GetRelativePath(root, catalogPath).Replace('\\', '/');

            foreach (var (id, node) in documents)
            {
                var path = node!.GetValue<string>();
                var full = SharedStandards.IsSharedPath(path)
                    ? Path.GetFullPath(Path.Combine(root, path))
                    : Path.GetFullPath(Path.Combine(pluginDir, path));
                Assert.True(File.Exists(full), $"{relativeCatalog} document '{id}' -> '{path}' is missing at {full}.");

                var text = File.ReadAllText(full);
                Assert.False(text.StartsWith("---", StringComparison.Ordinal), $"{path} must not have YAML frontmatter for gh-aw import.");
                Assert.DoesNotContain("${{", text, StringComparison.Ordinal);
                Assert.DoesNotContain("{{#", text, StringComparison.Ordinal);
            }
        }
    }
}
