# TTX.App

Core logic. It's important to leave technical dependencies like external 
services in the `TTX.Infrastructure` package. The exceptions are:

- Entity Framework
- Fluent Validation
- Hosting Extension


## Development Setup

It's important to us to follow Test Driven Development. Please test your code
in the `tests/TTX.App.Tests` directory.
