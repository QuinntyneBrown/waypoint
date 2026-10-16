# 106 · Staying fast

## Welcome

A young person on a shared Chromebook will not wait long for a page. A program director with hundreds of records should not feel the system slow down as the program grows. Waypoint sets itself clear speed targets, called performance budgets, and checks them. This video explains the targets in plain words, and shows the one place where you can see them at work.

[pause 2s]

## The targets

There are four. First, the server answers an ordinary request quickly. For half of all requests, within three hundred milliseconds. For nearly all of them, within eight hundred milliseconds. That is the time the server itself takes, and it leaves out the network.

Second, lists, boards and reports do not load everything at once. The server sends them in pages, so the amount of data stays small, however large the organisation becomes.

Third, the main screen a young person uses should become usable within about three seconds on a mid-range device, even on a slow connection, without freezing the page while it loads.

Fourth, many people can use Waypoint at once without more errors appearing.

[pause 2s]

## Pages you can see

Here is one place where the page size is easy to see. David Osei has made twenty-six privacy requests. His history does not show all twenty-six at once. It says, showing twenty-five of twenty-six requests, and it lists the first twenty-five.

At the bottom there is a button, load more requests. David presses it, and the rest arrive. Now the heading says, showing twenty-six of twenty-six requests. Each trip to the server brings a limited amount, so a long history never makes the page heavy.

[pause 2s]

## How the targets are checked

These targets are not left as hopes. The project has an automated load check that runs in continuous integration. It simulates many signed-in people reading from the server at a steady rate, and it fails if the median time, the slow tail, the error rate or the size of a response goes over its limit. A separate browser test checks that the main youth screen becomes interactive within three seconds under a throttled connection and a slower processor.

[pause 2s]

## Things to remember

Waypoint has numbers to meet. Three hundred milliseconds for the typical request, and eight hundred for nearly all of them. Long lists arrive in pages. The main youth screen should be usable within about three seconds on a modest device. And automated checks run as changes are made, so a slowdown is caught early.

Next, we will look at how Waypoint is operated and watched once it is running.
