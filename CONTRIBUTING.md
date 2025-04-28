# Contributing

This is a monorepo, each project has a README that outlines how to get up and running.

- [TTX](https://github.com/ttxdev/TTX/tree/main/src/TTX) handles our core logic, utilized by external layers
- [Api](https://github.com/ttxdev/TTX/tree/main/src/TTX.Api) is a RESTful service that queries and handles commands into our core package.
- [Web](https://github.com/ttxdev/TTX/tree/main/src/Web) is our primary frontend for players and creators
- [StreamMonitor](https://github.com/ttxdev/TTX/tree/main/src/TTX.StreamMonitor) is responsible for listening to Twitch channels and keeping our Creator stream statuses up-to-date.
- [ValueMonitor](https://github.com/ttxdev/TTX/tree/main/src/TTX.ValueMonitor) is responsible for calculating the value of a streamer. This is an open-source version of our ValueMonitor service.
