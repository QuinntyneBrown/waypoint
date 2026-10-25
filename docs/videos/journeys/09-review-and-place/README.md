# 09 · Review and place

> **Runtime:** about 3 min 32 s · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [09-review-and-place.mp4](09-review-and-place.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

Applications are only useful once someone decides on them. This video follows Pat Morgan through the queue, a minor who cannot take part until a guardian consents, a soft decline, and the approval that places Amara Bell in a cohort with a mentor. It follows the designs for [reviewing an application](../../../detailed-designs/application-intake/review-application/README.md), [deciding and enrolling](../../../detailed-designs/application-intake/decide-and-enroll-applicant/README.md), and the acceptance criteria in [the application intake L2 specification](../../../specs/application-intake/L2.md).

## Learning objectives

By the end, the viewer can:

- Find an application in the queue and open its review.
- Explain why a minor without consent can be approved but cannot take part.
- Describe a soft decline.
- State what an approval needs and what it creates.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| Why is approve off for Imani? | Guardian participation consent is not on record for a minor |
| What does a decline do? | Keeps the application and optional note, creates no account |
| What does an approval need? | A cohort and a mentor, then it creates the account and enrolls the youth |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/queue-1280.png` | The application queue |
| `images/blocked-1280.png` | The review window for a minor without consent |
| `images/decline-confirm-1280.png` | The decline confirmation |
| `images/placement-1280.png` | Amara's review with cohort and mentor |
| `images/approve-confirm-1280.png` | The approve confirmation |
| `images/approved-1280.png` | The approved list |

Captures are made by `frontend/video-capture/journeys/09-review-and-place.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:24 | Welcome | Applications from the public site all land in one place, a queue for the program administrator. |
| 00:26-01:03 | The queue | Pat opens the applications page. |
| 01:05-01:37 | A minor without consent | Pat presses review on Imani's row, and a review window opens. |
| 01:39-02:10 | Declining softly | Pat decides there is no place for Imani this term, and types a note, no place is available this term. |
| 02:12-03:02 | Placing Amara | Now for Amara. |
| 03:04-03:32 | Things to remember | Every application lands in one queue, and minors are clearly marked. |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts journeys/09
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/journeys/09-review-and-place --engine kokoro
node tools/video-build/build-video.mjs docs/videos/journeys/09-review-and-place
```

## Pitfalls

- The queue image hides other applicants in the shared test data so only the two in the story show.
- Applicant last names carry a short random tag for unique lookup.
