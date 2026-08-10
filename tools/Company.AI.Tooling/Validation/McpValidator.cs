using System.Text.Json.Nodes;
using Company.AI.Tooling.Io;
using Company.AI.Tooling.Loading;

namespace Company.AI.Tooling.Validation;

/// <summary>
/// MCP semantics the schema documents as out of scope: it types <c>command</c> as any non-empty
/// string and says of <c>cwd</c> that "filesystem containment is validated separately" and of
/// <c>url</c> that "URL semantics are defined by the Agent Plugins specification".
/// </summary>
internal sealed class McpValidator(RepositoryContext context)
{
    public void Validate(IReadOnlyList<PluginPackage> plugins)
    {
        foreach (var plugin in plugins)
        {
            if (plugin.Mcp?["mcpServers"] is not JsonObject servers || plugin.McpPath is null)
            {
                continue;
            }

            var relative = context.Relative(plugin.McpPath);

            foreach (var entry in servers.OrderBy(s => s.Key, StringComparer.Ordinal))
            {
                if (entry.Value is not JsonObject server)
                {
                    continue;
                }

                switch (server["type"]?.GetValue<string>())
                {
                    case "stdio":
                        ValidateStdio(entry.Key, server, relative);
                        break;

                    case "streamable-http":
                        ValidateHttp(entry.Key, server, relative);
                        break;

                    case "sse":
                        ValidateHttp(entry.Key, server, relative);
                        context.Diagnostics.Warning(
                            relative,
                            $"server '{entry.Key}' uses the deprecated HTTP+SSE transport, which clients " +
                            "support only optionally. Prefer 'streamable-http'.");
                        break;
                }
            }
        }
    }

    private void ValidateStdio(string name, JsonObject server, string relative)
    {
        if (server["command"]?.GetValue<string>() is { } command)
        {
            ValidateCommand(name, command, relative);
        }

        if (server["cwd"]?.GetValue<string>() is { } cwd && PathUtils.HasTraversalSegment(cwd))
        {
            context.Diagnostics.SpecFatal(
                relative,
                $"server '{name}' cwd '{cwd}' escapes its root with '..'. " +
                "An explicit cwd must stay inside the plugin root or the plugin data directory.");
        }

        if (server["env"] is not JsonObject env)
        {
            return;
        }

        foreach (var variable in env.OrderBy(e => e.Key, StringComparer.Ordinal))
        {
            if (AgentPluginSpec.LikelySecret.IsMatch(variable.Key))
            {
                context.Diagnostics.SpecFatal(
                    relative,
                    $"server '{name}' env '{variable.Key}' looks credential-related. " +
                    "Plugin files are literal, visible package data; authentication is client-managed.");
            }
        }
    }

    /// <summary>
    /// The command is one executable token: a bare name resolved by platform search rules, or a
    /// plugin-relative path beginning with "./". It is not a shell command and gets no expansion.
    /// </summary>
    private void ValidateCommand(string name, string command, string relative)
    {
        if (command.StartsWith("./", StringComparison.Ordinal))
        {
            if (PathUtils.HasTraversalSegment(command))
            {
                context.Diagnostics.SpecFatal(
                    relative,
                    $"server '{name}' command '{command}' escapes the plugin root with '..'.");
            }

            return;
        }

        if (AgentPluginSpec.BareExecutable.IsMatch(command))
        {
            return;
        }

        var reason = command.Contains(' ', StringComparison.Ordinal)
            ? "looks like a shell command; put arguments in 'args'"
            : "is neither a bare executable name nor a plugin-relative './' path";

        context.Diagnostics.SpecFatal(relative, $"server '{name}' command '{command}' {reason}.");
    }

    private void ValidateHttp(string name, JsonObject server, string relative)
    {
        if (server["url"]?.GetValue<string>() is { } url)
        {
            ValidateUrl(name, url, relative);
        }

        if (server["headers"] is not JsonObject headers)
        {
            return;
        }

        var seen = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var header in headers.OrderBy(h => h.Key, StringComparer.Ordinal))
        {
            if (!AgentPluginSpec.HttpFieldName.IsMatch(header.Key))
            {
                context.Diagnostics.SpecFatal(
                    relative, $"server '{name}' header '{header.Key}' is not a valid HTTP field name.");
            }

            if (seen.TryGetValue(header.Key, out var existing))
            {
                context.Diagnostics.SpecFatal(
                    relative,
                    $"server '{name}' declares headers '{existing}' and '{header.Key}', which differ only by case. " +
                    "HTTP field names are case-insensitive.");
            }
            else
            {
                seen[header.Key] = header.Key;
            }

            if (AgentPluginSpec.LikelySecret.IsMatch(header.Key))
            {
                context.Diagnostics.SpecFatal(
                    relative,
                    $"server '{name}' header '{header.Key}' looks credential-related. Configured headers are " +
                    "literal, visible package data and must not contain credentials.");
            }
        }
    }

    private void ValidateUrl(string name, string url, string relative)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            context.Diagnostics.SpecFatal(relative, $"server '{name}' url '{url}' is not an absolute URL.");
            return;
        }

        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
        {
            context.Diagnostics.SpecFatal(
                relative, $"server '{name}' url must use http or https, not '{uri.Scheme}'.");
            return;
        }

        if (!string.IsNullOrEmpty(uri.UserInfo))
        {
            context.Diagnostics.SpecFatal(
                relative, $"server '{name}' url must not contain user information.");
        }

        if (!string.IsNullOrEmpty(uri.Fragment))
        {
            context.Diagnostics.SpecFatal(
                relative, $"server '{name}' url must not contain a fragment.");
        }

        if (uri.Scheme == Uri.UriSchemeHttp && !uri.IsLoopback)
        {
            context.Diagnostics.SpecFatal(
                relative,
                $"server '{name}' url '{url}' uses plain HTTP. Non-loopback endpoints must use HTTPS.");
        }
    }
}
