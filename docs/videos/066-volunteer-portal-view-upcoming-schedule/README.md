# 066 · Viewing your upcoming schedule

> **Runtime:** about 2 min 45 s · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [066-volunteer-portal-view-upcoming-schedule.mp4](066-volunteer-portal-view-upcoming-schedule.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

Volunteers need to plan ahead without seeing anything about the young people. This video shows the schedule list, its scope, and the empty state. It follows [the schedule design](../../detailed-designs/volunteer-portal/view-upcoming-schedule/README.md) and the acceptance criteria of L2-VOLUNTEER-002 in [the volunteer portal L2 specification](../../specs/volunteer-portal/L2.md).

## Learning objectives

By the end, the viewer can:

- List what each schedule card shows.
- Explain whose sessions appear on the schedule.
- Describe the empty state and when it appears.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| What does a schedule card show? | Title, status badge, day and start time, length, location |
| Whose sessions are listed? | Only the sessions assigned to the signed-in volunteer, nearest first |
| What if none are assigned? | A no upcoming sessions message that says they appear once the volunteer is scheduled |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/schedule-1280.png` | Three upcoming sessions |
| `images/empty-1280.png` | The empty schedule |

Captures are made by `frontend/video-capture/066-volunteer-portal-view-upcoming-schedule.capture.ts`. Session titles include a short code added by the seed data. The "scope" slide is text only because a refused request has no dedicated screen in this capture.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:20 | Welcome | Why the schedule exists |
| 00:20-01:10 | Opening the schedule | Cards, badges, order |
| 01:10-01:45 | Only your own sessions | Scope and server checks |
| 01:45-02:15 | When nothing is scheduled yet | Empty state |
| 02:15-02:45 | Recap | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts video-capture/066-volunteer-portal-view-upcoming-schedule.capture.ts
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/066-volunteer-portal-view-upcoming-schedule --engine kokoro
node tools/video-build/build-video.mjs docs/videos/066-volunteer-portal-view-upcoming-schedule
```

## Pitfalls

- A session whose attendance has been taken shows a different badge from upcoming; the narration mentions this but only the upcoming badge is captured.
- The forbidden response for out-of-scope requests is described from the specification and is not shown on screen.
