# 088 · Planned maintenance

> **Runtime:** about 2 min 22 s · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [088-platform-services-handle-planned-maintenance.mp4](088-platform-services-handle-planned-maintenance.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

During planned maintenance people see a calm page instead of an error. This video shows that page and what the server sends. It follows [the design](../../detailed-designs/platform-services/handle-planned-maintenance/README.md) and the acceptance criteria of L2-PLATFORM-009 (criteria 1 and 2) in [the platform services L2 specification](../../specs/platform-services/L2.md).

## Learning objectives

By the end, the viewer can:

- Say what the server returns in maintenance mode.
- Read the wording of the maintenance page.
- Explain what the Try again button does.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| What does the server return in maintenance mode? | A 503 service unavailable response with a retry hint |
| What does the page promise? | That nothing is lost |
| What does Try again do? | It reloads the page so the server is asked again |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/maintenance-1280.png` | The scheduled maintenance page |

Captures are made by `frontend/video-capture/088-platform-services-handle-planned-maintenance.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:16 | Welcome | Welcome |
| 00:16-00:40 | What the server says | What the server says |
| 00:40-01:28 | What Amara sees | What Amara sees |
| 01:28-01:54 | Checking again | Checking again |
| 01:54-02:22 | Things to remember | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts video-capture/088-platform-services-handle-planned-maintenance.capture.ts
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/088-platform-services-handle-planned-maintenance --engine kokoro
node tools/video-build/build-video.mjs docs/videos/088-platform-services-handle-planned-maintenance
```

## Pitfalls

- Maintenance mode is simulated in the capture: every API response is replaced with a 503 and a Retry-After header, and the real portal redirects to its maintenance page. The first slide is text only because the server response has no screen.
