# Veritas brand assets

This directory is the canonical source for Veritas logo and browser icon artwork. Applications should reference these files directly through their build tooling rather than maintaining editable copies.

## Assets

- `veritas-lockup.svg` — horizontal Veritas mark and outlined wordmark for light surfaces.
- `veritas-mark.svg` — tightly cropped standalone mark.
- `favicon.svg` — square, optically padded browser icon master.
- `favicon.ico` — 16, 32, 48, and 64 pixel browser fallback.
- `apple-touch-icon.png` — 180 pixel touch icon.
- `icon-192.png` and `icon-512.png` — application icon derivatives for future manifests.
- `veritas-docs-og.png` — generated social-preview artwork for the public documentation site.

Logo and icon assets have transparent backgrounds. The SVG files contain native vector geometry with no embedded raster images or external font dependencies. Icon derivatives are generated from `favicon.svg`; edit the SVG masters rather than the PNG or ICO files. The documentation social preview is a standalone raster composition generated for Veritas and should be regenerated when its headline, status, or visual identity changes.

The current lockup is intended for light surfaces. Add a deliberately designed reversed variant before using it on dark backgrounds.
