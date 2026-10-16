# 065 · Viewing today's session

> **Runtime:** about 2 min 45 s · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [065-volunteer-portal-view-todays-session.mp4](065-volunteer-portal-view-todays-session.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

The volunteer home page must show a volunteer what they need and nothing more. This video shows the session card, the head count that replaces any roster detail, and the empty state. It follows [the today design](../../detailed-designs/volunteer-portal/view-todays-session/README.md) and the acceptance criteria of L2-VOLUNTEER-001 in [the volunteer portal L2 specification](../../specs/volunteer-portal/L2.md).

## Learning objectives

By the end, the viewer can:

- List what the today card shows.
- Explain why the card shows a head count and no names.
- Describe what a volunteer sees on a day without a session.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| What is on the today card? | Title, day and time, length, location, roster count, and a take attendance button |
| What does a volunteer never see here? | Youth names, progress or records; other sessions |
| What if there is no session today? | A no session today message that points to the schedule |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/today-1280.png` | The session card |
| `images/empty-1280.png` | The no session today state |

Captures are made by `frontend/video-capture/065-volunteer-portal-view-todays-session.capture.ts`. The session title includes a short code added by the seed data. The "scope" slide is text only because a refused request has no dedicated screen in this capture.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:20 | Welcome | Why the page is short |
| 00:20-01:05 | Landing on today | The session card |
| 01:05-01:35 | What a volunteer does not see | Scope and server checks |
| 01:35-01:50 | Going to attendance | The button |
| 01:50-02:15 | A day with no session | Empty state |
| 02:15-02:40 | Recap | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts video-capture/065-volunteer-portal-view-todays-session.capture.ts
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/065-volunteer-portal-view-todays-session --engine kokoro
node tools/video-build/build-video.mjs docs/videos/065-volunteer-portal-view-todays-session
```

## Pitfalls

- The forbidden response for out-of-scope requests is described from the specification and is not shown on screen.
