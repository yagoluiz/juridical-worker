variable "region" {
  type        = string
  default     = "us-east1"
  description = "GCP project region default."
}

variable "project_id" {
  type        = string
  description = "GCP project id."
}

variable "credentials_file" {
  type        = string
  description = "GCP service account credentials key JSON file."
}
