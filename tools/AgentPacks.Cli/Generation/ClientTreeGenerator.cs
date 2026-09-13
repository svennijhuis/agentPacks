using System.Text;
using System.Text.Json.Nodes;
using AgentPacks.Cli.Loading;

namespace AgentPacks.Cli.Generation;

/// <summary>
/// Builds every client tree from the neutral source: the four hook dialects, the per-client agent
/// and command formats, rule translations, and the manifests/catalogs that route each provider to
/// its own dialect.
/// </summary>
internal sealed class ClientTreeGenerator(RepositoryContext context, ModelCatalog models)
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

        // Claude and Copilot are handled inside their own methods: their hooks files also carry
        // the generated rules hook, and emitting the same path twice would write it twice and diff
        // it against itself.
        //
        // pack-check and git put Claude-shaped hooks at plugin-root hooks/hooks.json so Claude
        // can auto-discover them (marketplace omits a hooks path). Cursor cannot read that
        // dialect, so those packs write Cursor-shaped hooks under .cursor-plugin/ and point
        // the Cursor manifest at them. Do not write Claude shape into Copilot's tree.
        foreach (var profile in ClientProfile.All
                     .Where(p => p.Client is not (Client.Claude or Client.Copilot)))
        {
            if (HookGenerator.Build(plugin, profile) is { } hooks)
            {
                var relative = profile.Client == Client.Cursor && plugin.ClaudeAutoDiscoversRootHooks
                    ? ClientProfile.RelocatedCursorHooks
                    : profile.PluginRelative("hooks/hooks.json");
                AddJson(relative, hooks);
            }
        }

        GenerateClaude(plugin, Add, AddJson);
        GenerateCursor(plugin, Add, AddJson);
        GenerateCodex(plugin, Add, AddJson);
        GenerateCopilot(plugin, Add, AddJson);
        GenerateShims(plugin, Add);

        // Warned once per rule, not once per client: only Cursor has a path-scoped rule concept,
        // so a rule carrying 'globs' reaches one of the four and silently does nothing on three.
        foreach (var scoped in plugin.Rules.Except(AlwaysApplyRules(plugin)))
        {
            context.Diagnostics.Warning(
                context.Relative(scoped.FilePath),
                "is scoped with 'globs'. Only Cursor has a path-scoped rule concept, so this rule " +
                "is generated for Cursor alone. Claude and Copilot receive always-on rules as a " +
                "SessionStart hook, which cannot carry a path scope, and Codex receives them in " +
                "AGENTS.md. Drop 'globs' to reach every client.");
        }
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
                new("model", ComponentWriter.Yaml(ResolveModel(agent, Client.Claude)))
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

        GenerateRulesContext(plugin, profile, add, addJson);
    }

    /// <summary>
    /// Claude names its tools in PascalCase. The vocabulary is <see cref="NeutralTools"/>, shared
    /// with the validator that rejects anything outside it, so the pass-through below is reached
    /// only by a name validation already refused.
    /// </summary>
    private static IEnumerable<string> ClaudeTools(IEnumerable<string> tools) =>
        tools.Select(tool => NeutralTools.Claude.TryGetValue(tool, out var claude) ? claude : tool);

    /// <summary>
    /// Turns always-on rules into a SessionStart hook that prints them as context, and writes the
    /// client's hooks document. Claude and Copilot both land here: neither has a rules component a
    /// plugin can declare, and a hook that prints the text is the only carrier both accept.
    /// </summary>
    private void GenerateRulesContext(
        PluginPackage plugin,
        ClientProfile profile,
        Action<string, string, bool> add,
        Action<string, JsonNode> addJson)
    {
        var always = AlwaysApplyRules(plugin);

        if (always.Count == 0)
        {
            if (HookGenerator.Build(plugin, profile) is { } authored)
            {
                addJson(profile.PluginRelative("hooks/hooks.json"), authored);
                MaybeWriteClaudeRootHooks(plugin, profile, addJson, authored);
            }

            return;
        }

        var text = string.Join("\n\n", always.Select(rule => rule.Body.TrimEnd('\n')));

        add(profile.PluginRelative("scripts/rules-context.sh"), RulesScriptPosix(text), true);
        add(profile.PluginRelative("scripts/rules-context.ps1"), RulesScriptPowerShell(text), false);
        add(profile.PluginRelative("scripts/rules-context"), Dispatcher("rules-context"), true);
        add(profile.PluginRelative("scripts/rules-context.cmd"), ShimCommand("rules-context"), false);

        var document = HooksWithRules(plugin, profile);
        addJson(profile.PluginRelative("hooks/hooks.json"), document);
        MaybeWriteClaudeRootHooks(plugin, profile, addJson, document);
    }

    /// <summary>
    /// Claude marketplace cannot declare a hooks path, so pack-check and git also emit the
    /// Claude-shaped document at plugin-root hooks/hooks.json. Copilot keeps
    /// <c>com.github.copilot/hooks/hooks.json</c> in its own dialect.
    /// </summary>
    private static void MaybeWriteClaudeRootHooks(
        PluginPackage plugin,
        ClientProfile profile,
        Action<string, JsonNode> addJson,
        JsonObject document)
    {
        if (profile.Client != Client.Claude || !plugin.ClaudeAutoDiscoversRootHooks)
        {
            return;
        }

        addJson("hooks/hooks.json", document);
    }

    /// <summary>Merges the generated rules hook into whatever the author declared.</summary>
    private static JsonObject HooksWithRules(PluginPackage plugin, ClientProfile profile)
    {
        var document = HookGenerator.Build(plugin, profile) ?? HookGenerator.EmptyDocument(profile);
        var hooks = (JsonObject)document["hooks"]!;
        var sessionStart = HookDialect.Event("sessionStart")!.Targets[profile.Client].Event;

        if (hooks[sessionStart] is not JsonArray existing)
        {
            existing = [];
            hooks[sessionStart] = existing;
        }

        HookGenerator.AppendRulesCommand(existing, profile, $"{profile.Directory}/scripts/rules-context");

        return document;
    }

    private static string RulesScriptPosix(string text) =>
        """
        #!/usr/bin/env bash
        # Generated from the plugin's alwaysApply rules. This client has no rules component, so the
        # rules are printed at SessionStart as additional context. Editing this file is pointless:
        # it is regenerated from rules/*.mdc.
        set -euo pipefail

        cat <<'AGENTPACKS_RULES'
        """ + "\n" + text + "\nAGENTPACKS_RULES\n";

    private static string RulesScriptPowerShell(string text) =>
        """
        # Generated from the plugin's alwaysApply rules. This client has no rules component, so the
        # rules are printed at SessionStart as additional context. Editing this file is pointless:
        # it is regenerated from rules/*.mdc.
        $ErrorActionPreference = 'Stop'

        Write-Output @'
        """ + "\n" + text + "\n'@\n";

    // ---------------------------------------------------------------- Cursor

    /// <summary>
    /// Cursor reads rules/*.mdc and commands/*.md from the plugin root. Authored agents stay the
    /// portable source. Generation writes remapped Cursor ids into <c>.cursor-plugin/agents/</c>
    /// so Cursor never receives a Claude alias, and Copilot/Codex are not the only clients that
    /// emit <c>model</c>.
    /// </summary>
    private void GenerateCursor(
        PluginPackage plugin,
        Action<string, string, bool> add,
        Action<string, JsonNode> addJson)
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

        if (plugin.ClaudeAutoDiscoversRootHooks &&
            HookGenerator.Build(plugin, ClientProfile.Cursor) is not null)
        {
            cursor["hooks"] = $"./{ClientProfile.RelocatedCursorHooks}";
        }

        addJson(".cursor-plugin/plugin.json", cursor);

        foreach (var agent in plugin.Agents)
        {
            var frontmatter = new List<KeyValuePair<string, string>>
            {
                new("name", ComponentWriter.Yaml(agent.Name)),
                new("description", ComponentWriter.Yaml(agent.Description)),
                new("model", ComponentWriter.Yaml(ResolveModel(agent, Client.Cursor)))
            };

            if (ComponentWriter.Flag(agent, "readonly"))
            {
                frontmatter.Add(new("readonly", "true"));
            }

            var tools = ComponentWriter.Sequence(agent, "tools");

            if (tools.Count > 0)
            {
                frontmatter.Add(new("tools", ComponentWriter.YamlList(tools)));
            }

            add(
                $".cursor-plugin/agents/{agent.Name}.md",
                ComponentWriter.Markdown(frontmatter, agent.Body),
                false);
        }
    }

    // ---------------------------------------------------------------- Codex

    /// <summary>
    /// Codex reads its manifest from .codex-plugin/, which lets it be pointed at a hooks file
    /// outside the root that Cursor owns. It cannot load subagents from a plugin at all, so the
    /// TOML agents are generated for a documented manual copy rather than pretending otherwise.
    /// </summary>
    private void GenerateCodex(
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

        foreach (var field in (string[])
                 ["version", "description", "author", "homepage", "repository", "license", "keywords"])
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

        // Keyed on what the generator actually produces, not on the source file existing: an event
        // declared with an empty entry array yields no hooks.json, and a manifest pointing at a
        // file that was never written fails the plugin at install time.
        var hasHooks = HookGenerator.Build(plugin, profile) is not null;

        if (hasHooks)
        {
            codex["hooks"] = $"./{profile.Directory}/hooks/hooks.json";
        }

        var hasMcp = plugin.Mcp?["mcpServers"] is JsonObject servers && servers.Count > 0;

        if (hasMcp)
        {
            codex["mcpServers"] = "./.mcp.json";
        }

        var description = manifest["description"]?.GetValue<string>() ?? plugin.DirectoryName;
        var developer = manifest["author"]?["name"]?.GetValue<string>() ?? "agentPacks Maintainers";
        var displayName = string.Join(' ', (plugin.Name ?? plugin.DirectoryName)
            .Split('-', StringSplitOptions.RemoveEmptyEntries)
            .Select(part => char.ToUpperInvariant(part[0]) + part[1..]));

        var capabilities = new JsonArray();

        if (plugin.HasSkillsDirectory)
        {
            capabilities.Add("Skills");
        }

        if (hasHooks)
        {
            capabilities.Add("Hooks");
        }

        if (hasMcp)
        {
            capabilities.Add("MCP");
        }

        codex["interface"] = new JsonObject
        {
            ["displayName"] = displayName,
            ["shortDescription"] = description,
            ["longDescription"] = description,
            ["developerName"] = developer,
            ["category"] = "Productivity",
            ["capabilities"] = capabilities,
            ["defaultPrompt"] = new JsonArray($"Use the {displayName} plugin when it is relevant to this task.")
        };

        addJson(".codex-plugin/plugin.json", codex);

        foreach (var agent in plugin.Agents)
        {
            add(
                profile.PluginRelative($"agents/{agent.Name}.toml"),
                ComponentWriter.Toml(
                    [
                        new("name", agent.Name),
                        new("description", agent.Description),
                        new("model", ResolveModel(agent, Client.Codex))
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
    /// Copilot takes its client-specific components from its reverse-domain namespace, and agents
    /// carry the .agent.md extension. Rules go the same way they do for Claude: Copilot's plugin
    /// schema declares agents, skills, commands, hooks, mcpServers and lspServers and nothing for
    /// instructions, so an instructions file inside the plugin is a file nothing ever loads.
    /// Copilot hides <c>/plugin:command</c> when the two names match, so Squad's factory is
    /// rematerialized as <c>run</c> (<c>/squad:run</c>). Claude, Cursor and Codex keep
    /// <c>squad</c>. <c>pack-check</c> is a setup slash and is not remapped.
    /// </summary>
    private void GenerateCopilot(
        PluginPackage plugin,
        Action<string, string, bool> add,
        Action<string, JsonNode> addJson)
    {
        var profile = ClientProfile.Copilot;

        foreach (var agent in plugin.Agents)
        {
            var frontmatter = new List<KeyValuePair<string, string>>
            {
                new("name", ComponentWriter.Yaml(agent.Name)),
                new("description", ComponentWriter.Yaml(agent.Description)),
                new("model", ComponentWriter.Yaml(ResolveModel(agent, Client.Copilot)))
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
            var name = CopilotCommandName(plugin, command);

            add(
                profile.PluginRelative($"commands/{name}.md"),
                ComponentWriter.Markdown(
                    [
                        new("name", ComponentWriter.Yaml(name)),
                        new("description", ComponentWriter.Yaml(command.Description))
                    ],
                    command.Body),
                false);
        }

        GenerateRulesContext(plugin, profile, add, addJson);
    }

    /// <summary>
    /// Copilot CLI drops the factory slash when command name equals plugin name. Only Squad's
    /// colliding factory is rewritten; a setup command such as <c>pack-check</c> stays.
    /// </summary>
    private static string CopilotCommandName(PluginPackage plugin, MarkdownComponent command)
    {
        var pluginName = plugin.Name ?? plugin.DirectoryName;

        if (string.Equals(pluginName, "squad", StringComparison.Ordinal) &&
            string.Equals(command.Name, "squad", StringComparison.Ordinal))
        {
            return "run";
        }

        return command.Name;
    }

    // ---------------------------------------------------------------- Shared

    /// <summary>
    /// The two halves that make one extensionless hook command work on both platforms. Claude,
    /// Cursor and Copilot have no per-OS hook field, so the emitted command names scripts/&lt;name&gt;
    /// with no extension: a POSIX shell runs the dispatcher, and cmd.exe — which never executes an
    /// extensionless file — appends PATHEXT and finds the .cmd shim instead.
    /// </summary>
    private static void GenerateShims(PluginPackage plugin, Action<string, string, bool> add)
    {
        foreach (var script in plugin.Scripts.Where(s => s.IsComplete))
        {
            add($"scripts/{script.Name}", Dispatcher(script.Name), true);
            add($"scripts/{script.Name}.cmd", ShimCommand(script.Name), false);
        }
    }

    // The POSIX half of the extensionless command. It only forwards: the authored .sh keeps its
    // extension so the pair stays obvious in the tree and the validator can insist on both halves.
    private static string Dispatcher(string name) =>
        "#!/usr/bin/env bash\n" +
        "# Generated. The hook command is extensionless so one string works on POSIX and on Windows;\n" +
        $"# this forwards to the authored {name}.sh. Editing it is pointless: it is regenerated.\n" +
        "set -euo pipefail\n" +
        $"exec \"$(dirname \"$0\")/{name}.sh\" \"$@\"\n";

    // Line endings stay LF: cmd.exe runs an LF batch file, and generated output is byte-compared
    // in CI, where a CRLF file written on Windows would report drift against the Linux runner.
    private static string ShimCommand(string name) =>
        "@echo off\n" +
        $"powershell -NoProfile -ExecutionPolicy Bypass -File \"%~dp0{name}.ps1\" %*\n" +
        "exit /b %ERRORLEVEL%\n";

    private static List<MarkdownComponent> AlwaysApplyRules(PluginPackage plugin) =>
        plugin.Rules.Where(rule => ComponentWriter.Flag(rule, "alwaysApply")).ToList();

    /// <summary>
    /// Maps the authored portable tier to the identifier this client accepts, including Codex
    /// <c>model =</c> in generated agent TOML.
    /// </summary>
    private string ResolveModel(MarkdownComponent agent, Client client)
    {
        var authored = agent.Frontmatter?.Scalar("model") ?? ModelCatalog.DefaultTier;
        return models.Resolve(authored, client);
    }
}
