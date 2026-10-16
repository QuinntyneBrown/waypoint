# 091 · When a page is not for your role

> **Runtime:** about 2 min 15 s · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [091-platform-services-recover-from-forbidden-access.mp4](091-platform-services-recover-from-forbidden-access.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

People sometimes open pages that belong to another role. This video shows the forbidden page and its two ways forward. It follows [the design](../../detailed-designs/platform-services/recover-from-forbidden-access/README.md) and the acceptance criteria of L2-PLATFORM-006 in [the platform services L2 specification](../../specs/platform-services/L2.md).

## Learning objectives

By the end, the viewer can:

- Describe what a person sees on a page their role does not allow.
- State that no data from the refused area appears.
- Name the two recovery buttons.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| What does the page offer? | Back to your home, and switch account |
| Does it reveal anything? | No. It explains the role mismatch and shows nothing from the refused page |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/forbidden-1280.png` | The forbidden page |
| `images/home-1280.png` | The youth home after recovery |

Captures are made by `frontend/video-capture/091-platform-services-recover-from-forbidden-access.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:20 | Welcome | Why a page may be refused |
| 00:20-00:55 | The wrong door | David opens a staff-only page |
| 00:55-01:15 | Nothing leaks | What the page does not show |
| 01:15-01:55 | Two ways forward | Back to your home, switch account |
| 01:55-02:15 | Recap | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts video-capture/091-platform-services-recover-from-forbidden-access.capture.ts
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/091-platform-services-recover-from-forbidden-access --engine kokoro
node tools/video-build/build-video.mjs docs/videos/091-platform-services-recover-from-forbidden-access
```

## Pitfalls

- The home page behind the recovery button is the youth home; the capture user has no cohort, so it shows the no-cohort message.
