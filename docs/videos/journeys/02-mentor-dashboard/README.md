# 02 · Mentor dashboard

> **Runtime:** about 3 min 45 s · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [02-mentor-dashboard.mp4](02-mentor-dashboard.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

Mentors start each day on the dashboard. This video follows one mentor from the dashboard to the roster to the record of a youth who is falling behind, and shows that she only ever sees her own youth. It draws on [manage mentor roster](../../../detailed-designs/mentoring/manage-mentor-roster/README.md) and [view youth record](../../../detailed-designs/mentoring/view-youth-record/README.md), and on L2-MENTOR-001 and L2-MENTOR-002 in [the mentoring L2 specification](../../../specs/mentoring/L2.md).

## Learning objectives

By the end, the viewer can:

- Name the four counts on the mentor dashboard and say whose youth they cover.
- Use the roster search and filters to find youth who need attention.
- Describe what the youth record shows first and who can see guardian contact.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| What do the dashboard counts cover? | Only the youth assigned to the signed-in mentor; overdue reviews are called out |
| How does a mentor find youth who need attention? | Search by name and the filters All, On track, At risk and New |
| What does the youth record show before anything else? | An at-risk banner with the reason, who flagged and when; guardian contact is visible only to the assigned mentor and staff |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/dashboard-1280.png` | Four counts, overdue in red, youth list and review preview |
| `images/roster-1280.png` | My youth with search and filters |
| `images/at-risk-1280.png` | The At risk filter applied |
| `images/record-1280.png` | David's record, banner, tabs and guardian contact |
| `images/attendance-1280.png` | The Attendance tab |

Captures are made by `frontend/video-capture/journeys/02-mentor-dashboard.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:32 | Welcome | Every morning, a mentor needs to know three things. |
| 00:34-01:29 | The dashboard | Grace signs in, and lands on her dashboard. |
| 01:31-02:13 | The roster | Grace presses View all youth. |
| 02:15-03:11 | David's record | Grace opens David. |
| 03:13-03:45 | Things to remember | The dashboard shows four counts for the mentor's own youth only, and calls out overdue reviews. |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts journeys/02
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/journeys/02-mentor-dashboard --engine kokoro
node tools/video-build/build-video.mjs docs/videos/journeys/02-mentor-dashboard
```

## Pitfalls

- The seeded cohort names carry a short random suffix; it is not part of the product.
- The reasons on the banner come from the seeded flag.
