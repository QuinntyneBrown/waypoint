# 108 · Protecting against overwritten work

> **Runtime:** about 2 min 30 s · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [108-non-functional-protect-concurrent-updates.mp4](108-non-functional-protect-concurrent-updates.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

When two people change the same record, the last save must not silently erase the first. This video uses mentor attendance to show a stale save being refused with a clear message while the person's own work stays on screen. It follows [the protect concurrent updates design](../../detailed-designs/non-functional/protect-concurrent-updates/README.md) and the acceptance criteria of L2-NFR-006 in [the non-functional L2 specification](../../specs/non-functional/L2.md).

## Learning objectives

By the end, the viewer can:

- Explain why a stale save is refused instead of overwriting the newer record.
- Describe what the person sees and what is kept when a save is refused.
- State that a successful save moves the version marker forward.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| What happens when two people save the same session? | The second save is refused with a conflict message |
| What does the person lose? | Nothing: their selections and note stay on screen |
| Are records locked while someone edits? | No; the check happens when saving |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/editing-1280.png` | Marks and a note entered, with unsaved changes |
| `images/conflict-1280.png` | The refused save and its message |

Captures are made by `frontend/video-capture/108-non-functional-protect-concurrent-updates.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:20 | Welcome | Why silent overwrites matter |
| 00:20-01:00 | Two people, one session | Unsaved marks, a second save moves the version |
| 01:00-01:35 | The save is refused | The message and what stays on screen |
| 01:35-02:15 | Why this matters | Optimistic concurrency, offline sync |
| 02:15-02:30 | Recap | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts video-capture/108-non-functional-protect-concurrent-updates.capture.ts
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/108-non-functional-protect-concurrent-updates --engine kokoro
node tools/video-build/build-video.mjs docs/videos/108-non-functional-protect-concurrent-updates
```

## Pitfalls

- The second person's save is made through the API during the capture so the order of events is exact.
- Attendance is the visible example; L2-NFR-006 also names submissions as covered.
