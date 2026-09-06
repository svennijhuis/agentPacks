namespace AgentPacks.Cli.Tests;

/// <summary>
/// Po 1–14 are done only when a named proving test exists. A checked box in the PR is not evidence.
/// Uses the existing xunit suite — no new framework.
/// </summary>
public sealed class TonightCriteriaTests
{
    [Theory]
    [InlineData("1 /squad+/squad-review flow", typeof(SquadContractTests),
        nameof(SquadContractTests.Two_user_invoked_entrypoints_are_squad_and_review))]
    [InlineData("2 README flow", typeof(SquadContractTests),
        nameof(SquadContractTests.Readme_mirrors_the_orchestrator_numbered_flow))]
    [InlineData("3 /squad-review", typeof(SquadContractTests),
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
    [InlineData("19 usable gates not essays two commands empty mcp scaffolds", typeof(SquadContractTests),
        nameof(SquadContractTests.Usable_gates_not_essays_exactly_two_commands_and_empty_mcp_scaffolds))]
    [InlineData("20 simplifier report-only one agent three axes two commands mcp", typeof(SquadContractTests),
        nameof(SquadContractTests.Simplifier_is_report_only_one_agent_three_axes))]
    [InlineData("21 no DotnetSolutionMcp empty mcpServers", typeof(PluginMcpContractTests),
        nameof(PluginMcpContractTests.No_dotnet_solution_mcp_project_and_plugin_mcp_servers_are_empty_scaffolds))]
    [InlineData("22 squad-* agents no user-facing loop-*", typeof(SquadContractTests),
        nameof(SquadContractTests.No_user_facing_loop_star_agents_are_squad_star))]
    [InlineData("23 typescript pack required slots loop audience", typeof(LanguagePackContractTests),
        nameof(LanguagePackContractTests.Typescript_pack_fills_required_slots_with_loop_audience))]
    [InlineData("24 README Codex agent toml copy one-liner", typeof(SquadContractTests),
        nameof(SquadContractTests.Readme_has_codex_agent_toml_copy_one_liner))]
    [InlineData("25 learnings-digest user-invoked Matt-tiny no rewrite", typeof(SquadContractTests),
        nameof(SquadContractTests.Learnings_digest_is_user_invoked_matt_tiny_no_rewrite))]
    [InlineData("26 worktree note only in /squad skill", typeof(SquadContractTests),
        nameof(SquadContractTests.Worktree_note_only_in_squad_skill))]
    [InlineData("27 caveman pins and /squad invokes by exact name", typeof(SquadContractTests),
        nameof(SquadContractTests.Caveman_external_pins_and_squad_invokes_by_exact_name))]
    [InlineData("28 review-contract drops findings below 80 confidence", typeof(SquadContractTests),
        nameof(SquadContractTests.Review_contract_drops_findings_below_80_confidence))]
    [InlineData("29 advisor-lite at confirm stuck handoff no slash", typeof(SquadContractTests),
        nameof(SquadContractTests.Squad_advisor_lite_at_confirm_stuck_handoff_no_slash))]
    [InlineData("30 verifier rejects happy-path-only coverage", typeof(SquadContractTests),
        nameof(SquadContractTests.Verifier_rejects_happy_path_only_coverage))]
    [InlineData("31 skill bodies references/standards only", typeof(LanguagePackContractTests),
        nameof(LanguagePackContractTests.Skill_bodies_point_only_at_references_standards))]
    [InlineData("32 test plan matrix", typeof(SquadContractTests),
        nameof(SquadContractTests.Planning_contract_requires_the_test_plan_matrix))]
    [InlineData("33 Internal do-not-run line", typeof(LanguagePackContractTests),
        nameof(LanguagePackContractTests.Loop_audience_skills_start_with_internal_do_not_run_directly))]
    [InlineData("33 Copilot user-invocable false", typeof(LanguagePackContractTests),
        nameof(LanguagePackContractTests.Copilot_emits_user_invocable_false_for_loop_audience))]
    [InlineData("33 pack-check README setup-only", typeof(PackCheckContractTests),
        nameof(PackCheckContractTests.Pack_check_readme_is_setup_only_squad_already_runs_check))]
    [InlineData("34 Codex tiers map to real ids", typeof(ModelCatalogTests),
        nameof(ModelCatalogTests.Codex_tiers_map_to_real_ids_not_all_inherit))]
    [InlineData("34 empty mcp scaffolds not named dotnet-only", typeof(SquadContractTests),
        nameof(SquadContractTests.Empty_mcp_scaffolds_test_is_not_named_dotnet_only))]
    [InlineData("34 anti-loop spawn none two fix rounds", typeof(SquadContractTests),
        nameof(SquadContractTests.Anti_loop_small_change_spawn_none_and_two_fix_rounds_max))]
    [InlineData("36 /squad-review save-markdown ask", typeof(SquadContractTests),
        nameof(SquadContractTests.Review_asks_save_markdown_yes_writes_file_no_stays_ide_only))]
    [InlineData("37 squad skill not user-invocable command is slash", typeof(SquadContractTests),
        nameof(SquadContractTests.Squad_skill_is_not_user_invocable_command_is_the_only_slash))]
    [InlineData("38 Claude package one squad and one squad-review command", typeof(SquadContractTests),
        nameof(SquadContractTests.Claude_package_ships_one_squad_and_one_squad_review_command))]
    [InlineData("38 caveman pins are not user-invocable", typeof(SquadContractTests),
        nameof(SquadContractTests.Caveman_pins_are_not_user_invocable))]
    [InlineData("38 user-facing surfaces say squad-review not bare review", typeof(SquadContractTests),
        nameof(SquadContractTests.User_facing_surfaces_say_squad_review_not_bare_review))]
    [InlineData("39 friendlier command blurbs still squad and squad-review", typeof(SquadContractTests),
        nameof(SquadContractTests.Squad_and_squad_review_command_blurbs_are_short_user_friendly))]
    [InlineData("39 learnings-digest is not user-invocable", typeof(SquadContractTests),
        nameof(SquadContractTests.Learnings_digest_is_not_user_invocable))]
    [InlineData("41 Copilot factory command name differs from plugin name", typeof(SquadContractTests),
        nameof(SquadContractTests.Copilot_factory_command_name_differs_from_plugin_name))]
    public void Each_ticket_criterion_has_a_proving_test(string criterion, Type fixture, string method)
    {
        Assert.True(
            fixture.GetMethod(method) is not null,
            $"{criterion} is prose-only: missing {fixture.Name}.{method}.");
    }
}
