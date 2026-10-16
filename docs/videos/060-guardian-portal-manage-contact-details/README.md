# 060 · Keeping contact details up to date

> **Runtime:** about 2 min 30 s · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [060-guardian-portal-manage-contact-details.mp4](060-guardian-portal-manage-contact-details.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

The program needs a current way to reach a guardian and an emergency contact, and guardians should be able to keep that right themselves. It follows [the design](../../detailed-designs/guardian-portal/manage-contact-details/README.md) and the acceptance criteria of L2-GUARDIAN-003 in [the L2 specification](../../specs/guardian-portal/L2.md).

## Learning objectives

By the end, the viewer can:

- List the fields a guardian can edit.
- Describe the confirmation shown after saving.
- Explain that a guardian can only change their own details.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| Which details can a guardian edit? | Name, relationship, email, phone, and emergency contact name and phone. |
| How does the guardian know it saved? | A message says contact details saved, and the values are still there after a reload. |
| Can a guardian edit someone else's details? | No. Access to another guardian's or a child's data is denied. |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/contact-1280.png` | Six fields, already filled in |
| `images/saved-1280.png` | Contact details saved |
| `images/after-reload-1280.png` | The change is stored by Waypoint |

Captures are made by `frontend/video-capture/060-guardian-portal-manage-contact-details.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:15 | Welcome | Why contact details matter |
| 00:15-00:55 | The contact card | The six fields |
| 00:55-01:25 | Making a change | New phone and emergency contact, save |
| 01:25-02:05 | Checking that it stuck | Reload shows the new values |
| 02:05-02:30 | Recap | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts video-capture/060-guardian-portal-manage-contact-details.capture.ts
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/060-guardian-portal-manage-contact-details --engine kokoro
node tools/video-build/build-video.mjs docs/videos/060-guardian-portal-manage-contact-details
```

## Pitfalls

- The email shown on the card is the synthetic sign-in address of the demo account.
