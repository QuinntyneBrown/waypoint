# 011 · Seeing only what your role allows

> **Runtime:** about 3 min · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [011-authentication-scope-data-by-role.mp4](011-authentication-scope-data-by-role.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

It follows [the scope data by role design](../../detailed-designs/authentication/scope-data-by-role/README.md) and the acceptance criteria of L2-AUTH-010, L2-AUTH-009 and L2-MENTOR-006 in [the authentication L2 specification](../../specs/authentication/L2.md).

## Learning objectives

By the end, the viewer can:

- State what a mentor, youth, guardian and volunteer can each see.
- Explain what happens when someone opens a page for a different role.
- Say why hiding a button is not enough.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| What does a mentor see? | Only the youth assigned to them |
| What does a guardian never see? | Lesson content, quiz answers, private reflections or account details |
| What does a volunteer see? | One assigned session and a names-only roster, with present or absent |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/mentor-1280.png` | The mentor dashboard |
| `images/blocked-1280.png` | The 403 page |
| `images/guardian-1280.png` | The guardian home |
| `images/volunteer-1280.png` | The volunteer Today page |

Captures are made by `frontend/video-capture/011-authentication-scope-data-by-role.capture.ts`.

## Run sheet

| Segment | Content |
| --- | --- |
| Welcome | Why scope matters |
| A mentor | Assigned youth only |
| A youth | Own records; 403 on other roles |
| A guardian | Summary of own child |
| A volunteer | One session, names only |
| Program administrators | All cohorts, private notes restricted |
| Recap | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts video-capture/011-authentication-scope-data-by-role.capture.ts
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/011-authentication-scope-data-by-role --engine kokoro
node tools/video-build/build-video.mjs docs/videos/011-authentication-scope-data-by-role
```

## Pitfalls

- The seeded mentor, youth and cohort names carry random suffixes, so the narration does not read names off the screen. The administrator part has no screen and is text only.
