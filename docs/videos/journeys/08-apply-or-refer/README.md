# 08 · Apply or refer

> **Runtime:** about 3 min 30 s · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [08-apply-or-refer.mp4](08-apply-or-refer.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

Every young person enters Waypoint through the public application form. This journey follows Ruth Clarke applying for her daughter Imani, and notes that a young person or a referrer uses the same form. It follows [submit application](../../../detailed-designs/application-intake/submit-application/README.md) and L2-INTAKE-001 in [the application intake L2 specification](../../../specs/application-intake/L2.md).

## Learning objectives

By the end, the viewer can:

- Name the three people who can start an application.
- Explain the age rule and when guardian details are required.
- Explain the two required acknowledgements and what the applicant sees afterwards.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| Who can apply? | The young person, a parent or guardian, or a referrer, with no account |
| What does the form check? | Ages twelve to eighteen, guardian details under eighteen, two acknowledgements |
| What happens after submitting? | An application received page; no account until acceptance |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/form-1280.png` | The who is filling this out question |
| `images/age-1280.png` | The eligible ages message |
| `images/guardian-1280.png` | The parent or guardian section |
| `images/acks-1280.png` | Missing acknowledgement messages |
| `images/confirmation-1280.png` | Application received |

Captures are made by `frontend/video-capture/journeys/08-apply-or-refer.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:25 | Welcome | Three ways to start an application |
| 00:25-01:05 | Who is filling this out | The apply page and first questions |
| 01:05-01:50 | Age and guardian details | Age check and guardian section |
| 01:50-02:25 | Consent and privacy | Two required boxes |
| 02:25-03:00 | Application received | Confirmation, referrer note |
| 03:00-03:30 | Recap | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts journeys/08
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/journeys/08-apply-or-refer --engine kokoro
node tools/video-build/build-video.mjs docs/videos/journeys/08-apply-or-refer
```

## Pitfalls

- The capture walks the guardian path; the referrer path uses the same form and is described, not shown.
- The eligible ages come from the specification, twelve to eighteen inclusive.
