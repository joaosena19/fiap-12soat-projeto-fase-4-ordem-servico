variable "aws_region" {
  description = "Região da AWS onde os recursos serão criados"
  type        = string
  default     = "us-east-1"
}

variable "project_identifier" {
  description = "Identificador único do projeto"
  type        = string
  default     = "fiap-12soat-fase4"
}

variable "db_cluster_identifier" {
  description = "Identificador do cluster DocumentDB para Ordem de Serviço"
  type        = string
  default     = "fase4-ordemservico-docdb"
}

variable "db_instance_identifier" {
  description = "Identificador da instância DocumentDB para Ordem de Serviço"
  type        = string
  default     = "fase4-ordemservico-docdb-instance"
}

variable "db_name" {
  description = "Nome do banco de dados inicial"
  type        = string
  default     = "ordemservico_db"
}

variable "db_master_username" {
  description = "Username do usuário master do banco de dados"
  type        = string
  sensitive   = true
  default     = "ordemservico_admin"
}

variable "db_master_password" {
  description = "Senha do usuário master do banco de dados DocumentDB"
  type        = string
  sensitive   = true
}

variable "db_port" {
  description = "Porta do banco de dados DocumentDB (MongoDB protocol)"
  type        = number
  default     = 27017
}

variable "backup_retention_period" {
  description = "Número de dias para reter backups automáticos"
  type        = number
  default     = 1
}

variable "preferred_backup_window" {
  description = "Janela de tempo preferida para backups (UTC)"
  type        = string
  default     = "03:00-04:00"
}

variable "preferred_maintenance_window" {
  description = "Janela de tempo preferida para manutenção (UTC)"
  type        = string
  default     = "sun:04:00-sun:05:00"
}

variable "skip_final_snapshot" {
  description = "Determina se um snapshot final deve ser criado antes da deleção"
  type        = bool
  default     = true
}

variable "docdb_engine_version" {
  description = "Versão do engine DocumentDB"
  type        = string
  default     = "5.0.0"
}

variable "docdb_instance_class" {
  description = "Classe da instância DocumentDB"
  type        = string
  default     = "db.t3.medium"
}

variable "terraform_state_bucket" {
  description = "Nome do bucket S3 onde está o state da infraestrutura"
  type        = string
  default     = "fiap-12soat-fase3-joao-dainese"
}

variable "infra_terraform_state_key" {
  description = "Chave do state do Terraform da infraestrutura"
  type        = string
  default     = "infra/terraform.tfstate"
}

variable "tls_enabled" {
  description = "Habilitar TLS para conexões ao DocumentDB"
  type        = bool
  default     = false
}
