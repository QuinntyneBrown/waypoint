# 045 · Authoring badges

> **Runtime:** about 3 min · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [045-curriculum-authoring-author-badges.mp4](045-curriculum-authoring-author-badges.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

Badges motivate youth, and the rules behind them must be clear. This video shows how automatic and manual badges are defined and how archiving works. It follows [the author badges design](../../detailed-designs/curriculum-authoring/author-badges/README.md) and the acceptance criteria of L2-CURRIC-005 in [the curriculum authoring L2 specification](../../specs/curriculum-authoring/L2.md).

## Learning objectives

By the end, the viewer can:

- Describe the fields of a badge definition.
- Name the five automatic rules.
- Explain what archiving a badge does and does not do.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| What are the two criteria kinds? | Automatic, which follows a rule, and manual, which a person gives |
| What are the automatic rules? | Module completed, lesson completed, quiz passed, assignment approved, five-session attendance streak |
| What does archiving do? | Stops new awards; youth who already earned the badge keep it |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/auto-form-1280.png` | New badge definition for an automatic badge |
| `images/auto-card-1280.png` | The saved automatic badge card |
| `images/manual-form-1280.png` | The form with the manual kind chosen |
| `images/manual-card-1280.png` | The manual badge card |
| `images/archived-1280.png` | The archive confirmation |

Captures are made by `frontend/video-capture/045-curriculum-authoring-author-badges.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:20 | Welcome | What a badge is |
| 00:20-01:10 | An automatic badge | Fields, rules, target, competency |
| 01:10-01:40 | Saving it | The card and one-time awards |
| 01:40-02:05 | A manual badge | Rule fields disappear |
| 02:05-02:30 | Archiving a badge | Retire a badge |
| 02:30-03:00 | Things to remember | Recap |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts video-capture/045-curriculum-authoring-author-badges.capture.ts
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/045-curriculum-authoring-author-badges --engine kokoro
node tools/video-build/build-video.mjs docs/videos/045-curriculum-authoring-author-badges
```

## Pitfalls

- The automatic award itself (once, with a notification) is stated from L2-CURRIC-005 criterion 2; the video shows only the definition, not a youth earning it.
