# 15 · Check in

> **Runtime:** about 3 min 19 s · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [15-check-in.mp4](15-check-in.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

Mentoring happens in conversations, and some of them are sensitive. This video follows one mentor from a youth's record to the check-in journal, and shows how each note is private unless she chooses to share it. It draws on [view youth record](../../../detailed-designs/mentoring/view-youth-record/README.md) and [record check-ins](../../../detailed-designs/mentoring/record-check-ins/README.md), and on L2-MENTOR-002 and L2-MENTOR-006 in [the mentoring L2 specification](../../../specs/mentoring/L2.md).

## Learning objectives

By the end, the viewer can:

- Describe what a check-in must contain and what else can be recorded.
- Explain the default visibility and how prayer requests are handled.
- Say how the history can be filtered.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| What is required for a check-in? | A youth, a date and a type |
| Who can see a new check-in? | Only mentors and allowed staff, until the mentor switches sharing on |
| What happens to prayer requests when sharing is on? | They are shared too, and the page says so in plain words |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/record-tab-1280.png` | The Check-ins tab on the youth record |
| `images/form-1280.png` | The journal with the form and the history |
| `images/filled-1280.png` | The form filled in |
| `images/share-1280.png` | The sharing switch and its plain-words notice |
| `images/history-1280.png` | The saved entry marked Shared |

Captures are made by `frontend/video-capture/journeys/15-check-in.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:22 | Welcome | Mentoring happens in conversations. |
| 00:24-00:44 | Starting from the record | Grace opens Amara's record, and chooses the Check-ins tab. |
| 00:46-01:20 | The check-in journal | Grace presses the button, and the Check-ins page opens with Amara already chosen. |
| 01:22-01:45 | Writing it down | A check-in needs three things. |
| 01:47-02:32 | Who can see it | Near the bottom, a switch says, share this check-in with the youth and guardian. |
| 02:34-02:53 | Saved | Grace presses Save check-in, and a message confirms it. |
| 02:55-03:19 | Things to remember | Every check-in needs a youth, a date, and a type. |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts journeys/15
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/journeys/15-check-in --engine kokoro
node tools/video-build/build-video.mjs docs/videos/journeys/15-check-in
```

## Pitfalls

- The older private entry is seeded data.
- The follow-up date is optional and left empty in the capture.
