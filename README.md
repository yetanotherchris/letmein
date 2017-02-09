# Letmein

[![Travis](https://img.shields.io/travis/yetanotherchris/letmein.svg)](https://travis-ci.org/yetanotherchris/letmein)
[![Docker Pulls](https://img.shields.io/docker/pulls/anotherchris/letmein.svg)](https://hub.docker.com/r/anotherchris/letmein/)

![](https://github.com/yetanotherchris/letmein/raw/master/src/Letmein.Web/wwwroot/images/favicon.png)

Letmein is an encrypted notes service, similar to cryptobin.co. No encryption keys or IVs are stored in the database, only the encrypted text. Notes last 12 hours with a background service cleaning up expired notes every 30 seconds (also configurable).

The service is intended to be run using Docker stack, tested on Linux. A demo is available at [www.letmein.io](http://www.letmein.io), run inside Kubernetes

### Tech stack

- .NET core 1.1
- Postgres (using [Marten](https://github.com/JasperFx/marten))
