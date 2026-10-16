# 113 · Checking what goes in and what comes out

> **Runtime:** about 3 min · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [113-non-functional-validate-input-and-output.mp4](113-non-functional-validate-input-and-output.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

Every text box is a doorway. This video shows the sign-in form's field messages, a message containing markup displayed as plain text, and explains the server-side validation, parameterised queries and upload checks behind them. It follows [the validate input and output design](../../detailed-designs/non-functional/validate-input-and-output/README.md) and the acceptance criteria of L2-NFR-001 in [the non-functional L2 specification](../../specs/non-functional/L2.md).

## Learning objectives

By the end, the viewer can:

- Describe what a person sees when a form field is wrong.
- Explain why the server checks a request even when the form already did.
- Explain what happens when someone types markup into a message.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| Is the form check enough? | No: the server validates every request for type, length, range and format |
| What does a refused request return? | A structured list naming each field and what is wrong |
| What happens to typed markup? | It is displayed as plain text and never runs |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/errors-1280.png` | The sign-in form field messages |
| `images/plain-text-1280.png` | A message with markup displayed as text |

Captures are made by `frontend/video-capture/113-non-functional-validate-input-and-output.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:25 | Welcome | Doorways in and out |
| 00:25-01:20 | Checking what goes in | Field messages, server checks |
| 01:20-02:05 | Showing what comes out | Markup shown as plain text |
| 02:05-02:40 | Behind the scenes | Parameters and upload checks |
| 02:40-03:00 | Recap | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts video-capture/113-non-functional-validate-input-and-output.capture.ts
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/113-non-functional-validate-input-and-output --engine kokoro
node tools/video-build/build-video.mjs docs/videos/113-non-functional-validate-input-and-output
```

## Pitfalls

- The field messages come from the sign-in form (taken by the 105 capture); the server-side checks are described in words because the browser form blocks the request first.
- Upload limits are set by policy in code; the narration does not quote a number.
