# 114 · The Waypoint video player

> **Runtime:** about 5 min 27 s · **Audience:** developers and designers who will use the player · **Prerequisites:** none

**Video:** [114-components-video-player.mp4](114-components-video-player.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

`w-video-player` in `@waypoint/components` plays a live fragmented-MP4 stream from a hub. This video shows every feature once in the light theme and once in the dark theme, so a team can see that the player follows the design system without extra code. It documents behaviour that is described in [the component README](../../../frontend/projects/components/src/lib/video-player/README.md).

## Learning objectives

By the end, the viewer can:

- Name the states the player moves through and what each looks like.
- Use the keyboard shortcuts and the controls, and say what changes on a narrow player.
- Explain the reconnect schedule, the stall timeouts and which errors offer Retry.
- Replace the player's text and colours from the host.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| Why is there no seek bar? | The player is live only: pause holds the picture, play or the live badge returns to the live edge |
| What happens when the connection drops? | Reconnect attempts after 0, 2, 5, 10 and 10 seconds, then a connection error |
| Which errors can be retried? | All except unsupported; Retry opens a fresh connection and asks for a new token |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/*-light.png`, `images/*-dark.png` | The same Storybook story in each theme |

Captures are made by `frontend/video-capture/114-components-video-player.capture.mjs` from the Storybook stories in `frontend/projects/components/src/lib/video-player/w-video-player.stories.ts`. The stories replay a ten-second fixture (`demo/media/demo-10s.fmp4`) through a demo transport.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:22 | Meet the player | What it is and how the video is organised |
| 00:22-00:55 | Connecting and playing live | Connecting state, live state, control bar |
| 00:55-01:24 | Pause and go back to live | Central play button, live badge as a button |
| 01:24-01:51 | Sound and volume | Mute, volume slider, narrow layout |
| 01:51-02:14 | Captions | Captions button and cue text |
| 02:14-02:46 | Keyboard and focus | Focus ring and shortcut keys |
| 02:46-03:03 | Fullscreen | Auto-hiding controls |
| 03:03-03:46 | When the connection wobbles | Buffering, waiting, reconnect schedule, stream ended |
| 03:46-04:20 | Errors and retry | Codes, Retry, alert |
| 04:20-05:04 | Your words and your colours | Localised text, themed tokens |
| 05:04-05:27 | Light and dark | Fixes made, recap |

## Demo commands

```powershell
cd frontend
npm run build-storybook
node video-capture/114-components-video-player.capture.mjs
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/114-components-video-player --engine kokoro
node tools/video-build/build-video.mjs docs/videos/114-components-video-player
```

The capture needs Microsoft Edge, because Playwright's bundled Chromium cannot play H.264 or AAC.

## Pitfalls

- The volume slider is hidden when the player is narrower than 47.5 em, so capture at a wider size.
- The live badge dot pulses; a still can catch it mid-pulse.
- Browsers may block autoplay with sound; the stories start muted.

## References

- [Component README](../../../frontend/projects/components/src/lib/video-player/README.md)
