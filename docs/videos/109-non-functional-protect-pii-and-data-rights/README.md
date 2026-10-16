# 109 · Protecting personal information

> **Runtime:** about 3 min · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [109-non-functional-protect-pii-and-data-rights.mp4](109-non-functional-protect-pii-and-data-rights.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

Waypoint holds personal information about young people and families. This video shows what is kept, the page where a person sees it and makes a data-rights request, and the staff page where requests are reviewed and fulfilled. It follows [the protect PII and data rights design](../../detailed-designs/non-functional/protect-pii-and-data-rights/README.md) and the acceptance criteria of L2-NFR-003 in [the non-functional L2 specification](../../specs/non-functional/L2.md), with L2-CONSENT-001 in [the consent and safeguarding L2 specification](../../specs/consent-and-safeguarding/L2.md).

## Learning objectives

By the end, the viewer can:

- Describe what a person sees on the privacy and data rights page.
- Explain what happens when a request is submitted and why the export starts disabled.
- Describe the staff step before a request is fulfilled.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| Who can see a young person's personal data? | Staff and the assigned mentor, within their roles |
| What does a person get when they make a request? | A reference number; the request is tracked and recorded |
| When can the full export be downloaded? | After staff have verified and fulfilled an access request |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/privacy-page-1280.png` | The stored account data and the request form |
| `images/privacy-received-1280.png` | Request received, with its reference |
| `images/admin-request-1280.png` | The request as staff see it |

Captures are made by `frontend/video-capture/109-non-functional-protect-pii-and-data-rights.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:20 | Welcome | Information held in trust |
| 00:20-01:05 | What Waypoint keeps | Limits, access, protection, stored data card |
| 01:05-01:55 | Asking for something | Request form, reference, export gating |
| 01:55-02:40 | The staff side | Review, verified corrections, retention |
| 02:40-03:00 | Recap | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts video-capture/109-non-functional-protect-pii-and-data-rights.capture.ts
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/109-non-functional-protect-pii-and-data-rights --engine kokoro
node tools/video-build/build-video.mjs docs/videos/109-non-functional-protect-pii-and-data-rights
```

## Pitfalls

- Retention and anonymisation are described as what the design expects; there is no retention screen to show.
- Log redaction is a requirement in L2-NFR-003 and L2-NFR-011; the narration says it is meant to happen, not that every log was audited.
- The staff capture pages through the shared pending list to find the synthetic request made in the same run.
