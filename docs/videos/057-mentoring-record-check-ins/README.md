# 057 · Recording a check-in

> **Runtime:** about 3 min · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [057-mentoring-record-check-ins.mp4](057-mentoring-record-check-ins.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

Check-ins are the mentor's own record of a conversation, and they carry the program's strictest visibility rules. This video shows the form, the sharing switch and the history. It follows [the record check-ins design](../../detailed-designs/mentoring/record-check-ins/README.md) and the acceptance criteria of L2-MENTOR-006 in [the mentoring L2 specification](../../specs/mentoring/L2.md).

## Learning objectives

By the end, the viewer can:

- Name the required fields of a check-in.
- Explain the default visibility and the special rule for prayer requests.
- Filter the check-in history.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| What is required? | Youth, date and type |
| Who sees a check-in? | Only the mentor and permitted staff, unless the mentor shares it; sharing shows it to the youth and guardian |
| What about prayer requests? | Mentor-only unless sharing is explicitly enabled |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/form-1280.png` | The journal with its private history entry |
| `images/filled-1280.png` | A filled-in form |
| `images/share-1280.png` | The sharing switch and its warning |
| `images/history-1280.png` | The saved, shared entry |

Captures are made by `frontend/video-capture/057-mentoring-record-check-ins.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:20 | Welcome | Why check-ins are written down |
| 00:20-00:50 | The check-in journal | Form and history |
| 00:50-01:15 | Writing it down | Required and optional fields |
| 01:15-02:00 | Who can see it | Default private, prayer requests, guardian informed |
| 02:00-02:30 | Saving and finding it again | Confirmation and filters |
| 02:30-03:00 | Recap | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts video-capture/057-mentoring-record-check-ins.capture.ts
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/057-mentoring-record-check-ins --engine kokoro
node tools/video-build/build-video.mjs docs/videos/057-mentoring-record-check-ins
```

## Pitfalls

- The private entry shown in the history comes from the seed data and contains placeholder prayer text.
- The history filters are described in the narration but not captured separately.
