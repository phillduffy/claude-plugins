<#
.SYNOPSIS
    Smoke tests for pragmatic-dev-team plugin
.DESCRIPTION
    Validates plugin structure, frontmatter, and JSON syntax
.EXAMPLE
    .\smoke-test.ps1
#>

$ErrorActionPreference = "Stop"
$script:testsPassed = 0
$script:testsFailed = 0

function Test-Pass($name) {
    Write-Host "[PASS] $name" -ForegroundColor Green
    $script:testsPassed++
}

function Test-Fail($name, $reason) {
    Write-Host "[FAIL] $name - $reason" -ForegroundColor Red
    $script:testsFailed++
}

$pluginRoot = Split-Path -Parent $PSScriptRoot

Write-Host "`n=== Pragmatic Dev Team Smoke Tests ===" -ForegroundColor Cyan
Write-Host "Plugin: $pluginRoot`n"

# Test 1: Plugin.json exists and valid
$pluginJson = Join-Path $pluginRoot ".claude-plugin\plugin.json"
if (Test-Path $pluginJson) {
    try {
        $json = Get-Content $pluginJson -Raw | ConvertFrom-Json
        if ($json.name -eq "pragmatic-dev-team") {
            Test-Pass "plugin.json valid"
        } else {
            Test-Fail "plugin.json" "name mismatch"
        }
    } catch {
        Test-Fail "plugin.json" "invalid JSON: $_"
    }
} else {
    Test-Fail "plugin.json" "not found"
}

# Test 2: Hooks.json valid
$hooksJson = Join-Path $pluginRoot "hooks\hooks.json"
if (Test-Path $hooksJson) {
    try {
        $hooks = Get-Content $hooksJson -Raw | ConvertFrom-Json
        if ($hooks.hooks) {
            Test-Pass "hooks.json valid"
        } else {
            Test-Fail "hooks.json" "missing hooks key"
        }
    } catch {
        Test-Fail "hooks.json" "invalid JSON: $_"
    }
} else {
    Test-Fail "hooks.json" "not found"
}

# Test 3: All agents have required frontmatter
$agentsDir = Join-Path $pluginRoot "agents"
$agentFiles = Get-ChildItem $agentsDir -Filter "*.md"
foreach ($agent in $agentFiles) {
    $content = Get-Content $agent.FullName -Raw
    if ($content -match "^---[\s\S]*?name:\s*\w+[\s\S]*?---") {
        Test-Pass "Agent frontmatter: $($agent.Name)"
    } else {
        Test-Fail "Agent frontmatter: $($agent.Name)" "missing or invalid frontmatter"
    }
}

# Test 4: All skills have required frontmatter + load directive
$skillsDir = Join-Path $pluginRoot "skills"
$skillFiles = Get-ChildItem $skillsDir -Recurse -Filter "SKILL.md"
foreach ($skill in $skillFiles) {
    $content = Get-Content $skill.FullName -Raw
    $hasName = $content -match "name:\s*\w+"
    $hasLoad = $content -match "load:\s*(on-demand|always|eager)"

    if ($hasName -and $hasLoad) {
        Test-Pass "Skill: $($skill.Directory.Name)"
    } elseif ($hasName) {
        Test-Fail "Skill: $($skill.Directory.Name)" "missing load directive"
    } else {
        Test-Fail "Skill: $($skill.Directory.Name)" "missing name in frontmatter"
    }
}

# Test 5: All commands have required frontmatter
$commandsDir = Join-Path $pluginRoot "commands"
$commandFiles = Get-ChildItem $commandsDir -Filter "*.md"
foreach ($cmd in $commandFiles) {
    $content = Get-Content $cmd.FullName -Raw
    if ($content -match "^---[\s\S]*?name:\s*\w+[\s\S]*?---") {
        Test-Pass "Command: $($cmd.Name)"
    } else {
        Test-Fail "Command: $($cmd.Name)" "missing or invalid frontmatter"
    }
}

# Test 6: No duplicate agent names
$agentNames = @()
foreach ($agent in $agentFiles) {
    $content = Get-Content $agent.FullName -Raw
    if ($content -match "name:\s*(\S+)") {
        $name = $Matches[1]
        if ($agentNames -contains $name) {
            Test-Fail "Duplicate agent" "name '$name' used multiple times"
        } else {
            $agentNames += $name
        }
    }
}
if ($agentNames.Count -eq $agentFiles.Count) {
    Test-Pass "No duplicate agent names"
}

# Summary
Write-Host "`n=== Summary ===" -ForegroundColor Cyan
Write-Host "Passed: $script:testsPassed" -ForegroundColor Green
Write-Host "Failed: $script:testsFailed" -ForegroundColor $(if ($script:testsFailed -gt 0) { "Red" } else { "Green" })

exit $script:testsFailed
