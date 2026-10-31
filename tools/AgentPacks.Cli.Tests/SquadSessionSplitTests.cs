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

    private sealed record PlanTask(string Number, string Title, string Goal, string DoneCheck, string Status, string Depends = "");

    [GeneratedRegex(@"^### (\d+(?:\.\d+)?)\. (.+)\n- Goal: (.+)\n- Done-check: (`.+`)\n(?:- Depends: (\d+(?:\.\d+)?(?:, \d+(?:\.\d+)?)*)\n)?- Status: (open|started|done)$", RegexOptions.Multiline)]
    private static partial Regex TaskPattern();

    private static List<PlanTask> Parse(string plan)
    {
        plan = plan.Replace("\r", "", StringComparison.Ordinal);
        var tasks = TaskPattern().Matches(plan)
            .Select(m => new PlanTask(m.Groups[1].Value, m.Groups[2].Value, m.Groups[3].Value, m.Groups[4].Value, m.Groups[6].Value, m.Groups[5].Value))
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
        Assert.Contains("that one task only", skill, StringComparison.Ordinal);
        Assert.Contains("`/squad split <request>` → forced split", skill, StringComparison.Ordinal);

        var contract = Source("skills", "squad", "references", "session-split.md");
        Assert.Contains("Not fit → write `docs/squad-plan.md`, build no code, spawn nobody, write no retro, and stop.", contract, StringComparison.Ordinal);
        Assert.Contains("Take the offered task the user said yes to.", contract, StringComparison.Ordinal);
        Assert.Contains("A request is never dropped", contract, StringComparison.Ordinal);
        Assert.Contains("edited in place, never rewritten", contract, StringComparison.Ordinal);
        Assert.Contains("Never rewrite an existing plan.", skill, StringComparison.Ordinal);
        Assert.Contains("Stop. Do not start the next task in this session.", contract, StringComparison.Ordinal);
        Parse(contract.Replace("<n>", "1", StringComparison.Ordinal)); // the template block is itself a valid task

        Assert.Contains("skills/squad/references/session-split.md", Source("README.md"), StringComparison.Ordinal);
        Assert.Contains("`/squad split <request>`", Source("README.md"), StringComparison.Ordinal);
        Assert.Contains("`/squad split <request>`", File.ReadAllText(Path.Combine(TestRepository.SourceRoot(), "README.md")), StringComparison.Ordinal);
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
        Assert.Contains("Any other request → the status line plus the new request\n   question; new request → normal run, and the plan is not read or written again.", contract.Replace("\r", "", StringComparison.Ordinal), StringComparison.Ordinal);
        Assert.Equal("Plan health and version endpoints: task 1 open. Do task 1, or run the new request?",
            Question(BeforePlan("new-request")!, RequestOf("new-request")));

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

    private enum Mode { Split, SplitAsk, Status, Ask, Close, Blocked, Normal, NothingToRun }

    private static string Contract() =>
        Source("skills", "squad", "references", "session-split.md").Replace("\r", "", StringComparison.Ordinal);

    /// <summary>The words that make a request name the plan, read from the contract itself.</summary>
    private static List<string> PlanNames()
    {
        var names = Regex.Matches(Regex.Match(Contract(), @"names the plan \((.+?)\)", RegexOptions.Singleline).Groups[1].Value, "`([^`]+)`")
            .Select(m => Regex.Escape(m.Groups[1].Value).Replace("<n>", @"\d+", StringComparison.Ordinal))
            .ToList();
        Assert.Equal(4, names.Count);
        return names;
    }

    private static string SplitWord()
    {
        var word = Regex.Match(Contract(), @"starts with the split word `(\w+)` \(`/squad (\w+) <request>`\)");
        Assert.True(word.Success, "the contract names the split word once");
        Assert.Equal(word.Groups[1].Value, word.Groups[2].Value);
        return word.Groups[1].Value;
    }

    private static string RequestText(string request)
    {
        var text = request.Trim();
        return text.StartsWith("/squad", StringComparison.Ordinal) ? text["/squad".Length..].Trim() : text;
    }

    /// <summary>Status refresh: a passing Done-check flips the task to done, edited in place.</summary>
    private static string Refresh(string plan, ISet<string> passing)
    {
        plan = plan.Replace("\r", "", StringComparison.Ordinal);
        foreach (var task in Parse(plan).Where(t => t.Status != "done" && passing.Contains(t.Number)))
        {
            var block = new Regex($@"(^### {Regex.Escape(task.Number)}\. [\s\S]+?^- Status: )(open|started)$", RegexOptions.Multiline);
            plan = block.Replace(plan, "${1}done", 1);
        }
        return plan;
    }

    private static bool IsDone(List<PlanTask> tasks, string number)
    {
        var own = tasks.Where(t => t.Number == number || t.Number.StartsWith(number + ".", StringComparison.Ordinal)).ToList();
        return own.Count > 0 && own.All(t => t.Status == "done");
    }

    private static string? WaitsOn(List<PlanTask> tasks, PlanTask task) =>
        task.Depends.Length == 0 ? null : task.Depends.Split(", ").FirstOrDefault(d => !IsDone(tasks, d));

    private static PlanTask? Offered(List<PlanTask> tasks) =>
        tasks.FirstOrDefault(t => t.Status != "done" && WaitsOn(tasks, t) is null);

    private static Mode ModeFor(string? plan, string request, ISet<string>? passing = null)
    {
        var text = RequestText(request);
        if (Regex.IsMatch(text, $@"^{SplitWord()}\b"))
            return plan is null ? Mode.Split : Mode.SplitAsk;
        var namesPlan = text.Length == 0 || Regex.IsMatch(text, string.Join('|', PlanNames()), RegexOptions.IgnoreCase);
        if (plan is null)
            return namesPlan && text.Length > 0 ? Mode.NothingToRun : Mode.Normal;
        var tasks = Parse(Refresh(plan, passing ?? new HashSet<string>()));
        if (tasks.All(t => t.Status == "done"))
            return Mode.Close;
        if (Offered(tasks) is null)
            return Mode.Blocked;
        return namesPlan ? Mode.Status : Mode.Ask;
    }

    /// <summary>The question squad asks first, built from the templates in the contract.</summary>
    private static string Question(string plan, string request, ISet<string>? passing = null)
    {
        var contract = Contract();
        string Template(string label) => Regex.Match(contract, $@"^- {label}: `(.+)`$", RegexOptions.Multiline).Groups[1].Value;
        var tasks = Parse(Refresh(plan, passing ?? new HashSet<string>()));
        var outcome = Regex.Match(plan, @"^# Squad plan: (.+)$", RegexOptions.Multiline).Groups[1].Value.TrimEnd('\r');
        var mode = ModeFor(plan, request, passing);
        if (mode == Mode.SplitAsk)
            return Regex.Match(contract, @"ask once: `(Plan <outcome> exists\..+?)`").Groups[1].Value.Replace("<outcome>", outcome, StringComparison.Ordinal);
        if (mode == Mode.Close)
            return Template("Close").Replace("<outcome>", outcome, StringComparison.Ordinal);

        var offered = Offered(tasks)!;
        var states = tasks.Take(tasks.IndexOf(offered) + 1).Select(t =>
            WaitsOn(tasks, t) is { } m ? $"task {t.Number} skipped (waits on task {m})"
            : t.Status == "started" ? $"task {t.Number} half done"
            : $"task {t.Number} {t.Status}");
        var label = mode == Mode.Ask ? "New request" : offered.Status == "started" ? "Half done" : "Status";
        return Template(label)
            .Replace("<outcome>", outcome, StringComparison.Ordinal)
            .Replace("<states>", string.Join(", ", states), StringComparison.Ordinal)
            .Replace("<n>", offered.Number, StringComparison.Ordinal)
            .Replace("Task <n>", $"Task {offered.Number}", StringComparison.Ordinal);
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

    private static HashSet<string> PassingOf(string fixture)
    {
        var path = Root(fixture, "passing.md");
        return File.Exists(path) ? File.ReadAllLines(path).Select(l => l.Trim()).Where(l => l.Length > 0).ToHashSet() : [];
    }

    private static string QuestionOf(string fixture) => File.ReadAllText(Root(fixture, "question.md")).Trim();

    [Theory]
    [InlineData("big-task", "Normal")]
    [InlineData("existing-plan", "Status")]
    [InlineData("task-split", "Status")]
    [InlineData("new-request", "Ask")]
    [InlineData("all-done", "Close")]
    [InlineData("forced-split", "Split")]
    [InlineData("split-existing", "SplitAsk")]
    [InlineData("status-question", "Status")]
    [InlineData("already-done", "Status")]
    [InlineData("half-done", "Status")]
    [InlineData("dependency-skip", "Status")]
    public void Each_fixture_follows_the_mode_table(string fixture, string expected) =>
        Assert.Equal(Enum.Parse<Mode>(expected), ModeFor(BeforePlan(fixture), RequestOf(fixture), PassingOf(fixture)));

    [Theory]
    [InlineData("existing-plan")]
    [InlineData("status-question")]
    [InlineData("already-done")]
    [InlineData("half-done")]
    [InlineData("all-done")]
    [InlineData("dependency-skip")]
    [InlineData("split-existing")]
    public void Each_fixture_asks_its_question_first(string fixture) =>
        Assert.Equal(QuestionOf(fixture), Question(BeforePlan(fixture)!, RequestOf(fixture), PassingOf(fixture)));

    [Fact]
    public void Plan_naming_words_only_matter_with_a_plan()
    {
        // No plan: a request that only names the plan has nothing to run; any other request is a normal run.
        Assert.Equal(Mode.NothingToRun, ModeFor(null, "/squad next task"));
        Assert.Equal(Mode.NothingToRun, ModeFor(null, "/squad task 2"));
        Assert.Equal(Mode.Normal, ModeFor(null, "/squad add src/ping.ts"));
        // Open plan: empty or plan-naming requests get the status question, others the new request question.
        var open = BeforePlan("status-question");
        Assert.Equal(Mode.Status, ModeFor(open, "/squad"));
        Assert.Equal(Mode.Status, ModeFor(open, "/squad next task"));
        Assert.Equal(Mode.Ask, ModeFor(open, "/squad add src/ping.ts"));
        // All done: always the close question first, also with a new request.
        var done = BeforePlan("all-done");
        Assert.Equal(Mode.Close, ModeFor(done, "/squad next task"));
        Assert.Equal(Mode.Close, ModeFor(done, "/squad add src/ping.ts"));
        // The split word only counts as the first word.
        Assert.Equal(Mode.Normal, ModeFor(null, "/squad add a split view"));

        const string Ask = "then always ask, also with no request";
        Assert.Contains(Ask, Source("skills", "squad", "SKILL.md"), StringComparison.Ordinal);
        Assert.Contains("then always ask, also with no request text", Source("commands", "squad.md"), StringComparison.Ordinal);
        Assert.Contains("then always ask, also when `/squad` has no request\n   text", Contract(), StringComparison.Ordinal);
    }

    [Fact]
    public void Forced_split_writes_only_the_plan_even_under_the_limits()
    {
        var request = RequestOf("forced-split");
        Assert.DoesNotMatch(new Regex(@"PBI-\d+"), request);
        var afterDir = Root("forced-split", "after");
        Assert.Equal([PlanPath], Tree(afterDir));
        var plan = File.ReadAllText(Path.Combine(afterDir, PlanPath));
        var tasks = Parse(plan);
        Assert.True(tasks.Count is >= 1 and <= 3, "a small forced split stays under the limits");
        Assert.All(tasks, t => Assert.Equal("open", t.Status));
        Assert.Contains(request.Trim(), plan, StringComparison.Ordinal);

        var contract = Contract();
        Assert.Contains("`/squad split <request>` splits the request into tasks even when it fits one session: write\n`docs/squad-plan.md`, build no code, spawn nobody, write no retro, and stop.", contract, StringComparison.Ordinal);
        Assert.Contains("a later `/squad-split <request>` command only runs it", contract, StringComparison.Ordinal);
    }

    [Fact]
    public void Split_on_an_existing_plan_asks_once_then_replaces_or_appends()
    {
        var before = File.ReadAllText(Root("split-existing", "before", PlanPath));
        var beforeTasks = Parse(before);

        var append = File.ReadAllText(Root("split-existing", "after-append", PlanPath));
        Assert.StartsWith(before, append, StringComparison.Ordinal);
        Assert.Equal([PlanPath], Tree(Root("split-existing", "after-append")));
        var added = Parse(append).Skip(beforeTasks.Count).ToList();
        Assert.NotEmpty(added);
        Assert.Equal(added.Select((_, i) => $"{beforeTasks.Count + i + 1}"), added.Select(t => t.Number));
        Assert.All(added, t => Assert.Equal("open", t.Status));

        var replaceDir = Root("split-existing", "after-replace");
        var archived = Assert.Single(Tree(replaceDir), f => f != PlanPath);
        Assert.Matches(@"^docs/squad-plans/\d{4}-\d{2}-\d{2}-health-and-version-endpoints\.md$", archived);
        Assert.Equal(before, File.ReadAllText(Path.Combine(replaceDir, archived)));
        Assert.Equal(File.ReadAllText(Root("forced-split", "after", PlanPath)), File.ReadAllText(Path.Combine(replaceDir, PlanPath)));
    }

    [Fact]
    public void Yes_does_one_task_and_the_next_run_asks_about_the_next()
    {
        // existing-plan: the question offers task 1; after yes only task 1 is done, so the next run offers task 2.
        Assert.Contains("Shall I do task 1?", QuestionOf("existing-plan"), StringComparison.Ordinal);
        var after = File.ReadAllText(Root("existing-plan", "after", PlanPath));
        Assert.EndsWith("task 1 done, task 2 open. Shall I do task 2?", Question(after, "/squad"), StringComparison.Ordinal);
        // status-question: task 2 offered; once task 2 is done the next run offers task 3.
        Assert.Contains("Shall I do task 2?", QuestionOf("status-question"), StringComparison.Ordinal);
        var next = Refresh(BeforePlan("status-question")!, new HashSet<string> { "2" });
        Assert.EndsWith("Shall I do task 3?", Question(next, "/squad"), StringComparison.Ordinal);
    }

    [Fact]
    public void Task_already_done_in_the_code_is_marked_done_in_place()
    {
        var before = BeforePlan("already-done")!;
        var after = File.ReadAllText(Root("already-done", "after", PlanPath));
        Assert.Equal(after, Refresh(before, PassingOf("already-done")));
        var lines = before.Split('\n').Zip(after.Split('\n')).Where(p => p.First != p.Second).ToList();
        Assert.Equal(("- Status: open", "- Status: done"), Assert.Single(lines));
    }

    [Fact]
    public void Half_done_task_is_offered_to_finish()
    {
        var tasks = Parse(BeforePlan("half-done")!);
        Assert.Contains(tasks, t => t.Status == "started");
        Assert.Contains("Task 2 was half done, shall I finish it?", QuestionOf("half-done"), StringComparison.Ordinal);
        Assert.Contains("Set it to\n   `Status: started` before any other work", Contract(), StringComparison.Ordinal);
    }

    [Fact]
    public void All_done_plan_asks_to_close_and_archives_on_yes()
    {
        var plan = File.ReadAllText(Root("all-done", "before", PlanPath));
        Assert.All(Parse(plan), t => Assert.Equal("done", t.Status));
        // No: the plan stays byte for byte.
        Assert.Equal([PlanPath], Tree(Root("all-done", "after")));
        Assert.Equal(plan, File.ReadAllText(Root("all-done", "after", PlanPath)));
        // Yes: the plan moves, unchanged, and no docs/squad-plan.md is left.
        var closeDir = Root("all-done", "after-close");
        var archived = Assert.Single(Tree(closeDir));
        Assert.Matches(@"^docs/squad-plans/\d{4}-\d{2}-\d{2}-health-and-version-endpoints\.md$", archived);
        Assert.Equal(plan, File.ReadAllText(Path.Combine(closeDir, archived)));
    }

    [Fact]
    public void Task_waiting_on_an_open_task_is_skipped_with_the_reason()
    {
        var tasks = Parse(BeforePlan("dependency-skip")!);
        Assert.Equal("3", tasks.Single(t => t.Number == "2").Depends);
        Assert.Contains("task 2 skipped (waits on task 3)", QuestionOf("dependency-skip"), StringComparison.Ordinal);

        // Every task left waits on another: nothing to offer, squad says why and stops.
        var cycle = BeforePlan("dependency-skip")!.Replace(
            "- Done-check: `npm test -- version`\n- Status: open",
            "- Done-check: `npm test -- version`\n- Depends: 2\n- Status: open", StringComparison.Ordinal);
        Assert.Equal(Mode.Blocked, ModeFor(cycle, "/squad"));
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
        Assert.Contains("run that one task only", command, StringComparison.Ordinal);
        Assert.Contains("`/squad split <request>` → forced split", command, StringComparison.Ordinal);
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
