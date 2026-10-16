# 112 · Syncing attendance reliably

> **Runtime:** about 3 min 15 s · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [112-non-functional-sync-attendance-reliably.mp4](112-non-functional-sync-attendance-reliably.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

Attendance is often taken where the connection is poor. This video shows how Waypoint keeps offline attendance, uploads it once when the connection returns, and asks when a saved change clashes with a newer one. It follows [the sync attendance reliably design](../../detailed-designs/non-functional/sync-attendance-reliably/README.md) and the acceptance criteria of L2-NFR-004 and L2-NFR-006 in [the non-functional L2 specification](../../specs/non-functional/L2.md), and L2-ENGAGE-003 in [the engagement L2 specification](../../specs/engagement/L2.md).

## Learning objectives

By the end, the viewer can:

- Describe what a volunteer sees when the connection drops during attendance.
- Explain why a retry cannot create a duplicate attendance record.
- State what happens when a saved offline change clashes with a newer server edit.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| Where do offline marks go? | Onto the device, shown as waiting to sync, private to the signed-in owner |
| When do they upload? | Automatically when the connection returns and the owner is signed in |
| What if the record changed meanwhile? | Waypoint asks the person to apply their changes or keep the server attendance |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/offline-banner-1280.png` | The offline bar above the attendance page |
| `images/queued-1280.png` | Saved offline, one update waiting to sync |
| `images/synced-1280.png` | All attendance changes synced |
| `images/conflict-1280.png` | The choose which attendance to keep panel |

Captures are made by `frontend/video-capture/112-non-functional-sync-attendance-reliably.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:20 | Welcome | Why offline attendance matters |
| 00:20-01:05 | Taking attendance offline | Offline bar, saved on the device, pending badge |
| 01:05-01:50 | Coming back online | Automatic sync, unique keys, private queue |
| 01:50-02:40 | When two people change the same record | The conflict panel and its two choices |
| 02:40-03:15 | Recap | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts video-capture/112-non-functional-sync-attendance-reliably.capture.ts
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/112-non-functional-sync-attendance-reliably --engine kokoro
node tools/video-build/build-video.mjs docs/videos/112-non-functional-sync-attendance-reliably
```

## Pitfalls

- The conflict panel appears on the mentor attendance page; the volunteer page shows the offline bar, pending badge and sync messages.
- The names on the volunteer roster carry a short suffix because the capture seeds synthetic youth.
