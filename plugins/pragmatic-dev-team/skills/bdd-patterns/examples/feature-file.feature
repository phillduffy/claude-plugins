# Feature File Pattern
# From: OfficeAddins/tests/Core.Application.Tests/Features/Licensing/ActivateLicence.feature
# Demonstrates: Gherkin syntax, Background, Rule blocks, semantic language

Feature: Activate Licence
  Activate a licence for the add-in.

  # BACKGROUND - shared setup across all scenarios
  Background:
    Given the licensing service is available

  # RULE BLOCKS - group related scenarios (Cucumber 6+ / Gherkin 6+)
  Rule: Licence can be activated

    Scenario: Activate licence successfully
      Given the licence repository can activate licences
      When I activate the licence
      Then the licence is activated

    Scenario: Activate licence fails when already active
      Given the licence is already active
      When I activate the licence
      Then the activation fails because the licence is already active

# KEY PATTERNS:
# 1. Feature description is concise (1 sentence)
# 2. Background for shared Given steps (DRY)
# 3. Rule blocks organize related scenarios
# 4. Steps use DOMAIN LANGUAGE, not test language
# 5. No error codes in feature files - "fails because..." is semantic
# 6. Each scenario tests ONE behavior
# 7. Steps are short and scannable

# ANTI-PATTERNS AVOIDED:
# - No "I click button X" (UI details)
# - No "the database contains..." (implementation details)
# - No "Given the user is logged in AND has role X AND..." (too many preconditions)
# - No Scenario Outline when only 2 examples exist
