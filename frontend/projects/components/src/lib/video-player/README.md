# w-video-player

Browser-only, standalone video player with two modes, chosen by its inputs.

- **Live** (`streamId`): the host's SignalR hub sends fragmented MP4 (H.264 video, AAC audio) with the
  MessagePack protocol; the player appends it to a `MediaSource` behind a native `<video>` element.
  No seek bar, pause holds the picture, resume jumps back to the live edge. Ported from Tessera's
  `@tessera/video-player`.
- **On demand** (`src`): plays a signed HLS master playlist with a seek bar over the full duration,
  elapsed and total time, a quality label and a poster. Native HLS where the browser has it, hls.js
  (light build, loaded lazily) elsewhere.

Setting both `src` and `streamId` is a mistake: the player enters `error` with code `configuration`
and does nothing else. Setting neither leaves it `idle`.

```html
<w-video-player
  hubUrl="https://example.org/hubs/video"
  streamId="lecture-hall-a"
  [accessTokenFactory]="token"
  [captions]="{ src: '/captions/a.vtt', srclang: 'en', label: 'English' }"
  (stateChange)="state = $event"
  (error)="report($event)"
/>
```

```html
<w-video-player
  [src]="playback.url"
  [poster]="playback.posterUrl"
  [autoplay]="false"
  (tokenExpired)="renew()"
  (error)="report($event)"
/>
```

Peer dependencies: `@microsoft/signalr`, `@microsoft/signalr-protocol-msgpack`, `hls.js`, `rxjs`.

## API

| Input                | Default        | Meaning                                                    |
| -------------------- | -------------- | ---------------------------------------------------------- |
| `hubUrl`             | `null`         | Hub endpoint, passed to the transport unchanged            |
| `streamId`           | `null`         | Stream to play; `null` stops and returns to `idle`         |
| `accessTokenFactory` | unset          | Called by the transport for every connection; never stored |
| `captions`           | unset          | One WebVTT track `{ src, srclang, label }`                 |
| `autoplay`           | `true`         | Play once the first frame is buffered                      |
| `muted`, `volume`    | `false`, `100` | Change without restarting the stream                       |
| `titleOverride`      | unset          | Replaces the descriptor title in the region name           |
| `src`                | `null`         | Signed HLS master playlist URL; selects on-demand mode     |
| `poster`             | `null`         | Image shown before the first play (on demand)              |

Outputs: `stateChange` (`idle`, `connecting`, `live`, `playing`, `buffering`, `paused`, `reconnecting`,
`ended`, `error`; also mirrored in the host's `data-state`; `playing` is the on-demand counterpart of
`live`), `error` (`code`, `message`, `cause`; codes include `configuration` for `src` plus
`streamId`), `stats` (every second while a live stream is active), `tokenExpired` (on demand only).

## On-demand mode

`HlsVodSession` owns the lifecycle against the host `<video>`. When
`canPlayType('application/vnd.apple.mpegurl')` is non-empty the element's `src` is set directly
(native HLS, no quality label); otherwise hls.js is created through the `HLS_FACTORY` token, whose
default imports `hls.js/dist/hls.light.mjs` on demand with `{ enableWorker: false }` and adaptive
bitrate on. Provide `HLS_FACTORY` (`() => Promise<HlsLike>`) to replace it, for example in tests.

Video events drive the state: `loadstart` `connecting`, `waiting` `buffering`, `playing` `playing`,
`pause` `paused`, `ended` `ended`, `error` `error`. A refused first playlist maps to `unauthorized`
(401, 403) or `not-found` (404); other fatal failures map to `source` or `decode`; a browser without
Media Source Extensions maps to `unsupported`.

**Renewing an expired link.** A 401, 403 or 404 on a playlist or segment request after playback has
started (for native HLS, a network `error` after metadata loaded) emits `tokenExpired` once and shows a
"Renewing your video link" status. The host replies by setting a new `src`; the player reloads it and
returns to the previous position instead of restarting at 0.

The controls add a seek range (`aria-valuetext` such as "23 minutes 41 seconds of 1 hour 12 minutes"),
elapsed and total time and, under hls.js, the active level height such as `720p`. The LIVE badge is
hidden.

## Transport

Default: `SignalRVideoStreamTransport` (hub methods `Describe(streamId)` and `Subscribe(streamId)`,
integer-keyed MessagePack; chunk `{ kind: 0 | 1, seq, data }`). Provide `VIDEO_STREAM_TRANSPORT` to
replace it. A rejection with `statusCode` 401 or 403 maps to `unauthorized`.

## Host security

- Content Security Policy needs `media-src blob:` and `connect-src` for the hub origin (live).
- On-demand playback needs `media-src 'self' blob:` (hls.js attaches a `MediaSource` object URL;
  native HLS loads from the same origin), `connect-src 'self'` for playlists and segments, and
  `img-src 'self'` for the `poster`. hls.js runs without a worker, so no `worker-src` entry is needed.
- The signed `src` carries its token in the URL path: do not log it.
- SignalR's WebSocket transport sends the token as the `access_token` query parameter: use TLS
  everywhere except `localhost`, and do not log query strings.
- Descriptor fields are untrusted and rendered as text only.

## Keyboard

With focus inside the player: Space or K play/pause, M mute, F fullscreen, C captions, Arrow Up/Down
volume by 5, Escape exits fullscreen. On demand, Arrow Left and Right seek by 5 seconds. Letters ignore Ctrl, Alt and Meta.

## Text and theming

Provide `VIDEO_PLAYER_I18N` with any subset of `VideoPlayerStrings`; the rest stay English
(`DEFAULT_VIDEO_PLAYER_STRINGS`).

Each colour, radius and duration resolves from a `--w-video-player-*` override, then the design-system
token: `scrim` (`--color-bg-surface`), `control-fg` (`--color-fg-default`), `control-bg-hover`
(`--color-bg-subtle-hover`), `accent` / `accent-fg` (`--color-brand` / `--color-fg-on-brand`), `live` and
`error` (`--color-danger-solid`), `error-fg`, `focus-ring` (`--color-focus-ring`), `skeleton`
(`--color-bg-surface-sunken`), `motion-duration` (`--duration-base`), `radius` (`--radius-lg`),
`caption-bg`, `caption-fg`.
