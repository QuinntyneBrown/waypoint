# 052 · Assessing competencies

> **Runtime:** about 2 min 15 s · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [052-mentoring-assess-competencies.mp4](052-mentoring-assess-competencies.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

Competencies are the one measure that only a mentor can set. This video shows the tracker, the four levels, the saved record and the summary. It follows [the assess competencies design](../../detailed-designs/mentoring/assess-competencies/README.md) and the acceptance criteria of L2-MENTOR-005 in [the mentoring L2 specification](../../specs/mentoring/L2.md).

## Learning objectives

By the end, the viewer can:

- Name the four competency levels.
- Say what is stored with each saved assessment.
- Explain where the latest assessment shows up.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| What levels can a mentor set? | Not started, Developing, Proficient, Mastered |
| What is saved with a level? | The assessing mentor and the time, plus notes |
| Where does the result show? | The summary count, the youth progress view and reports |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/tracker-1280.png` | The tracker and its summary |
| `images/card-1280.png` | One competency card being filled in |
| `images/saved-1280.png` | The card after saving |
| `images/summary-1280.png` | The updated summary |

Captures are made by `frontend/video-capture/052-mentoring-assess-competencies.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:20 | Welcome | Why competencies matter |
| 00:20-00:45 | The competency tracker | Summary and cards |
| 00:45-01:25 | Setting a level | Level, note, save |
| 01:25-01:55 | The summary updates | Count, youth view, reports |
| 01:55-02:15 | Recap | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts video-capture/052-mentoring-assess-competencies.capture.ts
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/052-mentoring-assess-competencies --engine kokoro
node tools/video-build/build-video.mjs docs/videos/052-mentoring-assess-competencies
```

## Pitfalls

- The seeded youth already has one competency at proficient, so the count goes from one to two on screen; the narration says it goes up by one.
