using System.Text.Json.Nodes;
using AgentPacks.Cli.Io;
using AgentPacks.Cli.Loading;
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
        foreach (var (pack, name) in new[]
        {
            ("dotnet", "dotnet-build"),
            ("dotnet", "dotnet-test-patterns"),
            ("dotnet", "dotnet-review"),
            ("dotnet", "dotnet-solution"),
            ("rust", "rust-build"),
            ("rust", "rust-test-patterns"),
            ("rust", "rust-review"),
            ("typescript", "typescript-build"),
            ("typescript", "typescript-test-patterns"),
            ("typescript", "typescript-review")
        })
        {
            var skill = SourceSkills.Text(pack, name);
            Assert.Contains("audience: loop", skill, StringComparison.Ordinal);
            Assert.Contains("not as a user entrypoint", skill, StringComparison.Ordinal);
            Assert.DoesNotContain("disable-model-invocation: true", skill, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void Language_slot_skills_name_their_canonical_standards()
    {
        var root = TestRepository.SourceRoot();
        foreach (var pack in new[] { "dotnet", "rust", "typescript" })
        {
            var plugin = Path.Combine(root, "plugins", pack);
            var standards = JsonNode.Parse(File.ReadAllText(Path.Combine(plugin, "standards.source.json")))!;
            foreach (var consumer in standards["consumers"]!.AsObject())
            {
                var skill = SourceSkills.Text(pack, consumer.Key);
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
        var root = TestRepository.SourceRoot();
        var plugin = Path.Combine(root, "plugins", "typescript");
        var manifest = JsonNode.Parse(File.ReadAllText(Path.Combine(plugin, "plugin.json")))!;
        var standards = JsonNode.Parse(File.ReadAllText(Path.Combine(plugin, "standards.source.json")))!;

        Assert.Equal("typescript", manifest["name"]!.GetValue<string>());
        Assert.Contains(LanguagePackContract.Keyword,
            manifest["keywords"]!.AsArray().Select(value => value!.GetValue<string>()));
        foreach (var skill in new[] { "typescript-build", "typescript-test-patterns", "typescript-review" })
        {
            var text = SourceSkills.Text("typescript", skill);
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
        var root = TestRepository.SourceRoot();
        var plugin = Path.Combine(root, "plugins", "rust");
        var manifest = JsonNode.Parse(File.ReadAllText(Path.Combine(plugin, "plugin.json")))!;
        var standards = JsonNode.Parse(File.ReadAllText(Path.Combine(plugin, "standards.source.json")))!;

        Assert.Equal("rust", manifest["name"]!.GetValue<string>());
        Assert.Contains(LanguagePackContract.Keyword,
            manifest["keywords"]!.AsArray().Select(value => value!.GetValue<string>()));
        foreach (var skill in new[] { "rust-build", "rust-test-patterns", "rust-review" })
        {
            Assert.True(File.Exists(Path.Combine(plugin, "skills", skill, SlotSkillRenderer.SourceFileName)), skill);
            Assert.True(SourceSkills.Skill("rust", skill).IsRendered, skill);
            Assert.NotNull(standards["consumers"]![skill]);
        }

        foreach (var document in new[] { "rust", "errors-concurrency", "testing" })
            Assert.True(File.Exists(Path.Combine(plugin, "standards", document + ".md")), document);
    }

    public const string InternalDoNotRunLine =
        "Internal. Do not run directly — Squad loads by exact Skill name.";

    /// <summary>Po 31: skill bodies cite generated references, never the authored standards tree.</summary>
    [Fact]
    public void Skill_bodies_point_only_at_references_standards()
    {
        foreach (var skill in SourceSkills.All())
        {
            var body = BodyAfterFrontmatter(skill.Text);
            Assert.DoesNotContain("authored tree", body, StringComparison.Ordinal);
            Assert.DoesNotContain("../../standards", body, StringComparison.Ordinal);
        }

        Language_test_patterns_ship_references_examples();
    }

    [Fact]
    public void Language_test_patterns_ship_references_examples()
    {
        var root = TestRepository.SourceRoot();
        foreach (var pack in new[] { "dotnet", "rust", "typescript" })
        {
            var skillDir = Path.Combine(root, "plugins", pack, "skills", $"{pack}-test-patterns");
            var examples = Path.Combine(skillDir, "references", "examples");
            Assert.True(Directory.Exists(examples), examples);
            Assert.NotEmpty(Directory.GetFiles(examples, "*.md"));
            var skill = SourceSkills.Text(pack, $"{pack}-test-patterns");
            Assert.Contains("references/standards/", skill, StringComparison.Ordinal);
            Assert.Contains("references/examples/", skill, StringComparison.Ordinal);
        }
    }

    /// <summary>
    /// Po 42 fail bar: <c>dotnet-review</c> ships 1–2 Matt-tiny Good/Bad finding
    /// cites. <c>standards.source.json</c> still maps the three canonical docs
    /// into this skill. SKILL.md still loads and cites <c>references/standards/</c>
    /// first. CI stays the existing one-job <c>validate</c> workflow.
    /// </summary>
    [Fact]
    public void Dotnet_review_examples_matt_tiny_standards_source_kept()
    {
        var root = TestRepository.SourceRoot();
        var skillDir = Path.Combine(root, "plugins", "dotnet", "skills", "dotnet-review");
        var examples = Path.Combine(skillDir, "references", "examples");
        Assert.True(Directory.Exists(examples), examples);

        var files = Directory.GetFiles(examples, "*.md");
        Assert.InRange(files.Length, 1, 2);
        foreach (var file in files)
        {
            var text = File.ReadAllText(file);
            var lines = text.Split('\n').Count(line => !string.IsNullOrWhiteSpace(line));
            Assert.Contains("Good:", text, StringComparison.Ordinal);
            Assert.Contains("Bad:", text, StringComparison.Ordinal);
            Assert.Contains(".md", text, StringComparison.Ordinal);
            Assert.True(lines <= 16, $"{Path.GetFileName(file)} is {lines} lines; Matt-tiny cap is 16.");
        }

        var catalog = JsonNode.Parse(File.ReadAllText(
            Path.Combine(root, "plugins", "dotnet", "standards.source.json")))!;
        var mapped = catalog["consumers"]!["dotnet-review"]!.AsArray()
            .Select(value => value!.GetValue<string>())
            .ToArray();
        Assert.Equal(["csharp", "async-errors", "testing", "layers", "http-api"], mapped);
        foreach (var document in mapped.Where(id => id != "http-api"))
        {
            Assert.Equal(
                $"standards/{document}.md",
                catalog["documents"]![document]!.GetValue<string>());
        }

        var skill = SourceSkills.Text("dotnet", "dotnet-review");
        Assert.Contains("Read every file in `references/standards/`.", skill, StringComparison.Ordinal);
        Assert.Contains("Standards in force:", skill, StringComparison.Ordinal);
        Assert.Contains("references/standards/", skill, StringComparison.Ordinal);
        Assert.DoesNotContain("Read every file in `references/examples/`.", skill, StringComparison.Ordinal);
        var standardsAt = skill.IndexOf("references/standards/", StringComparison.Ordinal);
        var examplesAt = skill.IndexOf("references/examples/", StringComparison.Ordinal);
        Assert.True(standardsAt >= 0 && (examplesAt < 0 || standardsAt < examplesAt),
            "SKILL.md must still read/cite references/standards/ before any example pointer.");

        new SquadContractTests().Pull_request_ci_stays_one_job_no_matrix();
    }

    /// <summary>
    /// Item 62: Claude marketplace omits hooks; authored command names do not collide with a
    /// skill or plugin name except the two homonyms Copilot already hides.
    /// </summary>
    [Fact]
    public void Command_names_differ_from_skill_and_plugin_names()
    {
        new ClaudeMarketplaceHooksTests().Claude_marketplace_omits_hooks_path_and_array();
        new HttpScenariosContractTests().Copilot_scenarios_command_name_differs_from_skill();
        new SquadContractTests().Copilot_factory_command_name_differs_from_plugin_name();

        var root = TestRepository.SourceRoot();
        var allowed = new HashSet<(string Plugin, string Name)>
        {
            ("squad", "squad"),
            ("pack-check", "pack-check")
        };

        foreach (var pluginDirectory in Directory.GetDirectories(Path.Combine(root, "plugins"))
                     .OrderBy(path => path, StringComparer.Ordinal))
        {
            var pluginName = JsonNode.Parse(
                File.ReadAllText(Path.Combine(pluginDirectory, "plugin.json")))!
                ["name"]!.GetValue<string>();

            var commandNames = AuthoredCommandNames(pluginDirectory);
            var skillNames = SourceSkills.All()
                .Where(skill => skill.Directory.StartsWith(pluginDirectory + Path.DirectorySeparatorChar, StringComparison.Ordinal))
                .Select(skill => skill.Frontmatter?.Scalar("name") ?? skill.DirectoryName)
                .ToHashSet(StringComparer.Ordinal);

            foreach (var command in commandNames)
            {
                if (command != pluginName && !skillNames.Contains(command))
                    continue;

                Assert.True(
                    allowed.Contains((pluginName, command)),
                    $"command '{command}' in plugin '{pluginName}' collides with a skill or plugin name.");
            }
        }
    }

    private static HashSet<string> AuthoredCommandNames(string pluginDirectory)
    {
        var directory = Path.Combine(pluginDirectory, "commands");
        if (!Directory.Exists(directory))
            return new HashSet<string>(StringComparer.Ordinal);

        return Directory.GetFiles(directory, "*.md")
            .Select(path => Frontmatter.TryParse(File.ReadAllText(path), out _)?.Scalar("name") ?? Path.GetFileNameWithoutExtension(path))
            .ToHashSet(StringComparer.Ordinal);
    }

    /// <summary>Po 33: loop-audience skills open with the Internal do-not-run line.</summary>
    [Fact]
    public void Loop_audience_skills_start_with_internal_do_not_run_directly()
    {
        var root = TestRepository.SourceRoot();
        var loopCount = 0;
        foreach (var skill in SourceSkills.All())
        {
            var text = skill.Text;
            if (!FrontmatterBlock(text).Contains("audience: loop", StringComparison.Ordinal))
            {
                continue;
            }

            loopCount++;
            Assert.Equal(InternalDoNotRunLine, FirstBodyLine(text));
            Assert.Contains("audience: loop", text, StringComparison.Ordinal);
        }

        Assert.True(loopCount >= 10, $"expected authored loop skills, found {loopCount}.");

        foreach (var relative in new[]
        {
            Path.Combine("plugins", "pack-check", "skills", "pack-check", "SKILL.md"),
            Path.Combine("plugins", "squad", "skills", "squad", "SKILL.md"),
            Path.Combine("plugins", "squad", "skills", "learnings-digest", "SKILL.md"),
            Path.Combine("plugins", "squad", "skills", "scenarios-md", "SKILL.md")
        })
        {
            var text = File.ReadAllText(Path.Combine(root, relative));
            Assert.DoesNotContain("audience: loop", FrontmatterBlock(text), StringComparison.Ordinal);
            Assert.NotEqual(InternalDoNotRunLine, FirstBodyLine(text));
        }

        foreach (var pack in new[] { "dotnet", "rust", "typescript" })
        {
            var readme = File.ReadAllText(Path.Combine(root, "plugins", pack, "README.md"));
            Assert.Contains("Slot skills are Squad internals", readme, StringComparison.Ordinal);
        }
    }

    /// <summary>Po 33: Copilot dialect emit sets user-invocable false for audience loop.</summary>
    [Fact]
    public void Copilot_emits_user_invocable_false_for_loop_audience()
    {
        using var repo = LanguagePack()
            .WithLoopSkill("dotnet-build")
            .WithLoopSkill("dotnet-test-patterns")
            .WithSkill("dotnet-error-handling", plugin: "dotnet");

        var run = repo.ValidateAndGenerate();
        Assert.False(run.HasErrors, run.Text);

        var generated = run.File("plugins/dotnet/com.github.copilot/skills/dotnet-build/SKILL.md").Text;
        Assert.Contains("user-invocable: false", generated, StringComparison.Ordinal);
        Assert.True(run.HasFile("plugins/dotnet/com.github.copilot/skills/dotnet-test-patterns/SKILL.md"));
        Assert.False(run.HasFile("plugins/dotnet/com.github.copilot/skills/dotnet-error-handling/SKILL.md"));
    }

    private static (int Start, int End) FrontmatterFence(string text)
    {
        const string fence = "---";
        var start = text.IndexOf(fence, StringComparison.Ordinal);
        Assert.True(start >= 0, "missing opening frontmatter fence");
        var end = text.IndexOf(fence, start + fence.Length, StringComparison.Ordinal);
        Assert.True(end > start, "missing closing frontmatter fence");
        return (start, end);
    }

    private static string FrontmatterBlock(string text)
    {
        var (start, end) = FrontmatterFence(text);
        return text[start..end];
    }

    private static string BodyAfterFrontmatter(string text)
    {
        var (_, end) = FrontmatterFence(text);
        return text[(end + "---".Length)..];
    }

    private static string FirstBodyLine(string text) =>
        BodyAfterFrontmatter(text)
            .Split('\n')
            .Select(line => line.TrimEnd('\r'))
            .First(line => !string.IsNullOrWhiteSpace(line));

}
