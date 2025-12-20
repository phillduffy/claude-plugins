# API Documentation

Best practices for OpenAPI, versioning, and changelogs.

## OpenAPI Best Practices

### Use OpenAPI 3.0+

```yaml
openapi: 3.0.3
info:
  title: Order Management API
  version: 2.1.0
  description: |
    API for managing orders and inventory.

    ## Changelog
    ### v2.1.0 (2025-01-15)
    - Added `timeout` parameter to POST /orders
    - Fixed response schema validation

paths:
  /orders:
    post:
      summary: Create a new order
      operationId: createOrder
      tags:
        - Orders
      requestBody:
        required: true
        content:
          application/json:
            schema:
              $ref: '#/components/schemas/CreateOrderRequest'
            example:
              customerId: "cust_123"
              items:
                - productId: "prod_456"
                  quantity: 2
      responses:
        '201':
          description: Order created
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/Order'
        '400':
          description: Invalid request
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/Error'
```

### Always Include Examples

```yaml
components:
  schemas:
    Order:
      type: object
      required:
        - id
        - status
        - total
      properties:
        id:
          type: string
          example: "ord_abc123"
        status:
          type: string
          enum: [pending, confirmed, shipped, delivered]
          example: "confirmed"
        total:
          type: number
          format: decimal
          example: 99.99
      example:
        id: "ord_abc123"
        status: "confirmed"
        total: 99.99
```

### Document Errors Completely

```yaml
components:
  schemas:
    Error:
      type: object
      properties:
        code:
          type: string
          description: Machine-readable error code
          example: "ORDER_NOT_FOUND"
        message:
          type: string
          description: Human-readable error message
          example: "Order with ID ord_123 was not found"
        details:
          type: array
          items:
            type: object
            properties:
              field:
                type: string
              reason:
                type: string

  responses:
    NotFound:
      description: Resource not found
      content:
        application/json:
          schema:
            $ref: '#/components/schemas/Error'
          example:
            code: "NOT_FOUND"
            message: "The requested resource was not found"
```

---

## Versioning Strategy

### URI Versioning (Recommended)

```
/v1/orders
/v2/orders
```

**Pros:** Explicit, cacheable, easy to route
**Cons:** URL changes between versions

### Header Versioning

```http
GET /orders
Api-Version: 2.0
```

**Pros:** Clean URLs
**Cons:** Less visible, harder to test

### Query Parameter

```
/orders?version=2
```

**Pros:** Easy to test
**Cons:** Cache key complexity

### Semantic Versioning

```
MAJOR.MINOR.PATCH
2.1.3

MAJOR: Breaking changes
MINOR: Backward-compatible features
PATCH: Bug fixes
```

---

## Changelog Best Practices

### Structure

```markdown
# Changelog

All notable changes to this API are documented here.

## [2.1.0] - 2025-01-15

### Added
- `timeout` parameter for order creation endpoint
- Rate limiting headers in responses

### Changed
- Default page size from 10 to 20

### Deprecated
- `legacyMode` parameter (removal: v3.0.0, December 2025)

### Fixed
- Response validation for nested objects
- Pagination cursor encoding

### Security
- Added CORS configuration for production domains

## [2.0.0] - 2024-12-01

### Breaking Changes
- Renamed `execute` to `run` in all endpoints
- Changed authentication from API key to OAuth2
- Removed deprecated v1 endpoints

### Migration Guide
See [v2 Migration Guide](./migration-v2.md)
```

### Categories

| Category | Description |
|----------|-------------|
| Added | New features |
| Changed | Changes to existing features |
| Deprecated | Features to be removed |
| Removed | Removed features |
| Fixed | Bug fixes |
| Security | Security-related changes |
| Breaking Changes | Incompatible changes (major version) |

---

## Deprecation Strategy

### Timeline

```
Announce → 6-12 months → Remove

v2.0: Feature deprecated, warning added
v2.x: Deprecation warning in responses
v3.0: Feature removed
```

### Documentation

```yaml
paths:
  /orders/{id}/legacy:
    get:
      deprecated: true
      summary: Get order (deprecated)
      description: |
        **Deprecated:** Use GET /orders/{id} instead.
        This endpoint will be removed in v3.0.0 (December 2025).
```

### Response Headers

```http
HTTP/1.1 200 OK
Deprecation: Sat, 01 Dec 2025 00:00:00 GMT
Sunset: Sat, 01 Dec 2025 00:00:00 GMT
Link: </orders/{id}>; rel="successor-version"
```

---

## Request/Response Examples

### Include Multiple Examples

```yaml
paths:
  /orders:
    post:
      requestBody:
        content:
          application/json:
            examples:
              simple:
                summary: Simple order
                value:
                  customerId: "cust_123"
                  items:
                    - productId: "prod_456"
                      quantity: 1
              withOptions:
                summary: Order with options
                value:
                  customerId: "cust_123"
                  items:
                    - productId: "prod_456"
                      quantity: 2
                  shippingMethod: "express"
                  giftWrap: true
```

### Show Error Responses

```yaml
responses:
  '400':
    description: Validation error
    content:
      application/json:
        examples:
          missingField:
            summary: Missing required field
            value:
              code: "VALIDATION_ERROR"
              message: "Validation failed"
              details:
                - field: "customerId"
                  reason: "Required field is missing"
          invalidFormat:
            summary: Invalid format
            value:
              code: "VALIDATION_ERROR"
              message: "Validation failed"
              details:
                - field: "quantity"
                  reason: "Must be a positive integer"
```

---

## Documentation Checklist

### For Each Endpoint

- [ ] Clear summary (what it does)
- [ ] Operation ID (for code generation)
- [ ] Tags (for grouping)
- [ ] Request body schema with examples
- [ ] All response codes documented
- [ ] Error responses with examples
- [ ] Authentication requirements

### For the API

- [ ] Version in info section
- [ ] Changelog in description or separate file
- [ ] Authentication section
- [ ] Rate limiting documentation
- [ ] Pagination conventions
- [ ] Error format specification

---

## Anti-Patterns

| Anti-Pattern | Problem | Fix |
|--------------|---------|-----|
| No examples | Users can't understand | Include request/response examples |
| Missing errors | Incomplete documentation | Document all error codes |
| Undocumented deprecation | Breaks clients | 6-12 month deprecation notice |
| No versioning | Breaking changes | Use semantic versioning |
| Stale changelog | Outdated info | Update with each release |
| Generic descriptions | Not helpful | Be specific about behavior |

---

## Tools

| Tool | Purpose |
|------|---------|
| **Swagger UI** | Interactive documentation |
| **Redoc** | Beautiful static docs |
| **Stoplight** | Design-first API platform |
| **Postman** | Testing + documentation |
| **Swagger Codegen** | Client generation |

## Sources

- [OpenAPI Specification](https://spec.openapis.org/oas/v3.0.3)
- [API Versioning Best Practices](https://redocly.com/blog/api-versioning-best-practices)
- [Google Cloud - Versioning](https://cloud.google.com/endpoints/docs/openapi/versioning-an-api)
- [Keep a Changelog](https://keepachangelog.com/)
