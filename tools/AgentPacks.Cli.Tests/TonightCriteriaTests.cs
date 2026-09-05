namespace AgentPacks.Cli.Tests;

/// <summary>
/// Po 1–9 are done only when a named proving test exists. A checked box in the PR is not evidence.
/// Uses the existing xunit suite — no new framework.
/// </summary>
public sealed class TonightCriteriaTests
{
    [Theory]
    [InlineData("1 /squad+/build flow", typeof(DeliveryLoopContractTests),
        nameof(DeliveryLoopContractTests.Two_user_invoked_entrypoints_are_build_and_review))]
    [InlineData("2 README flow", typeof(DeliveryLoopContractTests),
        nameof(DeliveryLoopContractTests.Readme_mirrors_the_orchestrator_numbered_flow))]
    [InlineData("3 /review", typeof(DeliveryLoopContractTests),
        nameof(DeliveryLoopContractTests.Review_is_dual_axis_and_startable_for_pr_uncommitted_and_main))]
    [InlineData("4 models.source.json", typeof(ModelCatalogTests),
        nameof(ModelCatalogTests.Claude_receives_the_mapped_tier_and_copilot_and_codex_emit_model))]
    [InlineData("5 learnings contract", typeof(DeliveryLoopContractTests),
        nameof(DeliveryLoopContractTests.Learnings_log_is_append_only_and_read_first))]
    [InlineData("6 apply fixture", typeof(LearningsLogTests),
        nameof(LearningsLogTests.A_prior_fail_forces_the_skipped_agent_on_the_next_gate))]
    [InlineData("7 coworker docs", typeof(DeliveryLoopContractTests),
        nameof(DeliveryLoopContractTests.Readme_mirrors_the_orchestrator_numbered_flow))]
    [InlineData("8 verify-path / evidence gate", typeof(VerificationEvidenceTests),
        nameof(VerificationEvidenceTests.Criterion_command_that_never_covers_is_not_verified_and_not_a_pass))]
    [InlineData("9 loop-only slots", typeof(LanguagePackContractTests),
        nameof(LanguagePackContractTests.A_slot_skill_without_loop_audience_is_rejected))]
    public void Each_ticket_criterion_has_a_proving_test(string criterion, Type fixture, string method)
    {
        Assert.True(
            fixture.GetMethod(method) is not null,
            $"{criterion} is prose-only: missing {fixture.Name}.{method}.");
    }
}
