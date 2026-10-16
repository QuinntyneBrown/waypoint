# 015 · Exploring the programs

> **Runtime:** about 2 min 20 s · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [015-public-site-explore-programs.mp4](015-public-site-explore-programs.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

Visitors want to know what Waypoint teaches. This video shows the programs page: tracks, modules, outcome badges and the mentorship model. It follows [the design](../../detailed-designs/public-site/explore-programs/README.md) and the acceptance criteria of L2-PUB-002 (criterion 1) in [the public site L2 specification](../../specs/public-site/L2.md).

## Learning objectives

By the end, the viewer can:

- Describe what each track card shows.
- Explain that only published content appears.
- Name the three parts of the mentorship model.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| What does a track card contain? | Name, description, numbered modules and outcome badges |
| Which content is shown? | Published tracks and modules only |
| What is the mentorship model? | Weekly check-ins, reflection reviews and extra support when needed |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/programs-1280.png` | The programs page |
| `images/tracks-1280.png` | Track cards with modules and badges |
| `images/mentorship-1280.png` | The mentorship section |

Captures are made by `frontend/video-capture/015-public-site-explore-programs.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:16 | Welcome | Why the programs page |
| 00:16-00:50 | The two tracks | Track cards |
| 00:50-01:35 | Modules and badges | Modules, badges, published only |
| 01:35-02:00 | Mentorship | The mentorship model |
| 02:00-02:20 | Recap | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts video-capture/015-public-site-explore-programs.capture.ts
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/015-public-site-explore-programs --engine kokoro
node tools/video-build/build-video.mjs docs/videos/015-public-site-explore-programs
```

## Pitfalls

- Track, module and badge names in the captures are demo content substituted in the browser for the video; the page is the real one.
