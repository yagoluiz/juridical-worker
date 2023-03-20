#!/bin/bash

gcloud beta emulators pubsub start --host-port=0.0.0.0:8085 &
PUBSUB_PID=$!

if [[ -z "${PUBSUB_CONFIG}" ]]; then
  echo "No PUBSUB_CONFIG supplied, no additional topics or subscriptions will be created"
else
  echo "Creating topics and subscriptions"
  python3 /root/bin/pubsub-client.py create ${PUBSUB_PROJECT_ID} "${PUBSUB_CONFIG}"

  if [ $? -eq 1 ]; then
    exit 1
  fi
fi

echo "[pubsub] Ready"
wait ${PUBSUB_PID}
