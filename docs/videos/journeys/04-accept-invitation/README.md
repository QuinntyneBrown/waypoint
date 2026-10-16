# 04 · Accept invitation

> **Runtime:** about 2 min 59 s · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [04-accept-invitation.mp4](04-accept-invitation.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

Everyone joins Waypoint through an invitation. This video follows one from the staff screen that sends it, through the page where a new youth sets up an account, to the cases where a link no longer works. It draws on [issue invitations](../../../detailed-designs/authentication/issue-invitations/README.md), [accept invitation](../../../detailed-designs/authentication/accept-invitation/README.md) and [manage pending invitations](../../../detailed-designs/authentication/manage-pending-invitations/README.md), and L2-AUTH-003 and L2-AUTH-004 in [the authentication L2 specification](../../../specs/authentication/L2.md).

## Learning objectives

By the end, the viewer can:

- Say who can send an invitation and what they choose.
- List what a new person provides to accept it.
- Explain what happens when a link is used twice, revoked or expired.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| What does an invitation carry? | A role, and optionally a cohort and a mentor, in a personal single use link |
| What does the invitee provide? | A display name, a password typed twice, and agreement to the community guidelines |
| What if the link no longer works? | The page says so, and offers a button to request a new invite; staff can also resend |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/issue-1280.png` | Send invitations form |
| `images/sent-1280.png` | Pending invitation with resend and revoke |
| `images/invite-1280.png` | The join page |
| `images/filled-1280.png` | The filled join form |
| `images/used-1280.png` | Already used |
| `images/revoked-1280.png` | No longer active |

Captures are made by `frontend/video-capture/journeys/04-accept-invitation.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:18 | Welcome | Nobody signs themselves up |
| 00:18-00:53 | Sending the invitation | Staff send and track invitations |
| 00:53-01:15 | Opening the link | The personal link |
| 01:15-01:51 | Setting up the account | Name, password, guidelines |
| 01:51-02:30 | Links that no longer work | Used, revoked or expired links |
| 02:30-02:59 | Things to remember | Recap |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts journeys/04
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/journeys/04-accept-invitation --engine kokoro
node tools/video-build/build-video.mjs docs/videos/journeys/04-accept-invitation
```

## Pitfalls

- The links expire after fourteen days, as the staff page states; this is the default in the specification.
- The revoke in the capture is made through the staff API, so the revoked link can be shown without waiting.
- The page that follows joining, the first time setup, has its own journey video.
