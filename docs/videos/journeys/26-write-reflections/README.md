# 26 · Write reflections

> **Runtime:** about 2 min 48 s · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [26-write-reflections.mp4](26-write-reflections.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

A reflection is where a youth thinks about a lesson in their own words, and chooses who sees it. This video follows one reflection from lesson to submission, using [write reflections](../../../detailed-designs/learning-experience/write-reflections/README.md) and [complete lessons](../../../detailed-designs/learning-experience/complete-lessons/README.md), and L2-LEARN-002 and L2-LEARN-003 in [the learning experience L2 specification](../../../specs/learning-experience/L2.md).

## Learning objectives

By the end, the viewer can:

- State what is required to submit a reflection.
- Explain who can read a reflection by default and how that changes.
- Describe what happens to unsaved words when a youth leaves the page.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| Who reads a reflection by default? | Nobody but the youth; sharing with the assigned mentor is off unless turned on |
| What is required to submit? | Reflection text and a chosen feeling; the action is optional |
| What happens to a half written reflection? | A warning appears on leaving, a draft can be saved, and it is restored on return |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/empty-1280.png` | Reflection page after completing a lesson |
| `images/required-1280.png` | Required text message |
| `images/share-1280.png` | Share with my mentor, off |
| `images/filled-1280.png` | Feeling chosen and words typed |
| `images/restored-1280.png` | Draft restored message |
| `images/submitted-1280.png` | Reflection submitted message |

Captures are made by `frontend/video-capture/journeys/26-write-reflections.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:16 | Welcome | The journey and the cast |
| 00:16-00:54 | The reflection page | Question, feeling and text boxes |
| 00:54-01:20 | What is required | Required text and feeling |
| 01:20-01:50 | Who can read it | Share with my mentor, off by default |
| 01:50-02:12 | Leaving and coming back | Warning and draft restored |
| 02:12-02:26 | Submitting | Share, then submit |
| 02:26-02:48 | Things to remember | Recap |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts journeys/26
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/journeys/26-write-reflections --engine kokoro
node tools/video-build/build-video.mjs docs/videos/journeys/26-write-reflections
```

## Pitfalls

- The warning on leaving the page is a browser dialog, so it is described but not captured.
- What a mentor sees of a shared reflection is not shown here; the design says only that the assigned mentor may read and respond during check ins.
- The seeded titles carry a short random suffix; it is not part of the product.
