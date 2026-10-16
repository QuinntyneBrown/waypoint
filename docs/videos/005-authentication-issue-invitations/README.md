# 005 · Sending invitations

> **Runtime:** about 2 min 30 s · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [005-authentication-issue-invitations.mp4](005-authentication-issue-invitations.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

Accounts in Waypoint exist only because staff invited them. This video shows how invitations are created, singly or in bulk, who may create them, and what a person without permission sees. It follows [the issue-invitations design](../../detailed-designs/authentication/issue-invitations/README.md) and the acceptance criteria of L2-AUTH-003 in [the authentication L2 specification](../../specs/authentication/L2.md).

## Learning objectives

By the end, the viewer can:

- Describe what staff provide to send an invitation.
- Explain how several people are invited at once and how the result is reported.
- State which roles can invite and what a role without permission sees.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| What does an invitation need? | An email address and a role; cohort and mentor are optional |
| How are many people invited? | One address per line in one box; one pending invitation per address and a count |
| Who may invite? | Program administrators, and mentors for youth only; everyone else is denied |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/form-1280.png` | The send invitations card filled in |
| `images/sent-1280.png` | The sent confirmation and the pending rows |
| `images/mentor-1280.png` | The mentor view of the same page |
| `images/forbidden-1280.png` | The denied page for a youth |

Captures are made by `frontend/video-capture/005-authentication-issue-invitations.capture.ts`. Cohort and mentor names are the synthetic names produced by the test seed.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:15 | Welcome | Invitation-only accounts |
| 00:15-00:50 | The invitation form | Role, optional cohort and mentor, addresses |
| 00:50-01:25 | What happens next | Count reported, pending rows |
| 01:25-02:10 | Mentors and everyone else | Mentor scope, denied access |
| 02:10-02:30 | Recap | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts 005
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/005-authentication-issue-invitations --engine kokoro
node tools/video-build/build-video.mjs docs/videos/005-authentication-issue-invitations
```

## Pitfalls

- The fourteen-day expiry is marked "confirm with product owner" in the specification; the video only repeats what the page says.
- The invitation email itself is not shown; the capture covers the screens only.
