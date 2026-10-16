# 017 · Offering to volunteer

> **Runtime:** about 2 min 30 s · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [017-public-site-register-volunteer-interest.mp4](017-public-site-register-volunteer-interest.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

Volunteers will work with young people. This video shows how the public volunteer page explains the screening and reference check and records the acknowledgement. It follows [the design](../../detailed-designs/public-site/register-volunteer-interest/README.md) and the acceptance criteria of L2-PUB-003 (criterion 2), L2-NFR-001 and L2-CONSENT-004 in [the public site L2 specification](../../specs/public-site/L2.md).

## Learning objectives

By the end, the viewer can:

- Describe the volunteer form fields.
- Explain that sign up is blocked until the screening acknowledgement is ticked.
- Say what the thank you page tells the volunteer.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| What is required? | Name, email and the screening acknowledgement |
| What if the box is not ticked? | A message under the box and nothing is sent |
| What does the thank you page say? | The team will reach out about next steps, including the screening and reference check |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/intro-1280.png` | Top of the get involved page |
| `images/blocked-1280.png` | The form blocked without the acknowledgement |
| `images/checked-1280.png` | The form with the acknowledgement ticked |
| `images/thanks-1280.png` | The thanks for volunteering page |

Captures are made by `frontend/video-capture/017-public-site-register-volunteer-interest.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:21 | Welcome | Why volunteers acknowledge screening |
| 00:21-00:58 | The get involved page | Page and cards |
| 00:58-01:27 | The volunteer form | Fields and checkbox |
| 01:27-02:07 | The acknowledgement | Blocked, then thanked |
| 02:07-02:30 | Recap | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts video-capture/017-public-site-register-volunteer-interest.capture.ts
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/017-public-site-register-volunteer-interest --engine kokoro
node tools/video-build/build-video.mjs docs/videos/017-public-site-register-volunteer-interest
```

## Pitfalls

- Recording the interest does not complete screening; the video says so and stops there.
