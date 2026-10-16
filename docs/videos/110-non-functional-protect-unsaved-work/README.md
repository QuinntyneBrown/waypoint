# 110 · Protecting work you have not saved

> **Runtime:** about 2 min 45 s · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [110-non-functional-protect-unsaved-work.mp4](110-non-functional-protect-unsaved-work.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

Losing words you just wrote is frustrating, and for staff it can mean losing attendance or grading. This video shows the unsaved-changes warning on a reflection, the restored draft, and the unsaved changes label on the attendance page. It follows [the protect unsaved work design](../../detailed-designs/non-functional/protect-unsaved-work/README.md) and the acceptance criteria of L2-NFR-007 in [the non-functional L2 specification](../../specs/non-functional/L2.md).

## Learning objectives

By the end, the viewer can:

- Describe what Waypoint does when someone leaves a page with unsaved work.
- Explain what happens to a saved draft when the person returns.
- Name other places where the same protection applies.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| What happens if you leave with unsaved work? | The browser asks whether to save the draft or stay on the page |
| What if you close the tab? | The browser's leave-page warning appears |
| Where else does this apply? | Attendance and grading, as well as reflections |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/typed-1280.png` | A reflection typed but not submitted |
| `images/restored-1280.png` | The restored draft |
| `images/attendance-1280.png` | The unsaved changes label on the attendance page |

Captures are made by `frontend/video-capture/110-non-functional-protect-unsaved-work.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:20 | Welcome | Why losing work hurts |
| 00:20-01:10 | Writing a reflection | Typed answer and the leave-page question |
| 01:10-01:35 | Coming back to it | Draft restored |
| 01:35-02:20 | Other places it applies | Attendance and grading |
| 02:20-02:45 | Recap | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts video-capture/110-non-functional-protect-unsaved-work.capture.ts
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/110-non-functional-protect-unsaved-work --engine kokoro
node tools/video-build/build-video.mjs docs/videos/110-non-functional-protect-unsaved-work
```

## Pitfalls

- The leave-page question is a native browser prompt, which a screenshot cannot show; the slide quotes the exact wording from the code instead.
- The prompt wording differs by editor: it names the editor, for example reflection, attendance or grading.
