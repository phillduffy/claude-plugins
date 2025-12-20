# Diagrams as Code

Create maintainable diagrams with Mermaid and C4 model.

## When to Use Diagrams

| Diagram Type | Use When |
|-------------|----------|
| Architecture overview | Explaining system boundaries |
| Sequence diagram | Showing API flows, interactions |
| Flowchart | Decision logic, process flows |
| State diagram | Lifecycle, state machines |
| ER diagram | Data models |

---

## Mermaid Basics

### Flowchart

```mermaid
flowchart TD
    A[Start] --> B{Is valid?}
    B -->|Yes| C[Process]
    B -->|No| D[Return error]
    C --> E[Save]
    E --> F[End]
    D --> F
```

```markdown
\`\`\`mermaid
flowchart TD
    A[Start] --> B{Is valid?}
    B -->|Yes| C[Process]
    B -->|No| D[Return error]
    C --> E[Save]
    E --> F[End]
    D --> F
\`\`\`
```

### Sequence Diagram

```mermaid
sequenceDiagram
    participant User
    participant API
    participant Service
    participant DB

    User->>API: POST /orders
    API->>Service: CreateOrder()
    Service->>DB: INSERT order
    DB-->>Service: order_id
    Service-->>API: Order
    API-->>User: 201 Created
```

```markdown
\`\`\`mermaid
sequenceDiagram
    participant User
    participant API
    participant Service
    participant DB

    User->>API: POST /orders
    API->>Service: CreateOrder()
    Service->>DB: INSERT order
    DB-->>Service: order_id
    Service-->>API: Order
    API-->>User: 201 Created
\`\`\`
```

### State Diagram

```mermaid
stateDiagram-v2
    [*] --> Draft
    Draft --> Submitted: submit()
    Submitted --> Confirmed: confirm()
    Submitted --> Cancelled: cancel()
    Confirmed --> Shipped: ship()
    Shipped --> Delivered: deliver()
    Cancelled --> [*]
    Delivered --> [*]
```

```markdown
\`\`\`mermaid
stateDiagram-v2
    [*] --> Draft
    Draft --> Submitted: submit()
    Submitted --> Confirmed: confirm()
    Confirmed --> Shipped: ship()
    Shipped --> Delivered: deliver()
\`\`\`
```

---

## C4 Model with Mermaid

### Context Diagram (Level 1)

Shows the big picture - system and external actors.

```mermaid
C4Context
    title System Context - Order Management

    Person(customer, "Customer", "Places orders")
    Person(admin, "Admin", "Manages inventory")

    System(orderSystem, "Order System", "Manages orders and inventory")

    System_Ext(payment, "Payment Gateway", "Processes payments")
    System_Ext(shipping, "Shipping Provider", "Handles delivery")

    Rel(customer, orderSystem, "Places orders")
    Rel(admin, orderSystem, "Manages inventory")
    Rel(orderSystem, payment, "Processes payments")
    Rel(orderSystem, shipping, "Schedules delivery")
```

### Container Diagram (Level 2)

Shows the high-level technical building blocks.

```mermaid
C4Container
    title Container Diagram - Order System

    Person(customer, "Customer")

    Container_Boundary(orderSystem, "Order System") {
        Container(webApp, "Web App", "React", "Customer-facing UI")
        Container(api, "API", "ASP.NET Core", "REST API")
        Container(worker, "Worker", ".NET", "Background processing")
        ContainerDb(db, "Database", "PostgreSQL", "Stores orders")
        Container(queue, "Message Queue", "RabbitMQ", "Async events")
    }

    System_Ext(payment, "Payment Gateway")

    Rel(customer, webApp, "Uses", "HTTPS")
    Rel(webApp, api, "Calls", "HTTPS/JSON")
    Rel(api, db, "Reads/writes")
    Rel(api, queue, "Publishes events")
    Rel(worker, queue, "Consumes events")
    Rel(api, payment, "Charges", "HTTPS")
```

---

## Diagram Best Practices

### Keep It Simple

```mermaid
%% GOOD: Focused on one aspect
flowchart LR
    A[Request] --> B[Validate]
    B --> C[Process]
    C --> D[Response]
```

```mermaid
%% BAD: Too much detail for one diagram
flowchart TD
    A[Request] --> B[Auth Check]
    B --> C[Rate Limit]
    C --> D[Validate Schema]
    D --> E[Validate Business]
    E --> F[Load Entity]
    F --> G[Check Permissions]
    G --> H[Execute]
    H --> I[Persist]
    I --> J[Publish Event]
    J --> K[Build Response]
    K --> L[Serialize]
    L --> M[Send]
```

### Use Subgraphs for Grouping

```mermaid
flowchart TB
    subgraph Frontend
        A[Web App]
        B[Mobile App]
    end

    subgraph Backend
        C[API Gateway]
        D[Order Service]
        E[Inventory Service]
    end

    subgraph Data
        F[(Order DB)]
        G[(Inventory DB)]
    end

    A --> C
    B --> C
    C --> D
    C --> E
    D --> F
    E --> G
```

---

## When NOT to Use Diagrams

| Skip Diagram When | Instead |
|-------------------|---------|
| Code is self-explanatory | Good naming, small functions |
| Diagram would duplicate code | Link to source |
| Relationships are simple | Text description |
| High maintenance burden | Simpler representation |

---

## Markdown Integration

### GitHub-Flavored Markdown

```markdown
## Architecture

The system uses a microservices architecture:

\`\`\`mermaid
flowchart LR
    A[API Gateway] --> B[Order Service]
    A --> C[User Service]
    B --> D[(Orders DB)]
    C --> E[(Users DB)]
\`\`\`

See [detailed architecture](./architecture.md) for more.
```

### Linking Diagrams

```markdown
## Overview

See the [Context Diagram](#context-diagram) for system boundaries.

## Context Diagram

\`\`\`mermaid
C4Context
    ...
\`\`\`

## Details

For implementation details, see:
- [Container Diagram](./containers.md)
- [Component Diagram](./components.md)
```

---

## Tool Comparison

| Tool | Strengths | Limitations |
|------|-----------|-------------|
| **Mermaid** | Git-friendly, GitHub renders | Limited styling |
| **PlantUML** | Full UML support | Needs server/plugin |
| **Structurizr** | Full C4 support | Paid for advanced |
| **draw.io** | Rich UI | Binary files |

---

## Common Patterns

### API Flow

```mermaid
sequenceDiagram
    participant C as Client
    participant A as API
    participant S as Service
    participant D as Database

    C->>A: POST /resource
    A->>A: Validate
    A->>S: Process(data)
    S->>D: Save(entity)
    D-->>S: entity
    S-->>A: Result
    A-->>C: 201 Created
```

### Event-Driven

```mermaid
flowchart LR
    A[Order Service] -->|OrderPlaced| B[Message Bus]
    B --> C[Inventory Service]
    B --> D[Email Service]
    B --> E[Analytics Service]
```

### Decision Flow

```mermaid
flowchart TD
    A[Receive Order] --> B{Stock available?}
    B -->|Yes| C{Payment valid?}
    B -->|No| D[Backorder]
    C -->|Yes| E[Fulfill]
    C -->|No| F[Reject]
```

---

## Anti-Patterns

| Anti-Pattern | Problem | Fix |
|--------------|---------|-----|
| Too much detail | Unreadable | One aspect per diagram |
| No legend | Confusing | Add key/description |
| Binary images | Can't version | Use Mermaid/PlantUML |
| Stale diagrams | Misleading | Update with code changes |
| Diagram without text | Missing context | Add explanatory prose |

---

## Quick Reference

### Mermaid Shapes

```
[Rectangle]     Rectangle node
(Rounded)       Rounded rectangle
{Diamond}       Decision diamond
[(Database)]    Database shape
((Circle))      Circle
>Asymmetric]    Asymmetric
```

### Arrow Types

```
-->   Solid arrow
---   Solid line
-.-   Dashed line
-.->  Dashed arrow
-->>  Solid with arrowhead (async)
```

## Sources

- [Mermaid Documentation](https://mermaid.js.org/)
- [C4 Model](https://c4model.com/)
- [Building C4 Diagrams with Mermaid](https://lukemerrett.com/building-c4-diagrams-in-mermaid/)
- [PlantUML](https://plantuml.com/)
