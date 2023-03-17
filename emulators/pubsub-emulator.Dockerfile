FROM google/cloud-sdk:latest

RUN pip install google-cloud-pubsub
RUN mkdir -p /root/bin

COPY start-pubsub.sh pubsub-client.py /root/bin/

RUN chmod +x /root/bin/start-pubsub.sh

EXPOSE 8085

CMD ["./root/bin/start-pubsub.sh"]
