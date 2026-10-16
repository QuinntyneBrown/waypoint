# 079 · Recording audit events

> **Runtime:** about 2 min 30 s · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [079-consent-and-safeguarding-record-audit-events.mp4](079-consent-and-safeguarding-record-audit-events.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

Accountability for safeguarding depends on a trustworthy record written without anyone having to remember to write it. It follows [the design](../../detailed-designs/consent-and-safeguarding/record-audit-events/README.md) and the acceptance criteria of L2-CONSENT-003 in [the L2 specification](../../specs/consent-and-safeguarding/L2.md).

## Learning objectives

By the end, the viewer can:

- List the kinds of action that are recorded.
- Describe what each entry holds.
- Explain how the trail is protected.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| What is recorded? | Mutating administrative and safeguarding actions, such as sign-ins, check-ins, attendance, badge awards, role changes and account deactivation. |
| What does an entry hold? | Who, the action, the target, when, and the source (device and location). |
| Can an entry be edited? | No. The trail is append-only and tamper-evident. |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/mentor-events-1280.png` | Written automatically |

Captures are made by `frontend/video-capture/079-consent-and-safeguarding-record-audit-events.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:15 | Welcome | Purpose |
| 00:15-00:50 | What gets recorded | Sign-in and check-in by Grace |
| 00:50-01:40 | What an entry holds | Rows in the audit log |
| 01:40-02:10 | Protected from change | Append-only and integrity |
| 02:10-02:30 | Recap | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts video-capture/079-consent-and-safeguarding-record-audit-events.capture.ts
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/079-consent-and-safeguarding-record-audit-events --engine kokoro
node tools/video-build/build-video.mjs docs/videos/079-consent-and-safeguarding-record-audit-events
```

## Pitfalls

- No screen records events; the capture shows their effect in the audit log.
- The source values shown come from the Testing host.
