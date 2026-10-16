# 096 · Asking the program for help

> **Runtime:** about 2 min 45 s · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [096-platform-services-submit-support-request.mp4](096-platform-services-submit-support-request.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

People need a way to ask for help. This video shows the support form and the acknowledgement. It follows [the design](../../detailed-designs/platform-services/submit-support-request/README.md) and the acceptance criteria of L2-PLATFORM-005 (criteria 2 and 3) in [the platform services L2 specification](../../specs/platform-services/L2.md).

## Learning objectives

By the end, the viewer can:

- Name what the support form asks for.
- Say what happens with a description that is too short.
- Explain what the person sees after sending.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| What is required? | A category, a description of at least ten characters and a reply email |
| Where does the request go? | It is saved and emailed to the program support inbox |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/form-1280.png` | The empty form |
| `images/required-1280.png` | The validation message |
| `images/filled-1280.png` | The filled form |
| `images/received-1280.png` | The acknowledgement |

Captures are made by `frontend/video-capture/096-platform-services-submit-support-request.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:20 | Welcome | Why support exists |
| 00:20-01:10 | The support form | The fields |
| 01:10-01:40 | A description is required | The validation message |
| 01:40-02:30 | Sent | The acknowledgement |
| 02:30-02:45 | Recap | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts video-capture/096-platform-services-submit-support-request.capture.ts
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/096-platform-services-submit-support-request --engine kokoro
node tools/video-build/build-video.mjs docs/videos/096-platform-services-submit-support-request
```

## Pitfalls

- The optional screenshot is described on the form; the video does not upload one.
- The emailing to the support inbox is not visible on screen and is stated from the requirement.
