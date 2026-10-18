# 116 · Streaming a lesson video

## Welcome

A youth opens a lesson and presses play on a video that may run three hours. It should start quickly, adapt to the connection, let them jump to any minute, and play only for someone who is allowed to see that lesson. This video explains the design that does all of that without a video vendor. The screens are mocks and design diagrams, and the player captures come from the existing live player.

[pause 2s]

## Why a plain video tag fails

Today the lesson page puts the address of the file straight into a video tag. The browser fetches that address on its own, and it cannot attach the access token, because Waypoint keeps that token in memory and uses no cookies. The request would be refused. The existing end to end test hides this by blocking the file on purpose. Videos that have been through the upload pipeline need a different way in.

[pause 2s]

## Asking for permission

When a lesson with a ready video opens, the page asks the server for a playback address. That request goes to the youth curriculum controller as a normal signed in call. The server applies the same lesson access rule the rest of the lesson uses. If the youth may see the lesson and the video is ready, the server signs an address and returns it. Mentors and administrators have a preview version of the same call.

The address has four parts: a media path, a token, the media identifier, and the master playlist file. The token holds an expiry time and the media identifier, and it is signed with a secret key using HMAC. By default it lasts eight hours.

[pause 2s]

## Why the token is in the path

The master playlist lists the renditions by relative addresses, and each rendition playlist lists its segments the same way. A browser resolves a relative address against the address of the file that mentioned it. So when the token sits in the path, every playlist request and every segment request carries it automatically. One decision at the start covers the whole viewing session. The server never redirects these addresses, because a redirect would change the base the browser resolves against.

[pause 2s]

## Serving the files

The media endpoint is one route that allows anonymous calls, because the token is the proof. For each request it checks the signature and the expiry. It checks that the media identifier in the token matches the one in the address. It accepts only a short list of file shapes, and it confirms that the resolved path stays inside that media's published folder. Then it sends the file straight from disk, with support for range requests. It never touches the database. Every rejection is a plain not found, so the endpoint reveals nothing about what exists.

Playlists are cached for five minutes. Segments, the initialization file, and the poster are cached as immutable for a year, because their content never changes. Only playlists are compressed.

[pause 2s]

## The player

The existing video player gains a source input and a poster input. When a source is set, it plays on demand. Safari plays the playlist natively. Other browsers load the hls.js library only when it is needed, which keeps it out of the main bundle. The player measures throughput, picks a rendition, and switches as the connection changes.

On demand mode adds a seek bar that spans the full duration, elapsed and total time, a quality label, and arrow key seeking by five seconds. Captions, volume, fullscreen, keyboard focus, text, and theming are shared with the live player. The captures show those shared controls in both themes. The seek bar and the quality label are not in them yet. The live code path is untouched.

[pause 2s]

## Expiry and failure

If a token expires while someone is watching, the next request is refused. The player reports it, the page asks for a fresh address, and playback resumes at the same position.

A forged token, or a token for another video, gets a not found. The lesson's scripture and teaching text stay readable, and the page offers retry.

Older media with no streaming ladder keeps today's video tag and retry control. That includes documents and videos already stored under twenty five megabytes. A video that is still being prepared shows a message instead of a player.

[pause 2s]

## Things to remember

Authorize once, then serve without the database. Put the token in the path. Playlists are short lived, and segments are immutable. The player adapts, seeks anywhere, and renews on expiry.
