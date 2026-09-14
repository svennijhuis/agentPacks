---
name: dotnet-test-patterns
description: When writing or running tests in a .NET repository.
title: .NET test patterns
language: C#
commands: shape, boundary, and commands
---

Good: `IClassFixture` for a shared factory; filtered test while implementing.
Bad: start Testcontainers in the constructor; full-suite every TDD cycle.
