---
description: 'Guidelines for building C# applications'
applyTo: '**/*.cs'
---

# C# Development

## Scope & Versions
- **Priority model:** Sections below use `Required`, `Recommended`, `Optional` tags to clarify importance.
- **Supported SDKs:** Supported: .NET 8 (minimum). Recommended: .NET 10 (LTS). Specify project requirements in the repository README when stricter constraints exist.

## Quick Start (Required)
- Create a new web API project (example):

```powershell
dotnet new webapi -n MyApi
cd MyApi
dotnet restore
dotnet build
dotnet run
```

- Run tests and formatting locally:

```powershell
dotnet test
dotnet tool run dotnet-format -- --folder
```

- Publish a Linux container (example):

```powershell
dotnet publish --os linux --arch x64 -c Release -p:PublishProfile=DefaultContainer
```

## C# Instructions

- Always use the latest version C#, currently C# 14 features.
- Write clear and concise comments for each function.

## General Instructions

- Make only high confidence suggestions when reviewing code changes.
- Write code with good maintainability practices, including comments on why certain design decisions were made.
- Handle edge cases and write clear exception handling.
- For libraries or external dependencies, mention their usage and purpose in comments.

- **Explicit Returns (Required):** All methods SHOULD include an explicit `return` statement. Rationale: explicit returns make control flow and exit points obvious to readers and reduce accidental fall-through behaviour. For `void` or `Task` methods prefer an explicit `return;` at the end of the method (or `return Task.CompletedTask;` for non-async Task factory methods where appropriate). For value-returning methods always use `return <value>;` rather than relying on implicit end-of-method returns.

## OOP Best Practices (Recommended)
- **DRY (Don't Repeat Yourself):** Avoid duplicated logic by extracting shared behavior into reusable methods, services or value objects.
- **SOLID principles:**
	- **Single Responsibility:** Each class should have one reason to change.
	- **Open/Closed:** Favor extension over modification; prefer well-defined extension points.
	- **Liskov Substitution:** Derived types must be substitutable for their base types.
	- **Interface Segregation:** Prefer small, focused interfaces over large, general ones.
	- **Dependency Inversion:** Depend on abstractions (interfaces) rather than concretions; prefer constructor injection.
- **Prefer Composition over Inheritance:** Use composition to assemble behavior; inherit only when there's a clear is-a relationship and no better composition exists.
- **Encapsulation & Cohesion:** Keep state private and expose behavior through methods; group related responsibilities together.
- **Loose Coupling & High Cohesion:** Design modules/services with minimal dependencies and focused responsibilities to simplify testing and maintenance.
- **Law of Demeter:** Avoid deep property chaining; call only immediate collaborators.
- **Value Objects & Immutability:** Use immutable types for small domain values; implement equality semantics where appropriate.
- **Design for Testability:** Prefer constructor injection, small interfaces, and side-effect-free methods where feasible; write unit tests for behavior, not implementation.
- **Keep Methods Small and Focused:** If a method grows complex, consider `Extract Method` or `Extract Class` refactorings.
- **Document Intent, Not Implementation:** Use short comments or design notes to explain why a design differs from the obvious alternative.
- **When to Use Patterns:** Apply patterns (Factory, Strategy, Adapter) where they simplify code or clarify intent—avoid premature abstraction.
- **Anti-Patterns to Avoid:** God objects, excessive use of static state, overuse of inheritance for code reuse, copy-paste duplication.
- **Review Checklist for OOP Design:** For non-trivial changes, verify SRP compliance, dependency directions, testability, and whether a smaller interface can be created.

**Member Order / Class Structure (Recommended)**
- **Preferred member order** (keeps classes predictable and easy to scan):
	1. File/header comments and attributes
	2. Constants and `public static readonly` fields
	3. `static` fields
	4. Instance fields (private → protected → internal → public) — prefer `_` prefix for private fields
	5. Properties (public → protected → internal → private)
	6. Constructors (static ctor first, then public → protected → internal → private)
	7. Public methods
	8. Protected/internal methods
	9. Private helper methods
 10. Nested types (classes, enums, delegates)

- **Grouping & readability:** Group related members (field + property + methods that operate on that data) together so the class tells a coherent story. Keep methods small; if a group grows large, consider `Extract Class`.

- **Enforcement:** StyleCop.Analyzers includes a rule for element ordering (SA1201/SA1202). We recommend enabling StyleCop and configuring it in CI to warn or fail the build if the project deviates from the agreed ordering.

- **Rationale:** A consistent member order reduces cognitive overhead when navigating types, accelerates code reviews, and makes diffs easier to interpret.

- **Repository Pattern (Recommended, when appropriate):**
  - Use repositories to decouple data access from domain logic and to provide test doubles for integration tests.
  - Prefer specific repositories (e.g., `IUserRepository`) over broad generic repositories to avoid leaky abstractions.
  - Do not create repository wrappers that simply mirror EF Core's API — prefer using `DbContext` directly if repository adds no value.
  - Keep transactions and unit-of-work concerns separate (service layer or dedicated component), not buried in repository methods.
  - Expose small, focused asynchronous interfaces (`Task<T>`) and avoid exposing ORM-specific types on repository interfaces.

**Repository Example (Recommended, Allman-Stil)**
```csharp
public interface IUserRepository
{
	Task<User?> GetByIdAsync(Guid id);
	Task AddAsync(User user);
	Task SaveChangesAsync();
}

public class EfUserRepository : IUserRepository
{
	private readonly AppDbContext _db;

	public EfUserRepository(AppDbContext db)
	{
		_db = db;
	}

	public async Task<User?> GetByIdAsync(Guid id)
	{
		return await _db.Users.FindAsync(id);
	}

	public async Task AddAsync(User user)
	{
		await _db.Users.AddAsync(user);
	}

	public async Task SaveChangesAsync()
	{
		await _db.SaveChangesAsync();
	}
}
```

## Naming Conventions

- Follow PascalCase for component names, method names, namespaces, and public members.
- Use camelCase with an underscore prefix (_) for private fields (e.g., `_userRepository`).
- Use camelCase for local variables and method parameters.
- Prefix interface names with "I" (e.g., IUserService).

- **Constants:** Use UPPER_CASE with underscores for constants (e.g., `DIES_IST_EINE_KONSTANTE`).

## Formatting

- Apply code-formatting style defined in `.editorconfig`.
	- Recommended: include a minimal `.editorconfig` snippet in the repo root to enforce basics (indentation, line endings, C# spacing).

```ini
[*.cs]
indent_style = space
indent_size = 4
insert_final_newline = true
charset = utf-8
dotnet_sort_system_directives_first = true
```
 - Prefer file-scoped namespace declarations and single-line using directives.
- Insert a newline before the opening curly brace of any code block (e.g., after `if`, `for`, `while`, `foreach`, `using`, `try`, etc.).
- Ensure that the final return statement of a method is on its own line.
- Use pattern matching and switch expressions wherever possible.
- Use `nameof` instead of string literals when referring to member names.
- Ensure that XML doc comments are created for any public APIs. When applicable, include `<example>` and `<code>` documentation in the comments.
 
**Example `Program.cs` (minimal for ASP.NET Core 10) — Recommended**

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
var app = builder.Build();
app.MapControllers();
app.Run();
```

## Style Enforcement (Required)
- **Brace Placement (Allman style, Required):** Opening and closing braces for control blocks and type/method declarations MUST be placed on their own lines (Allman/ANSI style). Rationale: a consistent explicit-brace style reduces visual ambiguity for long blocks and makes diffs clearer for teams who prefer explicit block delimiters.

- Recommended editorconfig snippet to enforce new-line-before-brace and require braces for control blocks:

```ini
[*.cs]
# Put opening braces on a new line for types, methods, properties, accessors and control blocks
csharp_new_line_before_open_brace = all

# Prefer explicit braces for control blocks
csharp_prefer_braces = true:warning

# Keep previously suggested settings
indent_style = space
indent_size = 4
dotnet_sort_system_directives_first = true
```

- Enforcement: enable Roslyn analyzers / StyleCop.Analyzers in the solution and treat relevant rules as warnings or errors in CI. Example analyzers to enable: SA1500/SA1503 (braces and brace spacing), and rules that enforce explicit braces for control statements. Configure `dotnet format` and `dotnet build` in CI to fail on style violations.
## Project Setup and Structure

- Guide users through creating a new .NET project with the appropriate templates.
- Explain the purpose of each generated file and folder to build understanding of the project structure.
- Demonstrate how to organize code using feature folders or domain-driven design principles.
- Show proper separation of concerns with models, services, and data access layers.
- Explain the Program.cs and configuration system in ASP.NET Core 10 including environment-specific settings.

## Nullable Reference Types

- Declare variables non-nullable, and check for `null` at entry points.
- Always use `is null` or `is not null` instead of `== null` or `!= null`.
- Trust the C# null annotations and don't add null checks when the type system says a value cannot be null.

## Data Access Patterns

- Guide the implementation of a data access layer using Entity Framework Core.
- Explain different options (SQL Server, SQLite, In-Memory) for development and production.
- Demonstrate repository pattern implementation and when it's beneficial.
- Show how to implement database migrations and data seeding.
- Explain efficient query patterns to avoid common performance issues.

## Authentication and Authorization

- Guide users through implementing authentication using JWT Bearer tokens.
- Explain OAuth 2.0 and OpenID Connect concepts as they relate to ASP.NET Core.
- Show how to implement role-based and policy-based authorization.
- Demonstrate integration with Microsoft Entra ID (formerly Azure AD).
- Explain how to secure both controller-based and Minimal APIs consistently.

## Validation and Error Handling

- Guide the implementation of model validation using data annotations and FluentValidation.
- Explain the validation pipeline and how to customize validation responses.
- Demonstrate a global exception handling strategy using middleware.
- Show how to create consistent error responses across the API.
- Explain problem details (RFC 9457) implementation for standardized error responses.

**Guard Clauses & Happy Path (Required)**
- Validate method parameters at the start of the method (Guard Clauses style). Return early or throw specific exceptions for invalid inputs so that the main "happy path" execution remains unindented and easy to follow.

Rationale: Validate inputs as early as possible to fail-fast and keep the normal execution path shallow and readable. This improves maintainability and reduces nested conditional complexity.

**Guard Clauses Example (Recommended, Allman-Stil)**
```csharp
public UserDto GetUser(Guid id)
{
	if (id == Guid.Empty)
	{
		throw new ArgumentException("id must be provided", nameof(id));
	}

	// Happy path — minimal nesting
	var user = _userRepository.Find(id);
	if (user is null)
	{
		throw new NotFoundException(id);
	}

	return _mapper.Map<UserDto>(user);
}
```

## API Versioning and Documentation

- Guide users through implementing and explaining API versioning strategies.
- Demonstrate Swagger/OpenAPI implementation with proper documentation.
- Show how to document endpoints, parameters, responses, and authentication.
- Explain versioning in both controller-based and Minimal APIs.
- Guide users on creating meaningful API documentation that helps consumers.

## Logging and Monitoring

- Guide the implementation of structured logging using Serilog or other providers.
- Explain the logging levels and when to use each.
- Demonstrate integration with Application Insights for telemetry collection.
- Show how to implement custom telemetry and correlation IDs for request tracking.
- Explain how to monitor API performance, errors, and usage patterns.

## Testing

- Always include test cases for critical paths of the application.
- Guide users through creating unit tests.
- Do not emit "Act", "Arrange" or "Assert" comments.
- Copy existing style in nearby files for test method names and capitalization.
- Explain integration testing approaches for API endpoints.
- Always include test cases for critical paths of the application.
- Guide users through creating unit tests.
- Do not emit explicit `// Arrange` / `// Act` / `// Assert` comments in tests; structure tests clearly instead.
- Copy existing style in nearby files for test method names and capitalization.
- Explain integration testing approaches for API endpoints.

**Test Naming Example (Recommended)**
- Unit test method name pattern: `MethodName_StateUnderTest_ExpectedBehavior` (e.g., `GetUser_WhenUserExists_ReturnsUser`).

**Canonical Unit Test Example (xUnit, Recommended)**
```csharp
public class UserServiceTests
{
	private readonly IMapper _mapper = /* create or mock mapper */;

	[Fact]
	public async Task GetUser_WhenUserExists_ReturnsUserDto()
	{
		// Arrange
		var user = new User { Id = Guid.NewGuid(), Name = "Alice" };
		var repo = new InMemoryUserRepository(user);
		var sut = new UserService(repo, _mapper);

		// Act
		var result = await sut.GetUserAsync(user.Id);

		// Assert
		result.Should().NotBeNull();
		result.Id.Should().Be(user.Id);
	}
}
```

Rationale: keep tests small, focused and readable. Prefer descriptive method names and avoid noisy inline comments — the Arrange/Act/Assert structure is fine when the code is clear.
- Demonstrate how to mock dependencies for effective testing.
- Show how to test authentication and authorization logic.
- Explain test-driven development principles as applied to API development.

## Performance Optimization

- Guide users on implementing caching strategies (in-memory, distributed, response caching).
- Explain asynchronous programming patterns and why they matter for API performance.
- Demonstrate pagination, filtering, and sorting for large data sets.
- Show how to implement compression and other performance optimizations.
- Explain how to measure and benchmark API performance.

## Deployment and DevOps

- Guide users through containerizing their API using .NET's built-in container support (`dotnet publish --os linux --arch x64 -p:PublishProfile=DefaultContainer`).
- Explain the differences between manual Dockerfile creation and .NET's container publishing features.
- Explain CI/CD pipelines for NET applications.
- Demonstrate deployment to Azure App Service, Azure Container Apps, or other hosting options.
- Show how to implement health checks and readiness probes.
- Explain environment-specific configurations for different deployment stages.

## CI Checks (Recommended)
- Typical pipeline steps to enforce the guidelines:

```yaml
# Example (GitHub Actions) - run formatting, build, test
name: CI
on: [push, pull_request]
jobs:
	build:
		runs-on: ubuntu-latest
		steps:
			- uses: actions/checkout@v4
			- name: Setup .NET
				uses: actions/setup-dotnet@v4
				with:
					dotnet-version: '8.0.x'
			- name: Install dotnet-format
				run: dotnet tool install -g dotnet-format --version 6.* || true
			- name: Format
				run: dotnet format --verify-no-changes
			- name: Restore
				run: dotnet restore
			- name: Build (with analyzers)
				run: dotnet build --configuration Release /p:TreatWarningsAsErrors=true
			- name: Test
				run: dotnet test --no-build --configuration Release

			- name: (Optional) Install StyleCop.Analyzers
				run: |
					# It is recommended to add StyleCop.Analyzers as a package reference in the solution projects.
					# Example (run locally to add): dotnet add <Project.csproj> package StyleCop.Analyzers
					echo "Ensure StyleCop.Analyzers is added to your projects; CI will fail if analyzers report errors."
```

## Notes & Recommendations
- Consider narrowing `applyTo` patterns (e.g., `src/**/*.cs`) to avoid loading rules for unrelated files in very large repos.
- Add explicit minimum/target SDK information to the repository README to avoid ambiguity.
