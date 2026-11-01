# Notes: poteto (Lauren Tan), "here's how i shipped 2,500 PRs last month to production"

Source: https://x.com/poteto/status/2102050467505430555 (2026-09-21, 38:02). Built from the machine transcript in `2500prs-transcript.md`. Timestamps are mm:ss. Quotes are taken from the transcript; anything in *italics* is my paraphrase. The audio could not show slides, so slide-only content (screenshots, the trust-graph chart, the code examples) is missing.

**What the talk is not about.** She does not cover planning, playbooks, subagent fan-out, review loops, merge policy, model choice or context budgets in any detail. pstack and its playbooks get one sentence each (13:18, 13:38). Under those themes below I write "not covered" rather than fill gaps. The talk has one thesis: **trust**. You earn it by building verification, encoding knowledge into the codebase, and running an outer loop.

## 0. Thesis and framing
| # | Item | Time |
|---|---|---|
| 0.1 | Says she "shipped 2,000 pull requests to production" last month. The post title says 2,500. She repeats "I never set out to ship 2000 requests a month... not a goal". | 00:09, 04:24 |
| 0.2 | Volume comes from **trust**: "how I can trust my agents to produce high quality work even when I'm not there". | 00:17 |
| 0.3 | Thesis: if you set up the agents' environment well, you get a personal or team "software factory" that produces high-quality code much faster. | 00:36 |
| 0.4 | She prefers the **Michelin kitchen** analogy to "factory". Building is creative work. You no longer cook the components yourself, but you own the final outcome and the kitchen setup: line cooks, sous chefs, equipment, training, the ratio of dishwashers to cooks. | 00:58-01:53 |
| 0.5 | Origin story: six months earlier she joined Cursor with no skills, in a fresh codebase (the new Cursor agents window), to fix performance issues. She had a React-team background. | 02:03-03:00 |
| 0.6 | Pain point: an "insurmountable wall of pull requests" with no way to tell whether performance regressed. She did manual Chrome DevTools traces and heap snapshots until she thought: "wait, we have agents, what am I doing?" | 03:09-03:52 |
| 0.7 | Mindset: "I am the bottleneck". Put your engineering knowledge into the team of agents so you are not the blocker for everything. | 04:49-05:05 |
| 0.8 | All the skills, tools and codebase changes "laddered up to this idea of trust". | 04:32 |

## 1. The trust ladder (autonomy)
| # | Item | Time |
|---|---|---|
| 1.1 | Stage "1 to 5 agents": you babysit every chat and keep course-correcting. If you are absent, nothing happens or the agents do the wrong thing. | 05:28 |
| 1.2 | This stage is the **hardest to leave**, because the way out is unclear. The answer is trust. | 05:56 |
| 1.3 | Going from 1-5 agents to around **100** fails without trust: "a ton of slop requests", regressions and shipped bugs. | 06:16-06:40 |
| 1.4 | Closing: climb the "Trust Graph" until you can parallelize and the whole team builds on the shared agent infrastructure. | 35:16 |
| 1.5 | End state: "you trust the environment so much that your agents can just be free". | 37:18 |

## 2. Verification and testing (largest practical section)
| # | Item | Time |
|---|---|---|
| 2.1 | Verification has levels. Low end: **verification skills** that teach the agent to run the app, drive it over Chrome DevTools Protocol (or another debug protocol), take performance traces and heap snapshots. | 07:05-07:41 |
| 2.2 | High end: **formal verification** (Lean, TLA+) to prove business-logic invariants always hold. "Very few people really do" this; skills already get you "very far". | 07:47-08:35 |
| 2.3 | First skill she built at Cursor: **"control glass"** (name as heard), which runs the app and takes traces over CDP. | 08:40 |
| 2.4 | Goal of verification skills: the agent runs the app, reads traces, finds hot spots and hill-climbs performance automatically. | 03:56 |
| 2.5 | A control/verification skill has **two parts** (reached by iteration). Part 1 is a **CLI inside the skill directory**, so runs and evidence collection are reproducible. Without it, each session writes its own one-off scripts that "differ between agent sessions". | 08:52-09:50 |
| 2.6 | Invest in that CLI so it covers many use cases and runs the app correctly. | 09:58 |
| 2.7 | Evidence must be **empirical**: proof the code works and "your performance bar is being met". | 09:30 |
| 2.8 | Part 2 is a **feature map**, a term she coined after a site map. It is "materialized memory": how the app works, which features exist, how a user reaches them (keyboard shortcuts, which DOM elements to click) and what each feature does. | 10:16-11:20 |
| 2.9 | Why the feature map exists: Slack reports arrived as a tiny vague screenshot with "three question marks". Agents could run the app but only guessed what the user meant. | 10:30-10:55 |
| 2.10 | The feature map lives **in the skill directory in the repo** and an **automation keeps it up to date**. | 11:25-11:35 |
| 2.11 | CLI plus feature map: agents can reproduce, trace, and understand requests from internal and external users. | 11:41-11:53 |
| 2.12 | The control skills became **"critical infrastructure"** that the team keeps maintaining. | 12:02 |
| 2.13 | Verification answers *correctness* ("does the checkout button actually check out the cart?") but says little about performance or code quality. Quality needs separate skills (section 3). | 12:34-13:12 |
| 2.14 | With verification you collect real perf metrics, numbers, statistics and telemetry. | 14:35 |

## 3. Skills, playbooks and pstack
| # | Item | Time |
|---|---|---|
| 3.1 | **pstack**: a plugin of skills built from her own workflows, so agents "work like real software engineers". | 13:12-13:38 |
| 3.2 | It ships playbooks and skills for task types: "debugging, feature development, programming". She gives no details. | 13:38-14:00 |
| 3.3 | Senior engineers should build a **team repository of skills** that makes agents smarter. | 14:01 |
| 3.4 | Engineering-quality skills combined with verification skills mean agents check that work is both **correct** and **high quality**. | 14:16 |
| 3.5 | *Playbook contents, planning steps, plan formats: not covered.* | - |

## 4. Agent-friendly codebase and architecture
| # | Item | Time |
|---|---|---|
| 4.1 | Refactor or rewrite the architecture to be agent-friendly, "one of the most important things you can do". If agents will write all the code, the codebase must "do the right thing by default". | 14:56-15:20 |
| 4.2 | "The code base is really the best form of memory": agents extend patterns already in their context. They won't refactor in every PR, they extend what exists. | 15:41-16:27 |
| 4.3 | Agents "love taking shortcuts", so design so that **the easy path is the right path**. | 19:25-19:41 |
| 4.4 | A locked-down codebase is "annoying for humans" but "the perfect environment for agents", above all for agents with little context. | 19:47-19:59 |
| 4.5 | Contributors are no longer only engineers. Designers, PMs and CEOs ship features, so agents driven by "busy people with not a lot of context" must still do well by default. | 19:59-20:32 |
| 4.6 | The reverse also holds: **anti-patterns spread like a virus**. One small workaround or explaining comment gets copied, and "in a matter of a few days or a few weeks" it becomes the de facto pattern. | 20:44-21:34 |
| 4.7 | Result if unchecked: "a very vibe coded code base", painful to maintain, with many performance issues. | 22:05-22:29 |
| 4.8 | Example: **comments in code**. Agents used the comments as justification for not solving the real problem and applied a band-aid instead. Dune, the framework behind Grok Bot, **bans comments** so the pattern can't spread. | 22:49-24:17 |
| 4.9 | **Gardener role**: every team needs someone who removes weeds and pests (bad patterns) "as soon as possible before they start propagating". | 24:24-25:11 |
| 4.10 | Dune principle 1: **delete existing tech debt**. | 25:18 |
| 4.11 | Dune principle 2: **one paved path** for blessed patterns, "one conventional way to do some things so that agents don't really need to guess", with enough guidance in code, CI and lint rules. | 25:24-25:50 |
| 4.12 | Dune principle 3: when you see tech debt or a bad pattern, your instinct is **"I need to write a lint rule against it"**. That "stops the bleeding" even before cleanup. Then have agents clean up, so the codebase is always in a state "where you would be happy if an agent were to copy it". | 25:51-26:40 |
| 4.13 | Dune conventions: where code lives and how it may be imported. Features are **co-located in one folder**. There is an entry point, like a route, in the React part, plus transcript parts, a host on the Grok Bot VM, and a client. Strict boundaries separate these "rings". | 27:02-27:57 |
| 4.14 | Concrete boundary: Electron main-thread code may not run on the renderer thread. Lesson from the Cursor agents window, where slow code was imported into the renderer by accident. | 27:57-28:20 |
| 4.15 | Numbers: no long tasks over **16 ms (60 fps)** or **8 ms (120 fps)** on the renderer. Chunk work instead of doing it all at once. | 28:25-28:52 |
| 4.16 | Boundaries are **enforced in code through the import/dependency graph**. That bug class was "categorically eliminated" by architecture. | 28:52-29:10 |
| 4.17 | Core lesson: build your own **agent-friendly framework** and encode your best engineers' tribal knowledge. Move it out of style-guide review comments into the framework, "the materialized snapshot of the state" you want agents to extend. Keep it "so pristine" that the next agent continues the good pattern. | 29:16-30:40 |
| 4.18 | Payoff: in such a codebase "it's almost impossible to write bad code", so even an agent with little context or reasoning writes good code. | 31:16-31:35 |
| 4.19 | Kitchen rule: if a cook keeps tripping over something, fix the kitchen. Same for the codebase. | 32:00-32:22 |

## 5. Enforcement ladder: what to do when you correct an agent (the closing slide)
Order runs from strongest to weakest. Use it on **every** correction. *She says there are five points (15:34). At 18:38 she names the four beside the style guide: "your code base, making things categorically impossible through better data structures or algorithms, static analysis and then of course you layer that with rules and skills". Skills for code quality come on top (37:07). The slide itself could not be seen.*
| # | Layer | Time |
|---|---|---|
| 5.1 | **Codebase/architecture/data structures**: make the mistake "categorically impossible". | 16:27-17:22, 36:48 |
| 5.2 | **Static analysis**: linters, compiler diagnostics, CI. If an agent repeats a mistake, add a lint rule, or better, refactor so it can't happen. | 16:28-17:07 |
| 5.3 | **Guidance**: rules, BugBot, skills. Agents mostly use them, but they can forget to read a rule and the human driving can ignore it, so it is less enforceable. | 17:09-17:51 |
| 5.4 | **Code quality skills** on top. | 37:07 |
| 5.5 | **Style guide**: only humans enforce it in review. At current PR rates, relying on it is "impossible", a "big glaring hole". Use human review comments as the place to find **what is missing** from the other layers. | 17:51-18:44 |
| 5.6 | "Whenever you find yourself correcting and intervening", ask which layer is the most effective place to fix it, in this order. | 35:58-36:55 |

## 6. Outer loop, automation and fan-out
| # | Item | Time |
|---|---|---|
| 6.1 | Grok Bot is the **outer loop**: connectors (Slack, Datadog, Sentry, PlanetScale) aggregate signals so it can make decisions. | 32:22-33:10 |
| 6.2 | You don't need a fancy "company brain", because agents are good with tools. Connect tools and let Grok Bot auto-kick off cloud agents. Little extra infrastructure needed. | 33:00-33:39 |
| 6.3 | **Grok Bot routines** subscribe to Slack or Sentry alerts and start work automatically. | 33:45 |
| 6.4 | Code, rules and skills **compound**. Grok Bot responds to outer-loop events and launches cloud agents. Cursor Automations and the Cursor SDK build more bots that reuse the same agent infrastructure. | 33:54-34:35 |
| 6.5 | Shown on screenshots: automations that **auto-reproduce bug reports** and **auto-open PRs**. | 34:40-35:05 |
| 6.6 | Warning: spawning 100 subagents or cloud agents **before** you have trust gives slop and regressions. | 06:25 |
| 6.7 | *How many parallel agents she runs, and how fan-out is orchestrated: not covered.* | - |

## 7. Review, merging and shipping
| # | Item | Time |
|---|---|---|
| 7.1 | Line-by-line human review against a style guide does not scale at this PR volume. Encode it in the lower layers. | 18:12-18:44 |
| 7.2 | BugBot is named as a guidance layer. | 17:09, 30:58 |
| 7.3 | *Merge policy, review loops, CI gates before merge, rollout: not covered.* | - |

## 8. Model choice and context
| # | Item | Time |
|---|---|---|
| 8.1 | Context: LLMs reuse what is in their context window, and files they open are part of it, so the code they read decides their output. | 15:57-16:20 |
| 8.2 | Design for **low-context, low-reasoning agents** to succeed. | 20:20, 31:21 |
| 8.3 | The feature map is persistent memory, so agents don't have to guess app structure each session. | 11:02 |
| 8.4 | *Specific models, reasoning levels, token budgets: not covered.* | - |

## 9. Failures and lessons
| # | Item | Time |
|---|---|---|
| 9.1 | Manual perf work could not keep up with the PR rate. That led to verification skills. | 03:09-03:56 |
| 9.2 | The first version of the control skill had no CLI and no feature map. She "iterated my way to this". | 08:52 |
| 9.3 | Agents guessed at vague screenshot bug reports. That led to the feature map. | 10:30 |
| 9.4 | Agents copied comment-justified workarounds. That led to the comment ban. | 23:30 |
| 9.5 | Slow code imported into the Electron renderer. That led to import-graph-enforced boundaries. | 28:00 |
| 9.6 | Workarounds spread across the codebase within days or weeks. That led to lint-first and the gardener role. | 21:14 |
| 9.7 | "It's not really a secret, it's a lot of hard work." | 37:36 |

## Tools and names mentioned
Chrome DevTools / CDP (03:30, 07:30, 08:50), heap snapshots and perf traces (03:35, 07:41), Lean and TLA+ (07:55), the "control glass" skill (08:43), feature map (10:16), pstack (13:18), BugBot (17:09), linters, compiler diagnostics, CI (16:28), Dune framework (19:04), Electron main and renderer (27:57), React (27:20), Slack, Datadog, Sentry, PlanetScale (32:45), Grok Bot routines (33:45), cloud agents (33:20), Cursor Automations and SDK (34:20).

## Numbers
2,000 PRs last month (the post title says 2,500) · 6 months at Cursor/SpaceX AI · 1-5 agents vs about 100 · a few days to a few weeks for a workaround to spread · 16 ms per frame at 60 fps · 8 ms at 120 fps · 5-layer enforcement ladder · 3 Dune principles · 2-part verification skill (CLI plus feature map).
