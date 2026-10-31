using System.Text.RegularExpressions;

namespace AgentPacks.Cli.Tests;

/// <summary>
/// Session split (docs/pbi/64-squad-session-split.md): work too big for one session gives a
/// <c>docs/squad-plan.md</c> and no code; a run on an existing plan does only the lowest open task
/// and marks it done. Fixtures under <c>Fixtures/squad-session-split/</c> hold the before and after trees.
/// </summary>
public sealed partial class SquadSessionSplitTests
{
    private const string PlanPath = "docs/squad-plan.md";

    private static string Root(params string[] parts) =>
        Path.Combine([AppContext.BaseDirectory, "Fixtures", "squad-session-split", .. parts]);

    private static string Source(params string[] parts) =>
        File.ReadAllText(Path.Combine([TestRepository.SourceRoot(), "plugins", "squad", .. parts]));

    private sealed record PlanTask(int Number, string Title, string Goal, string DoneCheck, string Status);

    [GeneratedRegex(@"^### (\d+)\. (.+)\n- Goal: (.+)\n- Done-check: (`.+`)\n- Status: (open|done)$", RegexOptions.Multiline)]
    private static partial Regex TaskPattern();

    private static List<PlanTask> Parse(string plan)
    {
        plan = plan.Replace("\r", "", StringComparison.Ordinal);
        var tasks = TaskPattern().Matches(plan)
            .Select(m => new PlanTask(int.Parse(m.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture),
                m.Groups[2].Value, m.Groups[3].Value, m.Groups[4].Value, m.Groups[5].Value))
            .ToList();
        Assert.Equal(Regex.Matches(plan, @"^### ", RegexOptions.Multiline).Count, tasks.Count);
        Assert.Equal(Enumerable.Range(1, tasks.Count), tasks.Select(t => t.Number));
        return tasks;
    }

    private static string[] Tree(string dir) =>
        Directory.GetFiles(dir, "*", SearchOption.AllDirectories)
            .Select(f => Path.GetRelativePath(dir, f).Replace('\\', '/'))
            .OrderBy(f => f, StringComparer.Ordinal)
            .ToArray();

    [Fact]
    public void Skill_and_readme_point_to_the_session_split_contract()
    {
        var skill = Source("skills", "squad", "SKILL.md");
        Assert.Contains("references/session-split.md", skill, StringComparison.Ordinal);
        Assert.Contains("more than one PBI or outcome, more than 3 steps, more than 10 files", skill, StringComparison.Ordinal);
        Assert.Contains("build no code, spawn nobody, stop", skill, StringComparison.Ordinal);
        Assert.Contains("lowest open task only", skill, StringComparison.Ordinal);

        var contract = Source("skills", "squad", "references", "session-split.md");
        Assert.Contains("Not fit → write `docs/squad-plan.md`, build no code, spawn nobody, write no retro, and stop.", contract, StringComparison.Ordinal);
        Assert.Contains("Take the lowest-numbered open task.", contract, StringComparison.Ordinal);
        Assert.Contains("Stop. Do not start the next task in this session.", contract, StringComparison.Ordinal);
        Parse(contract.Replace("<n>", "1", StringComparison.Ordinal)); // the template block is itself a valid task

        Assert.Contains("skills/squad/references/session-split.md", Source("README.md"), StringComparison.Ordinal);
        Assert.Contains("Session split: <n> steps", Source("agents", "squad-planner.md"), StringComparison.Ordinal);
    }

    [Fact]
    public void Big_task_gives_a_plan_and_no_code()
    {
        var request = File.ReadAllText(Root("big-task", "request.md"));
        var pbis = Regex.Matches(request, @"^- PBI-\d+:", RegexOptions.Multiline).Count;
        Assert.True(pbis > 1, $"fixture request must trip the more-than-one-PBI limit; has {pbis}.");

        var after = Root("big-task", "after");
        Assert.Equal([PlanPath], Tree(after));

        var tasks = Parse(File.ReadAllText(Path.Combine(after, PlanPath)));
        Assert.True(tasks.Count >= 2, "a split plan has at least two tasks");
        Assert.All(tasks, t => Assert.Equal("open", t.Status));
        Assert.All(tasks, t => Assert.False(string.IsNullOrWhiteSpace(t.Goal)));
    }

    [Fact]
    public void Run_on_existing_plan_does_only_task_one_and_marks_it_done()
    {
        var before = Parse(File.ReadAllText(Root("existing-plan", "before", PlanPath)));
        var afterDir = Root("existing-plan", "after");
        var after = Parse(File.ReadAllText(Path.Combine(afterDir, PlanPath)));

        Assert.All(before, t => Assert.Equal("open", t.Status));
        Assert.Equal(before.Count, after.Count);
        Assert.Equal(before[0] with { Status = "done" }, after[0]);
        Assert.Equal(before.Skip(1), after.Skip(1));

        var code = Tree(afterDir).Where(f => f != PlanPath).ToArray();
        Assert.NotEmpty(code);
        Assert.All(code, f => Assert.Contains(f, after[0].Goal, StringComparison.Ordinal));
    }
}
