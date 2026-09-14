using System.Text.Json.Nodes;
using AgentPacks.Cli.Io;
using AgentPacks.Cli.Validation;
using Scriban;
using Scriban.Runtime;
using Scriban.Syntax;

namespace AgentPacks.Cli.Loading;

/// <summary>
/// Renders a language-pack slot skill from its authored <c>SKILL.source.md</c> and the one shared
/// skeleton at <c>shared/templates/slot-skill.sbn</c>. The source carries only what differs per
/// pack (name, description, title, language, an optional intro, and the Markdown body); the
/// contract lines, the loop-audience frontmatter and the <c>Standards in force</c> list (taken from
/// <c>standards.source.json</c>) come from the template, so they exist once.
/// </summary>
internal sealed class SlotSkillRenderer(RepositoryContext context)
{
    public const string SourceFileName = "SKILL.source.md";

    public const string TemplateRelative = "shared/templates/slot-skill.sbn";

    private static readonly IReadOnlySet<string> AllowedKeys =
        new HashSet<string>(StringComparer.Ordinal)
        {
            "name", "description", "title", "language", "intro", "commands"
        };

    private static readonly string[] RequiredKeys = ["name", "description", "title", "language"];

    private Template? _template;

    public string TemplatePath => Path.Combine(context.Root, TemplateRelative.Replace('/', Path.DirectorySeparatorChar));

    /// <summary>
    /// Renders one skill directory. Returns null after recording a diagnostic when the source,
    /// the template or the catalog cannot produce a skill.
    /// </summary>
    public string? Render(string skillDirectory, string sourcePath, StandardsDefinition? standards)
    {
        var relative = context.Relative(sourcePath);
        var directoryName = Path.GetFileName(skillDirectory);

        var slot = LanguagePackContract.Slots
            .FirstOrDefault(s => directoryName.EndsWith("-" + s, StringComparison.Ordinal));

        if (slot is null)
        {
            context.Diagnostics.Policy(
                relative,
                $"{SourceFileName} is only for contracted slot skills " +
                $"(<lang>-{string.Join(", <lang>-", LanguagePackContract.Slots)}). Author SKILL.md directly.");
            return null;
        }

        var template = LoadTemplate();

        if (template is null)
        {
            return null;
        }

        string text;

        try
        {
            text = File.ReadAllText(sourcePath);
        }
        catch (IOException ex)
        {
            context.Diagnostics.SpecFatal(relative, $"could not be read: {ex.Message}");
            return null;
        }

        var frontmatter = Frontmatter.TryParse(text, out var error);

        if (frontmatter is null)
        {
            context.Diagnostics.SpecFatal(relative, error!);
            return null;
        }

        var valid = true;

        foreach (var key in frontmatter.Keys.Where(k => !AllowedKeys.Contains(k)))
        {
            context.Diagnostics.Policy(
                relative,
                $"unknown key '{key}'. The template owns the rest of the frontmatter; a slot source " +
                $"carries only {string.Join(", ", AllowedKeys)}.");
            valid = false;
        }

        foreach (var key in RequiredKeys.Where(k => string.IsNullOrWhiteSpace(frontmatter.Scalar(k))))
        {
            context.Diagnostics.Policy(relative, $"must declare '{key}'.");
            valid = false;
        }

        var name = frontmatter.Scalar("name");

        if (name is not null && name != directoryName)
        {
            context.Diagnostics.SpecFatal(relative, $"name '{name}' must match the directory name '{directoryName}'.");
            valid = false;
        }

        var standardFiles = ConsumedStandards(standards, directoryName);

        if (standardFiles.Count == 0)
        {
            context.Diagnostics.Policy(
                relative,
                $"'{directoryName}' consumes no document in {PluginLoader.StandardsFileName}. A slot skill " +
                "names its standards there; the rendered 'Standards in force' list comes from that entry.");
            valid = false;
        }

        if (!valid)
        {
            return null;
        }

        var references = Path.Combine(skillDirectory, "references");
        var examplesDirectory = Path.Combine(references, "examples");

        var examples = Directory.Exists(examplesDirectory)
            ? Directory.GetFiles(examplesDirectory, "*.md")
                .OrderBy(path => path, StringComparer.Ordinal)
                .Select(path => new ScriptObject
                {
                    ["label"] = Path.GetFileNameWithoutExtension(path),
                    ["path"] = "references/examples/" + Path.GetFileName(path)
                })
                .ToList()
            : [];

        var hasCommands = File.Exists(Path.Combine(references, "commands.md"));
        var commands = frontmatter.Scalar("commands");

        if (hasCommands && string.IsNullOrWhiteSpace(commands))
        {
            context.Diagnostics.Policy(
                relative,
                "must declare 'commands' (the link text for references/commands.md) because that file exists.");
            return null;
        }

        var model = new ScriptObject
        {
            ["name"] = name,
            ["description"] = frontmatter.Scalar("description"),
            ["title"] = frontmatter.Scalar("title"),
            ["language"] = frontmatter.Scalar("language"),
            ["intro"] = NormalizeIntro(frontmatter.Scalar("intro")),
            ["slot"] = slot,
            ["standards"] = new ScriptArray(standardFiles),
            ["examples"] = new ScriptArray(examples),
            ["has_commands"] = hasCommands,
            ["commands"] = commands,
            ["has_checklist"] = File.Exists(Path.Combine(references, "checklist.md")),
            ["body"] = frontmatter.Body.TrimEnd('\n')
        };

        var templateContext = new TemplateContext { StrictVariables = true };
        templateContext.PushGlobal(model);

        try
        {
            return TextFile.Normalize(template.Render(templateContext));
        }
        catch (ScriptRuntimeException ex)
        {
            context.Diagnostics.SpecFatal(TemplateRelative, $"failed to render {relative}: {ex.Message}");
            return null;
        }
    }

    /// <summary>Document ids the catalog maps into this skill, in catalog order, as <c>id.md</c>.</summary>
    private static List<string> ConsumedStandards(StandardsDefinition? standards, string skill)
    {
        if (standards?.Document["consumers"] is not JsonObject consumers ||
            consumers[skill] is not JsonArray ids)
        {
            return [];
        }

        return ids
            .OfType<JsonValue>()
            .Select(id => id.TryGetValue<string>(out var value) ? value : null)
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Select(id => id + ".md")
            .ToList();
    }

    /// <summary>A folded YAML scalar keeps its newlines; the intro is one paragraph.</summary>
    private static string? NormalizeIntro(string? intro)
    {
        if (string.IsNullOrWhiteSpace(intro))
        {
            return null;
        }

        return string.Join(' ', intro.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
    }

    private Template? LoadTemplate()
    {
        if (_template is not null)
        {
            return _template;
        }

        if (!File.Exists(TemplatePath))
        {
            context.Diagnostics.SpecFatal(
                TemplateRelative,
                $"is missing, so no {SourceFileName} can be rendered.");
            return null;
        }

        var parsed = Template.Parse(File.ReadAllText(TemplatePath), TemplateRelative);

        if (parsed.HasErrors)
        {
            foreach (var message in parsed.Messages)
            {
                context.Diagnostics.SpecFatal(TemplateRelative, message.ToString());
            }

            return null;
        }

        _template = parsed;
        return parsed;
    }
}
