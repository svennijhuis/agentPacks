---
name: dotnet-build
description: When building, restoring, or formatting a .NET repository.
title: .NET build
language: C#
intro: >-
  The facts an agent needs before it touches a `.csproj`. Read the repository's own files first;
  the common layout is not a guarantee. When opening a solution or package graph, load
  `dotnet-solution` by exact Skill tool name. Use local `dotnet sln list` / `dotnet list package`.
  Never a remote service.
commands: commands, packages, and failures
---

Good: version in `Directory.Packages.props`, Version-less `PackageReference`.
Bad: put `Version` on the csproj under CPM.
