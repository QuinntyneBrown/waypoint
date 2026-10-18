# 116 · Streaming a lesson video

> **Runtime:** about 5 min 33 s · **Audience:** developers and designers who will build or review lesson video playback · **Prerequisites:** video 115

**Video:** [116-learning-experience-stream-lesson-video.mp4](116-learning-experience-stream-lesson-video.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

A youth should be able to play a lesson video of up to 3 hours quickly, at a quality that suits their connection, seek to any point, and only if they may see that lesson. A bare video tag cannot send the in-memory access token, so the design issues a signed, expiring URL and serves files without touching the database. This video explains that design in [Stream a lesson video](../../detailed-designs/learning-experience/stream-lesson-video/README.md). The feature is not built yet.

## Learning objectives

By the end, the viewer can:

- Explain why a plain video tag fails against the current authentication.
- Describe how a playback URL is authorized once and signed with an expiry.
- Say why the token sits in the URL path and why the server never redirects.
- List what the media endpoint checks before it sends a file, and its cache rules.
- Describe the on-demand player mode, token renewal and the legacy fallback.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| Why not use the access token on the video request? | The token is held in memory only and no cookies exist, so a media element cannot attach it |
| Why put the token in the path? | Relative playlist and segment addresses inherit it, so one decision covers the whole session |
| Why does the endpoint return 404 for every rejection? | It reveals nothing about which media exist |
| What happens when the token expires mid-video? | The next request is refused, the page requests a fresh address, and playback resumes at the same position |

## Code / assets on screen

The feature is not implemented, so there is no running UI to capture. Every image is one of four real sources.

| File | What to show |
| --- | --- |
| `images/lesson-watch-1280.png` | Watch card in the youth lesson mock, with the video placeholder |
| `images/c4-component-frontend.png`, `images/c4-component-backend.png` | Design diagram: components, cropped from the detailed design into a browser part and a server part |
| `images/seq-authorize.png`, `images/seq-play.png`, `images/seq-renew.png`, `images/seq-reject.png` | Simplified phase diagrams, sources in `images/diagrams/*.puml`, drawn from the design's sequence diagram with larger text |
| `images/live-light.png`, `images/live-dark.png` | Captures of the existing live player, copied from video 114. They show the shared controls only; the seek bar and quality label are not in them |

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:30 | Welcome | Goals of playback and what the screens are |
| 00:30-01:03 | Why a plain video tag fails | No token on media requests, e2e stub hides it |
| 01:03-01:55 | Asking for permission | Playback endpoint, access rule, signed address, 8 hours |
| 01:55-02:32 | Why the token is in the path | Relative addresses, no redirects |
| 02:32-03:33 | Serving the files | Validation order, Range, cache rules |
| 03:33-04:32 | The player | `src` and `poster`, native HLS or hls.js, seek bar, quality label |
| 04:32-05:18 | Expiry and failure | Renewal, rejection, legacy video tag, preparing message |
| 05:18-05:33 | Things to remember | Recap |

## Demo commands

```powershell
node tools/video-build/capture-mocks.py docs/videos/116-learning-experience-stream-lesson-video
java -jar C:/tools/plantuml.jar -charset UTF-8 -tpng -o .. docs/videos/116-learning-experience-stream-lesson-video/images/diagrams/*.puml
node tools/video-audio/generate-audio.mjs docs/videos/116-learning-experience-stream-lesson-video --engine kokoro
node tools/video-build/build-video.mjs docs/videos/116-learning-experience-stream-lesson-video
```

## Pitfalls

- The portal now declares a Content Security Policy that allows the player (`media-src 'self' blob:`). The video does not mention it.
- Say that the player captures come from the live player and do not yet show the seek bar or quality label.
- The signing key and the media root path are `<TO SUPPLY>` in the design.

## References

- [Detailed design: Stream a lesson video](../../detailed-designs/learning-experience/stream-lesson-video/README.md)
- [L2-LEARN-002 Lesson delivery and completion](../../specs/learning-experience/L2.md), acceptance criteria 1, 4, 5 and 6
- [Video 115 · Uploading a lesson video](../115-curriculum-authoring-upload-lesson-video/README.md)
