namespace AgentPacks.Cli.Tests;

/// <summary>
/// Named proving tests for executable contracts. Docs and skill wording is reviewed,
/// not ticket-locked with <c>Assert.Contains</c>.
/// </summary>
public sealed class TonightCriteriaTests
{
    [Theory]
    [InlineData("1 /squad+/squad-review flow", typeof(SquadContractTests),
        nameof(SquadContractTests.Two_user_invoked_entrypoints_are_squad_and_review))]
    [InlineData("3 command inventory", typeof(SquadContractTests),
        nameof(SquadContractTests.Squad_commands_are_exactly_squad_and_review))]
    [InlineData("4 models.source.json", typeof(ModelCatalogTests),
        nameof(ModelCatalogTests.Claude_receives_the_mapped_tier_and_copilot_and_codex_emit_model))]
    [InlineData("6 apply fixture", typeof(LearningsLogTests),
        nameof(LearningsLogTests.A_prior_fail_forces_the_skipped_agent_on_the_next_gate))]
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
    [InlineData("15 empty squad mcp", typeof(PluginMcpContractTests),
        nameof(PluginMcpContractTests.Squad_mcp_is_empty_scaffold_and_docs_example_is_read_only_http_without_secrets))]
    [InlineData("16 no shipped Dotnet MCP / empty scaffolds", typeof(PluginMcpContractTests),
        nameof(PluginMcpContractTests.No_dotnet_solution_mcp_project_and_plugin_mcp_servers_are_empty_scaffolds))]
    [InlineData("21 no DotnetSolutionMcp empty mcpServers", typeof(PluginMcpContractTests),
        nameof(PluginMcpContractTests.No_dotnet_solution_mcp_project_and_plugin_mcp_servers_are_empty_scaffolds))]
    [InlineData("22 squad-* agents", typeof(SquadContractTests),
        nameof(SquadContractTests.No_user_facing_loop_star_agents_are_squad_star))]
    [InlineData("23 typescript pack required slots loop audience", typeof(LanguagePackContractTests),
        nameof(LanguagePackContractTests.Typescript_pack_fills_required_slots_with_loop_audience))]
    [InlineData("25 learnings-digest user-invoked", typeof(SquadContractTests),
        nameof(SquadContractTests.Learnings_digest_is_user_invoked_matt_tiny_no_rewrite))]
    [InlineData("27 caveman pins", typeof(SquadContractTests),
        nameof(SquadContractTests.Caveman_external_pins_and_squad_invokes_by_exact_name))]
    [InlineData("30 verifier rejects happy-path-only coverage", typeof(VerificationEvidenceTests),
        nameof(VerificationEvidenceTests.Happy_path_only_agent_written_tests_are_rejected))]
    [InlineData("31 skill bodies references/standards only", typeof(LanguagePackContractTests),
        nameof(LanguagePackContractTests.Skill_bodies_point_only_at_references_standards))]
    [InlineData("32 test plan matrix", typeof(VerificationEvidenceTests),
        nameof(VerificationEvidenceTests.Blank_or_unconfirmed_seam_and_unnamed_internal_hits_are_not_verified))]
    [InlineData("33 Copilot user-invocable false", typeof(LanguagePackContractTests),
        nameof(LanguagePackContractTests.Copilot_emits_user_invocable_false_for_loop_audience))]
    [InlineData("34 Codex tiers map to real ids", typeof(ModelCatalogTests),
        nameof(ModelCatalogTests.Codex_tiers_map_to_real_ids_not_all_inherit))]
    [InlineData("34 empty mcp scaffolds", typeof(SquadContractTests),
        nameof(SquadContractTests.Empty_mcp_scaffolds_test_is_not_named_dotnet_only))]
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
    [InlineData("42 dotnet-review examples standards.source kept", typeof(LanguagePackContractTests),
        nameof(LanguagePackContractTests.Dotnet_review_examples_matt_tiny_standards_source_kept))]
    [InlineData("43 lang test-patterns local vs deployed smoke examples", typeof(SquadContractTests),
        nameof(SquadContractTests.Lang_test_patterns_local_vs_deployed_smoke_examples))]
    [InlineData("43 smoke-matrix not user slash no new plugin", typeof(SquadContractTests),
        nameof(SquadContractTests.Smoke_matrix_not_user_slash_no_new_plugin))]
    [InlineData("44 user commands stay squad squad-review pack-check scenarios security-audit", typeof(HttpScenariosContractTests),
        nameof(HttpScenariosContractTests.User_commands_stay_squad_squad_review_pack_check_plus_http_scenarios))]
    [InlineData("47 Claude marketplace omits hooks path and array", typeof(ClaudeMarketplaceHooksTests),
        nameof(ClaudeMarketplaceHooksTests.Claude_marketplace_omits_hooks_path_and_array))]
    [InlineData("47 Claude plugin root hooks json is Claude shaped", typeof(ClaudeMarketplaceHooksTests),
        nameof(ClaudeMarketplaceHooksTests.Claude_plugin_root_hooks_json_is_claude_shaped))]
    [InlineData("47 Copilot ships scenarios command", typeof(HttpScenariosContractTests),
        nameof(HttpScenariosContractTests.Copilot_ships_http_scenarios_command))]
    [InlineData("48 all plugins version 0.1.3", typeof(PluginVersionContractTests),
        nameof(PluginVersionContractTests.All_plugins_version_0_1_3))]
    [InlineData("49 Copilot_http_scenarios_renamed_to_scenarios", typeof(HttpScenariosContractTests),
        nameof(HttpScenariosContractTests.Copilot_http_scenarios_renamed_to_scenarios))]
    [InlineData("49 All_trees_command_is_scenarios", typeof(HttpScenariosContractTests),
        nameof(HttpScenariosContractTests.All_trees_command_is_scenarios))]
    [InlineData("49 User_slash_is_scenarios_not_http_scenarios", typeof(HttpScenariosContractTests),
        nameof(HttpScenariosContractTests.User_slash_is_scenarios_not_http_scenarios))]
    [InlineData("50 Copilot_hooks_set_cwd_plugin_root", typeof(ClaudeMarketplaceHooksTests),
        nameof(ClaudeMarketplaceHooksTests.Copilot_hooks_set_cwd_plugin_root))]
    [InlineData("Cursor relocated hooks when Claude owns root", typeof(ClaudeMarketplaceHooksTests),
        nameof(ClaudeMarketplaceHooksTests.Cursor_loads_relocated_cursor_shaped_hooks_when_claude_owns_root))]
    [InlineData("51 Copilot_scenarios_command_name_differs_from_skill", typeof(HttpScenariosContractTests),
        nameof(HttpScenariosContractTests.Copilot_scenarios_command_name_differs_from_skill))]
    [InlineData("51 Slash_stays_scenarios_or_squad_scenarios", typeof(HttpScenariosContractTests),
        nameof(HttpScenariosContractTests.Slash_stays_scenarios_or_squad_scenarios))]
    [InlineData("52 Skill_descriptions_are_short_when_to_use", typeof(SkillHygieneContractTests),
        nameof(SkillHygieneContractTests.Skill_descriptions_are_short_when_to_use))]
    [InlineData("52 Skill_bodies_progressive_disclosure_to_references", typeof(SkillHygieneContractTests),
        nameof(SkillHygieneContractTests.Skill_bodies_progressive_disclosure_to_references))]
    [InlineData("52 No_astra_personality_in_portable_skills", typeof(SkillHygieneContractTests),
        nameof(SkillHygieneContractTests.No_astra_personality_in_portable_skills))]
    [InlineData("official-schema Cursor catalog ships", typeof(CursorCatalogContractTests),
        nameof(CursorCatalogContractTests.Official_schema_cursor_catalog_ships))]
    [InlineData("official-schema Cursor catalog rejects extra fields", typeof(CursorCatalogContractTests),
        nameof(CursorCatalogContractTests.Official_schema_cursor_catalog_rejects_extra_entry_fields))]
    [InlineData("remapped squad agents load", typeof(CursorCatalogContractTests),
        nameof(CursorCatalogContractTests.Remapped_squad_agents_load))]
    [InlineData("lean one-job CI", typeof(SquadContractTests),
        nameof(SquadContractTests.Pull_request_ci_stays_one_job_no_matrix))]
    [InlineData("no twin of closed #27/#29/#30", typeof(CursorCatalogContractTests),
        nameof(CursorCatalogContractTests.Cursor_catalog_is_the_sole_catalog_track))]
    [InlineData("53 A1 Seam column + blank-seam fail", typeof(SquadContractTests),
        nameof(SquadContractTests.Test_plan_matrix_seam_column_blank_or_unnamed_internal_is_not_verified))]
    [InlineData("56 B6 Anti-reentry on three agents", typeof(SquadContractTests),
        nameof(SquadContractTests.Review_agents_have_anti_reentry_on_every_provider_tree))]
    [InlineData("Still_five_user_commands", typeof(SquadContractTests),
        nameof(SquadContractTests.Still_five_user_commands))]
    [InlineData("squad SKILL.md not grown", typeof(SquadContractTests),
        nameof(SquadContractTests.Squad_skill_body_is_not_grown))]
    [InlineData("58 four bodies in references pointers in SKILL", typeof(SquadContractTests),
        nameof(SquadContractTests.Squad_skill_progressive_disclosure_moves_four_bodies_to_references))]
    [InlineData("59 HTTP collection envelope over root array", typeof(LanguagePackContractTests),
        nameof(LanguagePackContractTests.Language_packs_map_http_api_to_shared_path))]
    [InlineData("62 command names differ from skill and plugin", typeof(LanguagePackContractTests),
        nameof(LanguagePackContractTests.Command_names_differ_from_skill_and_plugin_names))]
    [InlineData("64 shared http-api emit", typeof(StandardsGenerationTests),
        nameof(StandardsGenerationTests.Shared_path_generates_into_consumer_references_without_a_pack_copy))]
    [InlineData("65 blocked row parsed distinct from not verified", typeof(VerificationEvidenceTests),
        nameof(VerificationEvidenceTests.Blocked_row_is_parsed_distinct_from_not_verified_and_is_not_a_pass))]
    [InlineData("66 measured value against bound gate", typeof(VerificationEvidenceTests),
        nameof(VerificationEvidenceTests.Quantitative_criterion_needs_a_measured_value_against_the_bound))]
    [InlineData("68 compile-only command gate", typeof(VerificationEvidenceTests),
        nameof(VerificationEvidenceTests.Compile_only_command_is_not_evidence_for_a_behavioral_criterion))]
    public void Each_ticket_criterion_has_a_proving_test(string criterion, Type fixture, string method)
    {
        Assert.True(
            fixture.GetMethod(method) is not null,
            $"{criterion} is prose-only: missing {fixture.Name}.{method}.");
    }
}
