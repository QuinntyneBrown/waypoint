# 035 · Reviewing your own progress

> **Runtime:** about 3 min · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [035-learning-experience-review-personal-progress.mp4](035-learning-experience-review-personal-progress.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

Youth should see their own progress without asking. This video shows the My progress page and its three tabs. It follows [the review personal progress design](../../detailed-designs/learning-experience/review-personal-progress/README.md) and the acceptance criteria of L2-LEARN-010 in [the learning experience L2 specification](../../specs/learning-experience/L2.md).

## Learning objectives

By the end, the viewer can:

- Name the four numbers on the overview.
- Say where competency levels come from.
- Describe what the Quizzes and Attendance tabs list.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| What is on the overview? | Overall progress, quiz average, lessons done, attendance, and competencies |
| Who sets competency levels? | Mentor assessments |
| What do the other tabs show? | Quiz history with score and pass state; attendance status per session |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/overview-1280.png` | Overview numbers and competencies |
| `images/quizzes-1280.png` | Quiz history |
| `images/attendance-1280.png` | Session attendance |

Captures are made by `frontend/video-capture/035-learning-experience-review-personal-progress.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:15 | Welcome | Why a self-view matters |
| 00:15-00:55 | The overview | Four numbers |
| 00:55-01:20 | Leadership competencies | Levels from mentor assessments |
| 01:20-01:50 | Quizzes | Quiz history |
| 01:50-02:15 | Attendance | Session status |
| 02:15-02:40 | Recap | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts video-capture/035-learning-experience-review-personal-progress.capture.ts
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/035-learning-experience-review-personal-progress --engine kokoro
node tools/video-build/build-video.mjs docs/videos/035-learning-experience-review-personal-progress
```

## Pitfalls

- The numbers on screen come from seeded test data and are not a model of a real youth.
