# 037 · Submitting an assignment

> **Runtime:** about 3 min · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [037-learning-experience-submit-assignments.mp4](037-learning-experience-submit-assignments.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

Assignments are how a youth shows learning through action. This video shows the brief, the checks on the form and the submitted result. It follows [the submit assignments design](../../detailed-designs/learning-experience/submit-assignments/README.md) and the acceptance criteria of L2-LEARN-006 in [the learning experience L2 specification](../../specs/learning-experience/L2.md).

## Learning objectives

By the end, the viewer can:

- List what an assignment brief shows.
- Explain what the form checks before it sends.
- Describe what a youth can and cannot do after submitting.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| What does the brief show? | Task, due date and time, allowed submission types, rubric preview with points |
| What is checked? | Required fields, the own work attestation, and photo type (JPG or PNG) and size |
| What happens after submit? | Status "Submitted", read-only copy, routed to submission status |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/brief-1280.png` | Task, allowed types, rubric |
| `images/validation-1280.png` | Photo type error |
| `images/ready-1280.png` | Photo ready |
| `images/status-1280.png` | Submitted status |

Captures are made by `frontend/video-capture/037-learning-experience-submit-assignments.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:15 | Welcome | What an assignment is |
| 00:15-01:00 | The brief | Task, due date, types, rubric |
| 01:00-01:35 | Checks before sending | Attestation and file checks |
| 01:35-02:10 | Ready to send | Photo, response, confirm |
| 02:10-02:40 | Submitted | Status page, read only |
| 02:40-03:05 | Recap | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts video-capture/037-learning-experience-submit-assignments.capture.ts
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/037-learning-experience-submit-assignments --engine kokoro
node tools/video-build/build-video.mjs docs/videos/037-learning-experience-submit-assignments
```

## Pitfalls

- The ten megabyte limit is a configured value; the screen shows the configured limit.
