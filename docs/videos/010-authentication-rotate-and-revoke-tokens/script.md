# 010 · Keeping a session safe

## Welcome

When someone signs in to Waypoint, the program gives their browser two small digital keys, called tokens. This video explains, in plain words, how those keys are kept safe, and what you see on screen when Waypoint decides a session must end.

[pause 2s]

## Two keys

Meet Amara. She signs in and lands on her home page. Behind the scenes, Waypoint has given her browser an access token and a refresh token.

The access token is the short-lived one. It lasts about an hour, so if it were ever copied, it would soon stop working. The refresh token is the long-lived one. Its job is to quietly ask for a fresh access token, so Amara is not asked to sign in again every hour.

Each time the refresh token is used, Waypoint gives back a new one and retires the old one. That is called rotating. The program also stores refresh tokens only in scrambled form, so even a peek at the stored data would not reveal a working key.

[pause 2s]

## A copied key

Now suppose someone copies an old refresh token and tries to use it after it has already been rotated. A used token should never come back. Waypoint treats this as a warning sign. It cancels the whole family of tokens from that sign-in, and everyone holding one of them is signed out.

Amara sees a page that says, you have been signed out. It explains that, for her security, she was signed out, and that any work in progress has been saved. A button says, sign in again. She signs in, and her work is waiting for her.

[pause 2s]

## Signing out

Many young people use shared computers, so signing out has to mean something. At the bottom of the menu there is a sign out button. When Amara presses it, Waypoint cancels her refresh tokens straight away, and clears the session from the browser. She lands on the sign-in page, and a protected page cannot be opened again without signing in. Changing a password does the same thing.

[pause 2s]

## In transit

Tokens only travel over secure connections. In production, Waypoint refuses connections that are not secure, and sets security headers on its responses.

[pause 2s]

## Things to remember

The access token is short-lived. The refresh token is rotated, and stored only in scrambled form. A reused token ends the whole session family, and the person signs in again with their work saved. Signing out and changing a password cancel the refresh tokens.

Next, we will look at how each role sees only the data it should.
