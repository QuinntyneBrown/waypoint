# 043 · Writing a reflection

> **Runtime:** about 3 min · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [043-learning-experience-write-reflections.mp4](043-learning-experience-write-reflections.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

Reflections are personal, so staff need to know what is private and what is kept as a draft. This video shows the reflection form, its validation, the sharing choice and draft restore. It follows [the write reflections design](../../detailed-designs/learning-experience/write-reflections/README.md) and the acceptance criteria of L2-LEARN-003 in [the learning experience L2 specification](../../specs/learning-experience/L2.md), with unsaved-changes protection from L2-NFR-007.

## Learning objectives

By the end, the viewer can:

- Name what is required to submit a reflection and what is optional.
- State who can read a reflection by default and how that changes.
- Explain what happens to unsaved words when a youth leaves the page.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| What is required to submit? | The reflection text and a chosen feeling; the action is optional |
| Who reads a reflection? | Only the youth, unless they turn on share with my mentor |
| What if the youth leaves with unsaved words? | A warning offering to save a draft, and the draft is restored on return |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/empty-1280.png` | The blank reflection page |
| `images/required-1280.png` | The required-text message |
| `images/share-1280.png` | The share with my mentor box |
| `images/filled-1280.png` | A feeling chosen and text written |
| `images/restored-1280.png` | The draft restored message |
| `images/submitted-1280.png` | The reflection submitted message |

Captures are made by `frontend/video-capture/043-learning-experience-write-reflections.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:15 | Welcome | Why reflect |
| 00:15-00:55 | The reflection page | Prompt, feeling, text |
| 00:55-01:25 | What is required | Validation |
| 01:25-01:50 | Who can read it | Sharing off by default |
| 01:50-02:35 | Drafts and leaving the page | Warning, restore, submit |
| 02:35-03:00 | Things to remember | Recap |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts 043
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/043-learning-experience-write-reflections --engine kokoro
node tools/video-build/build-video.mjs docs/videos/043-learning-experience-write-reflections
```

## Pitfalls

- The unsaved-changes warning is a browser dialog, so it has no screen capture.
- The lesson name on screen carries a random suffix because the capture uses seeded test data.
