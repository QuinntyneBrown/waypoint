# 10 · Submit assignments

> **Runtime:** about 3 min 29 s · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [10-submit-assignments.mp4](10-submit-assignments.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

An assignment is the main way a youth shows what they have learned. This video follows one submission end to end, including a request for changes, using [submit assignments](../../../detailed-designs/learning-experience/submit-assignments/README.md) and [track and revise submissions](../../../detailed-designs/learning-experience/track-and-revise-submissions/README.md), and L2-LEARN-006 and L2-LEARN-007 in [the learning experience L2 specification](../../../specs/learning-experience/L2.md).

## Learning objectives

By the end, the viewer can:

- Describe what the brief tells a youth before they start.
- Explain what Waypoint checks before a submission is sent.
- Follow a submission through review, revision and approval.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| What does a youth see before starting? | Task, due date, allowed submission types and the rubric with points |
| What happens to the work once it is submitted? | It becomes a read only copy, and the timeline records every step |
| What does a request for changes include? | The mentor note and a revise and resubmit button with the earlier answer filled in |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/brief-1280.png` | Assignment brief |
| `images/validation-1280.png` | The file check message |
| `images/ready-1280.png` | Own work box and submit button |
| `images/submitted-1280.png` | Status page, submitted |
| `images/revision-1280.png` | Needs revision with feedback |
| `images/approved-1280.png` | Timeline, rubric result, badge |

Captures are made by `frontend/video-capture/journeys/10-submit-assignments.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:17 | Welcome | The journey and the cast |
| 00:17-01:02 | Reading the brief | Task, due date, allowed types, rubric |
| 01:02-01:33 | Checks before sending | File checks and the own work box |
| 01:33-01:49 | Sending it | Submit and confirm |
| 01:49-02:09 | Waiting for review | Status page and timeline |
| 02:09-02:37 | When changes are needed | Feedback, revise and resubmit |
| 02:37-02:59 | Approved | Points, comment and badge |
| 02:59-03:29 | Things to remember | Recap |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts journeys/10
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/journeys/10-submit-assignments --engine kokoro
node tools/video-build/build-video.mjs docs/videos/journeys/10-submit-assignments
```

## Pitfalls

- The seeded titles carry a short random suffix; it is not part of the product.
- The mentor decisions in the capture are made through the mentor API; the grading screen has its own video.
