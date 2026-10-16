# 056 · The mentor dashboard and roster

> **Runtime:** about 2 min 30 s · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [056-mentoring-manage-mentor-roster.mp4](056-mentoring-manage-mentor-roster.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

The dashboard and roster are a mentor's starting point each day. This video shows the four counts, the roster filters, the empty states and the rule that a mentor only ever sees their own youth. It follows [the manage mentor roster design](../../detailed-designs/mentoring/manage-mentor-roster/README.md) and the acceptance criteria of L2-MENTOR-001 in [the mentoring L2 specification](../../specs/mentoring/L2.md), with the scoping rule from L2-AUTH-010.

## Learning objectives

By the end, the viewer can:

- Name the four dashboard counts.
- Search and filter the roster.
- Explain what a mentor does not see.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| Which youth are counted? | Only those assigned to the signed-in mentor |
| Which filters exist? | All, On track, At risk, New |
| What if nothing matches? | A labelled empty state with a clear filters button |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/dashboard-1280.png` | The four counts and the youth list |
| `images/roster-1280.png` | The roster |
| `images/at-risk-1280.png` | The At risk filter |
| `images/no-match-1280.png` | The no-match empty state |
| `images/empty-1280.png` | A mentor with no youth |

Captures are made by `frontend/video-capture/056-mentoring-manage-mentor-roster.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:20 | Welcome | What a mentor needs each morning |
| 00:20-01:00 | The dashboard | Counts, youth list, reviews preview |
| 01:00-01:30 | The roster | Columns, search, filters |
| 01:30-02:10 | When nothing matches | Empty states |
| 02:10-02:30 | Recap | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts video-capture/056-mentoring-manage-mentor-roster.capture.ts
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/056-mentoring-manage-mentor-roster --engine kokoro
node tools/video-build/build-video.mjs docs/videos/056-mentoring-manage-mentor-roster
```

## Pitfalls

- The video is about managing what a mentor sees; assigning youth to mentors is a program administrator task in another video.
- The seeded youth shows 100 percent progress and an at-risk tag because of the seed data; the narration does not claim they relate.
