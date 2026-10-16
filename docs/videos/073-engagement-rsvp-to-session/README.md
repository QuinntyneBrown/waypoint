# 073 · Replying to a session

> **Runtime:** about 2 min 41 s · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [073-engagement-rsvp-to-session.mp4](073-engagement-rsvp-to-session.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

Youth tell the program whether they will come. This video shows the answers, the note to the mentor, the reminder and changing an answer. It follows [the RSVP to session design](../../detailed-designs/engagement/rsvp-to-session/README.md) and the acceptance criteria of L2-ENGAGE-005 in [the engagement L2 specification](../../specs/engagement/L2.md).

## Learning objectives

By the end, the viewer can:

- Name the three answers.
- Add a note and set a reminder.
- Explain what can not make it does to the reminder.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| Which answers exist? | Going, maybe and can not make it |
| Who receives the note? | The mentor |
| What happens to the reminder on can not make it? | It is cleared and disabled |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/schedule-1280.png` | The schedule and RSVP form |
| `images/filled-1280.png` | Maybe, a note and a reminder |
| `images/saved-1280.png` | RSVP saved |
| `images/cant-make-it-1280.png` | Can not make it, reminder off |

Captures are made by `frontend/video-capture/073-engagement-rsvp-to-session.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:18 | Welcome | Welcome |
| 00:18-00:53 | My schedule | My schedule |
| 00:53-01:23 | Answering with a note | Answering with a note |
| 01:23-01:56 | Saved, and remembered | Saved, and remembered |
| 01:56-02:14 | Changing her mind | Changing her mind |
| 02:14-02:41 | Things to remember | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts 073
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/073-engagement-rsvp-to-session --engine kokoro
node tools/video-build/build-video.mjs docs/videos/073-engagement-rsvp-to-session
```

## Pitfalls

- The portal has no mentor screen for RSVPs; the narration states only what the L2 criteria say about the mentor seeing the answer.
