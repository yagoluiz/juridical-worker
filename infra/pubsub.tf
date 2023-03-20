resource "google_pubsub_topic" "juridical-process-topic" {
  name = "juridical.legal-process.resulted"
}

resource "google_pubsub_subscription" "juridical-message-subscription" {
  name  = "juridical.message.sended"
  topic = google_pubsub_topic.juridical-process-topic.name

  message_retention_duration = "604800s"
  ack_deadline_seconds       = 60

  expiration_policy {
    ttl = ""
  }

  retry_policy {
    minimum_backoff = "10s"
    maximum_backoff = "600s"
  }

  enable_message_ordering = true
}
