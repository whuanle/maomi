# NuGet Usage Documentation Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make all package `USAGE.md` files practical, accurate entry points that explain real workflows and important boundaries.

**Architecture:** Keep documentation beside each packable project and derive claims from its public APIs and current implementation. Prioritize Maomi.Core module composition and Maomi.I18n resource lifecycle, then align the five integration/package guides with the same copy-run-troubleshoot structure.

**Tech Stack:** Markdown, C# examples, XML project snippets; validate links, code signatures, and repository diff without changing runtime behavior.

---

### Task 1: Clarify Maomi.Core module composition

**Files:**
- Modify: `framework/Maomi.Core/USAGE.md`

- [x] Explain one or multiple `IModule` types per assembly, module dependency graphs, and the difference between explicit `[InjectModule<T>]` references and `CustomAssembies` discovery (which selects only one module per custom assembly).
- [x] Add a practical multi-project setup, DI registration example, scan boundaries, and source-verified caveats.
- [x] Verify every documented overload and attribute against `ModuleExtensions.cs`, `ModuleBuilder.cs`, and `ModuleOptions.cs`.

### Task 2: Explain Maomi.I18n resources and runtime changes

**Files:**
- Modify: `framework/Maomi.I18n/USAGE.md`

- [x] Present a complete JSON/localizer setup and explain global versus assembly-scoped resources.
- [x] Document how to add, replace, and remove `I18nResource` entries through the singleton `I18nResourceFactory`, including keeping `Resources` and `SupportedCultures` consistent.
- [x] Explain generic localizer resource snapshots, request-scope refresh, default fallback behavior, and custom mutable resources.
- [x] Validate all examples against the factory, resource, and localizer implementations.

### Task 3: Improve integration guides

**Files:**
- Modify: `framework/Maomi.I18n.AspNetCore/USAGE.md`
- Modify: `framework/Maomi.I18n.Wpf/USAGE.md`
- Modify: `framework/Maomi.Swagger/USAGE.md`
- Modify: `framework/Maomi.EventBus/USAGE.md`
- Modify: `framework/Maomi.Web.Core/USAGE.md`

- [x] Make each guide start with a runnable setup and show its package-specific workflow.
- [x] Link related package guides and distinguish startup-time configuration from genuinely live changes.
- [x] Preserve useful limitations and troubleshooting details supported by source.

### Task 4: Validate documentation and branch state

**Files:**
- Review: all seven `framework/**/USAGE.md` files

- [x] Check code fences, package versions, target frameworks, referenced API names, and local relative links.
- [x] Inspect `git diff --check` and the final changed-file list; do not modify runtime code or commit.
