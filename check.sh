#!/bin/bash
# Local pre-push gate: compile + EditMode tests.
# Catches CS errors in ~30s instead of a 5-minute CI round trip, and stops
# "it's green" being claimed before anything has actually been checked.
set -u
UNITY=/Applications/Unity/Hub/Editor/2021.3.12f1/Unity.app/Contents/MacOS/Unity
LOG=${TMPDIR:-/tmp}/pepemon-check.log
PROJ="$(cd "$(dirname "$0")" && pwd)"

echo "==> compiling ($PROJ)"
"$UNITY" -batchmode -quit -nographics -projectPath "$PROJ" -logFile "$LOG" >/dev/null 2>&1
if grep -q "error CS" "$LOG"; then
  echo "COMPILE FAILED:"; grep "error CS" "$LOG" | sort -u | head -20; exit 1
fi
grep -q "Exiting batchmode successfully" "$LOG" || { echo "editor did not exit cleanly - see $LOG"; exit 1; }
echo "    compile OK"

echo "==> EditMode tests"
"$UNITY" -batchmode -runTests -testPlatform EditMode -projectPath "$PROJ" \
  -testResults "${TMPDIR:-/tmp}/pepemon-tests.xml" -logFile "${LOG}.tests" >/dev/null 2>&1
RC=$?
R="${TMPDIR:-/tmp}/pepemon-tests.xml"
if [ -f "$R" ]; then
  python3 - "$R" <<'PY'
import sys,xml.etree.ElementTree as ET
r=ET.parse(sys.argv[1]).getroot()
t,f=r.get('total'),r.get('failed')
print(f"    tests: {t} total, {f} failed")
if f and int(f)>0:
    for tc in r.iter('test-case'):
        if tc.get('result')!='Passed': print("      FAIL:",tc.get('fullname'))
    sys.exit(1)
PY
  [ $? -ne 0 ] && exit 1
else
  echo "    no results file (rc=$RC) - see ${LOG}.tests"; exit 1
fi
echo "==> ALL CHECKS PASSED"
