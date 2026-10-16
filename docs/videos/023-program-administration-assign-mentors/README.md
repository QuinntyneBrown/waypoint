# 023 · Assigning mentors

> **Runtime:** about 3 min · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [023-program-administration-assign-mentors.mp4](023-program-administration-assign-mentors.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

This video shows what a program administrator sees and does for this part of program administration. It follows [the design](../../detailed-designs/program-administration/assign-mentors/README.md) and the acceptance criteria of L2-ADMIN-005 in [the program administration L2 specification](../../specs/program-administration/L2.md).

## Learning objectives

By the end, the viewer can:

- Describe how an administrator pairs a youth with a mentor and when the mentor sees the change.
- Explain what the mentor load cards and the target band tell the administrator.
- State how lead and supporting mentors are set for a cohort and that a mentor can hold different roles in different cohorts.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| When does a pairing show on the mentor's roster? | Immediately after saving |
| What does a load card show? | Youth count, check-ins, reviews and a badge against the target band; overload is flagged |
| Can one mentor lead one cohort and support another? | Yes; roles belong to the cohort |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/overview-1280.png` | The Mentor assignments page, both panels |
| `images/load-1280.png` | A mentor's load card |
| `images/team-1280.png` | Cohort team saved |

Captures are made by `frontend/video-capture/023-program-administration-assign-mentors.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:15 | Welcome | Why assignment matters |
| 00:15-01:05 | Pairing a youth with a mentor | Pair a youth, save, roster updates |
| 01:05-01:50 | Watching the load | Load cards and target band |
| 01:50-02:35 | Cohort mentor teams | Lead and supporting mentors |
| 02:35-03:00 | Recap | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts video-capture/023-program-administration-assign-mentors.capture.ts
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/023-program-administration-assign-mentors --engine kokoro
node tools/video-build/build-video.mjs docs/videos/023-program-administration-assign-mentors
```

## Pitfalls

- The target band (four to eight active youth) is the value shown on the page; the capture shows an Under target mentor because the overloaded state needs more than eight youth.
- Mentor names on the load card come from the seeded demo accounts.
