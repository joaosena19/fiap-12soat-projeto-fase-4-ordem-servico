output "docdb_cluster_id" {
  description = "ID do cluster DocumentDB do Ordem de Servico"
  value       = aws_docdb_cluster.ordemservico_docdb.id
}

output "docdb_cluster_endpoint" {
  description = "Endpoint do cluster DocumentDB do Ordem de Servico"
  value       = aws_docdb_cluster.ordemservico_docdb.endpoint
}

output "docdb_cluster_reader_endpoint" {
  description = "Endpoint de leitura do cluster DocumentDB do Ordem de Servico"
  value       = aws_docdb_cluster.ordemservico_docdb.reader_endpoint
}

output "docdb_cluster_port" {
  description = "Porta do cluster DocumentDB do Ordem de Servico"
  value       = aws_docdb_cluster.ordemservico_docdb.port
}

output "docdb_instance_id" {
  description = "ID da instancia DocumentDB do Ordem de Servico"
  value       = aws_docdb_cluster_instance.ordemservico_docdb_instance.id
}

output "docdb_instance_endpoint" {
  description = "Endpoint da instancia DocumentDB do Ordem de Servico"
  value       = aws_docdb_cluster_instance.ordemservico_docdb_instance.endpoint
}

output "db_name" {
  description = "Nome do banco de dados do Ordem de Servico"
  value       = var.db_name
}

output "db_username" {
  description = "Username master do banco de dados do Ordem de Servico"
  value       = aws_docdb_cluster.ordemservico_docdb.master_username
  sensitive   = true
}

output "db_security_group_id" {
  description = "ID do Security Group do DocumentDB do Ordem de Servico"
  value       = aws_security_group.ordemservico_docdb_sg.id
}

output "db_subnet_group_name" {
  description = "Nome do DB Subnet Group do Ordem de Servico"
  value       = aws_docdb_subnet_group.ordemservico_subnet_group.name
}

# Connection string para uso nos ConfigMaps/Secrets do K8s
output "db_connection_string" {
  description = "Connection string do banco de dados do Ordem de Servico (sem senha)"
  value       = "mongodb://${aws_docdb_cluster.ordemservico_docdb.master_username}@${aws_docdb_cluster.ordemservico_docdb.endpoint}:${aws_docdb_cluster.ordemservico_docdb.port}/${var.db_name}?retryWrites=false"
  sensitive   = true
}

# Connection string completa com senha (para uso em secrets)
output "db_connection_string_with_password" {
  description = "Connection string completa do banco de dados do Ordem de Servico"
  value       = "mongodb://${aws_docdb_cluster.ordemservico_docdb.master_username}:${var.db_master_password}@${aws_docdb_cluster.ordemservico_docdb.endpoint}:${aws_docdb_cluster.ordemservico_docdb.port}/${var.db_name}?retryWrites=false"
  sensitive   = true
}
