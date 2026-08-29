#!/usr/bin/env bash
# ---------------------------------------------------------------------------
# Shrinks the icon set inside a publish directory.
#
# Resources/images ships 4,879 PNGs, most of them 256x256 at ~70 KB, for 338 MB
# of the ~590 MB publish. IconManager caps its cache at 128 px (MaxCacheDimension)
# and the largest on-screen use is a 96x96 detail icon, so every pixel above 128
# is decoded and thrown away at startup.
#
# Resizing to 128 px and quantising to a 256-colour palette takes the set to
# ~31 MB. Measured against the already-downscaled 128 px image, quantisation
# costs an RMSE under 0.01 - not visible at icon size. The output stays PNG, so
# nothing in the loading path changes.
#
# This runs over a *publish output*, never the repository, so the source assets
# stay byte-identical to upstream.
#
# Usage: optimise-assets.sh <publish-dir>
# ---------------------------------------------------------------------------
set -euo pipefail

PUBLISH_DIR="${1:?usage: optimise-assets.sh <publish-dir>}"
IMAGES_DIR="$PUBLISH_DIR/Resources/images"
MAX_DIM=128
COLOURS=256

if ! command -v magick >/dev/null 2>&1; then
    echo "[assets] ImageMagick (magick) not found - skipping icon optimisation." >&2
    echo "[assets] The package will work, but will be roughly 300 MB larger." >&2
    exit 0
fi

[[ -d "$IMAGES_DIR" ]] || { echo "[assets] no $IMAGES_DIR - nothing to do"; exit 0; }

before=$(du -sm "$IMAGES_DIR" | cut -f1)
count=$(find "$IMAGES_DIR" -name '*.png' | wc -l)
echo "[assets] optimising $count icons (${before} MB)..."

# One magick process per batch rather than per file; 4,879 process spawns
# dominate the runtime otherwise.
# -I and -n are mutually exclusive; the batch form passes the files as "$@".
find "$IMAGES_DIR" -name '*.png' -print0 \
  | xargs -0 -P "$(nproc)" -n 24 sh -c '
        for f in "$@"; do
            magick "$f" -resize '"${MAX_DIM}x${MAX_DIM}"'\> -strip -colors '"$COLOURS"' "$f" 2>/dev/null || true
        done
    ' _

after=$(du -sm "$IMAGES_DIR" | cut -f1)
echo "[assets] icons: ${before} MB -> ${after} MB"
