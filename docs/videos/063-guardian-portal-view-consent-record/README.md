# 063 · Viewing the consent record

> **Runtime:** about 2 min 30 s · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [063-guardian-portal-view-consent-record.mp4](063-guardian-portal-view-consent-record.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

A guardian should be able to see exactly what they signed and keep a copy. It follows [the design](../../detailed-designs/guardian-portal/view-consent-record/README.md) and the acceptance criteria of L2-GUARDIAN-003 in [the L2 specification](../../specs/guardian-portal/L2.md).

## Learning objectives

By the end, the viewer can:

- List what the consent record shows.
- Describe how to have a copy emailed.
- Recognise the no-record state.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| What does the record show? | Signer and relationship, date and time, method (typed e-signature), valid-until date and a reference number. |
| How does a guardian get a copy? | Email me a copy sends the record to their own address and a message confirms it. |
| What if no youth is linked? | The page says no consent record yet. |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/record-1280.png` | Signer, date, method, validity, reference |
| `images/emailed-1280.png` | Your consent record has been emailed |
| `images/empty-1280.png` | Nothing to show until a youth is linked |

Captures are made by `frontend/video-capture/063-guardian-portal-view-consent-record.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:15 | Welcome | Why the record exists |
| 00:15-01:00 | The record | Details on the card |
| 01:00-01:40 | Emailing a copy | Button, message, email contents |
| 01:40-02:10 | When there is no record | Empty state |
| 02:10-02:30 | Recap | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts video-capture/063-guardian-portal-view-consent-record.capture.ts
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/063-guardian-portal-view-consent-record --engine kokoro
node tools/video-build/build-video.mjs docs/videos/063-guardian-portal-view-consent-record
```

## Pitfalls

- The email contents described in the narration are those of the Testing email sender used for the capture.
