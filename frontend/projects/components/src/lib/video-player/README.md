# w-video-player

Browser-only, standalone live video player. The host's SignalR hub sends fragmented MP4 (H.264 video,
AAC audio) with the MessagePack protocol; the player appends it to a `MediaSource` behind a native
`<video>` element and owns a slim, accessible control bar. Live only: no seek bar, pause holds the
picture, resume jumps back to the live edge. Ported from Tessera's `@tessera/video-player`.

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

Peer dependencies: `@microsoft/signalr`, `@microsoft/signalr-protocol-msgpack`, `rxjs`.

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

Outputs: `stateChange` (`idle`, `connecting`, `live`, `buffering`, `paused`, `reconnecting`, `ended`,
`error`; also mirrored in the host's `data-state`), `error` (`code`, `message`, `cause`), `stats`
(every second while active).

## Transport

Default: `SignalRVideoStreamTransport` (hub methods `Describe(streamId)` and `Subscribe(streamId)`,
integer-keyed MessagePack; chunk `{ kind: 0 | 1, seq, data }`). Provide `VIDEO_STREAM_TRANSPORT` to
replace it. A rejection with `statusCode` 401 or 403 maps to `unauthorized`.

## Host security

- Content Security Policy needs `media-src blob:` and `connect-src` for the hub origin.
- SignalR's WebSocket transport sends the token as the `access_token` query parameter: use TLS
  everywhere except `localhost`, and do not log query strings.
- Descriptor fields are untrusted and rendered as text only.

## Keyboard

With focus inside the player: Space or K play/pause, M mute, F fullscreen, C captions, Arrow Up/Down
volume by 5, Escape exits fullscreen. Letters ignore Ctrl, Alt and Meta.

## Text and theming

Provide `VIDEO_PLAYER_I18N` with any subset of `VideoPlayerStrings`; the rest stay English
(`DEFAULT_VIDEO_PLAYER_STRINGS`).

Each colour, radius and duration resolves from a `--w-video-player-*` override, then the design-system
token: `scrim` (`--color-bg-surface`), `control-fg` (`--color-fg-default`), `control-bg-hover`
(`--color-bg-subtle-hover`), `accent` / `accent-fg` (`--color-brand` / `--color-fg-on-brand`), `live` and
`error` (`--color-danger-solid`), `error-fg`, `focus-ring` (`--color-focus-ring`), `skeleton`
(`--color-bg-surface-sunken`), `motion-duration` (`--duration-base`), `radius` (`--radius-lg`),
`caption-bg`, `caption-fg`.
