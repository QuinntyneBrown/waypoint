# 115 · Uploading a lesson video

> **Runtime:** about 5 min 39 s · **Audience:** developers, designers and program staff who will build or use lesson video upload · **Prerequisites:** none

**Video:** [115-curriculum-authoring-upload-lesson-video.mp4](115-curriculum-authoring-upload-lesson-video.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

Authors record their own lessons, from 2 minutes to 3 hours. The current lesson media path accepts an MP4 of at most 25 MB and stores the bytes in SQL Server, which cannot carry a long recording. This video explains the designed replacement: resumable chunked upload, a leased background transcode, and an atomic publish of an HTTP Live Streaming (HLS) rendition ladder. It documents the design in [Upload a lesson video](../../detailed-designs/curriculum-authoring/upload-lesson-video/README.md). The feature is not built yet.

## Learning objectives

By the end, the viewer can:

- Explain why video gets its own upload path and documents keep the existing one.
- Describe how chunks are written at their offset so an upload can resume.
- Name the checks done at completion and why the probe runs immediately.
- Describe the transcode worker: lease, single ffmpeg pass, renditions, atomic publish.
- Say what happens on failure, retry and delete, and what the author sees.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| How does a 3-hour upload survive a dropped connection? | 8 MiB chunks written at `index × chunk size`, a received-chunk bitmap, and only the missing chunks resent |
| Why probe at completion rather than in the worker? | A file that is not a real video is rejected in seconds instead of hours later |
| How does a crashed server get noticed? | A 5-minute lease extended every 60 seconds while ffmpeg runs |
| Why can a viewer never see half a video? | Output goes to `work/{mediaId}` and moves to `hls/{mediaId}` in one step on success |

## Code / assets on screen

The feature is not implemented, so there is no running UI to capture. Every image is one of three real sources.

| File | What to show |
| --- | --- |
| `images/editor-media-1280.png` | Media card in the lesson editor mock, with the Attach media button and a video row. The mock does not draw progress, Processing or Failed states |
| `images/c4-container.png` | Design diagram: containers (copied from the detailed design) |
| `images/seq-upload.png`, `images/seq-complete.png`, `images/seq-transcode.png` | Simplified phase diagrams, sources in `images/diagrams/*.puml`, drawn from the design's sequence diagram with larger text for video |

The mock was captured with `tools/video-build/capture-mocks.py` from `images/shots.json`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:31 | Welcome | What the video covers and what the screens are |
| 00:31-01:03 | Why a new path | 25 MB byte-array path and its limits |
| 01:03-01:59 | Uploading in chunks | Session start checks, offset writes, resume, token refresh |
| 01:59-02:41 | Checking the file | Length, signature, immediate probe, one unit of work |
| 02:41-04:12 | The transcode worker | Lease, one ffmpeg pass, rendition ladder, atomic publish |
| 04:12-04:52 | When something fails | Time limit, two attempts, retry, trash and sweep |
| 04:52-05:23 | What the author sees | Four states, polling, missing mock states |
| 05:23-05:39 | Things to remember | Recap and preview of video 116 |

## Demo commands

```powershell
node tools/video-build/capture-mocks.py docs/videos/115-curriculum-authoring-upload-lesson-video
java -jar C:/tools/plantuml.jar -charset UTF-8 -tpng -o .. docs/videos/115-curriculum-authoring-upload-lesson-video/images/diagrams/*.puml
node tools/video-audio/generate-audio.mjs docs/videos/115-curriculum-authoring-upload-lesson-video --engine kokoro
node tools/video-build/build-video.mjs docs/videos/115-curriculum-authoring-upload-lesson-video
```

## Pitfalls

- The designs mark the media root path and real transcode throughput as `<TO SUPPLY>`; the video says so rather than quoting a figure for the deployment host.
- Transcode speed was measured on an 8-core development machine with the `veryfast` preset: 84 minutes for a 3-hour still picture, and slower than real time for worst-case high-motion footage. The deployment host figure is still to be supplied.
- Do not describe the mock as showing upload progress; it does not.

## References

- [Detailed design: Upload a lesson video](../../detailed-designs/curriculum-authoring/upload-lesson-video/README.md)
- [L2-CURRIC-002 Lessons authoring](../../specs/curriculum-authoring/L2.md), acceptance criteria 2, 6 and 7
- [L2-NFR-001 Input validation and injection defense](../../specs/non-functional/L2.md), acceptance criterion 4
