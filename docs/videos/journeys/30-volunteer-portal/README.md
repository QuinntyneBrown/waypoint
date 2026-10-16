# 30 · Volunteer portal

> **Runtime:** about 4 min · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [30-volunteer-portal.mp4](30-volunteer-portal.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

A volunteer's whole working day, end to end: finding today's session, recording attendance and planning ahead, while seeing only names and never youth records. It follows the designs [view today's session](../../../detailed-designs/volunteer-portal/view-todays-session/README.md), [record session attendance](../../../detailed-designs/volunteer-portal/record-session-attendance/README.md) and [view upcoming schedule](../../../detailed-designs/volunteer-portal/view-upcoming-schedule/README.md), and the volunteer portal criteria in [the L2 specification](../../../specs/volunteer-portal/L2.md).

## Learning objectives

By the end, the viewer can:

- Describe what a volunteer sees on the home page and what is deliberately left out.
- Explain how attendance is recorded and what the roster shows.
- Say what the schedule lists and what a volunteer sees on a day with nothing assigned.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| What does a volunteer see about the youth? | A head count on today's page; names and present or absent on the roster; nothing else |
| Whose sessions appear on the schedule? | Only the volunteer's own, nearest first, with a status badge |
| What if nothing is assigned? | A calm empty message and a view schedule button |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/today-1280.png` | Today's session card |
| `images/roster-1280.png` | The attendance roster |
| `images/saved-1280.png` | Attendance saved with the running tally |
| `images/schedule-1280.png` | The upcoming schedule with badges |
| `images/empty-1280.png` | No session today |

Captures are made by `frontend/video-capture/journeys/30-volunteer-portal.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:25 | Welcome | Sam and the working day |
| 00:25-01:10 | Today's session | The card and the head count |
| 01:10-02:10 | Taking attendance | Roster, tally, save |
| 02:10-03:00 | Looking ahead | The schedule and its scope |
| 03:00-03:30 | A day with nothing assigned | The empty state |
| 03:30-04:00 | Recap | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts journeys/30
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/journeys/30-volunteer-portal --engine kokoro
node tools/video-build/build-video.mjs docs/videos/journeys/30-volunteer-portal
```

## Pitfalls

- The seeded session titles and youth names carry a short random suffix; the narration uses the plain names.
- Offline saving is mentioned in one sentence only; the attendance design video shows it in full.
