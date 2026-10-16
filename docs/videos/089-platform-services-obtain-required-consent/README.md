# 089 · When a guardian needs to sign off

> **Runtime:** about 2 min 30 s · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [089-platform-services-obtain-required-consent.mp4](089-platform-services-obtain-required-consent.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

Waypoint is used by minors, so participation depends on guardian consent. This video shows the consent-required page: what a youth sees when consent is missing and the two ways to get it. It follows [the design](../../detailed-designs/platform-services/obtain-required-consent/README.md) and the acceptance criteria of L2-PLATFORM-011 (criterion 1) and L2-CONSENT-002 in [the platform services L2 specification](../../specs/platform-services/L2.md).

## Learning objectives

By the end, the viewer can:

- Describe what a youth sees when guardian consent is missing.
- Name the two ways to get the guardian to sign.
- Explain who gives the consent.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| What does a youth see without consent? | A page headed "A guardian needs to sign off", explaining that a parent or guardian must give participation consent |
| How can the guardian be reached? | Notify my guardian sends a secure link by email; My guardian can sign now signs the youth out so the guardian can sign in and consent |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/consent-required-1280.png` | The consent-required page |
| `images/notified-1280.png` | The delivery confirmation with the masked address |

Captures are made by `frontend/video-capture/089-platform-services-obtain-required-consent.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:20 | Welcome | Why consent matters |
| 00:20-01:00 | When consent is missing | Imani is stopped at the consent page |
| 01:00-02:00 | Two ways to get the signature | Notify, or sign now |
| 02:00-02:30 | Recap | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts video-capture/089-platform-services-obtain-required-consent.capture.ts
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/089-platform-services-obtain-required-consent --engine kokoro
node tools/video-build/build-video.mjs docs/videos/089-platform-services-obtain-required-consent
```

## Pitfalls

- The capture seeds a youth with a linked guardian, has the guardian withdraw consent, and lets the youth try to finish a lesson. The seeded youth name is not visible on the consent page.
- The destination of the sign-now button is the guardian consent page after sign-in; the video says only that.
