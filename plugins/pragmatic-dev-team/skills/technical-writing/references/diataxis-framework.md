# Diátaxis Framework

Organize documentation into four distinct types based on user needs.

## The Four Quadrants

```
                   LEARNING                          WORKING
              ┌─────────────────┬─────────────────┐
              │                 │                 │
              │   TUTORIALS     │   HOW-TO        │
    PRACTICAL │  (learning-     │   GUIDES        │
              │   oriented)     │  (task-         │
              │                 │   oriented)     │
              │                 │                 │
              ├─────────────────┼─────────────────┤
              │                 │                 │
              │  EXPLANATION    │   REFERENCE     │
  THEORETICAL │  (understanding-│  (information-  │
              │   oriented)     │   oriented)     │
              │                 │                 │
              └─────────────────┴─────────────────┘
```

---

## Tutorials

**Purpose:** Learning-oriented - teach through doing.

**User:** New, wants to learn.

**Characteristics:**
- Step-by-step lessons
- Achievable goals (sense of accomplishment)
- Hand-holding approach
- Focus on success, minimize failure

### Structure

```markdown
# Tutorial: Build Your First API

## What You'll Learn
- Create a REST API with ASP.NET Core
- Add endpoints for CRUD operations
- Test with curl

## Prerequisites
- .NET 8 SDK installed
- Basic C# knowledge

## Steps

### 1. Create the Project
First, create a new web API project:
dotnet new webapi -n MyFirstApi

### 2. Add Your First Endpoint
Open Controllers/WeatherForecastController.cs...

### 3. Run and Test
dotnet run
curl http://localhost:5000/weatherforecast

## What's Next?
- [Add a database](./tutorial-database.md)
- [Deploy to Azure](./tutorial-deploy.md)
```

### Do's and Don'ts

| Do | Don't |
|----|-------|
| Ensure every step works | Include broken examples |
| Explain what, not why | Go deep into theory |
| Keep scope achievable | Cover everything |
| Test the tutorial | Assume it works |

---

## How-To Guides

**Purpose:** Task-oriented - solve a specific problem.

**User:** Working, knows what they want.

**Characteristics:**
- Goal-focused
- Assumes prior knowledge
- Real-world scenarios
- Practical steps

### Structure

```markdown
# How to Configure OAuth2 Authentication

This guide shows how to add OAuth2 to an existing ASP.NET Core API.

## Prerequisites
- ASP.NET Core 8 project
- OAuth provider credentials (e.g., Google, Azure AD)

## Steps

### 1. Install Required Packages
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer

### 2. Configure Authentication
// In Program.cs
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "https://login.microsoftonline.com/{tenant}";
        options.Audience = "{client-id}";
    });

### 3. Protect Endpoints
[Authorize]
[HttpGet("protected")]
public IActionResult GetProtected() => Ok("Authenticated!");

## Verification
curl -H "Authorization: Bearer {token}" https://localhost:5001/protected

## Troubleshooting

### "401 Unauthorized" Error
- Check token expiration
- Verify audience matches configuration
- Ensure HTTPS is enabled

## Related
- [Understanding OAuth2 Flows](../explanation/oauth2-flows.md)
- [JWT Configuration Reference](../reference/jwt-options.md)
```

### Do's and Don'ts

| Do | Don't |
|----|-------|
| Focus on the goal | Explain theory |
| Assume knowledge | Start from scratch |
| Include troubleshooting | Leave user stuck |
| Link to reference | Repeat reference content |

---

## Reference

**Purpose:** Information-oriented - accurate, complete facts.

**User:** Working, needs to look something up.

**Characteristics:**
- Structured for lookup (tables, lists)
- Comprehensive
- Accurate and up-to-date
- No opinions or guidance

### Structure

```markdown
# JWT Bearer Authentication Options

## Configuration Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Authority` | string | - | Token issuer URL |
| `Audience` | string | - | Expected audience claim |
| `RequireHttpsMetadata` | bool | true | Require HTTPS for metadata |
| `TokenValidationParameters` | object | - | Token validation settings |

## TokenValidationParameters

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `ValidateIssuer` | bool | true | Validate token issuer |
| `ValidateAudience` | bool | true | Validate token audience |
| `ValidateLifetime` | bool | true | Check token expiration |
| `ClockSkew` | TimeSpan | 5 min | Allowed clock drift |

## Events

### OnAuthenticationFailed
Raised when authentication fails.

```csharp
options.Events = new JwtBearerEvents
{
    OnAuthenticationFailed = context =>
    {
        // Handle failure
        return Task.CompletedTask;
    }
};
```

## See Also
- [How to Configure OAuth2](../how-to/configure-oauth2.md)
- [Understanding JWT](../explanation/jwt-tokens.md)
```

### Do's and Don'ts

| Do | Don't |
|----|-------|
| Be complete | Leave gaps |
| Use tables for properties | Write prose |
| Include all options | Skip "obvious" ones |
| Stay objective | Give opinions |

---

## Explanation

**Purpose:** Understanding-oriented - provide context and reasoning.

**User:** Wants to understand deeply.

**Characteristics:**
- Answers "why" questions
- Provides background and context
- Discusses alternatives and trade-offs
- Can be read without doing

### Structure

```markdown
# Understanding the Result Pattern

## Overview
The Result pattern provides an alternative to exceptions for handling
expected failures in your application.

## Why Not Just Exceptions?

Exceptions have costs:
- **Performance:** Creating exception objects is expensive
- **Hidden flow:** Callers don't know a method might throw
- **Lost context:** Generic catch blocks lose error information

## The Alternative: Explicit Results

Instead of:
public User GetUser(int id)
{
    if (!Exists(id))
        throw new UserNotFoundException(id);
    return LoadUser(id);
}

We return:
public Result<User> GetUser(int id)
{
    if (!Exists(id))
        return Result.Fail<User>("User not found");
    return Result.Ok(LoadUser(id));
}

## Trade-offs

| Aspect | Exceptions | Result |
|--------|-----------|--------|
| Caller awareness | Hidden | Explicit |
| Performance | Slower for expected failures | Constant |
| Verbosity | Concise | More code |
| Framework integration | Built-in | Requires library |

## When to Use Each

- **Exceptions:** Truly exceptional, unexpected failures
- **Result:** Expected, recoverable business failures

## Further Reading
- [Functional Error Handling](https://example.com/functional-errors)
- [Railway-Oriented Programming](https://example.com/rop)
```

### Do's and Don'ts

| Do | Don't |
|----|-------|
| Explain reasoning | List steps |
| Discuss alternatives | Declare "best" approach |
| Provide context | Assume reader knows background |
| Link to how-to/reference | Repeat their content |

---

## Documentation Structure

```
docs/
├── tutorials/
│   ├── getting-started.md
│   ├── first-api.md
│   └── first-deployment.md
├── how-to/
│   ├── configure-authentication.md
│   ├── add-caching.md
│   └── handle-errors.md
├── reference/
│   ├── configuration.md
│   ├── api-endpoints.md
│   └── error-codes.md
└── explanation/
    ├── architecture-overview.md
    ├── result-pattern.md
    └── authentication-flows.md
```

---

## Quick Reference

| Type | User Need | Question | Author Focus |
|------|-----------|----------|--------------|
| Tutorial | Learn | "How do I get started?" | Success |
| How-To | Solve | "How do I do X?" | Steps |
| Reference | Look up | "What are the options?" | Accuracy |
| Explanation | Understand | "Why does it work this way?" | Context |

## Key Principle

> **Don't mix types.** A tutorial shouldn't explain theory. Reference shouldn't teach.

## Sources

- [Diátaxis Framework](https://diataxis.fr/)
- [What is Diátaxis](https://idratherbewriting.com/blog/what-is-diataxis-documentation-framework)
- [Canonical Diátaxis Foundation](https://ubuntu.com/blog/diataxis-a-new-foundation-for-canonical-documentation)
