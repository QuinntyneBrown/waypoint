# 113 · Checking what goes in and what comes out

## Welcome

Every box in Waypoint is a doorway. People type names, emails, messages and notes into it, and Waypoint later shows that text to other people. Two things must be true for this to be safe. What goes in must be checked. And what comes out must be shown as plain text, never as something that can run. This video shows both.

[pause 2s]

## Checking what goes in

Sam Whitfield opens the sign-in page. He leaves both boxes empty and presses sign in. Nothing is sent. Under each box, a short sentence says what is wrong. Enter a valid email address. Enter your password.

That is the friendly part, and it helps people fix a mistake straight away. But Waypoint does not trust it. The server checks every request again, against rules for the type of each value, its length, its range and its format. If something does not pass, the server refuses it and replies with a plain list that names each field and what is wrong with it. So a request that skips the form, and goes straight to the server, is still checked.

[pause 2s]

## Showing what comes out

Now Amara Bell is messaging her mentor. She sends a message that contains the kind of text a trickster might use to attack a page. It has bold tags, and a script tag with an alert in it. Look at the conversation. Her message appears exactly as she typed it, tags and all, as ordinary words. Nothing turns bold, and nothing runs.

That is output encoding. Waypoint treats everything people type as text to display, never as instructions for the browser. So a message, a note or a name cannot be used to plant code in someone else's screen.

[pause 2s]

## Behind the scenes

Two more protections work behind the scenes. When Waypoint looks things up in its database, it passes the values as separate parameters, and never glues them into the query text, so typed text cannot change what a query does. And files that people upload are checked for type and size, and are sent back to the browser with an instruction not to guess what kind of file they are.

[pause 2s]

## Things to remember

Every request is checked on the server, whatever the form on screen did. Mistakes come back as plain messages that name the field. Whatever people type is shown as text, never as code. Database queries use parameters. Uploaded files are checked and served safely.

Next, we will look at how all of these protections fit together.
