#!/usr/bin/env bash
# Compiles Assembly-CSharp with Unity's own Roslyn, without opening the editor and without a
# licence.
#
# Unity's licensing client refuses to issue a token against this editor version, so ./check.sh
# cannot run here, and CI is a 35-minute round trip. But compiling only needs reference
# assemblies, and those are all on disk: Unity ships the UnityEngine modules and Roslyn itself,
# and Library/ScriptAssemblies holds the plugin assemblies from the last successful import.
#
# This catches what actually breaks builds - typos, wrong overloads, missing usings, bad
# namespaces. It does NOT catch scene wiring, and it is not a substitute for CI.
#
#   ./compile-check.sh
set -euo pipefail

UNITY="/Applications/Unity/Hub/Editor/2021.3.12f1/Unity.app/Contents"
CSC="$UNITY/DotNetSdkRoslyn/csc.dll"
DOTNET="$UNITY/NetCoreRuntime/dotnet"
OUT="${TMPDIR:-/tmp}/pepemon-compile-check"

if [ ! -x "$DOTNET" ]; then echo "Unity 2021.3.12f1 not found at $UNITY" >&2; exit 1; fi
if [ ! -d Library/ScriptAssemblies ]; then
  echo "Library/ScriptAssemblies is missing - it comes from a previous editor import." >&2
  echo "Without it the plugin references cannot be resolved. Run a Unity import first." >&2
  exit 1
fi

mkdir -p "$OUT"
rm -f "$OUT"/refs.rsp "$OUT"/sources.rsp

# --- references -------------------------------------------------------------------------
{
  # ProjectSettings has apiCompatibilityLevel: 6, i.e. the full .NET Framework profile rather
  # than netstandard, so the Mono BCL is what the editor actually compiles against. Using the
  # netstandard reference set instead makes legacy plugins fail with mscorlib 2.0 errors.
  for d in "$UNITY"/MonoBleedingEdge/lib/mono/unityjit-macos/*.dll; do echo "-r:$d"; done
  # Facades map netstandard type-forwards onto the Mono BCL. Nethereum and UniTask are built
  # against netstandard, so without these every async method fails with CS1983.
  for d in "$UNITY"/MonoBleedingEdge/lib/mono/unityjit-macos/Facades/*.dll; do echo "-r:$d"; done
  for d in "$UNITY"/Managed/UnityEngine/*.dll; do echo "-r:$d"; done
  # Plugin and package assemblies, minus the ones this compile produces.
  for d in Library/ScriptAssemblies/*.dll; do
    case "$(basename "$d")" in
      Assembly-CSharp.dll|Assembly-CSharp-Editor.dll|Pepemon.BattleRules.Tests.dll|Tests.dll) ;;
      *) echo "-r:$d" ;;
    esac
  done
  # Native libraries carry no managed metadata and make Roslyn error out, so skip them.
  find Assets -name "*.dll" -not -path "*/Editor/*" \
    | grep -Eiv "/(x86|x64|Windows|Android|iOS|Linux|ARM[0-9]*)/" | sed 's/^/-r:/'
} | sort -u > "$OUT/refs.rsp"

# --- sources ----------------------------------------------------------------------------
# Assembly-CSharp is everything under Assets that is not inside an assembly definition and not
# in an Editor folder.
find Assets -name "*.cs" \
  -not -path "Assets/Thirdweb/*" \
  -not -path "Assets/Plugins/*" \
  -not -path "Assets/UnitySQLiteAsync/*" \
  -not -path "Assets/Tests/*" \
  -not -path "Assets/Scripts/BattleRules/*" \
  -not -path "Assets/Scripts/Store/*" \
  -not -path "*/Editor/*" \
  | sort > "$OUT/sources.rsp"

echo "==> compiling Pepemon.Store"
"$DOTNET" "$CSC" -nologo -target:library -langversion:9.0 -nostdlib+ -nowarn:CS0169,CS0414,CS0649 \
  -out:"$OUT/Pepemon.Store.dll" \
  $(for d in "$UNITY"/MonoBleedingEdge/lib/mono/unityjit-macos/*.dll; do echo "-r:$d"; done) \
  $(for d in "$UNITY"/MonoBleedingEdge/lib/mono/unityjit-macos/Facades/*.dll; do echo "-r:$d"; done) \
  Assets/Scripts/Store/*.cs

echo "==> compiling Assembly-CSharp ($(wc -l < "$OUT/sources.rsp" | tr -d ' ') files)"
"$DOTNET" "$CSC" -nologo -target:library -langversion:9.0 -unsafe -nostdlib+ \
  -nowarn:CS0169,CS0414,CS0649,CS0618,CS0672,CS0114,CS0108,CS1998,CS4014,CS0162 \
  -define:UNITY_2021_3_OR_NEWER -define:UNITY_2021_3 -define:UNITY_2021 -define:UNITY_5_3_OR_NEWER \
  -define:UNITY_EDITOR -define:UNITY_STANDALONE_OSX -define:ODIN_INSPECTOR -define:ODIN_INSPECTOR_3 \
  -out:"$OUT/Assembly-CSharp.dll" \
  "-r:$OUT/Pepemon.Store.dll" \
  "@$OUT/refs.rsp" "@$OUT/sources.rsp"

NUNIT=$(find Library/PackageCache -name "nunit.framework.dll" 2>/dev/null | head -1)
TESTRUNNER=$(ls Library/ScriptAssemblies/UnityEngine.TestRunner.dll 2>/dev/null || true)

if [ -n "$NUNIT" ]; then
  echo "==> compiling EditMode tests"
  "$DOTNET" "$CSC" -nologo -target:library -langversion:9.0 -nostdlib+ \
    -nowarn:CS0169,CS0414,CS0649 \
    -out:"$OUT/Pepemon.BattleRules.Tests.dll" \
    $(for d in "$UNITY"/MonoBleedingEdge/lib/mono/unityjit-macos/*.dll; do echo "-r:$d"; done) \
    $(for d in "$UNITY"/MonoBleedingEdge/lib/mono/unityjit-macos/Facades/*.dll; do echo "-r:$d"; done) \
    $(for d in "$UNITY"/Managed/UnityEngine/*.dll; do echo "-r:$d"; done) \
    ${TESTRUNNER:+-r:$TESTRUNNER} \
    "-r:$NUNIT" \
    "-r:$OUT/Pepemon.Store.dll" \
    -r:Library/ScriptAssemblies/Pepemon.BattleRules.dll \
    Assets/Tests/EditMode/*.cs
else
  echo "==> skipping EditMode tests (nunit.framework.dll not found in Library/PackageCache)"
fi

echo
echo "==> COMPILE OK"
