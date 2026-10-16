# 28 · Explore the public site

> **Runtime:** about 3 min 30 s · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [28-explore-public-site.mp4](28-explore-public-site.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

The public site is how visitors learn about Waypoint and decide to apply, volunteer, give or ask a question. This journey follows Sam Whitfield through it. It follows the [public site designs](../../../detailed-designs/public-site/navigate-public-site/README.md) and L2-PUB-001 to L2-PUB-003 in [the public site L2 specification](../../../specs/public-site/L2.md).

## Learning objectives

By the end, the viewer can:

- Name what a visitor can see without signing in.
- Explain that the page shows program structure, never youth details.
- Describe how volunteering, donating and asking a question work.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| What do the impact numbers count? | Tracks, cohorts, modules and neighbourhoods, never individuals |
| What must a volunteer acknowledge? | The screening and reference check |
| Does Waypoint see card details? | No, the payment is finished with an external provider |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/hero-1280.png` | The home page |
| `images/tracks-1280.png` | Published tracks, modules and badges |
| `images/cohorts-1280.png` | Current cohorts |
| `images/involved-1280.png` | The get involved page |
| `images/donate-1280.png` | The donation form |
| `images/contact-1280.png` | The contact page |

Captures are made by `frontend/video-capture/journeys/28-explore-public-site.capture.ts`. Track and cohort names are demo content supplied through the same public content request the page makes.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:20 | Welcome | Who Sam is |
| 00:20-01:00 | The home page | Headline, journey, impact numbers |
| 01:00-01:45 | Programs and cohorts | Tracks, badges, cohorts |
| 01:45-02:20 | Getting involved | Volunteer screening, cards |
| 02:20-03:10 | Donating and asking a question | Donation and contact |
| 03:10-03:30 | Recap | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts journeys/28
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/journeys/28-explore-public-site --engine kokoro
node tools/video-build/build-video.mjs docs/videos/journeys/28-explore-public-site
```

## Pitfalls

- The monthly donation amounts are still a product decision; the narration does not quote them.
- The volunteer form is described rather than submitted; see the volunteer video for that flow.
