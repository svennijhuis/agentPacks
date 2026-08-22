using System.Text;
using System.Text.Json.Nodes;
using AgentPacks.Cli.Loading;

namespace AgentPacks.Cli.Generation;

/// <summary>
/// Builds every client tree from the neutral source: the four hook dialects, the per-client agent
/// and command formats, the rule translations, and the manifests that point Claude and Codex away
/// from the plugin root that Cursor owns.
/// </summary>
internal sealed class ClientTreeGenerator(RepositoryContext context)
{
    public IReadOnlyList<GeneratedFile> Generate(IReadOnlyList<PluginPackage> plugins)
    {
        var files = new List<GeneratedFile>();

        foreach (var plugin in plugins.Where(p => p.Manifest is not null))
        {
            GeneratePlugin(plugin, files);
        }

        return files;
    }

    private void GeneratePlugin(PluginPackage plugin, List<GeneratedFile> files)
    {
        var root = Path.Combine("plugins", plugin.DirectoryName);

        void Add(string pluginRelative, string text, bool executable = false) =>
            files.Add(new GeneratedFile(
                Path.Combine(root, pluginRelative.Replace('/', Path.DirectorySeparatorChar)),
                text,
                executable));

        void AddJson(string pluginRelative, JsonNode content) =>
            files.Add(GeneratedFile.FromJson(
                Path.Combine(root, pluginRelative.Replace('/', Path.DirectorySeparatorChar)),
                content));

        // Claude is handled inside GenerateClaude: its hooks file also carries the generated rules
        // hook, and emitting the same path twice would write it twice and diff it against itself.
        foreach (var profile in ClientProfile.All.Where(p => p.Client != Client.Claude))
        {
            if (HookGenerator.Build(plugin, profile) is { } hooks)
            {
                AddJson(profile.PluginRelative("hooks/hooks.json"), hooks);
            }
        }

        GenerateClaude(plugin, Add, AddJson);
        GenerateCursor(plugin, AddJson);
        GenerateCodex(plugin, Add, AddJson);
        GenerateCopilot(plugin, Add);
        GenerateShims(plugin, Add);
    }

    // ---------------------------------------------------------------- Claude

    /// <summary>
    /// Claude reads agents/ and commands/ as Markdown with its own frontmatter keys, and has no
    /// rules concept at all — always-on rules become a SessionStart hook that prints them as
    /// context, with the text baked into the generated script so the hook reads no files at runtime.
    /// </summary>
    private void GenerateClaude(
        PluginPackage plugin,
        Action<string, string, bool> add,
        Action<string, JsonNode> addJson)
    {
        var profile = ClientProfile.Claude;

        foreach (var agent in plugin.Agents)
        {
            var frontmatter = new List<KeyValuePair<string, string>>
            {
                new("name", ComponentWriter.Yaml(agent.Name)),
                new("description", ComponentWriter.Yaml(agent.Description)),
                new("model", ComponentWriter.Yaml(agent.Frontmatter?.Scalar("model") ?? "inherit"))
            };

            var tools = ComponentWriter.Sequence(agent, "tools");

            if (tools.Count > 0)
            {
                frontmatter.Add(new("tools", ComponentWriter.YamlList(ClaudeTools(tools))));
            }

            add(
                profile.PluginRelative($"agents/{agent.Name}.md"),
                ComponentWriter.Markdown(frontmatter, agent.Body),
                false);
        }

        foreach (var command in plugin.Commands)
        {
            add(
                profile.PluginRelative($"commands/{command.Name}.md"),
                ComponentWriter.Markdown(
                    [new("description", ComponentWriter.Yaml(command.Description))],
                    command.Body),
                false);
        }

        var always = AlwaysApplyRules(plugin);

        if (always.Count == 0)
        {
            if (HookGenerator.Build(plugin, profile) is { } authored)
            {
                addJson(profile.PluginRelative("hooks/hooks.json"), authored);
            }

            return;
        }

        foreach (var scoped in plugin.Rules.Except(always))
        {
            context.Diagnostics.Warning(
                context.Relative(scoped.FilePath),
                "is scoped with 'globs'. Claude has no path-scoped rule concept, so this rule is " +
                "not generated for Claude. Cursor and Copilot still receive it.");
        }

        var text = string.Join("\n\n", always.Select(rule => rule.Body.TrimEnd('\n')));

        add(profile.PluginRelative("scripts/rules-context.sh"), RulesScriptPosix(text), true);
        add(profile.PluginRelative("scripts/rules-context.ps1"), RulesScriptPowerShell(text), false);
        add(profile.PluginRelative("scripts/rules-context.cmd"), ShimCommand("rules-context"), false);

        addJson(profile.PluginRelative("hooks/hooks.json"), ClaudeHooksWithRules(plugin, profile));
    }

    /// <summary>
    /// Claude names its tools in PascalCase. The neutral format is lowercase because that is what
    /// Cursor and Codex use, so the well-known ones are mapped and anything else passes through.
    /// </summary>
    private static IEnumerable<string> ClaudeTools(IEnumerable<string> tools) =>
        tools.Select(tool => tool switch
        {
            "read" => "Read",
            "write" => "Write",
            "edit" => "Edit",
            "grep" => "Grep",
            "glob" => "Glob",
            "bash" => "Bash",
            "web" => "WebFetch",
            _ => tool
        });

    /// <summary>Merges the generated rules hook into whatever the author declared.</summary>
    private static JsonObject ClaudeHooksWithRules(PluginPackage plugin, ClientProfile profile)
    {
        var document = HookGenerator.Build(plugin, profile) ?? new JsonObject { ["hooks"] = new JsonObject() };
        var hooks = (JsonObject)document["hooks"]!;

        var entry = new JsonObject
        {
            ["hooks"] = new JsonArray
            {
                new JsonObject
                {
                    ["type"] = "command",
                    ["command"] = $"\"{profile.PluginRootToken}/{profile.Directory}/scripts/rules-context\""
                }
            }
        };

        if (hooks["SessionStart"] is JsonArray existing)
        {
            existing.Add(entry);
        }
        else
        {
            hooks["SessionStart"] = new JsonArray { entry };
        }

        return document;
    }

    private static string RulesScriptPosix(string text) =>
        """
        #!/usr/bin/env bash
        # Generated from the plugin's alwaysApply rules. Claude has no rules component, so the rules
        # are printed at SessionStart as additional context. Editing this file is pointless: it is
        # regenerated from rules/*.mdc.
        set -euo pipefail

        cat <<'AGENTPACKS_RULES'
        """ + "\n" + text + "\nAGENTPACKS_RULES\n";

    private static string RulesScriptPowerShell(string text) =>
        """
        # Generated from the plugin's alwaysApply rules. Claude has no rules component, so the rules
        # are printed at SessionStart as additional context. Editing this file is pointless: it is
        # regenerated from rules/*.mdc.
        $ErrorActionPreference = 'Stop'

        Write-Output @'
        """ + "\n" + text + "\n'@\n";

    // ---------------------------------------------------------------- Cursor

    /// <summary>
    /// Cursor is the one client that consumes the authored root directly: rules/*.mdc, agents/*.md
    /// and commands/*.md are already its dialect. It only needs the manifest that turns the
    /// directory from an Agent Plugin into a Cursor plugin, plus its own hooks dialect.
    /// </summary>
    private static void GenerateCursor(PluginPackage plugin, Action<string, JsonNode> addJson)
    {
        var manifest = plugin.Manifest!;

        var cursor = new JsonObject
        {
            ["name"] = plugin.Name ?? plugin.DirectoryName
        };

        foreach (var field in (string[])["version", "description", "author", "license", "keywords"])
        {
            if (manifest[field] is { } value)
            {
                cursor[field] = value.DeepClone();
            }
        }

        addJson(".cursor-plugin/plugin.json", cursor);
    }

    // ---------------------------------------------------------------- Codex

    /// <summary>
    /// Codex reads its manifest from .codex-plugin/, which lets it be pointed at a hooks file
    /// outside the root that Cursor owns. It cannot load subagents from a plugin at all, so the
    /// TOML agents are generated for a documented manual copy rather than pretending otherwise.
    /// </summary>
    private static void GenerateCodex(
        PluginPackage plugin,
        Action<string, string, bool> add,
        Action<string, JsonNode> addJson)
    {
        var profile = ClientProfile.Codex;
        var manifest = plugin.Manifest!;

        var codex = new JsonObject
        {
            ["name"] = plugin.Name ?? plugin.DirectoryName
        };

        foreach (var field in (string[])["version", "description"])
        {
            if (manifest[field] is { } value)
            {
                codex[field] = value.DeepClone();
            }
        }

        if (plugin.HasSkillsDirectory)
        {
            codex["skills"] = "./skills/";
        }

        if (plugin.Hooks is not null)
        {
            codex["hooks"] = $"./{profile.Directory}/hooks/hooks.json";
        }

        addJson(".codex-plugin/plugin.json", codex);

        foreach (var agent in plugin.Agents)
        {
            add(
                profile.PluginRelative($"agents/{agent.Name}.toml"),
                ComponentWriter.Toml(
                    [
                        new("name", agent.Name),
                        new("description", agent.Description)
                    ],
                    "developer_instructions",
                    agent.Body),
                false);
        }

        var always = AlwaysApplyRules(plugin);

        if (always.Count == 0)
        {
            return;
        }

        var builder = new StringBuilder();

        builder.Append("<!-- Generated from rules/*.mdc. Codex reads AGENTS.md from the workspace, not\n")
            .Append("     from a plugin, so copy this file into the repository that needs it. -->\n\n");

        foreach (var rule in always)
        {
            builder.Append(rule.Body.TrimEnd('\n')).Append("\n\n");
        }

        add(profile.PluginRelative("AGENTS.md"), builder.ToString().TrimEnd('\n') + "\n", false);
    }

    // ---------------------------------------------------------------- Copilot

    /// <summary>
    /// Copilot takes its client-specific components from its reverse-domain namespace: agents carry
    /// the .agent.md extension, and rules become instruction files scoped by applyTo.
    /// </summary>
    private static void GenerateCopilot(PluginPackage plugin, Action<string, string, bool> add)
    {
        var profile = ClientProfile.Copilot;

        foreach (var agent in plugin.Agents)
        {
            var frontmatter = new List<KeyValuePair<string, string>>
            {
                new("name", ComponentWriter.Yaml(agent.Name)),
                new("description", ComponentWriter.Yaml(agent.Description))
            };

            var tools = ComponentWriter.Sequence(agent, "tools");

            if (tools.Count > 0)
            {
                frontmatter.Add(new("tools", ComponentWriter.YamlList(tools)));
            }

            add(
                profile.PluginRelative($"agents/{agent.Name}.agent.md"),
                ComponentWriter.Markdown(frontmatter, agent.Body),
                false);
        }

        foreach (var command in plugin.Commands)
        {
            add(
                profile.PluginRelative($"commands/{command.Name}.md"),
                ComponentWriter.Markdown(
                    [new("description", ComponentWriter.Yaml(command.Description))],
                    command.Body),
                false);
        }

        foreach (var rule in plugin.Rules)
        {
            var globs = ComponentWriter.Sequence(rule, "globs");
            var applyTo = globs.Count > 0 ? string.Join(",", globs) : "**";

            add(
                profile.PluginRelative($"instructions/{rule.FileName}.instructions.md"),
                ComponentWriter.Markdown(
                    [
                        new("description", ComponentWriter.Yaml(rule.Description)),
                        new("applyTo", ComponentWriter.Yaml(applyTo))
                    ],
                    rule.Body),
                false);
        }
    }

    // ---------------------------------------------------------------- Shared

    /// <summary>
    /// The Windows half of an authored script pair. Claude, Cursor and Copilot have no per-OS hook
    /// field, so the emitted command is extensionless and cmd.exe resolves this shim via PATHEXT.
    /// </summary>
    private static void GenerateShims(PluginPackage plugin, Action<string, string, bool> add)
    {
        foreach (var script in plugin.Scripts.Where(s => s.IsComplete))
        {
            add($"scripts/{script.Name}.cmd", ShimCommand(script.Name), false);
        }
    }

    // Line endings stay LF: cmd.exe runs an LF batch file, and generated output is byte-compared
    // in CI, where a CRLF file written on Windows would report drift against the Linux runner.
    private static string ShimCommand(string name) =>
        "@echo off\n" +
        $"powershell -NoProfile -ExecutionPolicy Bypass -File \"%~dp0{name}.ps1\" %*\n";

    private static List<MarkdownComponent> AlwaysApplyRules(PluginPackage plugin) =>
        plugin.Rules.Where(rule => ComponentWriter.Flag(rule, "alwaysApply")).ToList();
}
