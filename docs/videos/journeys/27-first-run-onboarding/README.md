# 27 · First-run onboarding

> **Runtime:** about 2 min 54 s · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [27-first-run-onboarding.mp4](27-first-run-onboarding.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

The first minutes decide whether a new youth feels at home. This video follows the guided setup from the first step to the home page, including the youth who is not placed in a cohort yet. It follows [onboard youth](../../../detailed-designs/authentication/onboard-youth/README.md) and [await cohort placement](../../../detailed-designs/platform-services/await-cohort-placement/README.md), and L2-AUTH-011 in [the authentication L2 specification](../../../specs/authentication/L2.md).

## Learning objectives

By the end, the viewer can:

- Name the three onboarding steps and the one required answer.
- Explain what a youth sees when no cohort is assigned yet.
- State where skip for now leads.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| What is required in onboarding? | Only the display name |
| Who chooses the cohort and mentor? | The program team, when sending the invitation or placing the youth |
| What if the youth skips? | They land on the youth home and can resume setup later |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/profile-1280.png` | Step one |
| `images/cohort-1280.png` | Step two, placed |
| `images/unplaced-1280.png` | Step two, not placed |
| `images/notifications-1280.png` | Step three |
| `images/home-1280.png` | Youth home after finishing |

Captures are made by `frontend/video-capture/journeys/27-first-run-onboarding.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:20 | Welcome | What first-run setup is |
| 00:20-00:58 | Step one, your profile | Name, pronouns, optional photo |
| 00:58-01:19 | Step two, your cohort | Placement confirmed |
| 01:19-01:43 | When there is no cohort yet | The unplaced message |
| 01:43-02:05 | Step three, notifications | Defaults and finish |
| 02:05-02:29 | Landing at home | Youth home, and skip |
| 02:29-02:54 | Things to remember | Recap |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts journeys/27
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/journeys/27-first-run-onboarding --engine kokoro
node tools/video-build/build-video.mjs docs/videos/journeys/27-first-run-onboarding
```

## Pitfalls

- The setup wizard is reached here by accepting an invitation, which has its own journey video.
- The unplaced screen is captured from a separate youth with no cohort.
- The seeded names carry a short random suffix; it is not part of the product.
