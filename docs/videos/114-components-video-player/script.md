# 114 · The Waypoint video player

## Meet the player

This is the Waypoint live video player. It plays a live stream from a hub, right in the browser, and it is drawn with Waypoint's own design tokens, so it looks at home in both the light theme and the dark theme. This video walks through every feature, once in each theme.

[pause 2s]

## Connecting and playing live

When a stream is chosen, the player says that it is connecting. Nothing plays yet, and the player tells you so in plain words.

Once the first picture is buffered, the player goes live and starts on its own. The control bar sits under the picture. From the left: play and pause, mute, volume, a live badge, the elapsed time, and fullscreen. The player stays a few seconds behind the newest picture, so playback stays smooth.

[pause 2s]

## Pause and go back to live

Press pause, and the picture holds. A big play button appears in the middle, so nobody wonders whether the video froze. While you are paused, the stream keeps arriving, so you fall behind. The live badge becomes a button. Press it, and the player jumps straight back to the live edge. Pressing play does the same. There is no seek bar, because this player is live only.

[pause 2s]

## Sound and volume

Mute is one button, and it says whether the sound is on. When the player is wide enough, a volume slider appears beside it, moving in steps of five. On a narrow player the slider is hidden to save room, and the keyboard still works. The page that hosts the player can also set the mute and the volume, and changing them never restarts the stream.

[pause 2s]

## Captions

When the host supplies a captions file, a captions button appears. Press it, and the words show at the bottom of the picture. The button reports whether captions are on, and the player keeps your choice even if the host swaps in a new file. The player never writes captions itself.

[pause 2s]

## Keyboard and focus

Everything works from the keyboard. Tab moves through the buttons in a sensible order, and a strong ring shows where you are. Inside the player, space or K plays and pauses. M mutes. F goes fullscreen. C turns captions on and off. The up and down arrows change the volume by five. Escape leaves fullscreen. On the volume slider itself, the arrow keys keep their normal job.

[pause 2s]

## Fullscreen

Fullscreen fills the screen. After three seconds without activity, the control bar and the mouse cursor tuck away, and any movement brings them back. Escape, or the same button, returns you to the page.

[pause 2s]

## When the connection wobbles

If the player runs out of buffered picture, it shows a spinner. If nothing arrives for ten seconds, it says it is waiting for the source. After thirty seconds, it gives up and reports an error.

If the connection itself drops, the player reconnects on its own and counts the attempts. The first try is immediate, then it waits two seconds, five seconds, ten seconds, and ten seconds again. After the fifth, it reports a connection error.

If the source finishes, the player shows that the stream ended, with the title and how long it was live.

[pause 2s]

## Errors and retry

Every failure shows a plain message, and tells the host a code, so the host can decide what to do. The codes are unsupported, unauthorized, not found, connection, source, decode, and stalled. Screen readers hear the message as an alert.

Every error except unsupported offers a Retry button. Retry starts a fresh connection and asks the host for a new token. The raw server text never reaches the screen.

[pause 2s]

## Your words and your colours

All the text can be replaced. The host gives the player any strings it wants, and the rest stay in English. Here the live badge and the buttons speak French.

All the colours, the corner radius, and the motion speed come from variables that start with the player's own name, and fall back to Waypoint's design tokens. Here the host has switched the accent colour and squared off the corners, in one line.

[pause 2s]

## Light and dark

Everything you saw is the same player in both themes. The control bar, the spinner, the messages, the focus ring, and the captions all follow the theme, with no extra code. While making this video we also fixed two things. The live badge no longer shows once a stream has ended or failed, and the stories that demonstrate the player now use a real stream, so every state can be shown for real.

That is the Waypoint video player: live, accessible, translatable, and themed.
