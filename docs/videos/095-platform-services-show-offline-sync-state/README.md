# 095 · Working without a connection

> **Runtime:** about 2 min 30 s · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [095-platform-services-show-offline-sync-state.mp4](095-platform-services-show-offline-sync-state.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

Sessions happen where the connection is weak. This video shows the offline banner and the pending-sync count. It follows [the design](../../detailed-designs/platform-services/show-offline-sync-state/README.md) and the acceptance criteria of L2-PLATFORM-012 and L2-NFR-004 in [the platform services L2 specification](../../specs/platform-services/L2.md).

## Learning objectives

By the end, the viewer can:

- Describe the offline banner.
- Explain what keeps working offline.
- Say how waiting changes are shown and what happens on reconnect.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| How does the person know they are offline? | A banner: "You’re offline — changes will save when you reconnect" |
| How are waiting changes shown? | A message and a "pending sync" badge with the count |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/online-1280.png` | Before going offline |
| `images/offline-banner-1280.png` | The offline banner |
| `images/pending-1280.png` | Saved offline with a pending badge |
| `images/synced-1280.png` | After reconnecting |

Captures are made by `frontend/video-capture/095-platform-services-show-offline-sync-state.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:20 | Welcome | Why offline matters |
| 00:20-00:35 | Taking attendance | Sam marks the roster |
| 00:35-01:05 | The device goes offline | The banner |
| 01:05-01:40 | Changes waiting to sync | Saved offline, pending badge |
| 01:40-02:10 | Back online | Banner clears, changes sync |
| 02:10-02:30 | Recap | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts video-capture/095-platform-services-show-offline-sync-state.capture.ts
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/095-platform-services-show-offline-sync-state --engine kokoro
node tools/video-build/build-video.mjs docs/videos/095-platform-services-show-offline-sync-state
```

## Pitfalls

- Offline is simulated in the browser, as in the end-to-end tests. The roster shows seeded demo names.
- The video shows a pending count of one.
