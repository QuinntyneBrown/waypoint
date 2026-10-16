# 053 · Assigning work

> **Runtime:** about 2 min 30 s · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [053-mentoring-assign-work.mp4](053-mentoring-assign-work.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

Mentors steer what each youth does next. This video shows the form, the recipients, the due date, reminder and message, and the confirmation step. It follows [the assign work design](../../detailed-designs/mentoring/assign-work/README.md) and the acceptance criteria of L2-MENTOR-008 in [the mentoring L2 specification](../../specs/mentoring/L2.md).

## Learning objectives

By the end, the viewer can:

- Name what can be assigned and to whom.
- Describe the due date, reminder and message options.
- Explain that nothing is created until the mentor confirms.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| What is required? | A type and an item, and recipients |
| Who can receive work? | Selected youth, or the whole cohort |
| What happens on confirm? | One assignment per recipient appears in their journey, the message is delivered, and the count is reported |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/form-1280.png` | The top of the form with type and item |
| `images/filled-1280.png` | Recipient, due date, reminder and message |
| `images/confirm-1280.png` | The confirmation window |

Captures are made by `frontend/video-capture/053-mentoring-assign-work.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:20 | Welcome | What assigning means |
| 00:20-00:45 | Choosing what to assign | Type and item |
| 00:45-01:30 | Choosing who and when | Recipients, due date, reminder, message |
| 01:30-02:10 | Review and confirm | The confirmation window |
| 02:10-02:30 | Recap | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts video-capture/053-mentoring-assign-work.capture.ts
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/053-mentoring-assign-work --engine kokoro
node tools/video-build/build-video.mjs docs/videos/053-mentoring-assign-work
```

## Pitfalls

- The result message after confirming is announced to assistive technology only and is not visible on screen, so the video does not show it.
- The seeded youth shows the at-risk tag because the seed gives them a missed session.
