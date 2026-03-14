# Repository Guidelines

## Project Structure & Module Organization
- `src/MultiView.DynamicViews.Domain`: view contracts, enums, and definition models only.
- `src/MultiView.DynamicViews.Core`: runtime services (serialization, caching, rule evaluation, action dispatch, DI extensions).
- `src/MultiView.DynamicViews.Blazor`: UI rendering layer (`Components/Views`, `Components/Fields`, `Components/Widgets`).
- `src/MultiView.DynamicViews.Sample`: runnable Blazor sample app (`Definitions/*.json`, `Data/`, `Actions/`, `Models/`, `wwwroot/`).
- Solution entry point: `MultiView.slnx`.
- Roadmap and product notes: `ROADMAP.md`, `README.md`.

## Build, Test, and Development Commands
- `dotnet restore MultiView.slnx`: restore all dependencies.
- `dotnet build MultiView.slnx -c Debug`: build all projects.
- `dotnet run --project src/MultiView.DynamicViews.Sample`: run the sample application locally.
- `dotnet test MultiView.slnx`: run tests when test projects are present.
- Optional quality pass: `dotnet format` before opening a PR.

## Coding Style & Naming Conventions
- Language targets: C# (`net10.0`) with `Nullable` and `ImplicitUsings` enabled.
- Use 4-space indentation and standard .NET formatting conventions.
- Naming:
  - `PascalCase` for types, public members, and Razor components (`DynamicListView.razor`).
  - `camelCase` for locals/parameters.
  - Interfaces start with `I` (`IViewActionDispatcher`).
- Keep Domain free of UI and infrastructure concerns; place business-agnostic rendering in Blazor and logic/services in Core.

## Testing Guidelines
- Current repository has no dedicated test project; add tests under `tests/` (for example `tests/MultiView.DynamicViews.Core.Tests`) for new behavior in Core/Domain.
- Prefer xUnit for .NET tests, with descriptive names like `MethodName_State_ExpectedResult`.
- Focus coverage on rule evaluation, definition composition/merge, and serializer/store behavior.

## Commit & Pull Request Guidelines
- Prefer Conventional Commit style seen in history: `feat(scope): ...`, `docs: ...`.
- Keep commits focused and atomic; avoid mixed refactor + feature changes.
- PRs should include:
  - concise summary and rationale,
  - linked issue/story (if applicable),
  - test evidence (`dotnet build`, `dotnet test`, or manual sample validation),
  - screenshots/GIFs for UI changes in Blazor views.
