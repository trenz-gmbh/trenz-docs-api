# TRENZ.Docs.API

An indexing and API service for Markdown documentation.

[![.NET](https://github.com/trenz-gmbh/trenz-docs-api/actions/workflows/dotnet.yml/badge.svg)](https://github.com/trenz-gmbh/trenz-docs-api/actions/workflows/dotnet.yml)
[![Docker Image CI](https://github.com/trenz-gmbh/trenz-docs-api/actions/workflows/docker-image.yml/badge.svg)](https://github.com/trenz-gmbh/trenz-docs-api/actions/workflows/docker-image.yml)

## Building Docker Images

To build the docker image, run the following command:

```
cd TRENZ.Docs.API
docker build .
```

## Running the `docker-compose.yml`

From the root of the repository, run:

```
cd TRENZ.Docs.API
cp appsettings.example.json appsettings.local.json
docker-compose up -d
```

This will start the indexing service and the API.

Additionally, you can add a `.env` file to specify environment variables:

| Variable                 | Default                 | Description                                                                  |
|--------------------------|-------------------------|------------------------------------------------------------------------------|
| `MEILISEARCH_API_KEY`    | `masterKey`             | The API key to use for Meilisearch (no public access).                       |
| `ASPNETCORE_ENVIRONMENT` | `Production`            | The environment in which the outside facing API will run (see [Environment]) |

## Available Settings

You can place a `appsettings.local.json` file next to the `docker-compose.yml`, which will get loaded on application startup.
In there you can configure the following settings:

```json
{
  "Sources": [
    {
      "Name": "My Docs",
      "Type": "git",
      "Url": "https://github.com/corp/docs",
      "Branch": "main",
      "Path": "docs/public/",
      "Username": "username",
      "Password": "password"
    },
    {
      "Name": "My General Docs",
      "Type": "local",
      "Root": "/opt/wiki/",
      "Path": "docs/public/"
    }
  ]
}
```

- `Sources`: An array specifying sources, which will be included in the Wiki and the index. `Type` can be `git` or `local`. See above for all available fields.

[Environment]: https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/web-host?view=aspnetcore-6.0#environment

## Deployment

To deploy a wiki using TRENZ.Docs.API, head to [trenz-gmbh/trenz-docs](https://github.com/trenz-gmbh/trenz-docs#user-content-deployment).
