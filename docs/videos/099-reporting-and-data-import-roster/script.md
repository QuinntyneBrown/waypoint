# 099 · Importing a roster

## Welcome

Many programs already keep their roster in a spreadsheet. Waypoint lets an administrator bring that roster in, in three reviewed steps, without typing every person by hand and without creating duplicate accounts. This video walks through the import page.

[pause 2s]

## Upload

Pat Morgan, a program administrator, opens the import roster page. A note at the top says to export a Google Sheet as a CSV first. The source sheet stays untouched, and only rows marked ready will create youth accounts.

Step one is upload. The page accepts a CSV file only, up to two megabytes. If Pat picks the wrong kind of file, a text file for example, the page says so right away. Choose a dot C S V file. Nothing is sent.

Pat chooses the real file, called summer roster. The page reads it and says four rows, seven columns.

[pause 2s]

## Map the columns

Step two is mapping. Every column in the file is listed with a sample value, and a menu for the Waypoint field it belongs to. Name, Email, Cohort, Mentor, Track, Status, or Skip.

Waypoint suggests matches, so full name becomes Name, and email address becomes Email. Pat sets the notes column to Status, and sets the leader column and the unused column to Skip. A skipped column is simply ignored. The page also needs exactly one column mapped to Email, because the email is how Waypoint recognizes a person.

[pause 2s]

## Preview and import

Pat presses preview roster, and Waypoint checks every row. Each row gets a status. Amara Bell is ready. David Osei is a duplicate, because his email already has an account. Imani Clarke is missing consent. And one row has an email that is not an email, so it is marked invalid. A summary above the table counts them. One ready, one needs consent, one duplicate, and one invalid.

The import button says how many people it will create. Pat presses it, and a confirmation appears. Only one ready row will create an account. Duplicates, missing consent rows, and invalid rows stay unchanged. Pat confirms.

The result says import complete, and counts what happened. One imported, one duplicate skipped, one needs consent, one invalid. The import is written to the audit log. And because people are matched by email, running the same file again does not create duplicates.

[pause 2s]

## Things to remember

Upload a CSV, map its columns, preview every row, then confirm. Only ready rows become accounts. Duplicates are skipped, and missing consent is flagged for follow up. The result is counted and recorded in the audit log.

Next, we will look at the cohort report.
