## Letmein

**New in version 2**
Letmein now supports S3, Azure Blobs, Google Cloud Buckets to store your pastes in, as well Postgres.

### What is it?

Letmein is an encrypted notes service, similar to cryptobin.co. No encryption keys are stored in the database, and the encryption is performed in the browser. Notes last 12 hours by default, but this is configurable (see below) with the option to have multiple expiry times.

A background service runs in the same process as the web server (in a separate thread), cleaning up expired notes every 5 minutes. This 5 minutes/300 second wait time is also configurable.

### Quick start

Letmein supports two different ways to store your pastes: file and database. File is cloud based, database is Postgres.

#### Using S3

*Note: you can also use Azure and Google Cloud, see the readme for more details. Local filesystem/mounted volumes is coming later.*

```
docker run -d -p 80:5000  \
 -e EXPIRY_TIMES=5,60,360,720 \
 -e REPOSITORY_TYPE=S3 \
 -e S3__AccessKey={YOUR-KEY} \
 -e S3__SecretKey={YOUR-OTHER-KEY} \
 -e S3__BucketName={S3-BUCKET} \
 -e S3__Region=eu-west-1 \
 anotherchris/letmein
```

#### Using Postgres

Start a Postgres container (it needs 9.5 or higher):

    docker run -d --name postgres -p 5432:5432 -e POSTGRES_USER=letmein -e POSTGRES_PASSWORD=letmein123 postgres 

Run the Letmein Docker container:

    docker run -d -p 8080:5000 --link postgres:postgres -e POSTGRES_CONNECTIONSTRING="host=postgres;database=letmein;password=letmein123;username=letmein" anotherchris/letmein

Now go to [http://localhost:8080](http://localhost:8080) and store some text.

### Help

For more information, see the [Github Repository](https://github.com/yetanotherchris/letmein)