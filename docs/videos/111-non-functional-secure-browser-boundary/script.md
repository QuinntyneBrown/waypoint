# 111 · Securing the browser boundary

## Welcome

Waypoint runs in a web browser, and a web browser talks to many websites at once. Some of those websites are not friendly. This video explains, in plain words, the protections that sit between a person's browser and Waypoint. There is almost nothing to see on screen, because good protection works quietly.

[pause 2s]

## Who may talk to Waypoint

When Amara Bell signs in, her browser holds a sign-in token, and the Waypoint page attaches that token to each request itself. It is not a cookie that the browser sends along automatically. So a different website cannot trick her browser into acting as her. A forged request from elsewhere simply arrives without the token, and is turned away.

On top of that, Waypoint's server only accepts browser requests from the web addresses it has been told to trust. Those approved addresses are a setting, and a production system is meant to list only its own. Anything else is refused.

[pause 2s]

## Safety labels on every answer

Every answer the server sends comes with a set of safety instructions for the browser. They tell the browser four things. Do not let this be shown inside another site's frame, so nobody can disguise Waypoint as part of a different page. Do not guess what kind of file this is. Reveal as little as possible about where the visitor came from. And always use a secure connection for this site.

A content security policy also tells the browser not to run anything the server did not ask for. These instructions travel with every response, including error responses.

[pause 2s]

## Slowing down guessers

The pages that matter most to attackers are sign-in, password reset and invitations. Waypoint limits how often those can be called from one address. By default, that is twenty requests in a minute, and it is a setting that can be changed. Go over the limit, and the server answers, too many requests, please try again later. A real person typing a password a few times is very unlikely to be affected. A program trying thousands of guesses is.

This works together with the account lock you saw in the sign-in video. The lock protects a single account. The rate limit protects the whole door.

[pause 2s]

## Things to remember

The sign-in token is attached by the page, not by a cookie, so other websites cannot borrow it. Only trusted web addresses may talk to the server. Every response carries safety instructions for the browser. And the busiest doors, sign-in, reset and invitations, are rate limited.

Next, we will look at how Waypoint checks everything people type, and everything it shows back.
