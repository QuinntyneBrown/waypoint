# 084 · Making Waypoint easier to read

> **Runtime:** about 2 min 26 s · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [084-platform-services-configure-display.mp4](084-platform-services-configure-display.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

Display settings let each person adjust text size and four accessibility options. This video shows the live preview and that saved choices persist across a reload. It follows [the design](../../detailed-designs/platform-services/configure-display/README.md) and the acceptance criteria of L2-PLATFORM-004 (criteria 1 and 2) in [the platform services L2 specification](../../specs/platform-services/L2.md).

## Learning objectives

By the end, the viewer can:

- List the display options.
- Explain that changes preview before they are saved.
- Say how a person keeps their choices.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| What can a person change? | Text size, high contrast, reduce motion, underline links and reading font |
| When does a change become permanent? | When the person presses Save settings |
| What proves it persisted? | After a reload the text size and switches are unchanged |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/default-1280.png` | The display page at 100 percent |
| `images/preview-1280.png` | Text at 120 percent with high contrast and underline links on |
| `images/saved-1280.png` | The saved message |
| `images/reloaded-1280.png` | The page after a reload |

Captures are made by `frontend/video-capture/084-platform-services-configure-display.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:19 | Welcome | Welcome |
| 00:19-00:59 | The display page | The display page |
| 00:59-01:32 | A live preview | A live preview |
| 01:32-01:58 | Saving and keeping it | Saving and keeping it |
| 01:58-02:26 | Things to remember | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts video-capture/084-platform-services-configure-display.capture.ts
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/084-platform-services-configure-display --engine kokoro
node tools/video-build/build-video.mjs docs/videos/084-platform-services-configure-display
```

## Pitfalls

- Reduce motion and reading font are described but not switched on in the captures. High contrast and underline links are shown.
