# 107 · Running and watching Waypoint

## Welcome

Most of what keeps Waypoint dependable is something program staff never see. It is how the system is started, checked, watched and looked after. This video explains that behind-the-scenes side in plain words, and shows the few parts that you can see for yourself.

[pause 2s]

## Is it up?

Waypoint answers two simple questions about itself. The first is, are you alive? The second is, are you ready to do work? Each question has its own address that tools and technical staff can ask. When the system is running, the first answers with one word, healthy. The second does the same, but only after it has checked that the database is reachable. If the database were down, the second answer would say so, and the system would not be treated as ready.

What you see here is deliberately plain. These pages are for monitoring tools, not for people, so they say very little.

[pause 2s]

## Leaving a trail

Every request to Waypoint carries a reference number, called a correlation id. It is sent back with the response, and it is attached to the log entries written while that request is handled. So if someone reports a problem, a technician can follow that one request from start to finish. When something goes wrong, the error report in the log is deliberately redacted, so it leaves out the raw error details. The requirement is that logs never carry secrets or personal information.

[pause 2s]

## Operating the system

Some jobs are done by technical staff, outside the website, through a command line tool. It can set up and update the database, create users and invitations, manage cohorts, import youth from a spreadsheet, produce reports, and award badges. It uses the same rules and checks as the website itself, so a job done this way cannot skip validation.

When Waypoint starts in production, it also checks its own settings first. If something essential is missing, such as the database connection or the signing keys, it stops and says so, instead of starting in a broken state.

Finally, there is a maintenance mode. When it is switched on, people see a calm page that says, we'll be right back, and nothing you've done is lost. The health checks keep answering, so technical staff can still watch the system.

[pause 2s]

## Things to remember

Two health checks tell tools whether Waypoint is alive and ready. Every request has a reference number that follows it through the logs, and the logs are required to keep secrets out. A command line tool handles operational jobs with the same rules as the website. In production, Waypoint refuses to start with missing settings. And during maintenance, people see a plain, reassuring page.

Next, we will look at how Waypoint protects personal information and honours data rights.
