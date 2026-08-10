using Company.AI.Tooling.Validation;

namespace Company.AI.Tooling.Tests;

public class McpValidationTests
{
    private static TestRepository WithServer(string serverJson) =>
        new TestRepository().WithValidPlugin().WithMcp($$"""
            {
              "$schema": "https://agent-plugins.org/schemas/1.0.0/mcp.schema.json",
              "mcpServers": { "docs": {{serverJson}} }
            }
            """);

    [Theory]
    [InlineData("\"npx -y some-server\"", "shell command")]
    [InlineData("\"../tool\"", "neither a bare executable")]
    [InlineData("\"./../tool\"", "escapes the plugin root")]
    public void Invalid_stdio_commands_are_rejected(string command, string expected)
    {
        using var repo = WithServer($$"""{ "type": "stdio", "command": {{command}} }""");

        var run = repo.Validate();

        Assert.Contains(run.Diagnostics, d => d.Message.Contains(expected));
    }

    [Theory]
    [InlineData("\"validator\"")]
    [InlineData("\"./bin/validator\"")]
    public void Valid_stdio_commands_are_accepted(string command)
    {
        using var repo = WithServer($$"""{ "type": "stdio", "command": {{command}} }""");

        var run = repo.Validate();

        Assert.Empty(run.Diagnostics);
    }

    [Fact]
    public void Cwd_escaping_its_root_is_rejected()
    {
        using var repo = WithServer("""
            { "type": "stdio", "command": "validator", "cwd": "${PLUGIN_ROOT}/../elsewhere" }
            """);

        var run = repo.Validate();

        Assert.Contains(run.Diagnostics, d => d.Message.Contains("escapes its root"));
    }

    [Fact]
    public void Reserved_environment_variables_cannot_be_overridden()
    {
        using var repo = WithServer("""
            { "type": "stdio", "command": "validator", "env": { "PLUGIN_ROOT": "/tmp" } }
            """);

        var run = repo.Validate();

        Assert.True(run.HasErrors);
    }

    [Fact]
    public void Args_must_be_strings()
    {
        using var repo = WithServer("""
            { "type": "stdio", "command": "validator", "args": [1, 2] }
            """);

        var run = repo.Validate();

        Assert.True(run.HasErrors);
    }

    [Fact]
    public void Secret_looking_environment_keys_are_rejected()
    {
        using var repo = WithServer("""
            { "type": "stdio", "command": "validator", "env": { "API_TOKEN": "abc123" } }
            """);

        var run = repo.Validate();

        Assert.Contains(run.Diagnostics, d => d.Message.Contains("credential-related"));
    }

    [Theory]
    [InlineData("https://user:pass@example.com/mcp", "user information")]
    [InlineData("https://example.com/mcp#section", "fragment")]
    [InlineData("http://example.com/mcp", "plain HTTP")]
    // On Unix a leading-slash path parses as an absolute file: URI, so it fails on the scheme.
    [InlineData("/relative/mcp", "http or https")]
    [InlineData("deploy.example.com/mcp", "absolute URL")]
    public void Invalid_remote_urls_are_rejected(string url, string expected)
    {
        using var repo = WithServer($$"""{ "type": "streamable-http", "url": "{{url}}" }""");

        var run = repo.Validate();

        Assert.Contains(run.Diagnostics, d => d.Message.Contains(expected));
    }

    [Theory]
    [InlineData("http://localhost:3000/mcp")]
    [InlineData("http://127.0.0.1:3000/mcp")]
    [InlineData("http://[::1]:3000/mcp")]
    [InlineData("https://deploy.example.com/mcp")]
    public void Loopback_http_and_remote_https_are_accepted(string url)
    {
        using var repo = WithServer($$"""{ "type": "streamable-http", "url": "{{url}}" }""");

        var run = repo.Validate();

        Assert.Empty(run.Diagnostics);
    }

    [Fact]
    public void Headers_differing_only_by_case_are_rejected()
    {
        using var repo = WithServer("""
            {
              "type": "streamable-http",
              "url": "https://deploy.example.com/mcp",
              "headers": { "X-Tenant": "public", "x-tenant": "public" }
            }
            """);

        var run = repo.Validate();

        Assert.Contains(run.Diagnostics, d => d.Message.Contains("differ only by case"));
    }

    [Fact]
    public void Credential_headers_are_rejected_because_package_data_is_visible()
    {
        using var repo = WithServer("""
            {
              "type": "streamable-http",
              "url": "https://deploy.example.com/mcp",
              "headers": { "Authorization": "Bearer abc123" }
            }
            """);

        var run = repo.Validate();

        Assert.Contains(run.Diagnostics, d => d.Message.Contains("credential-related"));
    }

    [Fact]
    public void Sse_is_allowed_but_warned_about_and_does_not_fail_the_build()
    {
        using var repo = WithServer("""
            { "type": "sse", "url": "https://deploy.example.com/mcp" }
            """);

        var run = repo.Validate();

        var diagnostic = Assert.Single(run.Diagnostics);
        Assert.Equal(DiagnosticKind.Warning, diagnostic.Kind);
        Assert.False(run.HasErrors);
    }
}
