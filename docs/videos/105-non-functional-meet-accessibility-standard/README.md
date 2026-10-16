# 105 · Accessible to everyone

> **Runtime:** about 2 min 30 s · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [105-non-functional-meet-accessibility-standard.mp4](105-non-functional-meet-accessibility-standard.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

Waypoint targets the W C A G two point one level double A accessibility standard. This video shows the parts a person can see: keyboard focus, labelled fields with words as well as colour, dialogs that manage focus, and the display preferences. It follows [the meet accessibility standard design](../../detailed-designs/non-functional/meet-accessibility-standard/README.md) and the acceptance criteria of L2-NFR-010 in [the non-functional L2 specification](../../specs/non-functional/L2.md), with L2-PLATFORM-004 in [the platform services L2 specification](../../specs/platform-services/L2.md).

## Learning objectives

By the end, the viewer can:

- Describe how a keyboard user can tell where they are on a page.
- Explain why error messages carry words as well as colour.
- Describe what a dialog does with focus when it opens and closes.
- List the display preferences a person can change.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| How do you know where you are with the keyboard? | A visible outline follows focus in a logical order |
| Is colour ever the only signal? | No: a sentence accompanies the red border |
| What happens to focus in a dialog? | It moves in, stays inside, and returns to the opener on Escape |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/focus-email-1280.png` | Focus outline on the email box |
| `images/focus-later-1280.png` | Error sentences under red boxes and focus on the sign in button |
| `images/dialog-1280.png` | The create cohort dialog |
| `images/display-high-1280.png` | Display and accessibility settings |

Captures are made by `frontend/video-capture/105-non-functional-meet-accessibility-standard.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:25 | Welcome | Who needs accessibility and the standard |
| 00:25-01:10 | Using only the keyboard | Focus outline, labels, words with colour |
| 01:10-01:35 | Dialogs | Focus in, escape out, focus returned |
| 01:35-02:05 | Display preferences | Text size, contrast, motion, links, font |
| 02:05-02:30 | Recap | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts video-capture/105-non-functional-meet-accessibility-standard.capture.ts
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/105-non-functional-meet-accessibility-standard --engine kokoro
node tools/video-build/build-video.mjs docs/videos/105-non-functional-meet-accessibility-standard
```

## Pitfalls

- A still image cannot show focus moving or Escape closing a dialog; the narration describes it and the e2e dialog specification checks it.
- The standard is a target for every screen, checked by automated tests; do not describe it as a certification.
