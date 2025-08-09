#!/bin/bash
# Pepemon Unity Local Development Setup Script
# Run this after installing Unity Hub and Unity 2021.3.12f1

echo "🎮 Setting up Pepemon Unity Local Development..."

# Check if Unity Hub is installed
if [ ! -d "/Applications/Unity Hub.app" ]; then
    echo "❌ Unity Hub not found. Installing..."
    brew install --cask unity-hub
    echo "✅ Unity Hub installed. Please open Unity Hub and install Unity 2021.3.12f1 with WebGL support."
    exit 1
fi

# Check if Unity 2021.3.12f1 is installed
UNITY_PATH="/Applications/Unity/Hub/Editor/2021.3.12f1/Unity.app/Contents/MacOS/Unity"
if [ ! -f "$UNITY_PATH" ]; then
    echo "❌ Unity 2021.3.12f1 not found."
    echo "Please install Unity 2021.3.12f1 with WebGL Build Support via Unity Hub"
    open -a "Unity Hub"
    exit 1
fi

# Check Git LFS
if ! command -v git-lfs &> /dev/null; then
    echo "📦 Installing Git LFS..."
    brew install git-lfs
    git lfs install
fi

# Pull LFS files
echo "📥 Pulling Git LFS files..."
git lfs pull

# Check project structure
if [ ! -f "ProjectSettings/ProjectVersion.txt" ]; then
    echo "❌ Not in Unity project root directory"
    exit 1
fi

echo "✅ Setup complete! You can now:"
echo "  1. Open Unity Hub"
echo "  2. Add this project folder to Unity Hub"
echo "  3. Open the project in Unity 2021.3.12f1"
echo "  4. Build via File → Build Settings → WebGL"
echo ""
echo "🎯 Project path: $(pwd)"
echo "🔧 Unity path: $UNITY_PATH"

# Optional: Open Unity Hub
read -p "Open Unity Hub now? (y/n): " -n 1 -r
echo
if [[ $REPLY =~ ^[Yy]$ ]]; then
    open -a "Unity Hub"
fi
