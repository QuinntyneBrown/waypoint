# 046 · Authoring lessons

> **Runtime:** about 3 min · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [046-curriculum-authoring-author-lessons.mp4](046-curriculum-authoring-author-lessons.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

Lessons are the core of the curriculum. This video shows every field of a lesson, how files are attached, and what youth never see. It follows [the author lessons design](../../detailed-designs/curriculum-authoring/author-lessons/README.md) and the acceptance criteria of L2-CURRIC-002 in [the curriculum authoring L2 specification](../../specs/curriculum-authoring/L2.md).

## Learning objectives

By the end, the viewer can:

- List what a lesson holds.
- Explain who sees leader guide notes.
- Describe publishing and archiving a lesson.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| Who sees leader guide notes? | Mentors only; they are never shown to youth |
| Which files can be attached? | A PDF document or an MP4 video, validated for type and size |
| What happens when a lesson is archived? | Youth progress is kept and any linked quiz stays intact |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/dialog-1280.png` | The add lesson form, fully filled |
| `images/media-1280.png` | A draft lesson with a file attached |
| `images/published-1280.png` | The lesson published message |
| `images/card-1280.png` | The published lesson card |

Captures are made by `frontend/video-capture/046-curriculum-authoring-author-lessons.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:20 | Welcome | Why lessons matter |
| 00:20-01:10 | Writing the lesson | The fields of the form |
| 01:10-01:35 | What youth never see | Leader guide notes |
| 01:35-02:00 | Adding a file | Upload and remove |
| 02:00-02:40 | Publishing and archiving | Publish, archive |
| 02:40-03:10 | Things to remember | Recap |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts video-capture/046-curriculum-authoring-author-lessons.capture.ts
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/046-curriculum-authoring-author-lessons --engine kokoro
node tools/video-build/build-video.mjs docs/videos/046-curriculum-authoring-author-lessons
```

## Pitfalls

- The leader-guide rule and archive behaviour are stated from L2-CURRIC-002 criteria 3 and 5. The video does not show the youth side.
