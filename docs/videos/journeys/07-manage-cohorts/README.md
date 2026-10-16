# 07 · Manage cohorts

> **Runtime:** about 3 min 27 s · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [07-manage-cohorts.mp4](07-manage-cohorts.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

A cohort is the unit a young person moves through the program in, and each youth needs a mentor. This video follows Pat Morgan creating a cohort, adding Amara Bell, pairing a youth with a mentor, and reading the mentor load. It follows the designs for [managing cohorts](../../../detailed-designs/program-administration/manage-cohorts/README.md), [assigning mentors](../../../detailed-designs/program-administration/assign-mentors/README.md), and the acceptance criteria in [the program administration L2 specification](../../../specs/program-administration/L2.md).

## Learning objectives

By the end, the viewer can:

- Create a cohort with a track and a lead mentor.
- Add a youth to a cohort from its page.
- Pair a youth with a mentor and read the mentor load card.
- Explain that mentor roles belong to the cohort.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| What does a cohort need? | A name, a curriculum track and a lead mentor |
| When does a pairing take effect? | At once, on the mentor roster |
| What is the target load? | Four to eight active youth per mentor |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/create-dialog-1280.png` | The create cohort dialog |
| `images/card-1280.png` | The new cohort card |
| `images/members-1280.png` | The cohort with Amara as a member |
| `images/pairing-1280.png` | The pair a youth panel |
| `images/load-1280.png` | A mentor load card |
| `images/team-1280.png` | The cohort mentor team panel |

Captures are made by `frontend/video-capture/journeys/07-manage-cohorts.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:21 | Welcome | A cohort is a group of young people who move through the program together, and every young person works alongside a mentor. |
| 00:23-01:06 | Creating a cohort | Pat opens Cohorts from the menu and presses new cohort. |
| 01:08-01:44 | Adding youth | Pat opens the new cohort. |
| 01:46-02:14 | Pairing with a mentor | Next, Pat opens Mentor assignments. |
| 02:16-03:00 | Watching the load | Further down, every active mentor has a card. |
| 03:02-03:27 | Things to remember | A cohort needs a name, a track, and a lead mentor. |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts journeys/07
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/journeys/07-manage-cohorts --engine kokoro
node tools/video-build/build-video.mjs docs/videos/journeys/07-manage-cohorts
```

## Pitfalls

- The cohort mentor team panel is cropped to the cohort and lead mentor, because the shared test data lists many mentors.
