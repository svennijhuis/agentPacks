using System.Diagnostics;
using AgentPacks.Cli.Generation;

namespace AgentPacks.Cli.Tests;

/// <summary>The validate-all --out tree installs on its own, and the docs name the headless Cursor run.</summary>
public sealed class OutTreeInstallTests
{
    [Fact]
    public void Out_tree_carries_the_authored_skills_references_and_commands()
    {
        var root = TestRepository.SourceRoot();
        var output = Directory.CreateTempSubdirectory("out-tree-").FullName;
        try
        {
            var info = new ProcessStartInfo("dotnet", [typeof(GeneratedFile).Assembly.Location, "validate-all", "--out", output])
            {
                WorkingDirectory = root, RedirectStandardOutput = true, RedirectStandardError = true,
            };
            using (var process = Process.Start(info)!)
            {
                var stdout = process.StandardOutput.ReadToEnd();
                var stderr = process.StandardError.ReadToEnd();
                process.WaitForExit();
                Assert.True(process.ExitCode == 0, stdout + stderr);
            }

            var squad = Path.Combine(output, "plugins", "squad");
            foreach (var relative in new[]
                     {
                         "skills/squad/SKILL.md", "skills/squad/references/planning-contract.md",
                         "commands/squad.md", "commands/squad-review.md", "agents/squad-planner.md",
                         ".cursor-plugin/plugin.json", "plugin.json",
                     })
            {
                Assert.True(File.Exists(Path.Combine(squad, relative)), $"out tree is missing plugins/squad/{relative}");
            }

            foreach (var script in new[] { "check-plan.sh", "check-plan.ps1", "check-retro.sh", "check-retro.ps1", "check-verify.sh", "check-verify.ps1" })
            {
                Assert.True(File.Exists(Path.Combine(output, "plugins", "pack-check", "skills", "pack-check", "scripts", script)), $"out tree is missing {script}");
            }

            var outNames = Directory.EnumerateFiles(output, "*", SearchOption.AllDirectories).Select(Path.GetFileName).ToHashSet(StringComparer.Ordinal);
            var sourceNames = Directory.EnumerateFiles(root, "*", SearchOption.TopDirectoryOnly).Select(Path.GetFileName).ToHashSet(StringComparer.Ordinal);
            var missing = new List<string>();
            foreach (var md in Directory.EnumerateFiles(Path.Combine(output, "plugins"), "*.md", SearchOption.AllDirectories)
                         .Where(f => Path.GetRelativePath(output, f).Replace('\\', '/').Split('/') is var parts && ((parts.Length > 3 && parts[2] is "skills" or "commands" or "agents") || (parts.Length == 3 && parts[2] == "README.md"))
                             // pack-author runs inside an agentPacks checkout, so its links to docs/ resolve there.
                             && parts[1] != "pack-author"))
            {
                var text = File.ReadAllText(md);
                foreach (System.Text.RegularExpressions.Match link in System.Text.RegularExpressions.Regex.Matches(text, @"\]\(([^)#\s]+)(#[^)]*)?\)"))
                {
                    var target = link.Groups[1].Value;
                    if (target.Contains("://", StringComparison.Ordinal) || target.StartsWith("mailto:", StringComparison.Ordinal)) continue;
                    if (!File.Exists(Path.GetFullPath(Path.Combine(Path.GetDirectoryName(md)!, target))) && !Directory.Exists(Path.GetFullPath(Path.Combine(Path.GetDirectoryName(md)!, target))))
                        missing.Add($"{Path.GetRelativePath(output, md)} links {target}");
                }
                foreach (System.Text.RegularExpressions.Match name in System.Text.RegularExpressions.Regex.Matches(text, @"[\w.-]+\.(json|md|sh|ps1|yml|yaml)\b"))
                {
                    // global.json is the product repo's own SDK pin, not a pack file.
                    if (name.Value != "global.json" && sourceNames.Contains(name.Value) && !outNames.Contains(name.Value))
                        missing.Add($"{Path.GetRelativePath(output, md)} names repo-only {name.Value}");
                }
            }
            Assert.True(missing.Count == 0, string.Join("\n", missing.Distinct()));
        }
        finally
        {
            Directory.Delete(output, true);
        }
    }

    [Theory]
    [InlineData("README.md")]
    [InlineData("docs/CONTRIBUTE.md")]
    [InlineData("docs/ADD-SKILL.md")]
    public void Headless_copy_step_stamps_an_existing_squad_plugin_root(string doc)
    {
        var root = TestRepository.SourceRoot();
        var text = File.ReadAllText(Path.Combine(root, doc));
        var at = text.IndexOf("mkdir -p .cursor/commands && cp ", StringComparison.Ordinal);
        Assert.True(at >= 0, "copy step missing");
        var lines = text[at..].Split('\n');
        var snippet = string.Join("\n", lines[0], lines[1]).Replace("/tmp/agentpacks-marketplace", root, StringComparison.Ordinal);
        var work = Directory.CreateTempSubdirectory("headless-copy-").FullName;
        try
        {
            var stampOnly = lines[1].Replace("/tmp/agentpacks-marketplace", root, StringComparison.Ordinal);
            foreach (var script in new[] { snippet, stampOnly, snippet })
            {
                var info = new ProcessStartInfo("bash", ["-c", script]) { WorkingDirectory = work, RedirectStandardError = true };
                using var process = Process.Start(info)!;
                var err = process.StandardError.ReadToEnd();
                process.WaitForExit();
                Assert.True(process.ExitCode == 0, err);
                Assert.Equal(1, File.ReadAllLines(Path.Combine(work, ".cursor", "commands", "squad.md")).Count(l => l.StartsWith("Squad plugin root:", StringComparison.Ordinal)));
            }

            var copy = File.ReadAllText(Path.Combine(work, ".cursor", "commands", "squad.md")).Split('\n');
            var close = Array.IndexOf(copy, "---", 1);
            Assert.True(close > 0, "frontmatter lost");
            var stamp = copy[close + 1];
            Assert.StartsWith("Squad plugin root: ", stamp, StringComparison.Ordinal);
            var stamped = stamp["Squad plugin root: ".Length..];
            Assert.True(File.Exists(Path.Combine(stamped, "skills", "squad", "SKILL.md")), stamped);
            Assert.Equal(1, copy.Count(l => l.StartsWith("Squad plugin root:", StringComparison.Ordinal)));
        }
        finally
        {
            Directory.Delete(work, true);
        }
    }

    [Theory]
    [InlineData("README.md")]
    [InlineData("docs/CONTRIBUTE.md")]
    [InlineData("docs/ADD-SKILL.md")]
    public void Install_docs_name_the_headless_cursor_smoke(string doc)
    {
        var text = File.ReadAllText(Path.Combine(TestRepository.SourceRoot(), doc));
        Assert.Contains("cp /tmp/agentpacks-marketplace/plugins/squad/commands/*.md .cursor/commands/", text, StringComparison.Ordinal);
        Assert.Contains("cursor-agent -p --trust -f --plugin-dir /tmp/agentpacks-marketplace/plugins/squad", text, StringComparison.Ordinal);
        Assert.Contains("--plugin-dir /tmp/agentpacks-marketplace/plugins/dotnet \"/squad <ask>\"", text, StringComparison.Ordinal);
        Assert.Contains("Replace `dotnet` with the language pack pack-check detects", text, StringComparison.Ordinal);
        Assert.Contains("`validate-all --out` writes a self-contained tree", text, StringComparison.Ordinal);
    }
}
