# 016 · Finding your way around the public site

> **Runtime:** about 2 min 30 s · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [016-public-site-navigate-public-site.mp4](016-public-site-navigate-public-site.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

The public site is the front door. This video shows the menu, footer, missing page, phone layout and where signing in fits. It follows [the design](../../detailed-designs/public-site/navigate-public-site/README.md) and the acceptance criteria of L2-PUB-001, L2-NFR-009 and L2-NFR-010 in [the public site L2 specification](../../specs/public-site/L2.md).

## Learning objectives

By the end, the viewer can:

- Name the links and buttons on the top menu.
- Describe what the footer, privacy page and missing page offer.
- Explain that Sign in leads to a separate sign in page.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| What is on the top menu? | Programs, About, Get involved, Contact, Sign in and Apply |
| What happens at a broken link? | A message with Back to home and Apply buttons |
| Where does Sign in lead? | To the separate sign in page |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/landing-1280.png` | The home page and top menu |
| `images/footer-1280.png` | The footer |
| `images/not-found-1280.png` | The page not found message |
| `images/phone-1280.png` | The home page at 375 pixels wide |
| `images/sign-in-1280.png` | The sign in page reached from Sign in |

Captures are made by `frontend/video-capture/016-public-site-navigate-public-site.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:20 | Welcome | The front door |
| 00:20-00:59 | The top menu | Menu and main buttons |
| 00:59-01:31 | The footer and the privacy page | Footer, privacy, missing page |
| 01:31-01:46 | On a phone | Narrow layout |
| 01:46-02:06 | Signing in | Hand-off to sign in |
| 02:06-02:30 | Recap | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts video-capture/016-public-site-navigate-public-site.capture.ts
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/016-public-site-navigate-public-site --engine kokoro
node tools/video-build/build-video.mjs docs/videos/016-public-site-navigate-public-site
```

## Pitfalls

- The phone capture uses a 375 pixel wide page; the other captures use 1280 by 800.
