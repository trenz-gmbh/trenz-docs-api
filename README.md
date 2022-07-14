# Meilidown

A Markdown indexing and API service.

[![.NET](https://github.com/trenz-gmbh/meilidown/actions/workflows/dotnet.yml/badge.svg)](https://github.com/trenz-gmbh/meilidown/actions/workflows/dotnet.yml)
[![Docker Image CI](https://github.com/trenz-gmbh/meilidown/actions/workflows/docker-image.yml/badge.svg)](https://github.com/trenz-gmbh/meilidown/actions/workflows/docker-image.yml)

## Building Docker Images

To build the docker image, run the following command:

```
cd Meilidown
docker build .
```

## Running the `docker-compose.yml`

From the root of the repository, run:

```
cd Meilidown
docker-compose up -d
```

This will start the indexing service and the API.

Additionally, you can add a `.env` file to specify environment variables:

| Variable                 | Default                 | Description                                                                  |
|--------------------------|-------------------------|------------------------------------------------------------------------------|
| `MEILISEARCH_API_KEY`    | `masterKey`             | The API key to use for Meilisearch (no public access).                       |
| `ASPNETCORE_ENVIRONMENT` | `Production`            | The environment in which the outside facing API will run (see [Environment]) |

## Available Settings

You can place a `appsettings.local.json` file in next to the `docker-compose.yml` and it will get loaded on application startup.
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
    }
  ]
}
```

- `Sources`: An array specifying file sources, which will be included in the Wiki and the index

[Environment]: https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/web-host?view=aspnetcore-6.0#environment

## Deployment

To deploy a wiki using Meilidown, head to [trenz-gmbh/wikidown](https://github.com/trenz-gmbh/wikidown#deployment).
