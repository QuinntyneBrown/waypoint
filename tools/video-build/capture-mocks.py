"""Screenshot the static mocks listed in <video-folder>/images/shots.json.

Usage: python tools/video-build/capture-mocks.py docs/videos/01-own-board-overview

Writes <name>-1280.png (1280x800 viewport) and, when "phone" is true,
<name>-360.png (360x800 viewport) next to shots.json. The review chrome is
hidden with ?chrome=0. Needs: python -m pip install playwright, and
python -m playwright install chromium.
"""
import json
import sys
from pathlib import Path

from playwright.sync_api import sync_playwright

ROOT = Path(__file__).resolve().parents[2]
MOCKS = ROOT / "docs" / "mocks"


def main() -> int:
    if len(sys.argv) != 2:
        print(__doc__)
        return 2
    images = Path(sys.argv[1]).resolve() / "images"
    shots = json.loads((images / "shots.json").read_text(encoding="utf-8"))
    sizes = {"1280": (1280, 800), "360": (360, 800)}
    with sync_playwright() as p:
        browser = p.chromium.launch()
        for shot in shots:
            mock = MOCKS / shot["mock"]
            if not mock.exists():
                print(f"MISSING {mock}")
                return 1
            for label in ("1280", "360") if shot.get("phone") else ("1280",):
                w, h = sizes[label]
                page = browser.new_page(
                    viewport={"width": w, "height": h}, reduced_motion="reduce"
                )
                page.goto(mock.as_uri() + "?chrome=0")
                if shot.get("tags"):
                    # Show the proposal's "Edited in admin" tags, keep review chrome hidden.
                    page.add_style_tag(content=".cms-tag{display:inline-flex !important}")
                if shot.get("scroll"):
                    page.evaluate(f"window.scrollTo(0, {int(shot['scroll'])})")
                page.wait_for_timeout(300)
                out = images / f"{shot['name']}-{label}.png"
                page.screenshot(path=str(out))
                page.close()
                print(f"wrote {out.relative_to(ROOT)}")
        browser.close()
    return 0


if __name__ == "__main__":
    sys.exit(main())
