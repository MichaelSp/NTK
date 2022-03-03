# create-svelte

Everything you need to build a Svelte project, powered by [`create-svelte`](https://github.com/sveltejs/kit/tree/master/packages/create-svelte).

## Developing

```bash
npm run dev

# or start the server and open the app in a new browser tab
npm run dev -- --open
```

## Building

To create a production version of your app:

```bash
docker build -t ntk .
```

and run

```shell
docker run --rm ntk
```

or with `docker-compose.yaml`:

```yaml
version: "3.8"

services:
  ntk:
    build:
      dockerfile: Dockerfile
    volumes:
      - $(pwd)/database.sqlite:/app/database.sqlite
```