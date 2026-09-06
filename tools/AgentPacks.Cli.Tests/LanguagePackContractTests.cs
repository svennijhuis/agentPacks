using System.Text.Json.Nodes;
using AgentPacks.Cli.Validation;

namespace AgentPacks.Cli.Tests;

/// <summary>
/// Squad loads a language pack's skills by exact name, so the names are the contract.
/// Every failure here is one nothing else in the build would report: the pack loads, validates and
/// installs, and the loop just never asks for the skill.
/// </summary>
public class LanguagePackContractTests
{
    private const string LanguagePackManifest = """
        {
          "$schema": "https://agent-plugins.org/schemas/1.0.0/plugin.schema.json",
          "name": "dotnet",
          "description": "Test language pack.",
          "keywords": ["language-pack"]
        }
        """;

    private static TestRepository LanguagePack() =>
        new TestRepository().WithPlugin("dotnet", LanguagePackManifest);

    [Fact]
    public void Shared_contract_names_both_required_slots()
    {
        Assert.Equal(["build", "test-patterns"], LanguagePackContract.RequiredSlots);
    }

    [Fact]
    public void A_pack_filling_both_required_slots_passes()
    {
        using var repo = LanguagePack()
            .WithLoopSkill("dotnet-build")
            .WithLoopSkill("dotnet-test-patterns");

        var run = repo.Validate();

        Assert.DoesNotContain(run.Diagnostics, d => d.Message.Contains("language-pack"));
    }

    [Theory]
    [InlineData("dotnet-build", "dotnet-test-patterns")]
    [InlineData("dotnet-test-patterns", "dotnet-build")]
    public void A_pack_missing_either_required_slot_is_rejected(string present, string missing)
    {
        using var repo = LanguagePack().WithLoopSkill(present);

        var run = repo.Validate();

        Assert.Contains(run.Diagnostics, d =>
            d.Message.Contains("missing required slot") && d.Message.Contains(missing));
    }

    [Fact]
    public void A_slot_skill_without_loop_audience_is_rejected()
    {
        using var repo = LanguagePack()
            .WithSkill("dotnet-build", plugin: "dotnet")
            .WithLoopSkill("dotnet-test-patterns");

        var run = repo.Validate();

        Assert.Contains(run.Diagnostics, d =>
            d.Message.Contains("metadata.audience") && d.Message.Contains("dotnet-build"));
    }

    [Fact]
    public void A_plugin_without_the_keyword_is_not_held_to_the_contract()
    {
        using var repo = new TestRepository().WithPlugin().WithSkill("engineering-review");

        var run = repo.Validate();

        Assert.DoesNotContain(run.Diagnostics, d => d.Message.Contains("missing required slot"));
    }

    [Theory]
    [InlineData("dotnet-test-pattern")]
    [InlineData("dotnet-reviews")]
    [InlineData("dotnet-builds")]
    public void A_near_miss_of_a_slot_name_is_rejected(string directoryName)
    {
        using var repo = LanguagePack()
            .WithLoopSkill("dotnet-build")
            .WithLoopSkill("dotnet-test-patterns")
            .WithSkill(directoryName, plugin: "dotnet");

        var run = repo.Validate();

        Assert.Contains(run.Diagnostics, d => d.Message.Contains("one small edit from the contracted slot"));
    }

    [Fact]
    public void A_language_prefixed_skill_that_is_not_a_slot_at_all_is_allowed()
    {
        // 'rust-error-handling' style names are the documented shape for language knowledge that
        // is not one of the loop's slots. Only a near miss is a mistake.
        using var repo = LanguagePack()
            .WithLoopSkill("dotnet-build")
            .WithLoopSkill("dotnet-test-patterns")
            .WithSkill("dotnet-error-handling", plugin: "dotnet");

        var run = repo.Validate();

        Assert.DoesNotContain(run.Diagnostics, d => d.Message.Contains("contracted slot"));
    }

    [Fact]
    public void A_framework_skill_is_not_measured_against_the_slots()
    {
        using var repo = LanguagePack()
            .WithLoopSkill("dotnet-build")
            .WithLoopSkill("dotnet-test-patterns")
            .WithSkill("aspnet-api-design", plugin: "dotnet");

        var run = repo.Validate();

        Assert.DoesNotContain(run.Diagnostics, d => d.Message.Contains("contracted slot"));
    }

    [Fact]
    public void Authored_slot_skills_are_loop_audience_not_user_entrypoints()
    {
        var root = SourceRoot();
        foreach (var relative in new[]
        {
            Path.Combine("plugins", "dotnet", "skills", "dotnet-build", "SKILL.md"),
            Path.Combine("plugins", "dotnet", "skills", "dotnet-test-patterns", "SKILL.md"),
            Path.Combine("plugins", "dotnet", "skills", "dotnet-review", "SKILL.md"),
            Path.Combine("plugins", "dotnet", "skills", "dotnet-solution", "SKILL.md"),
            Path.Combine("plugins", "rust", "skills", "rust-build", "SKILL.md"),
            Path.Combine("plugins", "rust", "skills", "rust-test-patterns", "SKILL.md"),
            Path.Combine("plugins", "rust", "skills", "rust-review", "SKILL.md"),
            Path.Combine("plugins", "typescript", "skills", "typescript-build", "SKILL.md"),
            Path.Combine("plugins", "typescript", "skills", "typescript-test-patterns", "SKILL.md"),
            Path.Combine("plugins", "typescript", "skills", "typescript-review", "SKILL.md")
        })
        {
            var skill = File.ReadAllText(Path.Combine(root, relative));
            Assert.Contains("audience: loop", skill, StringComparison.Ordinal);
            Assert.Contains("not as a user entrypoint", skill, StringComparison.Ordinal);
            Assert.DoesNotContain("disable-model-invocation: true", skill, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void Language_slot_skills_name_their_canonical_standards()
    {
        var root = SourceRoot();
        foreach (var pack in new[] { "dotnet", "rust", "typescript" })
        {
            var plugin = Path.Combine(root, "plugins", pack);
            var standards = JsonNode.Parse(File.ReadAllText(Path.Combine(plugin, "standards.source.json")))!;
            foreach (var consumer in standards["consumers"]!.AsObject())
            {
                var skill = File.ReadAllText(Path.Combine(plugin, "skills", consumer.Key, "SKILL.md"));
                Assert.Contains($"exact Skill tool name `{consumer.Key}`", skill, StringComparison.Ordinal);
                Assert.Contains("Standards in force:", skill, StringComparison.Ordinal);
                Assert.Contains("references/standards/", skill, StringComparison.Ordinal);
                Assert.Contains("Good:", skill, StringComparison.Ordinal);
                Assert.Contains("Bad:", skill, StringComparison.Ordinal);
                foreach (var document in consumer.Value!.AsArray())
                    Assert.Contains(document!.GetValue<string>() + ".md", skill, StringComparison.Ordinal);
            }
        }
    }

    [Fact]
    public void Typescript_pack_fills_required_slots_with_loop_audience()
    {
        var root = SourceRoot();
        var plugin = Path.Combine(root, "plugins", "typescript");
        var manifest = JsonNode.Parse(File.ReadAllText(Path.Combine(plugin, "plugin.json")))!;
        var standards = JsonNode.Parse(File.ReadAllText(Path.Combine(plugin, "standards.source.json")))!;

        Assert.Equal("typescript", manifest["name"]!.GetValue<string>());
        Assert.Contains(LanguagePackContract.Keyword,
            manifest["keywords"]!.AsArray().Select(value => value!.GetValue<string>()));
        foreach (var skill in new[] { "typescript-build", "typescript-test-patterns", "typescript-review" })
        {
            var text = File.ReadAllText(Path.Combine(plugin, "skills", skill, "SKILL.md"));
            Assert.Contains("audience: loop", text, StringComparison.Ordinal);
            Assert.Contains("not as a user entrypoint", text, StringComparison.Ordinal);
            Assert.DoesNotContain("disable-model-invocation: true", text, StringComparison.Ordinal);
            Assert.NotNull(standards["consumers"]![skill]);
        }

        Assert.False(Directory.Exists(Path.Combine(plugin, "skills", "typescript-security-review")));
        Assert.False(Directory.Exists(Path.Combine(plugin, "skills", "react-component-scaffold")));
        Assert.False(File.Exists(Path.Combine(plugin, "mcp.json")));
        Assert.Contains("`package.json`, `tsconfig.json`",
            File.ReadAllText(Path.Combine(root, "plugins", "pack-check", "skills", "pack-check", "references", "packs.md")),
            StringComparison.Ordinal);
    }

    [Fact]
    public void Authored_rust_pack_fills_all_three_slots_and_maps_its_standards()
    {
        var root = SourceRoot();
        var plugin = Path.Combine(root, "plugins", "rust");
        var manifest = JsonNode.Parse(File.ReadAllText(Path.Combine(plugin, "plugin.json")))!;
        var standards = JsonNode.Parse(File.ReadAllText(Path.Combine(plugin, "standards.source.json")))!;

        Assert.Equal("rust", manifest["name"]!.GetValue<string>());
        Assert.Contains(LanguagePackContract.Keyword,
            manifest["keywords"]!.AsArray().Select(value => value!.GetValue<string>()));
        foreach (var skill in new[] { "rust-build", "rust-test-patterns", "rust-review" })
        {
            Assert.True(File.Exists(Path.Combine(plugin, "skills", skill, "SKILL.md")), skill);
            Assert.NotNull(standards["consumers"]![skill]);
        }

        foreach (var document in new[] { "rust", "errors-concurrency", "testing" })
            Assert.True(File.Exists(Path.Combine(plugin, "standards", document + ".md")), document);
    }

    private static string SourceRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory);
             directory is not null;
             directory = directory.Parent)
        {
            if (Directory.Exists(Path.Combine(directory.FullName, "plugins")) &&
                Directory.Exists(Path.Combine(directory.FullName, "tools", "AgentPacks.Cli")))
            {
                return directory.FullName;
            }
        }

        throw new DirectoryNotFoundException("Could not locate the agentPacks source root.");
    }

}
