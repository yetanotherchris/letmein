# Letmein

[![GHCR Latest](https://ghcr-badge.egpl.dev/yetanotherchris/letmein/latest_tag?trim=major&label=latest)](https://github.com/yetanotherchris/letmein/pkgs/container/letmein)

### What is it?

Letmein is an encrypted notes service, similar to cryptobin.co. No encryption keys are stored in the database, and the encryption is performed in the browser. Notes last 12 hours by default, but this is configurable (see below) with the option to have multiple expiry times. A background service cleans up expired notes every 5 minutes. This 5 minutes/300 second wait time is also configurable.

### Tech stack

- .NET 9
- React using Vite and Tailwind
- [Sjcl](https://github.com/bitwiseshiftleft/sjcl) Javascript library for encryption.
- Postgres (using [Marten](https://github.com/JasperFx/marten))
- Cloud storage (S3, Azure, Google) (using [CloudFileStore](https://github.com/yetanotherchris/CloudFileStore))

### Quick start

Letmein now supports 3 ways to store your pastes: local storage, cloud file store and database. It listens on port 8080 by default.

#### Step 1. Configure a storage provider

##### Local storage

Pastes can be stored inside the `storage` directory inside the container, the full path is `/app/storage`. If you want to persist the pastes, simply map this as a volume.  

You don't need to customise any environmental variables for this, it will launch using FileSystem by default, as the storage provider.

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

    docker run -d -p 8080:8080 --link postgres:postgres -e POSTGRES_CONNECTIONSTRING="host=postgres;database=letmein;password=letmein123;username=letmein" ghcr.io/yetanotherchris/letmein:latest

Now go to [http://localhost:8080](http://localhost:8080) and store some text.

### Customisations

The letmein image is fairly customisable. The various customisations can be done via environmental variables passed to the Docker container, which are uppercase by convention (except the cloud storage environmental variables above):

- `REPOSITORY_TYPE` - Where pastes are stored. Possible values: "FileSystem", "Postgres", "S3", "GoogleCloud", "AzureBlobs". Default is FileSystem.
- `POSTGRES_CONNECTIONSTRING` - The connection string to the Postgres database.
- `EXPIRY_TIMES` - A comma-separated list of minutes that pastes expire after. For example "90, 600" would be 1 hour 30 minutes, and 10 hours. The default for this setting is 720 minutes (12 hours)
- `CLEANUP_SLEEPTIME` - Number of seconds to sleep in between checking for expired pastes. The default for this setting is 300 seconds.
- `ID_TYPE` - Short url ID type. Possible values: 
  - `default (bips39-two-words)`
  - `bips39-two-words`
  - `bips39-two-words-and-number`
  - `pronounceable`
  - `short-pronounceable`
  - `random-with-pronounceable`
  - `short-mixedcase`
  - `shortcode`. 

As letmein doesn't check if a note id already exists, it's worth knowing the clash rate is about 1 in 4 million for bips39-two-words. It's 1 in 40 billion for bips39-two-words-and-number. See the UniqueIdGenerator.cs class for notes on clash rates. 