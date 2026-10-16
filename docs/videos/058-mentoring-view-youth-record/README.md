# 058 · The youth record

> **Runtime:** about 3 min · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [058-mentoring-view-youth-record.mp4](058-mentoring-view-youth-record.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

The youth record gathers what a mentor needs before a conversation, and it applies the strictest scoping in the product. This video shows the banner and each tab. It follows [the view youth record design](../../detailed-designs/mentoring/view-youth-record/README.md) and the acceptance criteria of L2-MENTOR-002 in [the mentoring L2 specification](../../specs/mentoring/L2.md), with the access rules from L2-AUTH-009.

## Learning objectives

By the end, the viewer can:

- Name the tabs of the youth record.
- Say what the at-risk banner shows.
- Explain who can see guardian contact and what happens for an unassigned youth.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| What does the at-risk banner show? | The reason, who flagged and when |
| Who sees guardian contact? | The assigned mentor and staff only |
| What if the youth is not assigned? | The request is refused |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/profile-1280.png` | The banner, profile and guardian contact (used twice) |
| `images/progress-1280.png` | The Progress tab |
| `images/attendance-1280.png` | The Attendance tab |
| `images/check-ins-1280.png` | The Check-ins tab |
| `images/submissions-1280.png` | The Submissions tab |
| `images/competencies-1280.png` | The Competencies tab |

Captures are made by `frontend/video-capture/058-mentoring-view-youth-record.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:15 | Welcome | The record in one place |
| 00:15-00:45 | Opening the record | Header and banner |
| 00:45-01:15 | Profile and guardian contact | Scoping |
| 01:15-02:00 | Progress, attendance, check-ins | Three tabs |
| 02:00-02:30 | Submissions and competencies | Two tabs and the tracker button |
| 02:30-03:00 | Recap | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts video-capture/058-mentoring-view-youth-record.capture.ts
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/058-mentoring-view-youth-record --engine kokoro
node tools/video-build/build-video.mjs docs/videos/058-mentoring-view-youth-record
```

## Pitfalls

- The refusal for an unassigned youth has no dedicated screen in the record view, so the video states it and does not show it.
- The seeded email addresses on the profile are synthetic.
