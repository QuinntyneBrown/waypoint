# 17 · Schedule sessions and RSVP

> **Runtime:** about 3 min 37 s · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [17-schedule-sessions-and-rsvp.mp4](17-schedule-sessions-and-rsvp.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

A session has to exist before anyone can attend it, and the mentor plans around who says they will come. This video follows an administrator adding a weekly session and a youth replying to it. It draws on [schedule sessions](../../../detailed-designs/engagement/schedule-sessions/README.md) and [RSVP to a session](../../../detailed-designs/engagement/rsvp-to-session/README.md), and on L2-ENGAGE-001 and L2-ENGAGE-005 in [the engagement L2 specification](../../../specs/engagement/L2.md).

## Learning objectives

By the end, the viewer can:

- List what a session needs and what repeat weekly does.
- Describe the three RSVP answers and the note and reminder.
- Say where a new session leads next.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| What does a session need? | A title, a cohort and a mentor, plus date, time and place |
| What does repeat weekly create? | One session a week, from the chosen date through the cohort's last day of term, and none after |
| What can a youth add to an RSVP? | A note to the mentor and a reminder, which turns off when she can't make it |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/form-1280.png` | The new session form filled in |
| `images/scheduled-1280.png` | The confirmation and the agenda |
| `images/schedule-1280.png` | The youth schedule with the RSVP form |
| `images/saved-1280.png` | RSVP saved with the reminder time |

Captures are made by `frontend/video-capture/journeys/17-schedule-sessions-and-rsvp.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:22 | Welcome | A session starts as an entry on the calendar, and ends as a record of who came. |
| 00:24-01:06 | The new session form | Pat opens the program calendar. |
| 01:08-01:52 | A weekly session | This session should happen every week, so Pat turns on the repeat weekly option. |
| 01:54-02:22 | What Amara sees | Now Amara opens her schedule page. |
| 02:24-03:03 | Replying | Amara thinks she may be late, so she chooses maybe. |
| 03:05-03:37 | Things to remember | A session needs a title, a cohort and a mentor, plus a date, a time and a place. |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts journeys/17
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/journeys/17-schedule-sessions-and-rsvp --engine kokoro
node tools/video-build/build-video.mjs docs/videos/journeys/17-schedule-sessions-and-rsvp
```

## Pitfalls

- The new session form is narrow, so the time and place fields clip long values in the real UI.
- The number of weekly sessions depends on the seeded cohort term and is not stated.
