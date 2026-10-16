# 054 · Grading a submission

> **Runtime:** about 3 min · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [054-mentoring-grade-submissions.mp4](054-mentoring-grade-submissions.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

Grading is the mentor's main decision point on submitted work. This video shows the rubric, the three outcomes and what each one does. It follows [the grade submissions design](../../detailed-designs/mentoring/grade-submissions/README.md) and the acceptance criteria of L2-MENTOR-003 in [the mentoring L2 specification](../../specs/mentoring/L2.md).

## Learning objectives

By the end, the viewer can:

- Describe how rubric scores become a total.
- Name the three outcomes and what each does for the youth.
- Say what a revision requires.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| What does approve do? | Marks it Approved, applies points and any badge, notifies the youth |
| What does a revision need? | A feedback note; guardian notification is on by default |
| What does a draft change? | Nothing the youth can see |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/submission-1280.png` | Submitted work and rubric |
| `images/revision-note-1280.png` | The feedback-required message |
| `images/draft-1280.png` | The draft saved message |
| `images/ready-1280.png` | A full score ready to approve |
| `images/queue-1280.png` | The queue after approving |

Captures are made by `frontend/video-capture/054-mentoring-grade-submissions.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:20 | Welcome | The three outcomes |
| 00:20-00:55 | The submission | Work, rubric, total |
| 00:55-01:30 | Asking for a revision | Mandatory feedback, guardian notice |
| 01:30-01:55 | Saving a draft | Nothing changes for the youth |
| 01:55-02:35 | Approving | Points, badge, notification, audit |
| 02:35-03:00 | Recap | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts video-capture/054-mentoring-grade-submissions.capture.ts
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/054-mentoring-grade-submissions --engine kokoro
node tools/video-build/build-video.mjs docs/videos/054-mentoring-grade-submissions
```

## Pitfalls

- The capture approves the same submission it drafted; the narration frames the approval as a later day.
- The assignment title carries a random suffix from the seed data.
