# 094 · Replacing an expired invitation

> **Runtime:** about 2 min 45 s · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [094-platform-services-replace-expired-invitation.mp4](094-platform-services-replace-expired-invitation.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

Invitation links expire. This video shows what a person sees and how they ask for a replacement. It follows [the design](../../detailed-designs/platform-services/replace-expired-invitation/README.md) and the acceptance criteria of L2-PLATFORM-011 (criterion 2) and L2-AUTH-004 in [the platform services L2 specification](../../specs/platform-services/L2.md).

## Learning objectives

By the end, the viewer can:

- State how long invite links last.
- Describe the expired-link page and its request button.
- Explain why the confirmation is worded carefully.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| What does the person see? | "This invite link has expired", with a request a new invite button |
| Why is the confirmation careful? | It is the same whether or not a replacement can be sent, so nobody can probe which invitations exist |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/expired-1280.png` | The expired-invitation page |
| `images/requested-1280.png` | The confirmation after requesting a new invite |

Captures are made by `frontend/video-capture/094-platform-services-replace-expired-invitation.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:20 | Welcome | Why links expire |
| 00:20-00:55 | An old link | Amara opens an expired link |
| 00:55-01:40 | Request a new invite | The request and the careful confirmation |
| 01:40-02:20 | Other link problems | Used, cancelled, not valid |
| 02:20-02:45 | Recap | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts video-capture/094-platform-services-replace-expired-invitation.capture.ts
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/094-platform-services-replace-expired-invitation --engine kokoro
node tools/video-build/build-video.mjs docs/videos/094-platform-services-replace-expired-invitation
```

## Pitfalls

- The expired state in the capture is shown by changing the state in the answer to the invitation check, because the real lifetime is fourteen days. The invitation and its link are real; the page is the real one.
- The other-link-problems part uses a text-only slide, taken from the page headings.
