# 104 · Live notifications and messages

> **Runtime:** about 2 min 45 s · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [104-non-functional-deliver-realtime-events.mp4](104-non-functional-deliver-realtime-events.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

Messages and notifications should reach a young person as they happen. This video shows the live updates badge, a notification and a message arriving without a reload, and what keeps delivery scoped and steady. It follows [the deliver realtime events design](../../detailed-designs/non-functional/deliver-realtime-events/README.md) and the acceptance criteria of L2-NFR-005 in [the non-functional L2 specification](../../specs/non-functional/L2.md), with L2-ENGAGE-007 and L2-ENGAGE-008 in [the engagement L2 specification](../../specs/engagement/L2.md).

## Learning objectives

By the end, the viewer can:

- Recognise the live updates badge and what it means.
- Describe what happens on screen when a mentor sends a message.
- Explain who a live notification is delivered to and what happens after a dropped connection.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| How does a new notification reach the page? | Live over the open connection, with no reload |
| Who receives it? | Only the user, cohort or role groups the signed-in person is authorised for |
| What happens after a dropped connection? | The page reconnects, reloads the latest items and does not duplicate any |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/waiting-1280.png` | The notifications page with live updates on and nothing yet |
| `images/arrived-1280.png` | The new message notification |
| `images/message-1280.png` | A live message in the thread |

Captures are made by `frontend/video-capture/104-non-functional-deliver-realtime-events.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:20 | Welcome | Why delivery should be live |
| 00:20-01:00 | A notification arrives | Live updates badge, new notification |
| 01:00-01:30 | A message arrives | Thread updates by itself, status line |
| 01:30-02:25 | Safe and steady | Token, scoped groups, catching up after a drop |
| 02:25-02:45 | Recap | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts video-capture/104-non-functional-deliver-realtime-events.capture.ts
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/104-non-functional-deliver-realtime-events --engine kokoro
node tools/video-build/build-video.mjs docs/videos/104-non-functional-deliver-realtime-events
```

## Pitfalls

- The group scoping and reconnect behaviour are not visible on screen; those slides are text only.
- The shared in-memory API can be slow, so the capture waits up to thirty seconds for the live badge.
