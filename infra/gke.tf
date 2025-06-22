resource "google_container_cluster" "juridical-cluster" {
  name     = "juridical-cluster"
  location = var.zone

  remove_default_node_pool = true
  initial_node_count       = 1

  deletion_protection = false
}

resource "google_container_node_pool" "juridical-node-pool" {
  name     = "juridical-node-pool"
  location = var.zone

  cluster    = google_container_cluster.juridical-cluster.name
  node_count = 1

  node_config {
    preemptible  = false
    machine_type = "e2-medium"

    # Create manually: https://github.com/yagoluiz/juridical-worker#deploy (create service account)
    service_account = var.service_account
    oauth_scopes = [
      "https://www.googleapis.com/auth/cloud-platform"
    ]
  }
}
