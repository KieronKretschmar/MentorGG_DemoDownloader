# DemoDownloader

Download CS:GO Demos and store them on network file storage.

## Building

```bash
  docker build -t demodownloader:<version> .
```

## Running

```bash
  docker run --name dd_container --rm demodownloader:<version>
```

## Enviroment Variables

- `DEMO_DIRECTORY` : Internal location where to store the Demo files, once downloaded.
