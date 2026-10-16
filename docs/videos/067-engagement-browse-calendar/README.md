# 067 · Browsing the program calendar

> **Runtime:** about 2 min 50 s · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [067-engagement-browse-calendar.mp4](067-engagement-browse-calendar.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

Staff find every session on the program calendar before they take attendance. This video shows the three calendar views, the cohort filter, what each session shows, and the link from a session to its attendance sheet. It follows [the browse calendar design](../../detailed-designs/engagement/browse-calendar/README.md) and the acceptance criteria of L2-ENGAGE-001 in [the engagement L2 specification](../../specs/engagement/L2.md).

## Learning objectives

By the end, the viewer can:

- Name the three calendar views and move between them.
- Narrow the calendar to one cohort.
- Say what each session shows and where its attendance link leads.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| Which views exist? | Month, week and agenda |
| How is the calendar narrowed? | The calendar cohort filter |
| What does a session show? | Time, place, mentor, cohort and youth count, plus a link to its attendance sheet |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/month-1280.png` | Month view filtered to one cohort |
| `images/week-1280.png` | Week view |
| `images/agenda-1280.png` | Agenda with the session card |
| `images/session-1280.png` | The take attendance button |

Captures are made by `frontend/video-capture/067-engagement-browse-calendar.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:17 | Welcome | Welcome |
| 00:17-00:59 | Three ways to look | Three ways to look |
| 00:59-01:40 | One cohort at a time | One cohort at a time |
| 01:40-02:27 | From session to attendance | From session to attendance |
| 02:27-02:50 | Things to remember | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts 067
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/067-engagement-browse-calendar --engine kokoro
node tools/video-build/build-video.mjs docs/videos/067-engagement-browse-calendar
```

## Pitfalls

- Cohort and session names in the captures carry a short random suffix because the data is seeded fresh for each capture.
- Mentors and volunteers have their own calendars with role-scoped data; this video shows the administrator calendar.
