# 077 · Inspecting and exporting the audit log

## Welcome

Waypoint keeps a record of who did what, and when. This video shows how a program administrator reads that audit log, narrows it down, and exports it, and why private details stay hidden even from this page.

[pause 2s]

## The audit log page

Pat Morgan is a program administrator. Only program administrators can open the audit log, so Pat opens Audit log from the menu. The page says it is an append-only record of who did what and when, with safeguarding details kept private. Append only means entries can be added, but never edited or deleted through the application.

At the top is a card of filters. Pat can filter by user, by action, and by a start date and an end date. Two buttons sit beside them. Apply filters, and Clear.

[pause 2s]

## Reading the entries

Below the filters is a table. Each row shows who did it, the action, the target and any detail, when it happened, and the source.

Pat picks the action, check-in logged, and the mentor, Grace Adeyemi. One row appears. It says private, and where the notes would be, it shows redacted. Grace wrote a private check-in about Amara Bell. Waypoint recorded that it happened, but the content of the private note is never exposed in the audit log.

[pause 2s]

## Filtering and paging

Now Pat looks at the exports. Pat chooses their own name and the action, data exported. The page tells Pat how many events match. Results are shown ten to a page. When there are more than ten, a summary such as page one of two appears, with Previous and Next buttons to move between pages.

If nothing matches, the page says, no activity for this filter, and offers a button to clear the filters.

[pause 2s]

## Exporting

Beside the title is a button, Export CSV. It exports what the filters currently show, as a file Pat can open in a spreadsheet. A green message confirms it.

The export is itself written to the audit log, as a data exported event. So even the act of taking a copy leaves a trace.

One more safeguard. If Waypoint's check of the log finds that records may have been changed, removed, or inserted outside the protected writer, a red warning says audit integrity check failed, and tells the administrator to escalate before relying on the log.

[pause 2s]

## Things to remember

Only program administrators see the audit log. It can be filtered by user, action, and date, and it is paged. Private note content is redacted. Export gives a CSV, and the export is logged too.

Next, we will look at how Waypoint records these events in the first place.
