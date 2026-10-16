# 093 · When something goes wrong on our side

> **Runtime:** about 2 min 30 s · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [093-platform-services-recover-from-server-error.mp4](093-platform-services-recover-from-server-error.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

Server errors happen. This video shows the server-error page and how the typed message comes back after try again. It follows [the design](../../detailed-designs/platform-services/recover-from-server-error/README.md) and the acceptance criteria of L2-PLATFORM-008 in [the platform services L2 specification](../../specs/platform-services/L2.md).

## Learning objectives

By the end, the viewer can:

- Describe the server-error page.
- State what is not shown.
- Explain what try again restores.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| What does the page say? | Something went wrong; it was not the person’s fault; please try again |
| What happens to typed work? | Try again returns to the page and restores it |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/draft-1280.png` | The support form with the typed message |
| `images/server-error-1280.png` | The server-error page |
| `images/restored-1280.png` | The form restored after try again |

Captures are made by `frontend/video-capture/093-platform-services-recover-from-server-error.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:20 | Welcome | Why a failure needs a plan |
| 00:20-00:45 | A message that does not send | David sends a support message |
| 00:45-01:25 | Something went wrong | The server-error page |
| 01:25-02:00 | Nothing is lost | Try again restores the draft |
| 02:00-02:30 | Recap | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts video-capture/093-platform-services-recover-from-server-error.capture.ts
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/093-platform-services-recover-from-server-error --engine kokoro
node tools/video-build/build-video.mjs docs/videos/093-platform-services-recover-from-server-error
```

## Pitfalls

- The server failure is simulated in the capture by answering the send request with an error, as the end-to-end tests do; the page shown is the real one.
