FROM alpine:latest

RUN apk update && apk add --no-cache bash curl && rm -rf /var/cache/apk/*

WORKDIR /app

COPY healthcheck.sh .

RUN chmod +x healthcheck.sh

CMD ["./healthcheck.sh"]
