# 078 · Capturing and managing guardian consent

> **Runtime:** about 3 min · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [078-consent-and-safeguarding-manage-guardian-consent.mp4](078-consent-and-safeguarding-manage-guardian-consent.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

Guardian consent decides whether a minor may take part and whether media may be used, so it must be captured clearly and kept as a record. It follows [the design](../../detailed-designs/consent-and-safeguarding/manage-guardian-consent/README.md) and the acceptance criteria of L2-CONSENT-001 in [the L2 specification](../../specs/consent-and-safeguarding/L2.md).

## Learning objectives

By the end, the viewer can:

- List what is required to give participation consent.
- Describe the optional, independent media choices.
- State what Waypoint stores with each consent.
- Explain what withdrawal does.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| What is required? | Guardian full name, relationship, the participation permission and a typed signature. |
| What is stored? | Signer name, method (typed e-signature), timestamp, a unique reference and a validity window; a copy is emailed. |
| What does withdrawal do? | It is recorded, and permissions such as media use are revoked going forward. |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/form-1280.png` | Required fields, optional media choices |
| `images/recorded-1280.png` | A reference number and an emailed copy |
| `images/record-1280.png` | Active, with method and validity |
| `images/media-1280.png` | Media choices as she set them |
| `images/withdrawn-1280.png` | The withdrawal is recorded |

Captures are made by `frontend/video-capture/078-consent-and-safeguarding-manage-guardian-consent.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:20 | Welcome | Two parts of consent |
| 00:20-01:10 | Signing the form | Required fields and media options |
| 01:10-01:50 | What Waypoint stores | Receipt and portal record |
| 01:50-02:30 | Changing her mind | Withdrawal, re-consent |
| 02:30-03:00 | Recap | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts video-capture/078-consent-and-safeguarding-manage-guardian-consent.capture.ts
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/078-consent-and-safeguarding-manage-guardian-consent --engine kokoro
node tools/video-build/build-video.mjs docs/videos/078-consent-and-safeguarding-manage-guardian-consent
```

## Pitfalls

- Re-consent each term depends on organization policy; the narration says so and the capture does not show it.
