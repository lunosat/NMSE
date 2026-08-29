#!/usr/bin/env bash
# ---------------------------------------------------------------------------
# Publishes NMSE as a self-contained native Linux binary and stages it for
# packaging. No Wine and no .NET runtime on the target machine.
#
# Usage: build.sh [--arch x64|arm64] [--no-optimise] [--out DIR]
# ---------------------------------------------------------------------------
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
ARCH="x64"
OPTIMISE=1
OUT_DIR=""

while [[ $# -gt 0 ]]; do
    case "$1" in
        --arch)         ARCH="$2"; shift 2 ;;
        --no-optimise)  OPTIMISE=0; shift ;;
        --out)          OUT_DIR="$2"; shift 2 ;;
        -h|--help)      sed -n '2,8p' "$0"; exit 0 ;;
        *) echo "unknown option: $1" >&2; exit 2 ;;
    esac
done

RID="linux-$ARCH"
OUT_DIR="${OUT_DIR:-$REPO_ROOT/Build/package/$RID}"

command -v dotnet >/dev/null 2>&1 || { echo "dotnet not found on PATH" >&2; exit 1; }

echo "[build] publishing $RID -> $OUT_DIR"
rm -rf "$OUT_DIR"

# Trimming is off: Avalonia resolves control themes and the app resolves
# JSON converters by reflection, which the trimmer cannot see through.
dotnet publish "$REPO_ROOT/NMSE.csproj" \
    -c Release \
    -r "$RID" \
    --self-contained true \
    -p:PublishTrimmed=false \
    -p:DebugType=none \
    -o "$OUT_DIR"

# The apphost is what users launch; make sure it is executable after any
# copy through a filesystem that drops the bit.
chmod +x "$OUT_DIR/NMSE"

[[ $OPTIMISE -eq 1 ]] && "$REPO_ROOT/packaging/optimise-assets.sh" "$OUT_DIR"

echo "[build] done: $(du -sh "$OUT_DIR" | cut -f1) in $OUT_DIR"
