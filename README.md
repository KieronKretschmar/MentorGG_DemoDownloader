# DemoDownloader

Download CS:GO Demos and store them on network file storage. (Azure Blob Storage)

## Single Download Operation

1. A **DownloadUrl** message is consumed from `AMPQ_DOWNLOAD_URL_QUEUE`.
2. The Demo is downloaded and stored an a Blob Container `BLOB_CONTAINER_REF`.
3. A **DemoUrl** message s produced and sent to `AMQP_DEMO_URL_QUEUE`.

## Building

```bash
  docker build -t demodownloader:<version> .
```

## Running

```bash
  docker run --name dd_container --rm demodownloader:<version>
```

## Enviroment Variables

**RabbitMQ**
- `AMQP_URI` : RabbitMQ Instance URI [\*]
- `AMQP_DOWNLOAD_URL_QUEUE` : Queue to consume Download Urls from. [\*]
- `AMQP_DEMO_URL_QUEUE` : Queue to publish Demo Urls to. [\*]

**Blob Storage**
- `BLOB_CONTAINER_REF` : Azure Blob Storage container reference (Default: `demos` )
- `BLOB_CONNECTION_STRING` : Azure Blob Storage connection string [\*]

[\*] *Required*

