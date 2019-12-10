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

**RabbitMQ**
- `AMQP_HOST` : RabbitMQ Instance Hostname [\*]
- `AMQP_USER` : RabbitMQ Instance Username [\*]
- `AMQP_VHOST` : RabbitMQ Instance Virtual Host [\*]
- `AMQP_PASSWORD` : RabbitMQ Instance Password [\*]

**Blob Storage**
- `BLOB_CONTAINER_REF` : Azure Blob Storage container reference (Default: `demos` )
- `BLOB_CONNECTION_STRING` : Azure Blob Storage connection string [\*]

[\*] *Required*

