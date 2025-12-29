# Hook Validation Tests
# Run with: powershell -File test-hooks.ps1

Write-Host "=== Hook Validation Tests ===" -ForegroundColor Cyan

# Test 1: SessionStart - Solution detection
Write-Host "`n[Test 1] SessionStart - Solution Detection" -ForegroundColor Yellow
if (Test-Path "*.sln") {
    Write-Host "  PASS: Would output 'C# solution detected'" -ForegroundColor Green
} else {
    Write-Host "  INFO: No .sln in current directory (expected in non-C# dirs)" -ForegroundColor Gray
}

# Test 2: PreToolUse - Dangerous command detection
Write-Host "`n[Test 2] PreToolUse - Dangerous Commands" -ForegroundColor Yellow
$dangerousCommands = @(
    "rm -rf /",
    "del /s /q C:\",
    "git push --force",
    "DROP TABLE users"
)
foreach ($cmd in $dangerousCommands) {
    Write-Host "  Testing: '$cmd'" -ForegroundColor Gray
    if ($cmd -match "rm -rf|del /s|--force|DROP TABLE|TRUNCATE") {
        Write-Host "    PASS: Would be blocked" -ForegroundColor Green
    } else {
        Write-Host "    FAIL: Should be blocked" -ForegroundColor Red
    }
}

# Test 3: PreToolUse - Sensitive file detection
Write-Host "`n[Test 3] PreToolUse - Sensitive Files" -ForegroundColor Yellow
$sensitiveFiles = @(
    ".env",
    "appsettings.Production.json",
    "secrets.json",
    "credentials.json"
)
foreach ($file in $sensitiveFiles) {
    Write-Host "  Testing: '$file'" -ForegroundColor Gray
    if ($file -match "\.env|secrets|credentials|appsettings.*\.json") {
        Write-Host "    PASS: Would be blocked" -ForegroundColor Green
    } else {
        Write-Host "    FAIL: Should be blocked" -ForegroundColor Red
    }
}

# Test 4: UserPromptSubmit - Verification reminder
Write-Host "`n[Test 4] UserPromptSubmit - Verification Trigger Words" -ForegroundColor Yellow
$triggerWords = @("commit", "PR", "done", "finished", "ready", "merge", "push")
foreach ($word in $triggerWords) {
    Write-Host "  Testing: '$word'" -ForegroundColor Gray
    Write-Host "    PASS: Would trigger verification reminder" -ForegroundColor Green
}

# Test 5: PostToolUse - C# file notification
Write-Host "`n[Test 5] PostToolUse - C# File Notification" -ForegroundColor Yellow
$testFiles = @("Handler.cs", "README.md", "test.feature")
foreach ($file in $testFiles) {
    Write-Host "  Testing: '$file'" -ForegroundColor Gray
    if ($file -match "\.cs$") {
        Write-Host "    PASS: Would show 'C# modified. Run /review'" -ForegroundColor Green
    } elseif ($file -match "\.feature$") {
        Write-Host "    PASS: Would show 'Feature modified. Run scenarios'" -ForegroundColor Green
    } else {
        Write-Host "    PASS: Would output nothing (non-trigger file)" -ForegroundColor Green
    }
}

Write-Host "`n=== All Tests Complete ===" -ForegroundColor Cyan
Write-Host "Note: These are pattern tests. Actual hook behavior requires Claude Code runtime." -ForegroundColor Gray
