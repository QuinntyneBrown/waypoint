# 038 · Submitting a coding project

> **Runtime:** about 2 min 30 s · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [038-learning-experience-submit-coding-projects.mp4](038-learning-experience-submit-coding-projects.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

Coding lessons end in a project that a mentor reviews. This video shows the lesson, the badge progress, and how a project is sent. It follows [the submit coding projects design](../../detailed-designs/learning-experience/submit-coding-projects/README.md) and the acceptance criteria of L2-LEARN-008 in [the learning experience L2 specification](../../specs/learning-experience/L2.md).

## Learning objectives

By the end, the viewer can:

- Describe the parts of a coding lesson: steps, editor, live preview and checklist.
- Explain what ticking every step records.
- State what a youth must do to send a project and what the mentor receives.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| What does the youth see in a coding lesson? | Steps with code samples, an editor, a live preview and a completion checklist |
| What is sent to the mentor? | The current code as a read-only copy, plus an optional note |
| What must the youth confirm? | That it is their own work |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/lesson-1280.png` | A new coding lesson at zero percent |
| `images/complete-1280.png` | The lesson complete with the badge message |
| `images/submit-1280.png` | The send for mentor review form |
| `images/status-1280.png` | The submission status page |

Captures are made by `frontend/video-capture/038-learning-experience-submit-coding-projects.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:15 | Welcome | Lessons that build something |
| 00:15-00:45 | The coding lesson | Steps, editor, preview |
| 00:45-01:15 | Finishing the lesson | Autosave, checklist, badge |
| 01:15-01:45 | Sending it for review | Note, own-work box |
| 01:45-02:10 | What happens next | Submission status |
| 02:10-02:30 | Things to remember | Recap |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts 038
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/038-learning-experience-submit-coding-projects --engine kokoro
node tools/video-build/build-video.mjs docs/videos/038-learning-experience-submit-coding-projects
```

## Pitfalls

- The lesson and badge names on screen carry a random suffix because the capture uses seeded test data.
