using Company.AI.Tooling.Io;
using Company.AI.Tooling.Loading;

namespace Company.AI.Tooling.Validation;

/// <summary>
/// Agent Skills publishes no JSON Schema, so its normative frontmatter table is transcribed here.
/// Agents reuse the symmetric checks, reported as company convention rather than conformance.
/// </summary>
internal sealed class SkillValidator(RepositoryContext context)
{
    public void Validate(IReadOnlyList<PluginPackage> plugins)
    {
        foreach (var plugin in plugins)
        {
            ValidateSkills(plugin);
            ValidateAgents(plugin);
        }
    }

    private void ValidateSkills(PluginPackage plugin)
    {
        var seen = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var skill in plugin.Skills)
        {
            if (skill.Frontmatter is not { } frontmatter)
            {
                continue;
            }

            var relative = context.Relative(skill.SkillFilePath);
            var name = ValidateName(frontmatter, relative, skill.DirectoryName);

            ValidateDescription(frontmatter, relative);
            ValidateCompatibility(frontmatter, relative);
            ValidateMetadata(frontmatter, relative);
            ValidateAllowedTools(frontmatter, relative);
            ValidateBody(frontmatter, relative);

            if (name is null)
            {
                continue;
            }

            if (seen.TryGetValue(name, out var first))
            {
                context.Diagnostics.SpecFatal(relative, $"skill name '{name}' is already used by {first}.");
            }
            else
            {
                seen[name] = relative;
            }
        }
    }

    private string? ValidateName(Frontmatter frontmatter, string relative, string directoryName)
    {
        var name = frontmatter.Scalar("name");

        if (string.IsNullOrWhiteSpace(name))
        {
            context.Diagnostics.SpecFatal(relative, "frontmatter must define a non-empty 'name'.");
            return null;
        }

        if (name.Length > AgentPluginSpec.SkillNameMaxLength)
        {
            context.Diagnostics.SpecFatal(
                relative,
                $"frontmatter 'name' is {name.Length} characters; the maximum is {AgentPluginSpec.SkillNameMaxLength}.");
        }

        if (!AgentPluginSpec.SkillName.IsMatch(name))
        {
            context.Diagnostics.SpecFatal(
                relative,
                $"frontmatter 'name' ('{name}') must be lowercase letters, digits and single hyphens, " +
                "without a leading or trailing hyphen. Unlike plugin names, skill names may not contain periods.");
        }

        if (!string.Equals(name, directoryName, StringComparison.Ordinal))
        {
            context.Diagnostics.SpecFatal(
                relative,
                $"frontmatter 'name' is '{name}' but the skill directory is '{directoryName}'. They must match.");
        }

        return name;
    }

    private void ValidateDescription(Frontmatter frontmatter, string relative)
    {
        var description = frontmatter.Scalar("description");

        if (string.IsNullOrWhiteSpace(description))
        {
            context.Diagnostics.SpecFatal(relative, "frontmatter must define a non-empty 'description'.");
            return;
        }

        if (description.Length > AgentPluginSpec.SkillDescriptionMaxLength)
        {
            context.Diagnostics.SpecFatal(
                relative,
                $"frontmatter 'description' is {description.Length} characters; " +
                $"the maximum is {AgentPluginSpec.SkillDescriptionMaxLength}.");
        }
    }

    private void ValidateCompatibility(Frontmatter frontmatter, string relative)
    {
        if (!frontmatter.Has("compatibility"))
        {
            return;
        }

        var value = frontmatter.Scalar("compatibility");

        if (string.IsNullOrWhiteSpace(value))
        {
            context.Diagnostics.SpecFatal(relative, "frontmatter 'compatibility' must be a non-empty string when present.");
            return;
        }

        if (value.Length > AgentPluginSpec.SkillCompatibilityMaxLength)
        {
            context.Diagnostics.SpecFatal(
                relative,
                $"frontmatter 'compatibility' is {value.Length} characters; " +
                $"the maximum is {AgentPluginSpec.SkillCompatibilityMaxLength}.");
        }
    }

    private void ValidateMetadata(Frontmatter frontmatter, string relative)
    {
        if (frontmatter.Has("metadata") && !frontmatter.IsStringMap("metadata"))
        {
            context.Diagnostics.SpecFatal(
                relative,
                "frontmatter 'metadata' must be a mapping of string keys to string values.");
        }
    }

    private void ValidateAllowedTools(Frontmatter frontmatter, string relative)
    {
        if (frontmatter.Has("allowed-tools") && frontmatter.Scalar("allowed-tools") is null)
        {
            context.Diagnostics.SpecFatal(
                relative,
                "frontmatter 'allowed-tools' must be a space-separated string.");
        }
    }

    private void ValidateBody(Frontmatter frontmatter, string relative)
    {
        if (string.IsNullOrWhiteSpace(frontmatter.Body))
        {
            context.Diagnostics.Policy(relative, "has no Markdown body. A skill needs instructions to be useful.");
        }
    }

    private void ValidateAgents(PluginPackage plugin)
    {
        var seen = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var agent in plugin.Agents)
        {
            if (agent.Frontmatter is not { } frontmatter)
            {
                continue;
            }

            var relative = context.Relative(agent.FilePath);
            var name = frontmatter.Scalar("name");

            if (string.IsNullOrWhiteSpace(name))
            {
                context.Diagnostics.Policy(relative, "frontmatter must define a non-empty 'name'.");
                continue;
            }

            if (string.IsNullOrWhiteSpace(frontmatter.Scalar("description")))
            {
                context.Diagnostics.Policy(relative, "frontmatter must define a non-empty 'description'.");
            }

            if (string.IsNullOrWhiteSpace(frontmatter.Body))
            {
                context.Diagnostics.Policy(relative, "has no Markdown body.");
            }

            if (seen.TryGetValue(name, out var first))
            {
                context.Diagnostics.Policy(relative, $"agent name '{name}' is already used by {first}.");
            }
            else
            {
                seen[name] = relative;
            }
        }
    }
}
