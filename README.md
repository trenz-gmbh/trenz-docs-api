# Meilidown

A Markdown indexing and API service.

## Building Docker Images

To build docker images individually, run the following command:

```
docker build . -f Meilidown.Indexer/Dockerfile
```

or 

```
docker build . -f Meilidown.API/Dockerfile
```

This ensures the correct context is used.

## Running the `docker-compose.yml`

From the root of the repository, run:

```
docker-compose up -d
```

This will start the indexing service and the API.

Additionally, you can add a `.env` file to specify environment variables.
