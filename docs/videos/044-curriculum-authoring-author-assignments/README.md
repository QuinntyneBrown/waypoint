# 044 · Authoring assignments

> **Runtime:** about 3 min · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [044-curriculum-authoring-author-assignments.mp4](044-curriculum-authoring-author-assignments.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

Assignments are how youth show what they learned. This video shows what a program administrator decides when building one: what a youth can hand in, how the rubric adds up, and the due rules. It follows [the author assignments design](../../detailed-designs/curriculum-authoring/author-assignments/README.md) and the acceptance criteria of L2-CURRIC-004 in [the curriculum authoring L2 specification](../../specs/curriculum-authoring/L2.md).

## Learning objectives

By the end, the viewer can:

- List the required parts of an assignment.
- Explain how rubric points become a total.
- Name the due rules and when an assignment reaches youth.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| What must an assignment have? | A title, a task description and at least one submission type |
| Who computes the total points? | Waypoint, from the rubric criteria; it stores the total |
| When do youth get the work? | When it is published; it is then assigned to the youth in the module cohorts |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/types-rubric-1280.png` | The form with title, task and submission types |
| `images/rubric-1280.png` | Two rubric criteria with points |
| `images/due-rules-1280.png` | Due date, grace period, late and approval options |
| `images/total-1280.png` | The draft assignment with eight points |
| `images/published-1280.png` | The publish message |

Captures are made by `frontend/video-capture/044-curriculum-authoring-author-assignments.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:20 | Welcome | Why assignments exist |
| 00:20-01:05 | The assignment form | Title, task, submission types |
| 01:05-01:30 | The rubric | Criteria and points |
| 01:30-02:05 | Due rules | Due date, grace, late work, approval |
| 02:05-02:45 | Saving and publishing | Draft, total, publish |
| 02:45-03:10 | Things to remember | Recap |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts video-capture/044-curriculum-authoring-author-assignments.capture.ts
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/044-curriculum-authoring-author-assignments --engine kokoro
node tools/video-build/build-video.mjs docs/videos/044-curriculum-authoring-author-assignments
```

## Pitfalls

- The permission rule (only a program administrator edits curriculum) comes from L2-CURRIC-001 criterion 5 and L2-AUTH-009.
