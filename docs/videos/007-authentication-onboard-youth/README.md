# 007 · Setting up a new youth

> **Runtime:** about 2 min 30 s · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [007-authentication-onboard-youth.mp4](007-authentication-onboard-youth.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

A newly activated youth is guided through a short setup before reaching home. This video shows the three steps, what is fixed by staff, and that the setup can be skipped. It follows [the onboard-youth design](../../detailed-designs/authentication/onboard-youth/README.md) and the acceptance criteria of L2-AUTH-011 in [the authentication L2 specification](../../specs/authentication/L2.md).

## Learning objectives

By the end, the viewer can:

- Name the three onboarding steps.
- Explain what the youth can and cannot set (profile yes; cohort and mentor no).
- State the notification defaults and what skip for now does.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| What can a youth set in onboarding? | Display name (required), pronouns, an optional photo, and notification preferences |
| Who chooses the cohort and mentor? | The program team; the youth sees them confirmed, read-only |
| What are the notification defaults? | Lesson reminders on, mentor messages on, announcements off |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/profile-1280.png` | Step one |
| `images/cohort-1280.png` | Step two, placed in a cohort |
| `images/notifications-1280.png` | Step three with the defaults |
| `images/home-1280.png` | The youth home after finishing |

Captures are made by `frontend/video-capture/007-authentication-onboard-youth.capture.ts`. The youth is created by accepting an invitation that names a cohort and mentor, so step two shows the placed state. The notifications step also lists mentor-oriented switches; the narration names only the three defaults the specification states.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:15 | Welcome | Why onboarding exists |
| 00:15-00:45 | Step one, your profile | Name, pronouns, photo |
| 00:45-01:10 | Step two, your cohort | Read-only confirmation |
| 01:10-01:30 | Step three, notifications | Defaults |
| 01:30-01:55 | Landing at home | Finish and skip |
| 01:55-02:20 | Recap | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts 007
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/007-authentication-onboard-youth --engine kokoro
node tools/video-build/build-video.mjs docs/videos/007-authentication-onboard-youth
```

## Pitfalls

- The skip path is described, not shown on screen; it is covered by the onboarding end-to-end specification.
