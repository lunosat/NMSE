#!/usr/bin/env bash
# ---------------------------------------------------------------------------
# Builds a portable .tar.gz: extract anywhere and run ./nmse.
#
# Usage: build-tarball.sh [--arch x64|arm64]
# ---------------------------------------------------------------------------
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
ARCH="x64"
[[ "${1:-}" == "--arch" ]] && ARCH="$2"

VERSION="$(python3 -c "
import json;d=json.load(open('$REPO_ROOT/version.json'))
print(f\"{d['major']}.{d['minor']}.{d['patch']}\")" 2>/dev/null || echo "0.0.0")"

STAGE="$REPO_ROOT/Build/package/linux-$ARCH"
[[ -d "$STAGE" ]] || "$REPO_ROOT/packaging/build.sh" --arch "$ARCH"

WORK="$REPO_ROOT/Build/tarball/NMSE-$VERSION-linux-$ARCH"
DIST="$REPO_ROOT/Build/dist"
rm -rf "$WORK"; mkdir -p "$WORK/lib" "$DIST"

cp -a "$STAGE/." "$WORK/lib/"
cp "$REPO_ROOT/packaging/io.github.vectorcmdr.NMSE.desktop" "$WORK/"
cp -a "$REPO_ROOT/packaging/icons" "$WORK/"
cp "$REPO_ROOT/LICENSE" "$WORK/"

# The app resolves Resources/ relative to the working directory, so the launcher
# changes into lib/ rather than invoking the binary by path.
cat > "$WORK/nmse" <<'EOF'
#!/bin/sh
here="$(cd "$(dirname "$0")" && pwd)"
cd "$here/lib"
exec "$here/lib/NMSE" "$@"
EOF
chmod +x "$WORK/nmse"

cat > "$WORK/install.sh" <<'EOF'
#!/bin/sh
# Optional: register the launcher and desktop entry for the current user.
set -e
here="$(cd "$(dirname "$0")" && pwd)"
mkdir -p "$HOME/.local/bin" "$HOME/.local/share/applications"
ln -sf "$here/nmse" "$HOME/.local/bin/nmse"
sed "s|^Exec=.*|Exec=$here/nmse %f|" "$here/io.github.vectorcmdr.NMSE.desktop" \
    > "$HOME/.local/share/applications/io.github.vectorcmdr.NMSE.desktop"
for size in 16 32 48 64 128 256; do
    d="$HOME/.local/share/icons/hicolor/${size}x${size}/apps"
    mkdir -p "$d"
    cp "$here/icons/${size}.png" "$d/io.github.vectorcmdr.NMSE.png"
done
command -v update-desktop-database >/dev/null 2>&1 && \
    update-desktop-database "$HOME/.local/share/applications" || true
echo "Installed. Run 'nmse' (ensure ~/.local/bin is on PATH)."
EOF
chmod +x "$WORK/install.sh"

OUT="$DIST/NMSE-$VERSION-linux-$ARCH.tar.gz"
tar -C "$(dirname "$WORK")" -czf "$OUT" "$(basename "$WORK")"
echo "[tarball] done: $(du -sh "$OUT" | cut -f1) -> $OUT"
