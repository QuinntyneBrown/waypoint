# 092 · When a page cannot be found

> **Runtime:** about 2 min 15 s · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [092-platform-services-recover-from-missing-content.mp4](092-platform-services-recover-from-missing-content.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

Links go stale and addresses are mistyped. This video shows the not-found page and where it leads. It follows [the design](../../detailed-designs/platform-services/recover-from-missing-content/README.md) and the acceptance criteria of L2-PLATFORM-007 in [the platform services L2 specification](../../specs/platform-services/L2.md).

## Learning objectives

By the end, the viewer can:

- Describe what a person sees for a missing page.
- Say where the back to safety button leads.
- Explain that a missing record shows the same page.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| What does the person see? | A page headed "Page not found" with a back to safety button |
| Where does the button lead? | To the home for the person’s role, or to sign in when nobody is signed in |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/not-found-1280.png` | The not-found page |
| `images/safe-home-1280.png` | The top of the dashboard after pressing back to safety |

Captures are made by `frontend/video-capture/092-platform-services-recover-from-missing-content.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:20 | Welcome | Why pages go missing |
| 00:20-00:50 | A link that leads nowhere | The old bookmark |
| 00:50-01:20 | Back to safety | The way home |
| 01:20-01:55 | When the data is missing | Missing records |
| 01:55-02:15 | Recap | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts video-capture/092-platform-services-recover-from-missing-content.capture.ts
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/092-platform-services-recover-from-missing-content --engine kokoro
node tools/video-build/build-video.mjs docs/videos/092-platform-services-recover-from-missing-content
```

## Pitfalls

- The safe-home capture is cropped to the heading because the dashboard figures below it come from the shared test data.
