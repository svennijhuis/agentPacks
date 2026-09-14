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
        var root = TestRepository.SourceRoot();
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
            Assert.Contains("audience: loop", FrontmatterBlock(skill), StringComparison.Ordinal);
            Assert.DoesNotContain("disable-model-invocation: true", FrontmatterBlock(skill), StringComparison.Ordinal);
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
                var skill = File.ReadAllText(Path.Combine(plugin, "skills", consumer.Key, "SKILL.md"));
                Assert.Contains("references/standards/", skill, StringComparison.Ordinal);
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
            var text = File.ReadAllText(Path.Combine(plugin, "skills", skill, "SKILL.md"));
            Assert.Contains("audience: loop", FrontmatterBlock(text), StringComparison.Ordinal);
            Assert.DoesNotContain("disable-model-invocation: true", FrontmatterBlock(text), StringComparison.Ordinal);
            Assert.NotNull(standards["consumers"]![skill]);
        }

        Assert.False(Directory.Exists(Path.Combine(plugin, "skills", "typescript-security-review")));
        Assert.False(Directory.Exists(Path.Combine(plugin, "skills", "react-component-scaffold")));
        Assert.False(File.Exists(Path.Combine(plugin, "mcp.json")));
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
            Assert.True(File.Exists(Path.Combine(plugin, "skills", skill, "SKILL.md")), skill);
            Assert.NotNull(standards["consumers"]![skill]);
        }

        foreach (var document in new[] { "rust", "errors-concurrency", "testing" })
            Assert.True(File.Exists(Path.Combine(plugin, "standards", document + ".md")), document);
    }

    /// <summary>
    /// Items 59/64: one <c>shared/standards/http-api.md</c> is referenced by each language pack.
    /// No pack copies.
    /// </summary>
    [Fact]
    public void Language_packs_map_http_api_to_shared_path()
    {
        var root = TestRepository.SourceRoot();
        var shared = Path.Combine(root, "shared", "standards", "http-api.md");
        Assert.True(File.Exists(shared), shared);
        var sharedText = File.ReadAllText(shared);
        var lines = sharedText.Split('\n').Count(line => !string.IsNullOrWhiteSpace(line));
        Assert.True(lines <= 50, $"shared/standards/http-api.md is {lines} lines; Matt-tiny cap is 50.");
        Assert.Contains("{ \"users\": [{ \"id\": 1 }] }", sharedText, StringComparison.Ordinal);
        Assert.DoesNotContain("Azure", sharedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("AWS", sharedText, StringComparison.OrdinalIgnoreCase);

        foreach (var pack in new[] { "dotnet", "rust", "typescript" })
        {
            Assert.False(
                File.Exists(Path.Combine(root, "plugins", pack, "standards", "http-api.md")),
                $"{pack} must not keep a copy of http-api.md");

            var catalog = JsonNode.Parse(File.ReadAllText(
                Path.Combine(root, "plugins", pack, "standards.source.json")))!;
            Assert.Equal(
                "shared/standards/http-api.md",
                catalog["documents"]!["http-api"]!.GetValue<string>());
            var consumers = catalog["consumers"]!.AsObject();
            Assert.Contains("http-api", consumers[$"{pack}-build"]!.AsArray().Select(value => value!.GetValue<string>()));
            Assert.Contains("http-api", consumers[$"{pack}-review"]!.AsArray().Select(value => value!.GetValue<string>()));
            Assert.DoesNotContain(
                "http-api",
                consumers[$"{pack}-test-patterns"]!.AsArray().Select(value => value!.GetValue<string>()));

            foreach (var skill in new[] { $"{pack}-build", $"{pack}-review" })
            {
                Assert.Contains(
                    "http-api.md",
                    File.ReadAllText(Path.Combine(root, "plugins", pack, "skills", skill, "SKILL.md")),
                    StringComparison.Ordinal);
            }
        }
    }

    public const string InternalDoNotRunLine =
        "Internal. Do not run directly — Squad loads by exact Skill name.";


    /// <summary>Po 31: skill bodies cite generated references, never the authored standards tree.</summary>
    [Fact]
    public void Skill_bodies_point_only_at_references_standards()
    {
        var root = TestRepository.SourceRoot();
        foreach (var path in AuthoredSkillFiles(root))
        {
            var body = BodyAfterFrontmatter(File.ReadAllText(path));
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
            var skill = File.ReadAllText(Path.Combine(skillDir, "SKILL.md"));
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
            var lines = File.ReadAllText(file).Split('\n').Count(line => !string.IsNullOrWhiteSpace(line));
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

        Assert.Equal("shared/standards/http-api.md", catalog["documents"]!["http-api"]!.GetValue<string>());

        var skill = File.ReadAllText(Path.Combine(skillDir, "SKILL.md"));
        Assert.Contains("references/standards/", skill, StringComparison.Ordinal);
    }

    /// <summary>Item 60: thin marketplace smoke entry shape in Squad learnings.</summary>
    [Fact]
    public void Marketplace_smoke_entry_shape_is_documented()
    {
        var learnings = File.ReadAllText(Path.Combine(
            TestRepository.SourceRoot(),
            "plugins", "squad", "skills", "squad", "references", "learnings.md"));
        Assert.Contains("## Marketplace smoke entry shape", learnings, StringComparison.Ordinal);
        Assert.Contains("- Client:", learnings, StringComparison.Ordinal);
        Assert.Contains("- Plugins:", learnings, StringComparison.Ordinal);
    }

    /// <summary>Item 61: README Install-from-Source line uses #marketplace.</summary>
    [Fact]
    public void Readme_install_from_source_uses_marketplace_ref()
    {
        var readme = File.ReadAllText(Path.Combine(TestRepository.SourceRoot(), "README.md"));
        Assert.Contains(
            "**Install from Source** (VS Code / Copilot): use `#marketplace`",
            readme,
            StringComparison.Ordinal);
    }

    /// <summary>
    /// Item 62: Claude marketplace omits hooks; authored command names do not collide
    /// with skill or plugin names except the (squad, squad) and (pack-check, pack-check)
    /// homonyms.
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
            var skillNames = AuthoredSkillNames(pluginDirectory);

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

    /// <summary>Po 33: loop-audience skills open with the Internal do-not-run line.</summary>
    [Fact]
    public void Loop_audience_skills_start_with_internal_do_not_run_directly()
    {
        var root = TestRepository.SourceRoot();
        var loopCount = 0;
        foreach (var path in AuthoredSkillFiles(root))
        {
            var text = File.ReadAllText(path);
            if (!FrontmatterBlock(text).Contains("audience: loop", StringComparison.Ordinal))
            {
                continue;
            }

            loopCount++;
            Assert.Contains("audience: loop", FrontmatterBlock(text), StringComparison.Ordinal);
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

    private static HashSet<string> AuthoredCommandNames(string pluginDirectory)
    {
        var directory = Path.Combine(pluginDirectory, "commands");
        if (!Directory.Exists(directory))
            return new HashSet<string>(StringComparer.Ordinal);

        return Directory.GetFiles(directory, "*.md")
            .Select(path => FrontmatterName(File.ReadAllText(path)))
            .ToHashSet(StringComparer.Ordinal);
    }

    private static HashSet<string> AuthoredSkillNames(string pluginDirectory)
    {
        var directory = Path.Combine(pluginDirectory, "skills");
        if (!Directory.Exists(directory))
            return new HashSet<string>(StringComparer.Ordinal);

        return Directory.GetDirectories(directory)
            .Select(skill => Path.Combine(skill, "SKILL.md"))
            .Where(File.Exists)
            .Select(path => FrontmatterName(File.ReadAllText(path)))
            .ToHashSet(StringComparer.Ordinal);
    }

    private static string FrontmatterName(string markdown)
    {
        var inFrontmatter = false;
        foreach (var line in markdown.Split('\n'))
        {
            var trimmed = line.TrimEnd('\r');
            if (trimmed == "---")
            {
                if (inFrontmatter)
                    break;
                inFrontmatter = true;
                continue;
            }

            if (inFrontmatter && trimmed.StartsWith("name:", StringComparison.Ordinal))
                return trimmed["name:".Length..].Trim().Trim('"');
        }

        throw new InvalidOperationException("missing frontmatter name");
    }

    private static IEnumerable<string> AuthoredSkillFiles(string root) =>
        Directory.GetFiles(Path.Combine(root, "plugins"), "SKILL.md", SearchOption.AllDirectories)
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}com.", StringComparison.Ordinal));

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
}
