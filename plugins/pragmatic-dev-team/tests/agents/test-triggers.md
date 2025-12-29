# Agent Triggering Test Cases

## architecture-reviewer

### Should Trigger
- "Does this follow Clean Architecture?"
- "Check the dependency direction"
- "This handler is too long, how should I refactor?"
- "I need to change this old handler but there are no tests"
- "I'm planning a new Notifications module"
- "Review the architecture of this module"

### Should NOT Trigger
- "Fix the typo in this comment"
- "What's the weather today?"
- "Run the tests"

---

## code-reviewer

### Should Trigger
- "Can you review my changes?"
- "This endpoint is slow"
- "What should I log here?"
- "Is this form accessible?"
- "Check this code for issues"

### Should NOT Trigger
- "What's Clean Architecture?"
- "Create a new feature"
- "Write tests for this"

---

## bdd-strategist

### Should Trigger
- "I need to add a password reset feature"
- "Do we have good test coverage?"
- "I want to find edge cases"
- "The stakeholder said they want reports"
- "Write scenarios for this feature"

### Should NOT Trigger
- "Review this code"
- "Fix this bug"
- "Check architecture"

---

## security-reviewer

### Should Trigger
- "I've implemented the login endpoint"
- "Added a search feature that queries the database"
- "Is this secure?"
- "Check for SQL injection"
- "Review authentication code"

### Should NOT Trigger
- "Style this component"
- "Add logging"
- "Refactor this"

---

## issue-specialist

### Should Trigger
- "Write an issue for this bug"
- "Work on issue #42"
- "Should I use this library?"
- "Build or buy for email templating?"
- "Evaluate these options"

### Should NOT Trigger
- "Review my code"
- "Check architecture"
- "Is this secure?"

---

## team-coordinator

### Should Trigger
- "I'm ready to create a PR"
- "I'm done for the day"
- "Ready to deploy to production"
- "What if this deployment fails?"
- "Give me a team review"

### Should NOT Trigger
- "Review just the code"
- "Check just the architecture"
- "Write just the tests"

---

## Expected Output Format

Each agent should:
1. Quote actual code from files (not fabricated)
2. Cite file:line for findings
3. Use appropriate severity levels
4. Say "No issues found" when search yields nothing
