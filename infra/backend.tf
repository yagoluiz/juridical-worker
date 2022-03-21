# To execute local comment this code
terraform {
  backend "gcs" {
    bucket = "tf-juridical"
    prefix = "terraform/state"
    # credentials = "../terraform-credentials.json"
  }
}
