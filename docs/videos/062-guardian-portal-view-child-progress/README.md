# 062 · Following a young person's progress

> **Runtime:** about 3 min · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [062-guardian-portal-view-child-progress.mp4](062-guardian-portal-view-child-progress.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

Guardians need to see how their young person is progressing without being shown content or notes that are not meant for them. It follows [the design](../../detailed-designs/guardian-portal/view-child-progress/README.md) and the acceptance criteria of L2-GUARDIAN-001 in [the L2 specification](../../specs/guardian-portal/L2.md).

## Learning objectives

By the end, the viewer can:

- List what the guardian progress view shows.
- Explain which quiz outcomes and mentor updates are visible.
- State what the view deliberately leaves out.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| What does a guardian see? | Overall progress, module completion, attendance history, approved quiz outcomes, competencies, earned and locked badges, upcoming schedule and shared mentor updates. |
| Which mentor notes are shown? | Only check-ins the mentor explicitly shared; private check-ins never appear. |
| Can a guardian edit anything? | No. There are no edit controls, and no lesson content, quiz answers or private reflections. |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/home-1280.png` | Three tiles and what is next |
| `images/modules-attendance-1280.png` | Modules and attendance history |
| `images/quiz-badges-1280.png` | Approved results and badges |
| `images/schedule-updates-1280.png` | Only what the mentor chose to share |

Captures are made by `frontend/video-capture/062-guardian-portal-view-child-progress.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:15 | Welcome | Purpose of the view |
| 00:15-00:45 | Home at a glance | Tiles and What's next |
| 00:45-01:25 | Modules and attendance | Modules list and attendance history |
| 01:25-02:05 | Quizzes, skills and badges | Approved outcomes, competencies, badges |
| 02:05-02:45 | Schedule and shared updates | Schedule, shared check-ins, what is left out |
| 02:45-03:00 | Recap | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts video-capture/062-guardian-portal-view-child-progress.capture.ts
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/062-guardian-portal-view-child-progress --engine kokoro
node tools/video-build/build-video.mjs docs/videos/062-guardian-portal-view-child-progress
```

## Pitfalls

- Module, session and badge names carry random suffixes from the Testing seed; the narration does not read them out.
- The still-to-unlock list can be long in the shared test database, so the capture crops to its first lines.
