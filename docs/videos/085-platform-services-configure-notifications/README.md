# 085 · Choosing which notifications you get

> **Runtime:** about 2 min 21 s · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [085-platform-services-configure-notifications.mp4](085-platform-services-configure-notifications.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

Notification settings let each person choose, per event, whether to be told in the app and by email. This video shows the page, one change, and that it persists. It follows [the design](../../detailed-designs/platform-services/configure-notifications/README.md) and the acceptance criteria of L2-PLATFORM-003 (criteria 1 and 2) in [the platform services L2 specification](../../specs/platform-services/L2.md).

## Learning objectives

By the end, the viewer can:

- Name the two channels for each event.
- Explain that a change saves as soon as it is made.
- State what a turned-off channel means.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| Which channels exist? | In-app, and email as an optional second channel |
| Is there a save button? | No. Changes save automatically and a message confirms it |
| What happens when a channel is off? | No notification is sent on that channel for that event |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/page-1280.png` | The list of events with In-app and Email boxes |
| `images/before-1280.png` | Announcements with both boxes empty |
| `images/saved-1280.png` | Announcements ticked and the saved message |
| `images/reloaded-1280.png` | The page after a reload |

Captures are made by `frontend/video-capture/085-platform-services-configure-notifications.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:16 | Welcome | Welcome |
| 00:16-01:03 | The notifications page | The notifications page |
| 01:03-01:29 | Changing a choice | Changing a choice |
| 01:29-01:55 | It sticks, and it is respected | It sticks, and it is respected |
| 01:55-02:21 | Things to remember | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts video-capture/085-platform-services-configure-notifications.capture.ts
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/085-platform-services-configure-notifications --engine kokoro
node tools/video-build/build-video.mjs docs/videos/085-platform-services-configure-notifications
```

## Pitfalls

- The captures use a mentor account. Announcements are off by default; the other events start with In-app on and Email off. The last point about disabled channels comes from the L2 criterion; the video does not demonstrate delivery.
