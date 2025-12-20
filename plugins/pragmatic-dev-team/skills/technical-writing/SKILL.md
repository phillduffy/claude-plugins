---
name: Technical Writing
description: This skill should be used when writing documentation, READMEs, developer guides, API documentation, or technical content. Based on "Docs for Developers" best practices and technical writing principles.
version: 0.1.0
---

# Technical Writing

Best practices for writing clear, useful technical documentation.

## Core Principles

### Audience First
Know who you're writing for:
- What do they already know?
- What do they need to accomplish?
- What's their context?

### Task-Oriented
Focus on what users want to DO, not what the system IS.

| Instead Of | Write |
|------------|-------|
| "The system has a retry mechanism" | "How to configure automatic retries" |
| "The API returns JSON" | "How to parse API responses" |

### Scannable
Users skim, they don't read. Design for scanning:
- Clear headings
- Short paragraphs
- Bullet lists
- Code examples
- Tables for comparisons

## Document Types

### README
First thing users see. Must answer:
- What is this?
- Why should I use it?
- How do I get started?

```markdown
# Project Name

One-line description of what it does.

## Quick Start

npm install mypackage
mypackage init

## Features

- Feature 1
- Feature 2

## Documentation

[Full docs](./docs)

## License

MIT
```

### How-To Guide
Task-oriented. User has a goal.

```markdown
# How to Configure Authentication

This guide shows how to set up OAuth2 authentication.

## Prerequisites

- Account with OAuth provider
- API credentials

## Steps

### 1. Create OAuth App

Navigate to developer settings...

### 2. Configure Environment

export OAUTH_CLIENT_ID=your-client-id

### 3. Update Configuration

// In appsettings.json
{
  "OAuth": {
    "ClientId": "..."
  }
}

## Verification

Test with: curl -H "Authorization: Bearer $TOKEN" /api/me

## Troubleshooting

### "Invalid client" error
Check that CLIENT_ID matches...
```

### Reference
Comprehensive, accurate, complete.

```markdown
# Configuration Reference

## Settings

| Setting | Type | Default | Description |
|---------|------|---------|-------------|
| `timeout` | int | 30 | Request timeout in seconds |
| `retries` | int | 3 | Number of retry attempts |
```

### Conceptual/Explanation
Understanding-oriented. Explains why and how.

```markdown
# Understanding the Result Pattern

The Result pattern provides an alternative to exceptions for handling expected failures...

## Why Not Exceptions?

Exceptions have performance costs and hidden control flow...

## How It Works

Instead of throwing, methods return a Result object...
```

## Writing Guidelines

### Be Direct
| Wordy | Direct |
|-------|--------|
| "In order to" | "To" |
| "It is necessary to" | "You must" |
| "The user should" | "You should" |
| "It should be noted that" | (delete) |

### Use Active Voice
| Passive | Active |
|---------|--------|
| "The file is created by the system" | "The system creates the file" |
| "Errors are logged" | "The service logs errors" |

### Be Specific
| Vague | Specific |
|-------|----------|
| "Wait a while" | "Wait 30 seconds" |
| "A large file" | "Files over 100MB" |
| "Several options" | "Three options" |

## Code Examples

### Good Examples
```csharp
// Complete, runnable
var client = new HttpClient();
var response = await client.GetAsync("https://api.example.com/users");
var users = await response.Content.ReadFromJsonAsync<List<User>>();
```

### With Explanation
```csharp
// Create an order with two items
var order = new Order(customerId: 123);

// Add items - returns Result, so check for failure
var result = order.AddItem(productId: "ABC", quantity: 2);
if (result.IsFailure)
{
    // Handle error - e.g., product not found
    Console.WriteLine(result.Error.Message);
}
```

## Structure

### Heading Hierarchy
```markdown
# Page Title (H1) - One per page

## Major Section (H2)

### Subsection (H3)

#### Minor heading (H4) - Use sparingly
```

### Logical Flow
1. Context/overview
2. Prerequisites
3. Steps (in order)
4. Verification
5. Next steps
6. Troubleshooting

## Anti-Patterns

| Anti-Pattern | Problem | Solution |
|--------------|---------|----------|
| Wall of text | Not scannable | Use headings, lists |
| Jargon without explanation | Excludes newcomers | Define terms |
| Outdated examples | Doesn't work | Test regularly |
| Incomplete examples | User stuck | Show full context |
| No troubleshooting | User abandoned | Add common issues |

## Maintenance

- Review quarterly
- Test examples on major version changes
- Track user questions (FAQ fodder)
- Delete obsolete content
- Version documentation with code
