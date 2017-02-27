# Letmein

[![Travis](https://img.shields.io/travis/yetanotherchris/letmein.svg)](https://travis-ci.org/yetanotherchris/letmein)
[![Docker Pulls](https://img.shields.io/docker/pulls/anotherchris/letmein.svg)](https://hub.docker.com/r/anotherchris/letmein/)

![](https://github.com/yetanotherchris/letmein/raw/master/src/Letmein.Web/wwwroot/images/favicon.png)

Letmein is an encrypted notes service, similar to cryptobin.co. No encryption keys are stored in the database, and the encryption is performed in the browser. Notes last 12 hours by default, but this is configurable (see below) with the option to have multiple expiry times. A background service runs in the same process as the web server (in a separate thread), cleaning up expired notes every 30 seconds. This 30 second wait time is also configurable.

The service is intended to be run using Docker stack, with Kestrel (Microsoft's cross-platform web server) and is currently tested on Linux but developed on Windows with Docker for Windows. A demo is available at [www.letmein.io](http://www.letmein.io) which runs inside Kubernetes on Google Cloud.

Running the Docker image will start the Kestrel web server with letmein running.

### Tech stack

- .NET core 1.1
- [Sjcl](https://github.com/bitwiseshiftleft/sjcl) Javascript library for encryption.
- Postgres (using [Marten](https://github.com/JasperFx/marten))

### Quick start

To run, start a Postgres container (it needs 9.5 or higher):

    docker run -d --name postgres -p 5432:5432 -e POSTGRES_USER=letmein -e POSTGRES_PASSWORD=letmein123 postgres 

And then run the Letmein container:

    docker run -d -p 8080:5000 --link postgres:postgres -e POSTGRES_CONNECTIONSTRING="host=postgres;database=letmein;password=letmein123;username=letmein" anotherchris/letmein

Now go to [http://localhost:8080](http://localhost:8080) and store some text.

### FAQ

There is a FAQ available in the application itself. You can read this on [Letmein.io][https://www.letmein.io/FAQ].

### Customisations

The letmein image is fairly customisable. The various customisations can be done via environmental variables passed to the Docker container, which are uppercase by convention:

- `POSTGRES_CONNECTIONSTRING` - The connection string to the Postgres database.
- `EXPIRY_TIMES` - A comma-seperated list of minutes that pastes expire after. For example "90, 600" would be 1 hour 30 minutes, and 10 hours. The default for this setting is 720 minutes (12 hours)
- `CLEANUP_SLEEPTIME` - Number of seconds to sleep inbetween checking for expired pastes. The default for this setting is 30 seconds.

#### UI Customisations

- `PAGE_TITLE` - The `<title>` prefix of each page.
- `HEADER_TEXT` - The header text on each page, next to the logo.
- `HEADER_SUBTEXT` - The text underneath the header.
- `FOOTER_TEXT` - Text in the footer. The version number in the footer isn't configurable.

You can also configure the applicaiton quite easily by forking the repository, and changing the views (`*.cshtml` files), and then running `docker build .` in the `src\Letmein.Web` folder.