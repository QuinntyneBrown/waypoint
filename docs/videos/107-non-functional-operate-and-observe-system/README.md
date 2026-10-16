# 107 · Running and watching Waypoint

> **Runtime:** about 3 min 15 s · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [107-non-functional-operate-and-observe-system.mp4](107-non-functional-operate-and-observe-system.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

Operations are mostly invisible to program staff. This video explains the health checks, correlation ids, the operational command line tool, start-up checks and maintenance mode, and shows the two visible effects: the health responses and the maintenance page. It follows [the operate and observe system design](../../detailed-designs/non-functional/operate-and-observe-system/README.md) and the acceptance criteria of L2-NFR-011 in [the non-functional L2 specification](../../specs/non-functional/L2.md).

## Learning objectives

By the end, the viewer can:

- Explain what the two health checks answer.
- Describe what a correlation id is for.
- Name the kinds of job the operational command line tool performs.
- Describe what people see in maintenance mode.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| What do the health checks report? | Alive for the process, ready only when the database is reachable |
| How is one request followed through the logs? | A correlation id returned with the response and attached to its log entries |
| What happens when a production setting is missing? | Waypoint stops at start-up instead of running broken |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/live-1280.png` | The live health response |
| `images/ready-1280.png` | The ready health response |
| `images/maintenance-1280.png` | The maintenance page |

Captures are made by `frontend/video-capture/107-non-functional-operate-and-observe-system.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:20 | Welcome | The invisible side of running Waypoint |
| 00:20-01:05 | Is it up? | The two health addresses |
| 01:05-01:45 | Leaving a trail | Correlation ids and redacted error reports |
| 01:45-02:55 | Operating the system | Command line tool, start-up checks, maintenance mode |
| 02:55-03:15 | Recap | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts video-capture/107-non-functional-operate-and-observe-system.capture.ts
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/107-non-functional-operate-and-observe-system --engine kokoro
node tools/video-build/build-video.mjs docs/videos/107-non-functional-operate-and-observe-system
```

## Pitfalls

- The health addresses answer a single word because they are for monitoring tools, not people.
- The log redaction is stated as a requirement; only the redacted error report is confirmed in code, so the narration does not claim every log field is scrubbed.
- The maintenance page is shown directly by its address; switching maintenance mode on is a configuration setting.
