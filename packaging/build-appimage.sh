#!/usr/bin/env bash
# ---------------------------------------------------------------------------
# Builds a native AppImage.
#
# This replaces scripts/linux/build-appimage.sh, which bundled a full Wine
# installation around the Windows executable. The payload here is the Linux
# binary itself, so the AppImage carries no Wine and no .NET runtime.
#
# Usage: build-appimage.sh [--arch x64|arm64]
# ---------------------------------------------------------------------------
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
ARCH="x64"
[[ "${1:-}" == "--arch" ]] && ARCH="$2"

case "$ARCH" in
    x64)   APPIMAGE_ARCH="x86_64" ;;
    arm64) APPIMAGE_ARCH="aarch64" ;;
    *) echo "unsupported arch: $ARCH" >&2; exit 2 ;;
esac

VERSION="$(python3 -c "
import json;d=json.load(open('$REPO_ROOT/version.json'))
print(f\"{d['major']}.{d['minor']}.{d['patch']}\")" 2>/dev/null || echo "0.0.0")"

STAGE="$REPO_ROOT/Build/package/linux-$ARCH"
APPDIR="$REPO_ROOT/Build/AppDir"
DIST="$REPO_ROOT/Build/dist"

[[ -d "$STAGE" ]] || "$REPO_ROOT/packaging/build.sh" --arch "$ARCH"

echo "[appimage] assembling AppDir"
rm -rf "$APPDIR"; mkdir -p "$APPDIR/usr/bin" "$APPDIR/usr/share/applications" \
    "$APPDIR/usr/share/icons/hicolor/256x256/apps" "$DIST"

cp -a "$STAGE/." "$APPDIR/usr/bin/"

# AppImage requires the desktop file, icon and AppRun at the AppDir root.
cp "$REPO_ROOT/packaging/io.github.vectorcmdr.NMSE.desktop" \
   "$APPDIR/usr/share/applications/"
cp "$REPO_ROOT/packaging/io.github.vectorcmdr.NMSE.desktop" "$APPDIR/"

ICON_SRC="$REPO_ROOT/packaging/nmse.png"
if [[ -f "$ICON_SRC" ]]; then
    cp "$ICON_SRC" "$APPDIR/usr/share/icons/hicolor/256x256/apps/io.github.vectorcmdr.NMSE.png"
    cp "$ICON_SRC" "$APPDIR/io.github.vectorcmdr.NMSE.png"
fi

cat > "$APPDIR/AppRun" <<'APPRUN'
#!/usr/bin/env bash
# AppImage entry point. $APPDIR is the mounted squashfs root.
set -euo pipefail
APPDIR="${APPDIR:-"$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"}"

# The app reads Resources/ relative to the binary, so run from that directory.
cd "$APPDIR/usr/bin"

# Avalonia picks X11 by default. Honour an explicit override, otherwise let it
# choose - the Wayland backend is still behind a flag upstream.
exec "$APPDIR/usr/bin/NMSE" "$@"
APPRUN
chmod +x "$APPDIR/AppRun"

# appimagetool builds the squashfs; zstd keeps the JSON localisation data small.
#
# The tool has to match the machine running it, not the AppImage being built:
# cross-building an aarch64 image from an x86_64 runner needs the x86_64 tool
# with ARCH pointing at the target, which is how it picks the runtime to embed.
HOST_ARCH="$(uname -m)"
TOOL="$REPO_ROOT/Build/appimagetool-$HOST_ARCH.AppImage"
if [[ ! -x "$TOOL" ]]; then
    echo "[appimage] fetching appimagetool for $HOST_ARCH"
    curl -fsSL -o "$TOOL" \
      "https://github.com/AppImage/appimagetool/releases/download/continuous/appimagetool-$HOST_ARCH.AppImage"
    chmod +x "$TOOL"
fi

OUT="$DIST/NMSE-$VERSION-$APPIMAGE_ARCH.AppImage"
echo "[appimage] building $OUT"
ARCH="$APPIMAGE_ARCH" "$TOOL" --comp zstd "$APPDIR" "$OUT"

echo "[appimage] done: $(du -sh "$OUT" | cut -f1)"
