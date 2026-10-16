# 103 · Adapting to every screen size

> **Runtime:** about 2 min 45 s · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [103-non-functional-adapt-layouts.mp4](103-non-functional-adapt-layouts.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

Waypoint is used on phones, laptops and large screens. This video shows how the same pages rearrange themselves, from a single column on a phone to several columns on a wide screen, and how tables and wide views are handled. It follows [the adapt layouts design](../../detailed-designs/non-functional/adapt-layouts/README.md) and the acceptance criteria of L2-NFR-009 in [the non-functional L2 specification](../../specs/non-functional/L2.md).

## Learning objectives

By the end, the viewer can:

- Describe how a page changes between a phone and a laptop.
- Explain what happens to a data table on a small screen.
- State that the page itself never scrolls sideways on a phone.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| What does a phone show? | A single column, a bottom bar of main places, and no sideways page scroll |
| What happens to tables on a phone? | Each row becomes a card with labelled cells |
| What about wide views like the progress board? | They scroll sideways inside their own container; the page stays still |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/youth-narrow-1280.png` | Amara's home on a 375 pixel wide phone |
| `images/youth-wide-1280.png` | The same home page on a laptop |
| `images/admin-wide-1280.png` | The program administrator dashboard on a laptop |
| `images/admin-narrow-1280.png` | The dashboard on a phone |
| `images/people-narrow-1280.png` | The people directory as labelled cards on a phone |
| `images/progress-narrow-1280.png` | The progress board on a phone |

Captures are made by `frontend/video-capture/103-non-functional-adapt-layouts.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:20 | Welcome | Why layouts adapt |
| 00:20-01:05 | On a phone | Single column, bottom bar |
| 01:05-01:30 | On a laptop | The same page in columns |
| 01:30-02:25 | Staff pages and wide tables | Dashboard, labelled cards, scrolling box |
| 02:25-02:45 | Recap | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts video-capture/103-non-functional-adapt-layouts.capture.ts
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/103-non-functional-adapt-layouts --engine kokoro
node tools/video-build/build-video.mjs docs/videos/103-non-functional-adapt-layouts
```

## Pitfalls

- The phone captures use a 375 pixel wide browser window; the images are named with 1280 only to match the shared capture convention.
- The progress board capture fits without scrolling because the seeded track has one module; the scrolling box is described in the narration rather than shown.
