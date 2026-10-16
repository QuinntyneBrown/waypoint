# 033 · Completing a coding lesson

> **Runtime:** about 3 min · **Audience:** program staff and stakeholders · **Prerequisites:** video 032

**Video:** [033-learning-experience-complete-coding-lessons.mp4](033-learning-experience-complete-coding-lessons.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

Coding lessons are hands-on. This video shows the steps, editor, live preview, checklist and project submission. It follows [the complete coding lessons design](../../detailed-designs/learning-experience/complete-coding-lessons/README.md) and the acceptance criteria of L2-LEARN-001 and L2-LEARN-008 in [the learning experience L2 specification](../../specs/learning-experience/L2.md).

## Learning objectives

By the end, the viewer can:

- Describe the parts of a coding lesson.
- Explain how progress toward a badge is recorded.
- Describe what happens when a project is submitted.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| Is work lost if a youth leaves? | No; the workspace is saved and resumes |
| What earns the badge? | Finishing the self-check items records progress; at one hundred percent the badge is shown as earned |
| What happens on submit? | A read-only copy, with an optional note, enters the same review lifecycle as assignments |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/start-1280.png` | Progress at zero, steps and editor |
| `images/preview-1280.png` | Live preview |
| `images/done-1280.png` | One hundred percent and badge earned |
| `images/submit-1280.png` | Mentor review form |
| `images/status-1280.png` | Submission status |

Captures are made by `frontend/video-capture/033-learning-experience-complete-coding-lessons.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:15 | Welcome | What a coding lesson is |
| 00:15-01:05 | The coding lesson | Progress bar, steps, editor, preview |
| 01:05-01:35 | Steps and the checklist | Checks, badge, resuming |
| 01:35-02:30 | Sending it for review | Note, own work box, status |
| 02:30-03:00 | Recap | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts video-capture/033-learning-experience-complete-coding-lessons.capture.ts
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/033-learning-experience-complete-coding-lessons --engine kokoro
node tools/video-build/build-video.mjs docs/videos/033-learning-experience-complete-coding-lessons
```

## Pitfalls

- The badge and lesson names on screen include a random suffix because the capture uses seeded test data.
