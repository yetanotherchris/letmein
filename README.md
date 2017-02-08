# Letmein

A .NET core, Postgres (using [Marten](https://github.com/JasperFx/marten)) encrypted notes service, similar to cryptobin.co.

No encryption keys or IVs are stored in the database, only the encrypted text. Notes last 12 hours (configurable) with a secondary CLI to destroy expired notes.

The service is intended to be run as a Docker stack. Demo available at [www.letmein.io](http://www.letmein.io).