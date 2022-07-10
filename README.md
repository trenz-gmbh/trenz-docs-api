# Meilidown

A Markdown indexing and API service.

## Building Docker Images

To build the docker image, run the following command:

```
docker build . -f Meilidown.API/Dockerfile
```

## Running the `docker-compose.yml`

From the root of the repository, run:

```
docker-compose up -d
```

This will start the indexing service and the API.

Additionally, you can add a `.env` file to specify environment variables:

| Variable                 | Default                 | Description                                                                  |
|--------------------------|-------------------------|------------------------------------------------------------------------------|
| `MEILISEARCH_API_KEY`    | `masterKey`             | The API key to use for Meilisearch (no public access).                       |
| `ASPNETCORE_ENVIRONMENT` | `Production`            | The environment in which the outside facing API will run (see [Environment]) |
| `ASPNETCORE_URLS`        | `http://localhost:5000` | The URL(s) of the outside facing API (see [Server URLs])                     |

## Available Settings

You can place a `appsettings.local.json` file in next to the `docker-compose.yml` and it will get loaded on application startup.
In there you can configure the following settings:

```json
{
  "Sources": [
    {
      "Name": "My Docs",
      "Url": "https://github.com/corp/docs",
      "Branch": "main",
      "Path": "docs/public/",
      "Username": "username",
      "Password": "password"
    }
  ],
  "ApiHost": "https://localhost:7262/api",
  "FrontendHost": "https://localhost:8080"
}
```

- `Sources`: An array specifying git repositories, which will be included in the Wiki and the index
- `ApiHost`: The endpoint for image links in the markdown sources. All links to images will be replaced like this:  
  `![image](../path/image.png)` => `![image](https://localhost:7262/api/../path/image.png)`
- `FrontendHost`: This hostname is used to configure CORS and limits the allowed origins from which API requests are allowed.

[Environment]: https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/web-host?view=aspnetcore-6.0#environment
[Server URLs]: https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/web-host?view=aspnetcore-6.0#server-urls