resource "google_pubsub_topic" "juridical-topic" {
  name = "juridical-sms-topic"

  labels = {
    foo = "sms"
  }
}

resource "google_pubsub_subscription" "juridical-subscription" {
  name  = "juridical-sms-subscription"
  topic = google_pubsub_topic.juridical-topic.name

  labels = {
    foo = "sms"
  }

  # 20 minutes
  message_retention_duration = "1200s"
  retain_acked_messages      = true

  ack_deadline_seconds = 20

  expiration_policy {
    ttl = "300000.5s"
  }
  retry_policy {
    minimum_backoff = "10s"
  }

  enable_message_ordering = true
}
