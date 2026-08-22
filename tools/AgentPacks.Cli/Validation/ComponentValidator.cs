using AgentPacks.Cli.Io;
using AgentPacks.Cli.Loading;
using YamlDotNet.RepresentationModel;

namespace AgentPacks.Cli.Validation;

/// <summary>
/// Validates the neutral agents, commands and rules. None of these has a published specification —
/// the Agent Plugins spec leaves all three out as too client-specific — so the rules here are the
/// intersection of what Claude, Cursor, Codex and Copilot accept, plus the repository's own
/// requirement that a name matches its filename so the generated files are predictable.
/// </summary>
internal sealed class ComponentValidator(RepositoryContext context)
{
    /// <summary>Models a neutral agent may request. 'inherit' is the portable default.</summary>
    private static readonly IReadOnlySet<string> AllowedModels =
        new HashSet<string>(StringComparer.Ordinal) { "inherit", "opus", "sonnet", "haiku" };

    private static readonly IReadOnlySet<string> AgentKeys =
        new HashSet<string>(StringComparer.Ordinal) { "name", "description", "model", "tools", "readonly" };

    private static readonly IReadOnlySet<string> CommandKeys =
        new HashSet<string>(StringComparer.Ordinal) { "name", "description" };

    private static readonly IReadOnlySet<string> RuleKeys =
        new HashSet<string>(StringComparer.Ordinal) { "name", "description", "alwaysApply", "globs" };

    public void Validate(IReadOnlyList<PluginPackage> plugins)
    {
        foreach (var plugin in plugins)
        {
            ValidateGroup(plugin.Agents, AgentKeys, ValidateAgent);
            ValidateGroup(plugin.Commands, CommandKeys, (_, _) => { });
            ValidateGroup(plugin.Rules, RuleKeys, ValidateRule);
        }
    }

    private void ValidateGroup(
        IReadOnlyList<MarkdownComponent> components,
        IReadOnlySet<string> allowedKeys,
        Action<MarkdownComponent, string> validateSpecific)
    {
        var seen = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var component in components)
        {
            if (component.Frontmatter is null)
            {
                continue;
            }

            var relative = context.Relative(component.FilePath);

            ValidateName(component, relative);
            ValidateDescription(component, relative);
            ValidateUnknownKeys(component, allowedKeys, relative);
            ValidateBody(component, relative);
            validateSpecific(component, relative);

            if (seen.TryGetValue(component.Name, out var first))
            {
                context.Diagnostics.SpecFatal(
                    relative, $"{component.Kind} name '{component.Name}' is already used by {first}.");
            }
            else
            {
                seen[component.Name] = relative;
            }
        }
    }

    /// <summary>
    /// The name must be present and match the filename. Clients disagree on which one wins when
    /// they differ — Cursor derives the name from the filename, Claude reads the frontmatter — so
    /// a mismatch means the same component answers to two different names depending on the client.
    /// </summary>
    private void ValidateName(MarkdownComponent component, string relative)
    {
        var name = component.Frontmatter!.Scalar("name");

        if (string.IsNullOrWhiteSpace(name))
        {
            // A rule carries no name in Cursor's .mdc format — the filename is the identity, and
            // adding a key Cursor does not read would make the authored file non-standard for the
            // one client that consumes it directly.
            if (component.Kind != "rule")
            {
                context.Diagnostics.SpecFatal(relative, $"{component.Kind} must define 'name'.");
            }

            return;
        }

        if (!AgentPluginSpec.SkillName.IsMatch(name))
        {
            context.Diagnostics.SpecFatal(
                relative,
                $"{component.Kind} name '{name}' must be kebab-case: lowercase letters, digits and " +
                "single hyphens.");
            return;
        }

        if (!string.Equals(name, component.FileName, StringComparison.Ordinal))
        {
            context.Diagnostics.SpecFatal(
                relative,
                $"{component.Kind} name '{name}' does not match the filename '{component.FileName}'. " +
                "Clients disagree on which one wins, so they must be identical.");
        }
    }

    private void ValidateDescription(MarkdownComponent component, string relative)
    {
        var description = component.Frontmatter!.Scalar("description");

        if (string.IsNullOrWhiteSpace(description))
        {
            context.Diagnostics.SpecFatal(
                relative,
                $"{component.Kind} must define 'description'. It is what a client uses to decide " +
                "whether to load this component at all.");
            return;
        }

        if (description.Length > AgentPluginSpec.SkillDescriptionMaxLength)
        {
            context.Diagnostics.SpecFatal(
                relative,
                $"{component.Kind} description is {description.Length} characters, over the " +
                $"{AgentPluginSpec.SkillDescriptionMaxLength} limit.");
        }
    }

    /// <summary>
    /// Unknown keys are rejected rather than passed through. A generator that does not recognise a
    /// key drops it, so an unknown key silently does nothing on three clients out of four.
    /// </summary>
    private void ValidateUnknownKeys(MarkdownComponent component, IReadOnlySet<string> allowed, string relative)
    {
        foreach (var key in component.Frontmatter!.Keys.Where(k => !allowed.Contains(k)))
        {
            context.Diagnostics.Policy(
                relative,
                $"frontmatter key '{key}' is not part of the neutral {component.Kind} format, so it " +
                $"would be dropped when generating. Allowed keys: {string.Join(", ", allowed.Order(StringComparer.Ordinal))}.");
        }
    }

    private void ValidateBody(MarkdownComponent component, string relative)
    {
        if (string.IsNullOrWhiteSpace(component.Body))
        {
            context.Diagnostics.SpecFatal(
                relative, $"{component.Kind} has no body. The body is the instruction the client runs.");
        }
    }

    private void ValidateAgent(MarkdownComponent agent, string relative)
    {
        var model = agent.Frontmatter!.Scalar("model");

        if (model is not null && !AllowedModels.Contains(model))
        {
            context.Diagnostics.Policy(
                relative,
                $"agent model '{model}' is not portable. Use one of: " +
                $"{string.Join(", ", AllowedModels.Order(StringComparer.Ordinal))}.");
        }

        if (agent.Frontmatter.Has("tools") && agent.Frontmatter.Node("tools") is not YamlSequenceNode)
        {
            context.Diagnostics.SpecFatal(relative, "agent 'tools' must be a list.");
        }

        var readOnly = agent.Frontmatter.Scalar("readonly");

        if (readOnly is not null && readOnly is not ("true" or "false"))
        {
            context.Diagnostics.SpecFatal(relative, "agent 'readonly' must be true or false.");
        }
    }

    /// <summary>
    /// A rule is either always on or scoped to paths. Declaring both is ambiguous, and declaring
    /// neither produces a rule no client ever applies.
    /// </summary>
    private void ValidateRule(MarkdownComponent rule, string relative)
    {
        var alwaysApply = rule.Frontmatter!.Scalar("alwaysApply") == "true";
        var globs = rule.Frontmatter.Node("globs") as YamlSequenceNode;
        var hasGlobs = globs is { Children.Count: > 0 };

        if (alwaysApply && hasGlobs)
        {
            context.Diagnostics.SpecFatal(
                relative,
                "rule declares both 'alwaysApply: true' and 'globs'. Pick one: always on, or scoped " +
                "to paths.");
        }

        if (!alwaysApply && !hasGlobs)
        {
            context.Diagnostics.SpecFatal(
                relative,
                "rule declares neither 'alwaysApply: true' nor 'globs', so no client would ever " +
                "apply it.");
        }

        if (rule.Frontmatter.Scalar("alwaysApply") is { } value and not ("true" or "false"))
        {
            context.Diagnostics.SpecFatal(relative, $"rule 'alwaysApply' must be true or false, not '{value}'.");
        }
    }
}
