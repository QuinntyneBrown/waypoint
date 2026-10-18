# 117 · Switching between light and dark

> **Runtime:** about 2 min 30 s · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [117-platform-services-switch-theme.mp4](117-platform-services-switch-theme.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

Waypoint ships a light theme and a dark theme, and each person chooses System, Light or Dark. This video shows the signed-out behavior, the live preview and that a saved choice overrides the device. It follows [the design](../../detailed-designs/platform-services/switch-theme/README.md) and the acceptance criteria of L2-PLATFORM-013 (AC1 to AC3 and AC5) in [the platform services L2 specification](../../specs/platform-services/L2.md).

## Learning objectives

By the end, the viewer can:

- Say what System, Light and Dark mean.
- Explain that a choice previews before it is saved.
- Say why a saved theme still applies after a reload on a device set to the other scheme.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| What does a signed-out page do? | It follows the device colour scheme |
| What is the default? | System, which matches the device |
| When does a choice become permanent? | When the person presses Save settings |
| Which wins, the device or a saved choice? | An explicit Light or Dark overrides the device |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/device-light-1280.png` | Sign in page with a light device |
| `images/device-dark-1280.png` | Sign in page with a dark device |
| `images/system-1280.png` | Theme control on System, light device |
| `images/preview-dark-1280.png` | Dark chosen, not yet saved |
| `images/saved-1280.png` | The saved message |
| `images/reloaded-1280.png` | Still dark after a reload on a light device |

Captures are made by `frontend/video-capture/117-platform-services-switch-theme.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:20 | Welcome | Welcome |
| 00:20-00:55 | Following the device | Signed-out pages follow the device |
| 00:55-01:20 | Choosing a theme | System, Light, Dark |
| 01:20-01:40 | A live preview | Dark previews at once |
| 01:40-02:10 | Saving and keeping it | Saved, and kept after a reload |
| 02:10-02:30 | Things to remember | Things to remember |

The times are estimates until the video is built; update them from the build output.

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts video-capture/117-platform-services-switch-theme.capture.ts
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/117-platform-services-switch-theme --engine kokoro
node tools/video-build/build-video.mjs docs/videos/117-platform-services-switch-theme
```

## Pitfalls

- The capture emulates the device colour scheme with Playwright (`page.emulateMedia`); the saved-theme shots use a light device so the override is visible.
