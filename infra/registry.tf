resource "google_artifact_registry_repository" "juridical-registry" {
  location      = var.region
  repository_id = "juridical"
  format        = "DOCKER"
}
