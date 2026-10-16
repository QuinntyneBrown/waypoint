# 112 · Syncing attendance reliably

## Welcome

Church halls and community centres do not always have good internet. Attendance still has to be taken, and it must never be lost or counted twice. This video shows what Waypoint does when the connection drops while someone is taking attendance, and how it keeps the record correct once the connection comes back.

[pause 2s]

## Taking attendance offline

Sam Whitfield is a volunteer. Tonight he opens the take attendance page for his session. It lists the young people by name, with a Present and an Absent button for each. Halfway through the evening, the connection drops.

A dark bar appears across the top of the screen. It says, you're offline, changes will save when you reconnect. Sam does not need to do anything special. He keeps marking, and presses save attendance.

Waypoint cannot send the marks yet, so it keeps them on his device. The page says, saved offline, one attendance update waiting to sync. A small badge reads, one pending sync. Nothing is lost, and Sam can see exactly what is waiting.

[pause 2s]

## Coming back online

When the connection returns, Sam does not press anything. Waypoint sends the saved marks by itself. The bar disappears, and the page says, all attendance changes synced.

Each saved update carries its own unique key. If the connection flickers and Waypoint has to try again, the server recognises the key and applies that update only once. So a retry never creates a second record for the same young person.

The saved marks also belong to the person who made them. They stay private on that device, and they only sync after that same person is signed in and connected.

[pause 2s]

## When two people change the same record

Now picture Grace Adeyemi, a mentor, who marked attendance offline for the same session. While she was away from the network, someone else changed that record on the server.

When her device reconnects, Waypoint does not quietly overwrite the newer change. It tells her. The page says, a queued attendance change conflicts with a newer server edit, review it before saving again. Below, a panel asks her to choose which attendance to keep. She can apply her changes, or keep the server attendance. Either way, she decides, and no record is lost without her knowing.

[pause 2s]

## Things to remember

Attendance marked offline is kept on the device and shown as waiting to sync. It uploads by itself when the connection returns. A unique key means a retry never counts anyone twice. Saved marks stay private to the person who made them. And if a saved change clashes with a newer one, Waypoint asks, instead of guessing.

Next, we will look at how Waypoint stops two people from overwriting each other's work.
