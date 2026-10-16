# 041 · Viewing badges

> **Runtime:** about 2 min 15 s · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [041-learning-experience-view-badges.mp4](041-learning-experience-view-badges.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

Badges motivate youth and show staff what progress looks like. This video shows the badges page, the locked-badge criteria and the milestone timeline. It follows [the view badges design](../../detailed-designs/learning-experience/view-badges/README.md) and the acceptance criteria of L2-LEARN-009 in [the learning experience L2 specification](../../specs/learning-experience/L2.md).

## Learning objectives

By the end, the viewer can:

- Say what an earned badge card shows.
- Say what a locked badge card shows.
- Explain why badge counts agree across screens.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| What does an earned badge show? | What it is for and the date it was earned |
| What does a locked badge show? | The criteria and counted progress, such as zero of two |
| Why do counts agree on every screen? | They are derived from a single source of truth |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/badges-1280.png` | The collection summary and an earned badge |
| `images/locked-1280.png` | Locked badges with progress |
| `images/timeline-1280.png` | The milestone timeline and current goal |

Captures are made by `frontend/video-capture/041-learning-experience-view-badges.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:15 | Welcome | Badges mark progress |
| 00:15-00:50 | Earned badges | Summary and earned card |
| 00:50-01:30 | Badges still to unlock | Criteria and counts |
| 01:30-01:55 | Milestones | Timeline and current goal |
| 01:55-02:15 | Things to remember | Recap |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts 041
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/041-learning-experience-view-badges --engine kokoro
node tools/video-build/build-video.mjs docs/videos/041-learning-experience-view-badges
```

## Pitfalls

- Badge names on screen carry a random suffix because the capture uses seeded test data in a shared test database, which also adds extra locked badges; the script does not quote the totals.
