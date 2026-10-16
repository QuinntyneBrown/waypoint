# 022 · Submitting an application

> **Runtime:** about 3 min 30 s · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [022-application-intake-submit-application.mp4](022-application-intake-submit-application.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

The application form is the front door to the program. This video shows what a parent or guardian sees, what the form checks, and what happens on success. It follows [the submit design](../../detailed-designs/application-intake/submit-application/README.md) and the acceptance criteria of L2-INTAKE-001 in [the application intake L2 specification](../../specs/application-intake/L2.md).

## Learning objectives

By the end, the viewer can:

- List what the form asks for and who may fill it in.
- Explain when guardian details are required.
- Describe what the confirmation says and why no account exists yet.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| Who can fill in the form? | The young person, a parent or guardian, or a referrer; no account is needed |
| When are guardian details required? | For applicants under eighteen; the section appears once the age is under eighteen |
| What blocks a submission? | Missing required fields, an age outside the eligible range, and unticked consent or privacy boxes, each with a field message |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/form-1280.png` | The top of the apply page |
| `images/age-1280.png` | The eligible-ages message |
| `images/guardian-1280.png` | The guardian section and the two boxes |
| `images/acks-1280.png` | The acknowledgement messages |
| `images/confirmation-1280.png` | Application received |

Captures are made by `frontend/video-capture/022-application-intake-submit-application.capture.ts` against the public site on port 4201.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:20 | Welcome | Ruth applies for Imani |
| 00:20-01:05 | The form | Who is filling, cohort, details |
| 01:05-01:50 | Age and guardian details | Eligible ages, guardian section |
| 01:50-02:35 | Consent and privacy | Two required boxes and field messages |
| 02:35-03:10 | Application received | Confirmation, no account yet, recorded as new |
| 03:10-03:30 | Recap | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts video-capture/022-application-intake-submit-application.capture.ts
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/022-application-intake-submit-application --engine kokoro
node tools/video-build/build-video.mjs docs/videos/022-application-intake-submit-application
```

## Pitfalls

- The eligible age range of twelve to eighteen is marked "confirm with product owner" in the specification; the narration only reports what the form shows.
- The "recorded as new" status is not visible to the applicant; the narration states it from the acceptance criteria.
