# 040 · Tracking and revising a submission

> **Runtime:** about 3 min · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [040-learning-experience-track-and-revise-submissions.mp4](040-learning-experience-track-and-revise-submissions.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

Youth need to know where their work stands and what to do when a mentor asks for changes. This video follows one submission through the whole lifecycle. It follows [the track and revise submissions design](../../detailed-designs/learning-experience/track-and-revise-submissions/README.md) and the acceptance criteria of L2-LEARN-007 in [the learning experience L2 specification](../../specs/learning-experience/L2.md).

## Learning objectives

By the end, the viewer can:

- Describe the timeline a youth sees from submission to outcome.
- Explain what a youth sees and can do when a mentor requests a revision.
- State what an approval shows.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| What does the youth see right after submitting? | Status Submitted, a read-only copy, and a timeline to Under review |
| What happens when a revision is requested? | Status Needs revision, the mentor's note and a revise and resubmit path |
| What does approval show? | Status Approved, points awarded, the mentor's comment and any badge unlocked |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/submitted-1280.png` | Just submitted |
| `images/revision-1280.png` | Needs revision with feedback |
| `images/resubmit-1280.png` | The revise page |
| `images/resubmitted-1280.png` | Back under review |
| `images/approved-1280.png` | Approved with points and badge |

Captures are made by `frontend/video-capture/040-learning-experience-track-and-revise-submissions.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:15 | Welcome | What happens after submitting |
| 00:15-00:45 | Under review | Read-only copy and timeline |
| 00:45-01:15 | When changes are needed | Needs revision and feedback |
| 01:15-01:40 | Revising | The revise page |
| 01:40-02:00 | Back under review | Full timeline |
| 02:00-02:25 | Approved | Points, comment, badge |
| 02:25-02:50 | Things to remember | Recap |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts 040
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/040-learning-experience-track-and-revise-submissions --engine kokoro
node tools/video-build/build-video.mjs docs/videos/040-learning-experience-track-and-revise-submissions
```

## Pitfalls

- The mentor's review actions are made through the API in the capture; the mentor screens are covered in other videos.
- Assignment and badge names on screen carry a random suffix because the capture uses seeded test data.
