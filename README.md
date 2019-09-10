# Letmein

[![Travis](https://img.shields.io/travis/yetanotherchris/letmein.svg)](https://travis-ci.org/yetanotherchris/letmein)
[![Docker Pulls](https://img.shields.io/docker/pulls/anotherchris/letmein.svg)](https://hub.docker.com/r/anotherchris/letmein/)

![](https://github.com/yetanotherchris/letmein/raw/master/src/Letmein.Web/wwwroot/images/favicon.png)

### What is it?

Letmein is an encrypted notes service, similar to cryptobin.co. No encryption keys are stored in the database, and the encryption is performed in the browser. Notes last 12 hours by default, but this is configurable (see below) with the option to have multiple expiry times. A background service runs in the same process as the web server (in a separate thread), cleaning up expired notes every 5 minutes. This 5 minutes/300 second wait time is also configurable.

The service is intended to be run using Docker stack, with Kestrel (Microsoft's cross-platform web server) and is currently tested on Linux but developed on Windows with Docker for Windows. 

Running the Docker image will start the Kestrel web server with letmein running.

### Tech stack

- .NET core 2.1
- [Sjcl](https://github.com/bitwiseshiftleft/sjcl) Javascript library for encryption.
- Postgres (using [Marten](https://github.com/JasperFx/marten))
- Cloud storage (S3, Azure, Google) (using [CloudFileStore](https://github.com/yetanotherchris/CloudFileStore))

### Quick start

Letmein now supports two different ways to store your pastes: file and database. File is cloud based, database is Postgres.

#### Step 1. Configure a storage provider

##### S3
Add the following to your environmental variables

```
REPOSITORY_TYPE=S3
S3__AccessKey=key
S3__BucketName=bucket
S3__Region=for example, eu-west-1
S3__SecretKey=secret
```

##### Google Cloud Storage
Add the following to your environmental variables

```
REPOSITORY_TYPE=GoogleCloud
GoogleCloud__BucketName=name of your bucket
GoogleCloud__type=<COPY FROM YOUR SERVICE ACCOUNT JSON FILE>
GoogleCloud__project_id=<COPY FROM YOUR SERVICE ACCOUNT JSON FILE>
GoogleCloud__private_key_id=<COPY FROM YOUR SERVICE ACCOUNT JSON FILE>
GoogleCloud__private_key=<COPY FROM YOUR SERVICE ACCOUNT JSON FILE>
GoogleCloud__client_email=<COPY FROM YOUR SERVICE ACCOUNT JSON FILE>
GoogleCloud__client_id=<COPY FROM YOUR SERVICE ACCOUNT JSON FILE>
GoogleCloud__auth_uri=<COPY FROM YOUR SERVICE ACCOUNT JSON FILE>
GoogleCloud__token_uri=<COPY FROM YOUR SERVICE ACCOUNT JSON FILE>
GoogleCloud__auth_provider_x509_cert_url=<COPY FROM YOUR SERVICE ACCOUNT JSON FILE>
GoogleCloud__client_x509_cert_url=<COPY FROM YOUR SERVICE ACCOUNT JSON FILE>
```

##### Azure Blobs
Add the following to your environmental variables

```
REPOSITORY_TYPE=AzureBlobs
Azure__ContainerName=container name
Azure__ConnectionString=get this from the Azure portal
```

##### Postgres

If you don't want to buy Postgres hosting (such as AWS RDS), you can run Postgres as a Docker container. The example below assumes the Postgres container is inside the same Docker network as your Letmein Docker container.

Start a Postgres container (it needs 9.5 or higher):

    docker run -d --name postgres -p 5432:5432 -e POSTGRES_USER=letmein -e POSTGRES_PASSWORD=letmein123 postgres 

#### Step 2. Run the Letmein Docker container

Run the Letmein Docker container (below assumes you're using Postgres):

    docker run -d -p 8080:5000 --link postgres:postgres -e POSTGRES_CONNECTIONSTRING="host=postgres;database=letmein;password=letmein123;username=letmein" anotherchris/letmein

Now go to [http://localhost:8080](http://localhost:8080) and store some text.

### FAQ

There is a FAQ available in the application itself. You can read this on [Letmein.io][https://www.letmein.io/FAQ].

### Customisations

The letmein image is fairly customisable. The various customisations can be done via environmental variables passed to the Docker container, which are uppercase by convention (except the cloud storage environmental variables above):

- `REPOSITORY_TYPE` - Where pastes are stored. Possible values:  "Postgres", "S3", "GoogleCloud", "AzureBlobs". Default is Postgres.
- `POSTGRES_CONNECTIONSTRING` - The connection string to the Postgres database.
- `EXPIRY_TIMES` - A comma-seperated list of minutes that pastes expire after. For example "90, 600" would be 1 hour 30 minutes, and 10 hours. The default for this setting is 720 minutes (12 hours)
- `CLEANUP_SLEEPTIME` - Number of seconds to sleep inbetween checking for expired pastes. The default for this setting is 300 seconds.
- `ID_TYPE` - Short url ID type. Possible values: `default (random-with-pronounceable)`, `pronounceable`, `short-pronounceable`, `short-mixedcase`, `shortcode`. See below for notes on clash rates.

#### UI Customisations

- `PAGE_TITLE` - The `<title>` prefix of each page.
- `HEADER_TEXT` - The header text on each page, next to the logo.
- `HEADER_SUBTEXT` - The text underneath the header.
- `FOOTER_TEXT` - Text in the footer. The version number in the footer isn't configurable.

You can also configure the applicaiton quite easily by forking the repository, and changing the views (`*.cshtml` files), and then running `docker build .` in the `src\Letmein.Web` folder.

##### ID_TYPE clash rates

The various types of short url types you configure change the chances you'll receive a clash between ids. Letmein doesn't check if an url already exists, so you can receive clashes if you use the short ids.

- `random-with-pronounceable` - 4 random characters  (a-z, A-Z, 0-9), and a pronounceable password (a non-dictionary word). 
  - `P = (1/64) * (1/64) * (1/64) * (1/64) * (1/500)`
- `pronounceable` - 8 character pronounceable password (a non-dictionary word).
  - `P = (1/500)`
- `short-pronounceable` - 5 character pronounceable password (a non-dictionary word).
  - `P = (1/300)`
- `short-mixedcase` - 4 random characters (a-z, A-Z, 0-9).
  - `P = (1/64) * (1/64) * (1/64) * (1/64)`
- `shortcode` - 2 numbers, 2 characters and 2 numbers. First two numbers are the 2 from the current time's millseconds, characters are 2 uppercase, 2 digits from the current time's seconds.
  - `P = (1/1000) * (1/26) * (1/26) * (1/60)`

These are rough estimates of probability (particularly the pronounceable passwords).
