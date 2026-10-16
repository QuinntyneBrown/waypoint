# 031 · The progress board

> **Runtime:** about 3 min · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [031-program-administration-view-progress-board.mp4](031-program-administration-view-progress-board.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

This video shows what a program administrator sees and does for this part of program administration. It follows [the design](../../detailed-designs/program-administration/view-progress-board/README.md) and the acceptance criteria of L2-ADMIN-004 in [the program administration L2 specification](../../specs/program-administration/L2.md).

## Learning objectives

By the end, the viewer can:

- Select a cohort and track and read each cell state.
- Explain the quiz average, overall percentage and cohort average row.
- Say what the exports and the empty track state do.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| What do the cell states mean? | Done, In progress, Not started, Not assigned for each youth and module |
| Does the board need a manual refresh? | It updates live; a Refresh button is also provided |
| What do the exports contain? | The current cohort and track selection, as CSV or PDF |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/board-1280.png` | Board |
| `images/live-1280.png` | Board after an update |
| `images/empty-1280.png` | Empty track |

Captures are made by `frontend/video-capture/031-program-administration-view-progress-board.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:20 | Welcome | Spreadsheet replacement |
| 00:20-00:50 | Choosing a cohort and track | Pickers and live badge |
| 00:50-01:35 | Reading the board | Cells and totals |
| 01:35-02:00 | Staying current | Live update |
| 02:00-02:45 | Exports and empty tracks | CSV, PDF, empty state |
| 02:45-03:05 | Recap | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts video-capture/031-program-administration-view-progress-board.capture.ts
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/031-program-administration-view-progress-board --engine kokoro
node tools/video-build/build-video.mjs docs/videos/031-program-administration-view-progress-board
```

## Pitfalls

- The Not assigned state is described but not shown; the sample cohort has no unassigned module.
- The board capture uses a seeded demo youth.
