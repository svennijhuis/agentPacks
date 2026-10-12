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

            Assert.True(File.Exists(Path.Combine(output, "plugins", "pack-check", "skills", "pack-check", "scripts", "check-plan.sh")));
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
    public void Install_docs_name_the_headless_cursor_smoke(string doc)
    {
        var text = File.ReadAllText(Path.Combine(TestRepository.SourceRoot(), doc));
        Assert.Contains("cp /tmp/agentpacks-marketplace/plugins/squad/commands/*.md .cursor/commands/", text, StringComparison.Ordinal);
        Assert.Contains("cursor-agent -p --trust --plugin-dir /tmp/agentpacks-marketplace/plugins/squad", text, StringComparison.Ordinal);
        Assert.Contains("`validate-all --out` writes a self-contained tree", text, StringComparison.Ordinal);
    }
}
