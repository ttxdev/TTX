# Contributing

This is a monorepo, each project has a README that outlines how to get up and running.

- [Core](https://github.com/ttxdev/TTX/tree/main/src/Core) handles our domain logic, it's clear of any technical dependencies and utilized by external layers to service our game logic.
- [Api](https://github.com/ttxdev/TTX/tree/main/src/Interface/Api) is a RESTful service that queries and handles commands into our core package.
- [Web](https://github.com/ttxdev/TTX/tree/main/src/Interface/Web) is our primary frontend for players and creators
- [Data](https://github.com/ttxdev/TTX/tree/main/src/Infrastructure/Data) our Timescale Postgres integration
- [StreamMonitor](https://github.com/ttxdev/TTX/tree/main/src/Interface/StreamMonitor) is responsible for listening to Twitch channels and keeping our Creator stream statuses up-to-date.
- [ValueMonitor](https://github.com/ttxdev/TTX.Interface.Bot) **closed-source** sentiment analysis over Twitch chat to calculate creator value.

