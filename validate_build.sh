#!/bin/bash

# Pepemon Degen Battleground - Build Validation Script
# This script simulates the validation that occurs in the GitHub Actions pipeline

echo "🎮 Pepemon Degen Battleground - Build Validation"
echo "================================================"
echo

# Function to check file exists
check_file_exists() {
    if [ -f "$1" ]; then
        echo "✅ $1 - Found"
        return 0
    else
        echo "❌ $1 - Missing"
        return 1
    fi
}

# Function to check for basic syntax issues in C# files
check_csharp_syntax() {
    local file=$1
    local errors=0
    
    echo "🔍 Checking $file..."
    
    # Check for balanced braces
    local open_braces=$(grep -o '{' "$file" | wc -l)
    local close_braces=$(grep -o '}' "$file" | wc -l)
    
    if [ "$open_braces" -ne "$close_braces" ]; then
        echo "  ⚠️  Potential brace mismatch: $open_braces opening, $close_braces closing"
        ((errors++))
    fi
    
    # Check for proper using statements
    if ! grep -q "using UnityEngine" "$file"; then
        if grep -q "MonoBehaviour\|GameObject\|Transform" "$file"; then
            echo "  ⚠️  Missing 'using UnityEngine;' but uses Unity classes"
            ((errors++))
        fi
    fi
    
    # Check for proper class declaration
    if ! grep -q "public class\|internal class\|private class" "$file"; then
        if [ $(wc -l < "$file") -gt 10 ]; then
            echo "  ⚠️  No class declaration found in non-trivial file"
            ((errors++))
        fi
    fi
    
    if [ $errors -eq 0 ]; then
        echo "  ✅ Basic syntax check passed"
    else
        echo "  ❌ $errors potential syntax issues found"
    fi
    
    return $errors
}

# Check project structure
echo "📁 Checking Project Structure..."
echo "--------------------------------"

critical_files=(
    "Assets/Scripts/UI/RefreshDeckListButton.cs"
    "Assets/Scripts/UI/MintDeckButtonHandler.cs" 
    "Assets/Scripts/UI/Loaders/DeckListLoader.cs"
    "Assets/Scripts/Controllers/ScreenManageDecks.cs"
    "Assets/Scripts/Contracts/PepemonCardDeck.cs"
    "ProjectSettings/ProjectVersion.txt"
    ".github/workflows/unity.yml"
)

missing_files=0
for file in "${critical_files[@]}"; do
    if ! check_file_exists "$file"; then
        ((missing_files++))
    fi
done

echo

# Check Unity version compatibility
echo "🔧 Checking Unity Version..."
echo "----------------------------"
if [ -f "ProjectSettings/ProjectVersion.txt" ]; then
    unity_version=$(grep "m_EditorVersion:" ProjectSettings/ProjectVersion.txt | cut -d' ' -f2)
    expected_version="2021.3.12f1"
    
    if [ "$unity_version" = "$expected_version" ]; then
        echo "✅ Unity version matches CI: $unity_version"
    else
        echo "⚠️  Unity version mismatch - Local: $unity_version, CI: $expected_version"
    fi
else
    echo "❌ ProjectVersion.txt not found"
    ((missing_files++))
fi

echo

# Check C# files for basic compilation issues
echo "🔍 Checking C# Files for Basic Issues..."
echo "----------------------------------------"

syntax_errors=0
modified_files=(
    "Assets/Scripts/UI/RefreshDeckListButton.cs"
    "Assets/Scripts/UI/MintDeckButtonHandler.cs"
    "Assets/Scripts/UI/Loaders/DeckListLoader.cs"
    "Assets/Scripts/Controllers/ScreenManageDecks.cs"
)

for file in "${modified_files[@]}"; do
    if [ -f "$file" ]; then
        check_csharp_syntax "$file"
        syntax_errors=$((syntax_errors + $?))
    fi
done

echo

# Check GitHub Actions workflow
echo "🚀 Checking CI/CD Configuration..."
echo "----------------------------------"
if [ -f ".github/workflows/unity.yml" ]; then
    echo "✅ GitHub Actions workflow found"
    
    # Check for required sections
    if grep -q "game-ci/unity-test-runner" .github/workflows/unity.yml; then
        echo "✅ Unity test runner configured"
    else
        echo "⚠️  Unity test runner not found in workflow"
    fi
    
    if grep -q "game-ci/unity-builder" .github/workflows/unity.yml; then
        echo "✅ Unity builder configured"
    else
        echo "❌ Unity builder not found in workflow"
    fi
    
    if grep -q "targetPlatform: WebGL" .github/workflows/unity.yml; then
        echo "✅ WebGL build target configured"
    else
        echo "⚠️  WebGL build target not explicitly set"
    fi
else
    echo "❌ GitHub Actions workflow missing"
    ((missing_files++))
fi

echo

# Check for potential runtime issues
echo "⚡ Checking for Potential Runtime Issues..."
echo "------------------------------------------"

runtime_issues=0

# Check for async/await patterns
echo "🔄 Checking async patterns..."
async_files=$(find Assets/Scripts -name "*.cs" -exec grep -l "async\|await" {} \;)
for file in $async_files; do
    if grep -q "async.*void" "$file"; then
        echo "  ⚠️  $file: async void detected (prefer async Task)"
        ((runtime_issues++))
    fi
done

# Check for proper null checking
echo "🛡️  Checking null safety..."
if grep -r "\.GetComponent<" Assets/Scripts --include="*.cs" | grep -v "?" | head -5 | while read line; do
    echo "  ℹ️  Consider null checking: ${line}"
done

# Check for Web3 dependencies
echo "🌐 Checking Web3 integration..."
if find Assets/Scripts -name "*.cs" -exec grep -l "ThirdwebManager\|Web3Controller" {} \; | wc -l | grep -q "^[1-9]"; then
    echo "✅ Web3 integration files found"
else
    echo "⚠️  No Web3 integration detected"
fi

echo

# Summary
echo "📋 Validation Summary"
echo "===================="

total_issues=$((missing_files + syntax_errors + runtime_issues))

if [ $missing_files -eq 0 ]; then
    echo "✅ All critical files present"
else
    echo "❌ $missing_files critical files missing"
fi

if [ $syntax_errors -eq 0 ]; then
    echo "✅ No basic syntax issues detected"
else
    echo "❌ $syntax_errors potential syntax issues found"
fi

if [ $runtime_issues -eq 0 ]; then
    echo "✅ No obvious runtime issues detected"
else
    echo "⚠️  $runtime_issues potential runtime issues found"
fi

echo

if [ $total_issues -eq 0 ]; then
    echo "🎉 BUILD READY - All validations passed!"
    echo "   The project appears ready for CI/CD pipeline execution."
    echo "   Next steps:"
    echo "   1. Commit changes to feature branch"
    echo "   2. Push to trigger GitHub Actions"
    echo "   3. Monitor build and test results"
    echo "   4. Create PR when tests pass"
    exit 0
else
    echo "🚨 BUILD ISSUES DETECTED - $total_issues total issues found"
    echo "   Please review and fix issues before pushing."
    echo "   Critical issues will cause build failures in CI."
    exit 1
fi
