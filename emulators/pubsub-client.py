# Example methods:
# https://github.com/googleapis/python-pubsub/blob/main/samples/snippets/publisher.py
# https://github.com/googleapis/python-pubsub/blob/main/samples/snippets/subscriber.py

import json
import sys

from google.cloud import pubsub_v1


def create_topics_and_subscriptions(project_id: str, json_config: str) -> None:
    """Create a new Pub/Sub topics and subscriptions."""

    try:
        pubsub_config = json.loads(json_config)
        for topic in pubsub_config:
            create_topic(project_id, topic["name"])
            if "subscriptions" in topic:
                for subscription in topic["subscriptions"]:
                    create_subscription(project_id, topic["name"], subscription)

    except:
        print("Failed do parse JSON Pub/Sub configuration, please verify the input:")
        print(sys.argv[2])
        raise


def create_topic(project_id: str, topic_id: str) -> None:
    """Create a new Pub/Sub topic."""

    publisher = pubsub_v1.PublisherClient()
    topic_path = publisher.topic_path(project_id, topic_id)

    topic = publisher.create_topic(request={"name": topic_path})

    print(f"Created topic: {topic.name}")


def create_subscription(project_id: str, topic_id: str, subscription_id: str) -> None:
    """Create a new pull subscription on the given topic."""

    subscriber = pubsub_v1.SubscriberClient()
    topic_path = subscriber.topic_path(project_id, topic_id)
    subscription_path = subscriber.subscription_path(project_id, subscription_id)

    with subscriber:
        subscription = subscriber.create_subscription(request={"name": subscription_path, "topic": topic_path})

    print(f"Subscription created: {subscription}")


def publish_message(project_id: str, topic_id: str, data: str) -> None:
    """Publish messages to a Pub/Sub topic."""

    publisher = pubsub_v1.PublisherClient()
    topic_path = publisher.topic_path(project_id, topic_id)

    future = publisher.publish(topic_path, data=data.encode("utf-8"))
    print(future.result())

    print(f"Published messages to {topic_path}")


if __name__ == "__main__":
    if sys.argv[1] == "create":
        create_topics_and_subscriptions(sys.argv[2], sys.argv[3])
    elif sys.argv[1] == "publish":
        publish_message(sys.argv[2], sys.argv[3], sys.argv[4])
    else:
        print("Unknown command {}".format(sys.argv[1]))
