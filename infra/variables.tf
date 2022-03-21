variable "project_id" {
  type        = string
  description = "GCP project id."
}

# variable "credentials_file" {
#   type        = string
#   description = "GCP service account credentials key JSON file."
# }

variable "service_account" {
  type        = string
  description = "GCP juridical project service account."
}

variable "region" {
  type        = string
  default     = "us-east1"
  description = "GCP project region default."
}

variable "zone" {
  type        = string
  default     = "us-east1-b"
  description = "GCP project zone default."
}
