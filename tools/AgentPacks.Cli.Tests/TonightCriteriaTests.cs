namespace AgentPacks.Cli.Tests;

/// <summary>
/// Po 1–14 are done only when a named proving test exists. A checked box in the PR is not evidence.
/// Uses the existing xunit suite — no new framework.
/// </summary>
public sealed class TonightCriteriaTests
{
    [Theory]
    [InlineData("1 /squad+/review flow", typeof(SquadContractTests),
        nameof(SquadContractTests.Two_user_invoked_entrypoints_are_squad_and_review))]
    [InlineData("2 README flow", typeof(SquadContractTests),
        nameof(SquadContractTests.Readme_mirrors_the_orchestrator_numbered_flow))]
    [InlineData("3 /review", typeof(SquadContractTests),
        nameof(SquadContractTests.Review_is_dual_axis_and_startable_for_pr_uncommitted_and_main))]
    [InlineData("4 models.source.json", typeof(ModelCatalogTests),
        nameof(ModelCatalogTests.Claude_receives_the_mapped_tier_and_copilot_and_codex_emit_model))]
    [InlineData("5 learnings contract", typeof(SquadContractTests),
        nameof(SquadContractTests.Learnings_log_is_append_only_and_read_first))]
    [InlineData("6 apply fixture", typeof(LearningsLogTests),
        nameof(LearningsLogTests.A_prior_fail_forces_the_skipped_agent_on_the_next_gate))]
    [InlineData("7 coworker docs", typeof(SquadContractTests),
        nameof(SquadContractTests.Readme_mirrors_the_orchestrator_numbered_flow))]
    [InlineData("8 verify-path / evidence gate", typeof(VerificationEvidenceTests),
        nameof(VerificationEvidenceTests.Criterion_command_that_never_covers_is_not_verified_and_not_a_pass))]
    [InlineData("9 loop-only slots", typeof(LanguagePackContractTests),
        nameof(LanguagePackContractTests.A_slot_skill_without_loop_audience_is_rejected))]
    [InlineData("10 no public-looking slot skills", typeof(LanguagePackContractTests),
        nameof(LanguagePackContractTests.Authored_slot_skills_are_loop_audience_not_user_entrypoints))]
    [InlineData("11 two commands only", typeof(SquadContractTests),
        nameof(SquadContractTests.Squad_commands_are_exactly_squad_and_review))]
    [InlineData("12 Matt-tiny agents", typeof(SquadContractTests),
        nameof(SquadContractTests.Loop_agent_bodies_stay_tiny))]
    [InlineData("13 Squad rename", typeof(SquadContractTests),
        nameof(SquadContractTests.User_facing_surfaces_do_not_say_delivery_loop))]
    [InlineData("14 docs pass", typeof(SquadContractTests),
        nameof(SquadContractTests.Readme_mirrors_the_orchestrator_numbered_flow))]
    [InlineData("15 empty squad mcp + read-only docs", typeof(PluginMcpContractTests),
        nameof(PluginMcpContractTests.Squad_mcp_is_empty_scaffold_and_docs_example_is_read_only_http_without_secrets))]
    [InlineData("16 no shipped Dotnet MCP / empty scaffolds", typeof(PluginMcpContractTests),
        nameof(PluginMcpContractTests.No_dotnet_solution_mcp_project_and_plugin_mcp_servers_are_empty_scaffolds))]
    [InlineData("17 no delivery* and no essay agents", typeof(SquadContractTests),
        nameof(SquadContractTests.No_user_facing_delivery_star_and_no_essay_agents))]
    [InlineData("cursor ids not Claude aliases", typeof(ModelCatalogTests),
        nameof(ModelCatalogTests.Claude_receives_the_mapped_tier_and_copilot_and_codex_emit_model))]
    [InlineData("research decisions drop-box", typeof(SquadContractTests),
        nameof(SquadContractTests.Optional_decisions_drop_box_is_read_not_eager_memory))]
    [InlineData("18 coworker local-dev docs", typeof(SquadContractTests),
        nameof(SquadContractTests.Coworker_local_dev_docs_name_validate_test_and_three_client_installs))]
    [InlineData("restored operational steps", typeof(SquadContractTests),
        nameof(SquadContractTests.Loop_agents_restore_operational_steps_not_empty_tiny))]
    [InlineData("language skills name standards", typeof(LanguagePackContractTests),
        nameof(LanguagePackContractTests.Language_slot_skills_name_their_canonical_standards))]
    [InlineData("19 usable gates not essays two commands mcp in dotnet", typeof(SquadContractTests),
        nameof(SquadContractTests.Usable_gates_not_essays_exactly_two_commands_and_mcp_only_in_dotnet))]
    [InlineData("20 simplifier report-only one agent three axes two commands mcp", typeof(SquadContractTests),
        nameof(SquadContractTests.Simplifier_is_report_only_one_agent_three_axes))]
    [InlineData("21 no DotnetSolutionMcp empty mcpServers", typeof(PluginMcpContractTests),
        nameof(PluginMcpContractTests.No_dotnet_solution_mcp_project_and_plugin_mcp_servers_are_empty_scaffolds))]
    public void Each_ticket_criterion_has_a_proving_test(string criterion, Type fixture, string method)
    {
        Assert.True(
            fixture.GetMethod(method) is not null,
            $"{criterion} is prose-only: missing {fixture.Name}.{method}.");
    }
}
