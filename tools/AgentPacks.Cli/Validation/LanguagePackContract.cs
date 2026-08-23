namespace AgentPacks.Cli.Validation;

/// <summary>The exact keyword and skill-name slots shared by language-pack validation and tests.</summary>
internal static class LanguagePackContract
{
    public const string Keyword = "language-pack";

    public static IReadOnlyList<string> Slots { get; } =
        ["build", "test-patterns", "review", "security-review"];

    public static IReadOnlyList<string> RequiredSlots { get; } = ["build", "test-patterns"];

    public static string SkillName(string language, string slot) => $"{language}-{slot}";
}
