# 020 · Deciding on an application and enrolling the applicant

> **Runtime:** about 3 min 20 s · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [020-application-intake-decide-and-enroll-applicant.mp4](020-application-intake-decide-and-enroll-applicant.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

A reviewed application ends in a decision. This video shows how a program administrator declines softly or approves and enrolls, and how consent gating stops a minor without guardian consent from being approved. It follows [the decide and enroll design](../../detailed-designs/application-intake/decide-and-enroll-applicant/README.md) and the acceptance criteria of L2-INTAKE-002 in [the application intake L2 specification](../../specs/application-intake/L2.md).

## Learning objectives

By the end, the viewer can:

- Explain why the approve button is off for a minor without recorded guardian consent.
- Describe what a soft decline keeps and what it does not create.
- State what an approval requires and what it creates.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| Why can a minor not be approved yet? | The status is needs consent; approval waits for a valid guardian consent record |
| What happens to a declined application? | Status becomes declined, the optional note is stored on the record, no account is created |
| What does approval need and create? | A cohort and a mentor are required; a youth account and enrollment are created and the applicant is notified |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/blocked-1280.png` | Review window for a minor with the consent-required warning |
| `images/decline-confirm-1280.png` | Decline confirmation |
| `images/declined-1280.png` | Declined message and list |
| `images/placement-1280.png` | Track, cohort, mentor and note |
| `images/approve-confirm-1280.png` | Approval confirmation |
| `images/approved-1280.png` | Approved and enrolled message |

Captures are made by `frontend/video-capture/020-application-intake-decide-and-enroll-applicant.capture.ts`. The names after the applicant's first name (a short code) and the cohort and mentor names come from the synthetic seed data.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:20 | Welcome | Purpose and who may decide |
| 00:20-01:05 | A minor without consent | Needs consent; approval allowed, participation waits for consent |
| 01:05-01:50 | Declining softly | Note, confirmation, declined |
| 01:50-02:50 | Approving and enrolling | Cohort, mentor, confirmation, enrolled |
| 02:50-03:20 | Recap | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts video-capture/020-application-intake-decide-and-enroll-applicant.capture.ts
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/020-application-intake-decide-and-enroll-applicant --engine kokoro
node tools/video-build/build-video.mjs docs/videos/020-application-intake-decide-and-enroll-applicant
```

## Pitfalls

- The email to the applicant follows the configured channels in the specification; the screen only promises an approval email to the applicant contact.
- The recording does not show how guardian consent is later recorded; that belongs to the consent videos.
