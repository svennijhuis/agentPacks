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

    private sealed record PlanTask(string Number, string Title, string Goal, string DoneCheck, string Status);

    [GeneratedRegex(@"^### (\d+(?:\.\d+)?)\. (.+)\n- Goal: (.+)\n- Done-check: (`.+`)\n- Status: (open|done)$", RegexOptions.Multiline)]
    private static partial Regex TaskPattern();

    private static List<PlanTask> Parse(string plan)
    {
        plan = plan.Replace("\r", "", StringComparison.Ordinal);
        var tasks = TaskPattern().Matches(plan)
            .Select(m => new PlanTask(m.Groups[1].Value, m.Groups[2].Value, m.Groups[3].Value, m.Groups[4].Value, m.Groups[5].Value))
            .ToList();
        Assert.Equal(Regex.Matches(plan, @"^### ", RegexOptions.Multiline).Count, tasks.Count);
        Assert.Equal(tasks.Count, tasks.Select(t => t.Number).Distinct().Count());
        var tops = tasks.Select(t => Top(t.Number)).Distinct().ToList();
        Assert.Equal(Enumerable.Range(1, tops.Count), tops);
        return tasks;
    }

    private static int Top(string number) =>
        int.Parse(number.Split('.')[0], System.Globalization.CultureInfo.InvariantCulture);

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
        Assert.Contains("Take the lowest open task in file order.", contract, StringComparison.Ordinal);
        Assert.Contains("A request is never dropped", contract, StringComparison.Ordinal);
        Assert.Contains("edited in place, never rewritten", contract, StringComparison.Ordinal);
        Assert.Contains("Never rewrite an existing plan.", skill, StringComparison.Ordinal);
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

    [Fact]
    public void New_request_with_an_open_plan_asks_and_leaves_the_plan_untouched()
    {
        var contract = Source("skills", "squad", "references", "session-split.md");
        Assert.Contains("Open task and any other request → ask once", contract, StringComparison.Ordinal);
        Assert.Contains("New request → normal run; the plan\n   is not read or written.", contract.Replace("\r", "", StringComparison.Ordinal), StringComparison.Ordinal);

        var request = File.ReadAllText(Root("new-request", "request.md")).Trim();
        Assert.NotEqual("/squad", request);
        Assert.DoesNotMatch(new Regex(@"squad-plan|next task|task \d+", RegexOptions.IgnoreCase), request);

        var before = File.ReadAllText(Root("new-request", "before", PlanPath));
        Assert.Contains("Status: open", before, StringComparison.Ordinal);
        var afterDir = Root("new-request", "after");
        Assert.Equal(before, File.ReadAllText(Path.Combine(afterDir, PlanPath)));

        var code = Tree(afterDir).Where(f => f != PlanPath).ToArray();
        Assert.NotEmpty(code);
        Assert.All(code, f => Assert.Contains(f, request, StringComparison.Ordinal));
    }

    [Fact]
    public void Too_big_task_is_split_in_place_and_done_rows_are_kept()
    {
        Assert.Equal("/squad", File.ReadAllText(Root("task-split", "request.md")).Trim());
        var before = Parse(File.ReadAllText(Root("task-split", "before", PlanPath)));
        var afterDir = Root("task-split", "after");
        Assert.Equal([PlanPath], Tree(afterDir));
        var after = Parse(File.ReadAllText(Path.Combine(afterDir, PlanPath)));

        Assert.Contains(before, t => t.Status == "done");
        var split = before.First(t => t.Status == "open");
        var index = before.IndexOf(split);
        var subtasks = after.Skip(index).TakeWhile(t => t.Number.StartsWith(split.Number + ".", StringComparison.Ordinal)).ToList();

        Assert.True(subtasks.Count >= 2, "a split task becomes at least two subtasks");
        Assert.Equal(subtasks.Select((_, i) => $"{split.Number}.{i + 1}"), subtasks.Select(t => t.Number));
        Assert.All(subtasks, t => Assert.Equal("open", t.Status));
        Assert.Equal(before.Take(index), after.Take(index));
        Assert.Equal(before.Skip(index + 1), after.Skip(index + subtasks.Count));
    }

    private enum Mode { Task, Ask, Normal, NothingToRun }

    /// <summary>
    /// The mode table of session-split.md "Which mode", with the plan-naming words read from the
    /// contract itself, so a contract edit changes what the fixtures must show.
    /// </summary>
    private static Mode ModeFor(string? plan, string request)
    {
        var contract = Source("skills", "squad", "references", "session-split.md").Replace("\r", "", StringComparison.Ordinal);
        var rule1 = Regex.Match(contract, @"^1\. .+?→ task mode\.", RegexOptions.Multiline | RegexOptions.Singleline).Value;
        var names = Regex.Matches(Regex.Match(rule1, @"names the plan \((.+?)\)", RegexOptions.Singleline).Groups[1].Value, "`([^`]+)`")
            .Select(m => Regex.Escape(m.Groups[1].Value).Replace("<n>", @"\d+", StringComparison.Ordinal))
            .ToList();
        Assert.Equal(4, names.Count);

        var text = request.Trim();
        text = text.StartsWith("/squad", StringComparison.Ordinal) ? text["/squad".Length..].Trim() : text;
        var namesPlan = text.Length > 0 && Regex.IsMatch(text, string.Join('|', names), RegexOptions.IgnoreCase);
        var openTask = plan is not null && Parse(plan).Any(t => t.Status == "open");

        if (openTask)
            return text.Length == 0 || namesPlan ? Mode.Task : Mode.Ask;
        return text.Length == 0 || namesPlan ? Mode.NothingToRun : Mode.Normal;
    }

    private static string RequestOf(string fixture)
    {
        var path = Root(fixture, "request.md");
        return File.Exists(path) ? File.ReadAllText(path) : "/squad";
    }

    private static string? BeforePlan(string fixture)
    {
        var path = Root(fixture, "before", PlanPath);
        return File.Exists(path) ? File.ReadAllText(path) : null;
    }

    [Theory]
    [InlineData("big-task", "Normal")]
    [InlineData("existing-plan", "Task")]
    [InlineData("task-split", "Task")]
    [InlineData("new-request", "Ask")]
    [InlineData("all-done", "NothingToRun")]
    public void Each_fixture_follows_the_mode_table(string fixture, string expected) =>
        Assert.Equal(Enum.Parse<Mode>(expected), ModeFor(BeforePlan(fixture), RequestOf(fixture)));

    [Fact]
    public void Open_task_check_binds_outside_the_or()
    {
        // (open task) AND (no text OR names the plan): a request that names the plan with no open
        // task left must not reach task mode.
        var plan = File.ReadAllText(Root("all-done", "before", PlanPath));
        Assert.DoesNotContain(Parse(plan), t => t.Status == "open");
        Assert.Equal(Mode.NothingToRun, ModeFor(plan, "/squad next task"));
        Assert.Equal(Mode.NothingToRun, ModeFor(plan, "/squad task 2"));
        Assert.Equal(Mode.Normal, ModeFor(plan, "/squad add src/ping.ts"));
        Assert.Equal(Mode.Task, ModeFor(BeforePlan("task-split"), "/squad next task"));

        var afterDir = Root("all-done", "after");
        Assert.Equal([PlanPath], Tree(afterDir));
        Assert.Equal(plan, File.ReadAllText(Path.Combine(afterDir, PlanPath)));

        const string Clause = "has an open task, and the request is empty or names the plan or a task";
        Assert.Contains(Clause, Source("skills", "squad", "SKILL.md"), StringComparison.Ordinal);
        Assert.Contains(Clause, Source("commands", "squad.md"), StringComparison.Ordinal);
        var contract = Source("skills", "squad", "references", "session-split.md").Replace("\r", "", StringComparison.Ordinal);
        Assert.Matches(new Regex(@"^1\. `docs/squad-plan\.md` has a `Status: open` task, and `/squad` has no request text or the\n   request names the plan", RegexOptions.Multiline), contract);
        Assert.Contains("A request that only\n   names the plan has nothing to run", contract, StringComparison.Ordinal);
        foreach (var doc in new[] { contract, Source("skills", "squad", "SKILL.md"), Source("commands", "squad.md") })
            Assert.DoesNotMatch(new Regex(@"no request text, or the request names"), doc);
    }

    [Fact]
    public void Squad_command_carries_the_session_split_gates()
    {
        var command = Source("commands", "squad.md").Replace("\r", "", StringComparison.Ordinal);
        var gate = command.IndexOf("Session split: read `skills/squad/references/session-split.md`", StringComparison.Ordinal);
        var step1 = command.IndexOf("\n1. Print the request", StringComparison.Ordinal);
        var step2 = command.IndexOf("\n2. For an obvious small change", StringComparison.Ordinal);
        Assert.True(gate > 0 && gate < step1 && step1 < step2, "the session split gate sits before the numbered steps");

        Assert.Contains("docs/squad-plan.md", command, StringComparison.Ordinal);
        Assert.Contains("Normal run, after step 1 and before step 2: over any limit (more than one PBI or outcome, more than 3 steps, more than 10 files) → write `docs/squad-plan.md`, build no code, spawn nobody, write no retro, stop.", command, StringComparison.Ordinal);
        Assert.Contains("run the lowest open task only", command, StringComparison.Ordinal);
        Assert.Contains("ask once: task or new request; a new request leaves the plan untouched", command, StringComparison.Ordinal);
        Assert.Contains("Never rewrite an existing plan.", command, StringComparison.Ordinal);
        Assert.Contains("A session-split stop gets no retro.", command, StringComparison.Ordinal);

        // Step 4 branches on the planner marker the same way it branches on `Design gap:`.
        var step4 = command[command.IndexOf("\n4. ", StringComparison.Ordinal)..command.IndexOf("\n5. ", StringComparison.Ordinal)];
        Assert.Contains("Planner returns `Design gap: <question>` →", step4, StringComparison.Ordinal);
        Assert.Contains("Planner returns `Session split: <n> steps` → delete this run's draft `docs/plans/` file", step4, StringComparison.Ordinal);
        Assert.Contains("spawn nobody, write no retro, stop.", step4, StringComparison.Ordinal);

        // The skill flow numbers orient as step 2; the command numbers it as step 1. Both must point at orient.
        Assert.Contains("\n2. Orient codebase", Source("skills", "squad", "SKILL.md").Replace("\r", "", StringComparison.Ordinal), StringComparison.Ordinal);
        Assert.Contains("Normal run, after step 2:", Source("skills", "squad", "SKILL.md"), StringComparison.Ordinal);
        Assert.Contains("(skill flow step 2, `/squad` step 1)", Source("skills", "squad", "references", "session-split.md").Replace("\n", " ", StringComparison.Ordinal), StringComparison.Ordinal);
    }

    [Fact]
    public void Session_split_marker_drops_the_draft_plan_on_every_side()
    {
        const string Marker = "Session split: <n> steps";
        const string Drop = "draft `docs/plans/` file";
        var docs = new Dictionary<string, string>
        {
            ["planner"] = Source("agents", "squad-planner.md"),
            ["planning-contract"] = Source("skills", "squad", "references", "planning-contract.md"),
            ["session-split"] = Source("skills", "squad", "references", "session-split.md").Replace("\n", " ", StringComparison.Ordinal),
            ["skill"] = Source("skills", "squad", "SKILL.md"),
            ["command"] = Source("commands", "squad.md"),
        };
        foreach (var (name, text) in docs)
        {
            Assert.True(text.Contains(Marker, StringComparison.Ordinal), $"{name} names `{Marker}`");
            Assert.True(text.Contains(Drop, StringComparison.Ordinal), $"{name} says the run's {Drop} is deleted");
        }
        Assert.Contains("write no `## Slices`", docs["planner"], StringComparison.Ordinal);
        Assert.DoesNotContain(", no plan)", docs["planner"], StringComparison.Ordinal);
    }
}
