# Scenario Anti-Patterns

Common Gherkin mistakes and how to fix them.

## The Cardinal Rule

> **One scenario = one behavior**

If your scenario tests multiple behaviors, split it.

---

## Anti-Pattern: Too Abstract

**Problem:** No concrete data makes scenarios untestable and unclear.

```gherkin
# BAD - Abstract
Scenario: Valid order
  Given a valid product
  When user places order
  Then order succeeds
```

**Fix:** Use specific, measurable data:

```gherkin
# GOOD - Concrete
Scenario: Order with sufficient stock
  Given product "Widget" with 10 in stock
  When customer orders 3 "Widget"
  Then order is confirmed
  And stock is reduced to 7
```

**Why it matters:**
- Concrete examples catch edge cases
- Tests are self-documenting
- Easier to debug failures

---

## Anti-Pattern: UI-Coupled (Imperative)

**Problem:** Scenarios describe HOW, not WHAT. Brittle when UI changes.

```gherkin
# BAD - UI-focused
Scenario: Login
  Given I am on the login page
  When I enter "user@test.com" in the email field
  And I enter "password123" in the password field
  And I click the login button
  Then I should see the dashboard
```

**Fix:** Describe behavior, not UI interactions:

```gherkin
# GOOD - Declarative
Scenario: Successful login
  Given a registered user with email "user@test.com"
  When they log in with valid credentials
  Then they should see their dashboard
```

**Rule of thumb:** If step mentions element names (field, button, link), it's too imperative.

---

## Anti-Pattern: Multiple When-Then Pairs

**Problem:** Testing multiple behaviors in one scenario. Hard to know what failed.

```gherkin
# BAD - Multiple behaviors
Scenario: User management
  Given a new user
  When they register
  Then account is created
  When they log in
  Then they see dashboard
  When they update profile
  Then changes are saved
```

**Fix:** One scenario per behavior:

```gherkin
Scenario: Successful registration
  Given a new user
  When they complete registration
  Then their account is created

Scenario: Successful login
  Given a registered user
  When they log in with valid credentials
  Then they see their dashboard

Scenario: Update profile
  Given a logged-in user
  When they update their profile name to "Jane Doe"
  Then the profile displays "Jane Doe"
```

---

## Anti-Pattern: Long Rambling Scenarios

**Problem:** Excessive incidental details obscure the core behavior.

```gherkin
# BAD - Too long
Scenario: Place order with discount
  Given I am a premium customer
  And I have been a member since 2020
  And I have made 15 previous orders
  And I live in the continental US
  And my account is in good standing
  And I have a valid payment method on file
  And the system is operational
  And product "Laptop" exists
  And product "Laptop" is in stock
  And product "Laptop" costs $999
  And I have a 10% discount code "SAVE10"
  When I add "Laptop" to my cart
  And I apply discount code "SAVE10"
  And I proceed to checkout
  Then my cart total is $899.10
```

**Fix:** Keep scenarios under 6 steps. Use Background for shared setup:

```gherkin
Background:
  Given a premium customer with valid payment
  And product "Laptop" priced at $999

Scenario: Apply percentage discount
  Given discount code "SAVE10" for 10% off
  When customer orders "Laptop" with code "SAVE10"
  Then order total is $899.10
```

---

## Anti-Pattern: Feature-Coupled Steps

**Problem:** Steps are too specific to one feature. No reuse.

```gherkin
# BAD - Feature-coupled
Given the user registration form is displayed
When I fill in the user registration email field
```

**Fix:** Write generic, reusable steps:

```gherkin
# GOOD - Reusable
Given a registration form
When I enter email "user@test.com"
```

---

## Anti-Pattern: Testing via UI Only

**Problem:** All scenarios go through UI. Slow, brittle, duplicate coverage.

**Fix:** Test pyramid approach:

| Layer | Coverage | Speed |
|-------|----------|-------|
| Unit | Business logic | Fast |
| Integration | Service interactions | Medium |
| E2E/UI | Critical user paths only | Slow |

```gherkin
# UI scenario (few of these)
@e2e
Scenario: Complete purchase flow
  Given I am logged in
  When I purchase "Laptop"
  Then I receive confirmation email

# Business logic scenario (many of these, run against domain)
Scenario: Calculate order total with tax
  Given an order with item "Laptop" at $999
  And shipping address in "CA"
  Then tax is calculated at 7.25%
  And order total is $1071.42
```

---

## Anti-Pattern: Technical Language

**Problem:** Business stakeholders can't read the scenarios.

```gherkin
# BAD - Technical
Scenario: HTTP 201 on POST /api/orders
  Given valid JWT token in Authorization header
  When POST request sent with OrderDTO payload
  Then response status code is 201
  And response body contains OrderId GUID
```

**Fix:** Use ubiquitous language:

```gherkin
# GOOD - Business language
Scenario: Order creation succeeds
  Given an authenticated customer
  When they place an order for "Laptop"
  Then the order is created
  And they receive an order confirmation number
```

---

## Anti-Pattern: Writing After Code

**Problem:** Scenarios written as afterthought, not driving design.

**True BDD:**
1. Write scenarios with Three Amigos (Dev + QA + Business)
2. Scenarios define what to build
3. Code implements scenarios
4. Scenarios become living documentation

**Anti-pattern:** Write code first, scenarios second. This is just "test automation with Gherkin syntax."

---

## Quick Reference

| Anti-Pattern | Symptom | Fix |
|--------------|---------|-----|
| Too abstract | "valid order", "succeeds" | Concrete data and outcomes |
| UI-coupled | "click button", "enter field" | Declarative behavior |
| Multiple When-Then | >1 action per scenario | Split into multiple scenarios |
| Long scenarios | >6 steps | Background + focused scenario |
| Feature-coupled | Steps not reusable | Generic step language |
| UI only | All tests through browser | Test pyramid approach |
| Technical language | HTTP codes, DTOs | Ubiquitous language |
| After code | Scenarios match code | Write scenarios first |

## Checklist

Before finalizing a scenario:

- [ ] One behavior per scenario?
- [ ] Concrete examples with real data?
- [ ] Declarative (what) not imperative (how)?
- [ ] Under 6 steps?
- [ ] Business readable?
- [ ] Reusable step definitions?

## Sources

- [Cucumber Anti-patterns (Part 1)](https://cucumber.io/blog/bdd/cucumber-antipatterns-part-one/)
- [Cucumber Anti-patterns (Part 2)](https://cucumber.io/blog/bdd/cucumber-anti-patterns-part-two/)
- [Writing Better Gherkin](https://cucumber.io/docs/bdd/better-gherkin/)
- [BDD Best Practices](https://support.smartbear.com/cucumberstudio/docs/tests/best-practices.html)
