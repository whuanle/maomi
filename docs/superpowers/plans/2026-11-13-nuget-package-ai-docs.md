# Maomi NuGet Package Metadata and AI Usage Docs Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Centralize the version of every framework NuGet package, update links to the current documentation site, and include a package-specific usage guide in each generated `.nupkg` for AI coding assistants.

**Architecture:** Add a root `Directory.nuget.props` containing the shared `PackageVersion` and common NuGet metadata/packing rules, then explicitly import it from the seven packable projects under `framework/`. Keep `Directory.Packages.props` focused on dependency package versions. Add a root-level `USAGE.md` to each framework package project and make sure each file is packed beside `README.md`; update documentation links in package metadata and repository-facing references.

**Tech Stack:** .NET SDK/MSBuild props and csproj files, NuGet packing, Markdown.

---

### Task 1: Centralize the framework package version and common package metadata

**Files:**
- Create: `Directory.nuget.props`
- Modify: `framework/Maomi.Core/Maomi.Core.csproj`
- Modify: `framework/Maomi.EventBus/Maomi.EventBus.csproj`
- Modify: `framework/Maomi.I18n/Maomi.I18n.csproj`
- Modify: `framework/Maomi.I18n.AspNetCore/Maomi.I18n.AspNetCore.csproj`
- Modify: `framework/Maomi.I18n.Wpf/Maomi.I18n.Wpf.csproj`
- Modify: `framework/Maomi.Swagger/Maomi.Swagger.csproj`
- Modify: `framework/Maomi.Web.Core/Maomi.Web.Core.csproj`
- Test: `dotnet msbuild framework/Maomi.Core/Maomi.Core.csproj -getProperty:PackageVersion`
- Test: `dotnet msbuild framework/Maomi.Web.Core/Maomi.Web.Core.csproj -getProperty:PackageVersion`

- [x] **Step 1: Add a shared props file for framework NuGet packages**

Create `Directory.nuget.props` with the common version and a shared `USAGE.md` pack rule. Keep package dependency versions in `Directory.Packages.props` unchanged.

```xml
<Project>
  <PropertyGroup>
    <PackageVersion>2.3.0</PackageVersion>
    <PackageProjectUrl>https://docs.whuanle.cn/zh/maomi_framework</PackageProjectUrl>
  </PropertyGroup>

  <ItemGroup>
    <None Remove="USAGE.md" />
    <None Include="USAGE.md" Pack="true" PackagePath="\" />
  </ItemGroup>
</Project>
```

Remove and explicitly re-include the local guide so the pack metadata is applied consistently across the framework projects' SDK item defaults.

- [x] **Step 2: Import the shared props in each packable framework project**

At the top of each of the seven package project files, immediately after `<Project ...>`, add:

```xml
  <Import Project="..\..\Directory.nuget.props" />
```

Remove each project's local `<PackageVersion>...</PackageVersion>` and `<PackageProjectUrl>...</PackageProjectUrl>` entries. Do not import this file into test, demo, or template projects, and do not change the legacy `Maomi.Core`/`Maomi.Web.Core` dependency entries in `Directory.Packages.props`.

- [x] **Step 3: Verify the shared version is evaluated for all package projects**

Run:

```powershell
Get-ChildItem framework -Directory | ForEach-Object {
  $project = Join-Path $_.FullName ($_.Name + '.csproj')
  if (Test-Path $project) {
    $result = dotnet msbuild $project -getProperty:PackageVersion 2>$null
    if ($LASTEXITCODE -eq 0 -and $result -match 'PackageVersion') { "$($_.Name): $result" }
  }
}
```

Expected: all seven packable framework projects report `2.3.0`; test projects do not acquire a package version by importing `Directory.nuget.props`.

### Task 2: Update documentation links

**Files:**
- Modify: `Readme.md`
- Modify: `templates/MaomiDemo.Api/MaomiDemo.Api.csproj`
- Modify: `demo/6/mmurl/mmurl.csproj`
- Covered by Task 1: the seven framework package project URLs
- Test: repository search for old documentation domains

- [x] **Step 1: Replace stale documentation URLs in repository-facing references**

Use `https://docs.whuanle.cn/zh/maomi_framework` as the documentation URL in `Readme.md`, `templates/MaomiDemo.Api/MaomiDemo.Api.csproj`, and `demo/6/mmurl/mmurl.csproj`. Task 1 sets the same URL for all framework NuGet packages.

- [x] **Step 2: Check for stale framework documentation domains**

Run:

```powershell
rg -n 'maomi\.whuanle\.cn|maonmi\.whuanle\.cn' -g '*.md' -g '*.csproj' -g '*.props' .
```

Expected: `rg` returns no matches for the old domains. Confirm the new URL appears in the root README, the package props file, and any package metadata that intentionally repeats it. Other unrelated external links are unchanged.

### Task 3: Add package usage guides for AI and pack them into NuGet archives

**Files:**
- Create: `framework/Maomi.Core/USAGE.md`
- Create: `framework/Maomi.EventBus/USAGE.md`
- Create: `framework/Maomi.I18n/USAGE.md`
- Create: `framework/Maomi.I18n.AspNetCore/USAGE.md`
- Create: `framework/Maomi.I18n.Wpf/USAGE.md`
- Create: `framework/Maomi.Swagger/USAGE.md`
- Create: `framework/Maomi.Web.Core/USAGE.md`
- Shared packaging rule: `Directory.nuget.props`
- Test: generated `.nupkg` contents for all seven projects

- [x] **Step 1: Write a focused guide for each NuGet package**

Each `USAGE.md` is written for an AI assistant that needs to make correct code changes in a consuming application. Use Chinese, concise Markdown, and package-specific content. Include package purpose and supported target frameworks, installation command using the exact package ID, prerequisites/dependencies, minimal complete setup and usage examples, important public APIs/options and lifecycle or ordering constraints, and common mistakes. Do not duplicate the repository's broad README; provide actionable instructions specific to the package and verify examples against the actual source APIs before documenting them.

Create one guide per package with these required topics:

| Package | Guide topics |
|---|---|
| `Maomi.Core` | `IModule` implementation, module registration/discovery, service registration attributes/lifetimes, application startup order |
| `Maomi.EventBus` | event definition, handler registration/attributes, publishing and dispatch, middleware/cancellation/error behavior supported by the API |
| `Maomi.I18n` | localization resources and JSON shape, resource registration, culture/resource lookup, injecting and using localizers |
| `Maomi.I18n.AspNetCore` | ASP.NET Core DI and request-culture setup, middleware order, request culture selection, data-annotation localization |
| `Maomi.I18n.Wpf` | WPF DI/setup, culture changes, binding/localizer use, UI refresh considerations supported by the implementation |
| `Maomi.Swagger` | service registration/options, API versioning and Swagger integration, endpoint/UI configuration |
| `Maomi.Web.Core` | `MaomiWebModule` integration, response models, dependencies and interaction with the framework modules |

- [x] **Step 2: Pack each package and inspect archive entries**

Run the following command for each package project, using a temporary output folder outside the repository:

```powershell
dotnet pack framework/Maomi.Core/Maomi.Core.csproj -c Release -o "$env:TEMP\maomi-nuget-check"
```

Repeat for the other six package projects. Inspect each `.nupkg` as a ZIP archive and confirm it contains root-level `README.md` and `USAGE.md`, along with its existing icon and assemblies. Confirm the generated package version is `2.3.0` and package metadata points to `https://docs.whuanle.cn/zh/maomi_framework`.

### Task 4: Validate the framework solution

**Files:**
- Test: `MaomiFramework.sln`
- Test: all seven package archives from Task 3

- [x] **Step 1: Build and run the framework tests**

Run:

```powershell
dotnet build MaomiFramework.sln --no-restore
dotnet test MaomiFramework.sln --no-build
```

Expected: build and tests pass; no package metadata or duplicate-item errors are introduced by the props import or `USAGE.md` packing rule.

Execution note: solution restore passed, but the solution build and test run did not fully pass. `Maomi.Swagger` net8.0 has an existing OpenAPI asset/source issue; the solution also has unrelated existing compile/test failures (including demo projects and one EventBus test). The requested package archive and metadata checks passed; no unrelated fixes were made.

- [x] **Step 2: Review final package/version/link coverage**

Search the seven packable project files for local `<PackageVersion>` entries and old documentation domains. Expected: no framework package project defines its own version or stale URL, each package imports `Directory.nuget.props`, and each package archive includes the matching usage guide.
