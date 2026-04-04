
# Copilot Instructions (global)

**IMPORTANT: Project-specific instructions, skills, and agent rules ALWAYS take precedence over these global rules. This is MANDATORY. If any project, skill, or agent instruction conflicts with a global rule, the project/skill/agent instruction MUST be followed without exception.**

**Note:** This file is written in English for clarity and consistency. All code comments must be in concise English.

## Naming Conventions (C#)
- Private fields: Start with an underscore, then camelCase (e.g., _myField)
- Properties: PascalCase
- Local variables: camelCase
- Method/function names: PascalCase, descriptive

## Code Comments & Documentation
- All code comments must be in English, short and to the point
- Function/method comments: Only if explicitly requested by a skill
- Public methods: No documentation required (unless for public APIs/libraries)

- Always use 4 spaces per indentation level (no tabs)
- Always use full if-statements with braces; braces on their own line
- Methods: Validate all arguments at the beginning (guard clauses/early return)
- The "happy path" should be as un-nested and clear as possible
- Avoid magic numbers: Always define values as constants (readonly fields are also allowed)
- For strings, always use string interpolation (e.g., $"{variable}") instead of concatenation
- No empty catch blocks – always handle or log errors
- Even for methods returning void or Task, prefer an explicit return; at the end (optional, see C# best practices)

## Usings & Namespaces
- Use GlobalUsings in a dedicated file (e.g., GlobalUsings.cs)
- Always use file-scoped namespaces (C# 10+)
- Remove all unused usings from each file
- Use usings for types instead of fully qualifying namespaces

## Loops & LINQ
- Prefer LINQ methods (e.g., .ForEach() on List<T>) where possible; otherwise, use a standard foreach loop

- In WinUI and WPF projects, always use the MVVM pattern with CommunityToolkit.Mvvm
  - Bind ViewModels to Views primarily in code-behind via DataContext, not in XAML (unless strictly necessary)
- Prefer asynchronous methods (async/await) wherever reasonable and possible

- Unless otherwise specified, always use the latest stable .NET and C# versions (no previews)
- Always enable and use Nullable Reference Types (`<Nullable>enable</Nullable>` in csproj)
- Before every commit, automatically format the project with 'dotnet format' (consider automating via Git hook)



## Accessibility (for UI projects)
- Ensure all UI components are accessible (keyboard navigation, screen reader support, etc.).

## Security
- Never commit secrets, passwords, or sensitive data to the repository.
- Use environment variables or secure vaults for configuration secrets.

## Performance
- Avoid premature optimization, but profile and address performance bottlenecks as needed.
- Use async streams and value types where appropriate for high-performance scenarios.

---
