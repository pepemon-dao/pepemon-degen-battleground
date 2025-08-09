#!/bin/bash
# Pepemon Unity WebGL Build Script

UNITY_PATH="/Applications/Unity/Hub/Editor/2021.3.12f1/Unity.app/Contents/MacOS/Unity"
PROJECT_PATH="$(pwd)"
BUILD_PATH="$PROJECT_PATH/Builds/WebGL"

echo "🚀 Building Pepemon WebGL..."
echo "📁 Project: $PROJECT_PATH"
echo "📦 Output: $BUILD_PATH"

# Check if Unity exists
if [ ! -f "$UNITY_PATH" ]; then
    echo "❌ Unity 2021.3.12f1 not found at $UNITY_PATH"
    echo "Please install Unity 2021.3.12f1 via Unity Hub first"
    exit 1
fi

# Create build directory
mkdir -p "$BUILD_PATH"

# Build WebGL
echo "⚙️ Starting WebGL build..."
"$UNITY_PATH" \
    -batchmode \
    -quit \
    -nographics \
    -projectPath "$PROJECT_PATH" \
    -buildTarget WebGL \
    -logFile "$PROJECT_PATH/build.log"

# Check build result
if [ $? -eq 0 ]; then
    echo "✅ WebGL build completed successfully!"
    echo "📁 Build location: $BUILD_PATH"
    echo "📜 Build log: $PROJECT_PATH/build.log"
    
    # Optional: Start local server for testing
    read -p "Start local HTTP server to test build? (y/n): " -n 1 -r
    echo
    if [[ $REPLY =~ ^[Yy]$ ]]; then
        echo "🌐 Starting local server at http://localhost:8080"
        echo "Press Ctrl+C to stop server"
        cd "$BUILD_PATH"
        python3 -m http.server 8080
    fi
else
    echo "❌ Build failed! Check build.log for details:"
    tail -n 20 "$PROJECT_PATH/build.log"
    exit 1
fi
