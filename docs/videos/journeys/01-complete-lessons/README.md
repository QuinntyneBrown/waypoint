# 01 · Complete lessons

> **Runtime:** about 3 min 23 s · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [01-complete-lessons.mp4](01-complete-lessons.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

Lessons are the core of what a youth does in Waypoint. This video follows one person through the whole journey: finding her place in the curriculum, meeting the locked items, reading and completing a lesson, and seeing what it opens. It draws on [browse curriculum](../../../detailed-designs/learning-experience/browse-curriculum/README.md), [complete lessons](../../../detailed-designs/learning-experience/complete-lessons/README.md) and [complete coding lessons](../../../detailed-designs/learning-experience/complete-coding-lessons/README.md), and on L2-LEARN-001, L2-LEARN-002 and L2-LEARN-008 in [the learning experience L2 specification](../../../specs/learning-experience/L2.md).

## Learning objectives

By the end, the viewer can:

- Describe how the curriculum shows order, status and locked items.
- Explain what marking a lesson complete does.
- Name what a coding lesson adds to a lesson.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| Why is the module quiz locked? | All lessons in the module must be complete first; the reason is shown, and the server enforces the order |
| What happens when a lesson is marked complete? | It is recorded with a time, what follows unlocks, and the youth goes to the reflection |
| What is different in a coding lesson? | Numbered steps, an editor with a live preview, and a way to send the project to a mentor |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/map-1280.png` | Curriculum with Foundations at thirty three percent |
| `images/gated-1280.png` | Locked quiz and locked second module |
| `images/lesson-1280.png` | Lesson page with text size and bookmark |
| `images/reflection-1280.png` | Reflection page reached after completing |
| `images/unlocked-1280.png` | Curriculum after completion, quiz open |
| `images/coding-1280.png` | Editor with live preview |

Captures are made by `frontend/video-capture/journeys/01-complete-lessons.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:20 | Welcome | The journey and the cast |
| 00:20-00:49 | Finding her place | Curriculum, modules and statuses |
| 00:49-01:17 | What is locked | Gated quiz and module |
| 01:17-01:43 | Reading the lesson | Lesson page, text size, bookmark |
| 01:43-02:05 | Marking it complete | Confirm, then the reflection |
| 02:05-02:25 | What opens next | Progress and the unlocked quiz |
| 02:25-02:56 | A coding lesson | Editor, live preview, send to mentor |
| 02:56-03:23 | Things to remember | Recap |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts journeys/01
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/journeys/01-complete-lessons --engine kokoro
node tools/video-build/build-video.mjs docs/videos/journeys/01-complete-lessons
```

## Pitfalls

- The seeded module and lesson names carry a short random suffix; it is not part of the product.
- The lesson media is blocked on purpose in the capture so the page does not wait on it; the narration does not mention media.
