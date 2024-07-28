## Letmein

### What is it?

Letmein is an encrypted notes service, similar to cryptobin.co. No encryption keys are stored in the database, and the encryption is performed in the browser. Notes last 12 hours by default, but this is configurable (see below) with the option to have multiple expiry times.

A background service runs in the same process as the web server (in a separate thread), cleaning up expired notes every 5 minutes. This 5 minutes/300 second wait time is also configurable.

### Quick start

Letmein supports two different ways to store your pastes: file and database. File is local or cloud based and database is Postgres.

The fastest way to run Letmein is with the defaults. This will store the pastes inside the container in the `/app/storage` directory. You can configure this is a volume if you need persistence.

```
docker run -d -p 8080:8080 anotherchris/letmein
```

There are a lot of customisation options available via environmental variables, see the [Github readme](https://github.com/yetanotherchris/letmein) for details.

#### Using S3

*Note: you can also use Azure and Google Cloud, see the Github readme for more details.*

```
docker run -d -p 8080:8080  \
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

    docker run -d -p 8080:8080 --link postgres:postgres -e POSTGRES_CONNECTIONSTRING="host=postgres;database=letmein;password=letmein123;username=letmein" anotherchris/letmein

Now go to [http://localhost:8080](http://localhost:8080) and store some text.

#### Docker compose example using Nginx as a proxy

```
version: '3'
services:
    letmein:
        ports:
            - '8080:8080'
        environment:
            - EXPIRY_TIMES=5,60,360,720
            - REPOSITORY_TYPE=S3
            - S3__AccessKey={YOUR KEY}
            - S3__SecretKey={YOUR SECRET KEY}
            - S3__BucketName={YOUR BUCKET}
            - S3__Region=eu-west-1
        container_name: letmein
        image: anotherchris/letmein
    nginx:
        ports:
            - '80:80'
            - '443:443'
        volumes:
            - '~/nginx:/etc/nginx/conf.d:ro'
        image: nginx
```

An example of the nginx.conf you would put into ~/nginx:

```
limit_req_zone $binary_remote_addr zone=one:10m rate=1r/s;

server {
    listen *:80;
    add_header Strict-Transport-Security max-age=15768000;
    return 301 https://$host$request_uri;
}

server {
    listen *:443    ssl;
    server_name     letmein.io;
    ssl_certificate /etc/nginx/conf.d/cert-bundle.pem;
    ssl_certificate_key /etc/nginx/conf.d/privatekey.pem;
    ssl_protocols TLSv1.1 TLSv1.2;
    ssl_prefer_server_ciphers on;
    ssl_ciphers "EECDH+AESGCM:EDH+AESGCM:AES256+EECDH:AES256+EDH";
    ssl_ecdh_curve secp384r1;
    ssl_session_cache shared:SSL:10m;
    ssl_session_tickets off;
    ssl_stapling on; #ensure your cert is capable
    ssl_stapling_verify on; #ensure your cert is capable

    add_header Strict-Transport-Security "max-age=63072000; includeSubdomains; preload";
    add_header X-Frame-Options DENY;
    add_header X-Content-Type-Options nosniff;
    proxy_set_header Host $host;
    proxy_set_header X-Forwarded-For $remote_addr;

    #Redirects all traffic
    location / {
        proxy_pass  http://letmein:8080;
        limit_req   zone=one burst=10 nodelay;
    }
}
```

### Help

For more information, see the [Github Repository](https://github.com/yetanotherchris/letmein)