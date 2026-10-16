# 19 · Manage people

> **Runtime:** about 3 min 58 s · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [19-manage-people.mp4](19-manage-people.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

Administrators need to find a person, read their record, and manage their account without mistakes. This video follows Pat Morgan through the directory, a youth record, a role change, and a deactivation. It follows the designs for [searching people](../../../detailed-designs/program-administration/search-people/README.md), [viewing a person record](../../../detailed-designs/program-administration/view-person-record/README.md), [managing the account lifecycle](../../../detailed-designs/program-administration/manage-account-lifecycle/README.md), and the acceptance criteria in [the program administration L2 specification](../../../specs/program-administration/L2.md).

## Learning objectives

By the end, the viewer can:

- Search and filter the people directory.
- Name the six tabs of a person record.
- Explain what a role change warns about.
- Explain what deactivation keeps and how it is reversed.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| How does Pat find someone? | Search by name or email, with role and cohort filters |
| What does a role change warn? | It grants staff access, is immediate, and revokes sessions |
| What does deactivation keep? | All progress, attendance, notes and reporting records |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/directory-1280.png` | The people directory |
| `images/filter-1280.png` | The directory filtered to youth |
| `images/record-1280.png` | A youth record |
| `images/role-dialog-1280.png` | The change role window |
| `images/deactivate-dialog-1280.png` | The deactivate window |
| `images/deactivated-1280.png` | A deactivated account |

Captures are made by `frontend/video-capture/journeys/19-manage-people.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:22 | Welcome | A program grows quickly. |
| 00:24-01:22 | Finding someone | Pat opens People from the menu. |
| 01:24-02:04 | A person's record | Pat opens a youth's record. |
| 02:06-02:48 | Changing a role | Now Pat opens the record of Sam Whitfield, a volunteer, and chooses the Account tab. |
| 02:50-03:34 | Deactivating an account | When someone steps back, Pat presses deactivate account. |
| 03:36-03:58 | Things to remember | Search by name or email, and filter by role and cohort. |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts journeys/19
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/journeys/19-manage-people --engine kokoro
node tools/video-build/build-video.mjs docs/videos/journeys/19-manage-people
```

## Pitfalls

- Removing a person is described in the narration but not captured.
- The directory search term is a random tag so only this story's people are listed.
