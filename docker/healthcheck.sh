#!/bin/bash

mkdir -p /app/healthcheck

while true; do
    curl -s -o "/app/healthcheck/default.json" "$DEFAULT_HC_URL" & \
    curl -s -o "/app/healthcheck/fallback.json" "$FALLBACK_HC_URL";
    sleep 5;
done