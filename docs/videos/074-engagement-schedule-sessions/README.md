# 074 · Scheduling sessions

> **Runtime:** about 2 min 41 s · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [074-engagement-schedule-sessions.mp4](074-engagement-schedule-sessions.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

Sessions must exist before attendance. This video shows the new session form, weekly recurrence through the cohort term and the result on the calendar. It follows [the schedule sessions design](../../detailed-designs/engagement/schedule-sessions/README.md) and the acceptance criteria of L2-ENGAGE-001 in [the engagement L2 specification](../../specs/engagement/L2.md).

## Learning objectives

By the end, the viewer can:

- Say which fields the form needs.
- Explain what repeat weekly creates and where it stops.
- Find the new sessions on the calendar.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| Which fields are required? | Title, cohort and mentor, plus date and time |
| Where does weekly recurrence stop? | At the term end date of the cohort, inclusive |
| What links from a session? | Its attendance sheet |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/empty-form-1280.png` | The new session form |
| `images/filled-1280.png` | The form filled with repeat weekly on |
| `images/scheduled-1280.png` | The agenda listing the new sessions |

Captures are made by `frontend/video-capture/074-engagement-schedule-sessions.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:15 | Welcome | Welcome |
| 00:15-00:51 | The new session form | The new session form |
| 00:51-01:34 | A weekly session | A weekly session |
| 01:34-02:11 | Scheduled | Scheduled |
| 02:11-02:41 | Things to remember | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts 074
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/074-engagement-schedule-sessions --engine kokoro
node tools/video-build/build-video.mjs docs/videos/074-engagement-schedule-sessions
```

## Pitfalls

- The calendar holds sessions from other cohorts; the capture filters to the seeded cohort.
