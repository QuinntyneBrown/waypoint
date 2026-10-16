# 018 · Starting a donation

> **Runtime:** about 2 min 30 s · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [018-public-site-start-donation.mp4](018-public-site-start-donation.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

Supporters can give monthly. This video shows the public donation page, its validation, and the hand-off to an external secure provider so Waypoint never handles card details. It follows [the design](../../detailed-designs/public-site/start-donation/README.md) and the acceptance criteria of L2-PUB-003 (criterion 3) in [the public site L2 specification](../../specs/public-site/L2.md).

## Learning objectives

By the end, the viewer can:

- Describe the monthly amounts and the fields on the donation page.
- Explain why there is no card entry on the page.
- Say what the thank you page shows.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| What does the page collect? | Name, email and a monthly amount |
| Where is payment finished? | With an external secure provider that opens in a new tab |
| Are the amounts final? | No; the tier amounts are a product decision to confirm with the product owner |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/form-1280.png` | The donation page with no amount chosen |
| `images/errors-1280.png` | Field-level errors with fifty dollars chosen |
| `images/thanks-1280.png` | The thank you message and provider button |

Captures are made by `frontend/video-capture/018-public-site-start-donation.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:14 | Welcome | Why donate |
| 00:14-01:07 | The donation page | Fields, amounts, provider note |
| 01:07-01:26 | When something is missing | Field-level errors |
| 01:26-02:06 | Confirming | Pledge recorded, hand-off |
| 02:06-02:30 | Recap | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts video-capture/018-public-site-start-donation.capture.ts
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/018-public-site-start-donation --engine kokoro
node tools/video-build/build-video.mjs docs/videos/018-public-site-start-donation
```

## Pitfalls

- The tier amounts are marked as a product decision in the L2 specification, so the script says they may change.
