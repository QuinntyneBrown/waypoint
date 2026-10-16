# 108 · Protecting against overwritten work

## Welcome

Sometimes two people work on the same record at the same time. If Waypoint simply saved whichever change arrived last, the first person's work would vanish without anyone noticing. This video shows how Waypoint prevents that quiet loss, using attendance as the example.

[pause 2s]

## Two people, one session

Grace Adeyemi is a mentor. She opens the attendance page for tonight's session and marks Amara Bell present. She adds a note, arrived early. She has not saved yet. At the bottom, the page says, unsaved changes.

Meanwhile, a second person opens the same session and saves their own attendance first. Behind the scenes, every attendance record carries a version marker. When the second person saved, that marker moved forward. Grace's page is still holding the old one.

[pause 2s]

## The save is refused

Grace presses save attendance. Waypoint compares her marker with the current one, sees they no longer match, and does not save. Instead, a message appears. It says, attendance changed on the server. Your selections remain here, review and reconcile them.

Look at what that message promises. Nothing was overwritten, and nothing of Grace's was thrown away. Her marks and her note are still on the screen. She can look at the latest attendance, decide what is right, and save again.

[pause 2s]

## Why this matters

This is called optimistic concurrency. It means Waypoint does not lock a record while someone looks at it, so people are never stuck waiting. It only checks, at the moment of saving, that nobody else got there first. When a save does succeed, the version marker moves forward, ready for the next check.

The same protection applies when attendance is saved offline and synced later. If the record has changed in the meantime, Waypoint asks which version to keep, instead of choosing for you.

[pause 2s]

## Things to remember

Two people can work on the same record, and neither silently erases the other. A stale save is refused, with a plain message. The person's own work stays on the screen, so they can reconcile and try again.

Next, we will look at how Waypoint warns people before they lose work they have not saved.
