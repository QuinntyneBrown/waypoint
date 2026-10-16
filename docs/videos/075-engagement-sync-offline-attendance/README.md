# 075 · Recording attendance without a connection

> **Runtime:** about 2 min 53 s · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [075-engagement-sync-offline-attendance.mp4](075-engagement-sync-offline-attendance.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

Sessions do not always have internet. This video shows the offline banner, saving on the device, automatic sync, a conflict and the unsaved-changes warning. It follows [the sync offline attendance design](../../detailed-designs/engagement/sync-offline-attendance/README.md) and the acceptance criteria of L2-ENGAGE-003 in [the engagement L2 specification](../../specs/engagement/L2.md).

## Learning objectives

By the end, the viewer can:

- Describe what the user sees when offline.
- Explain how queued attendance syncs.
- Explain what happens on a conflict.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| What does the user see offline? | An offline banner and a pending sync badge |
| Are duplicates created on sync? | No; the update is merged once |
| What happens on a conflict? | It is shown clearly and resolved deliberately; nothing is silently lost |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/offline-marked-1280.png` | Marking while offline, with the banner |
| `images/queued-1280.png` | Saved offline, one update waiting |
| `images/synced-1280.png` | All attendance changes synced |
| `images/conflict-1280.png` | The conflict message |

Captures are made by `frontend/video-capture/075-engagement-sync-offline-attendance.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:18 | Welcome | Welcome |
| 00:18-00:47 | When the connection drops | When the connection drops |
| 00:47-01:15 | Saved on the device | Saved on the device |
| 01:15-01:38 | Catching up | Catching up |
| 01:38-02:26 | When two people edit the same session | When two people edit the same session |
| 02:26-02:53 | Things to remember | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts 075
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/075-engagement-sync-offline-attendance --engine kokoro
node tools/video-build/build-video.mjs docs/videos/075-engagement-sync-offline-attendance
```

## Pitfalls

- The unsaved-changes prompt is a native browser dialog and cannot be captured, so it is only narrated.
