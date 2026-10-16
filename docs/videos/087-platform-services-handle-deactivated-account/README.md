# 087 · When an account is deactivated

> **Runtime:** about 2 min 10 s · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [087-platform-services-handle-deactivated-account.mp4](087-platform-services-handle-deactivated-account.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

When an account is deactivated, sign-in is refused and the person is sent to a clear page. This video shows that page and the ways forward. It follows [the design](../../detailed-designs/platform-services/handle-deactivated-account/README.md) and the acceptance criteria of L2-PLATFORM-010 (criterion 2) in [the platform services L2 specification](../../specs/platform-services/L2.md).

## Learning objectives

By the end, the viewer can:

- Describe what happens when a deactivated person signs in.
- Name the two buttons on the account inactive page.
- Say who can restore access.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| Does the right password help? | No. Sign-in is refused for a deactivated account |
| What does the person see? | The "This account is inactive" page |
| Where does the person go for help? | Contact the director, which opens the public site contact page |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/sign-in-1280.png` | The sign-in form before David presses Sign in |
| `images/inactive-1280.png` | The account inactive page |

Captures are made by `frontend/video-capture/087-platform-services-handle-deactivated-account.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:18 | Welcome | Welcome |
| 00:18-00:43 | A deactivated account | A deactivated account |
| 00:43-01:05 | What David sees | What David sees |
| 01:05-01:45 | Ways forward | Ways forward |
| 01:45-02:10 | Things to remember | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts video-capture/087-platform-services-handle-deactivated-account.capture.ts
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/087-platform-services-handle-deactivated-account --engine kokoro
node tools/video-build/build-video.mjs docs/videos/087-platform-services-handle-deactivated-account
```

## Pitfalls

- The capture deactivates a synthetic youth through the people API as a seeded administrator, then signs in through the real form. The Contact the director link goes to the public site, which is not captured here.
