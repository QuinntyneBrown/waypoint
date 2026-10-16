# 034 · Reading and completing a lesson

> **Runtime:** about 3 min · **Audience:** program staff and stakeholders · **Prerequisites:** video 032

**Video:** [034-learning-experience-complete-lessons.mp4](034-learning-experience-complete-lessons.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

Reading a lesson and marking it complete is the core learning action. This video shows the lesson page, remembered reading preferences, media failure handling and the route into the reflection. It follows [the complete lessons design](../../detailed-designs/learning-experience/complete-lessons/README.md) and the acceptance criteria of L2-LEARN-002 and L2-LEARN-003 in [the learning experience L2 specification](../../specs/learning-experience/L2.md).

## Learning objectives

By the end, the viewer can:

- List what a lesson shows and what it never shows.
- Explain what is remembered between visits.
- Describe what happens when a lesson is marked complete.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| What is never shown to youth? | Leader guide notes |
| What if media fails? | The text stays readable and a retry control is offered |
| What happens on mark complete? | Confirmation, completion with a time, gating updates, and routing to the reflection |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/lesson-1280.png` | Step progress, scripture, teaching |
| `images/preferences-1280.png` | Text size, bookmark, saved message |
| `images/media-1280.png` | Media failure and retry, complete button |
| `images/reflection-1280.png` | The reflection page after completing |

Captures are made by `frontend/video-capture/034-learning-experience-complete-lessons.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:15 | Welcome | What this video covers |
| 00:15-00:50 | The lesson page | Step progress, scripture, teaching, prompt |
| 00:50-01:15 | Reading preferences | Text size and bookmark |
| 01:15-01:45 | When media does not load | Message and retry |
| 01:45-02:35 | Marking it complete | Confirm, unlock, reflection |
| 02:35-03:00 | Recap | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts video-capture/034-learning-experience-complete-lessons.capture.ts
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/034-learning-experience-complete-lessons --engine kokoro
node tools/video-build/build-video.mjs docs/videos/034-learning-experience-complete-lessons
```

## Pitfalls

- The media failure is produced in the capture by blocking the media request; it is the real retry message of the product.
- The reflection itself is covered in its own video.
