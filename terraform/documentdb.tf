# Security Group para o DocumentDB do servico de Ordem de Servico
resource "aws_security_group" "ordemservico_docdb_sg" {
  name        = "${var.db_cluster_identifier}-sg"
  description = "Security group para DocumentDB Cluster do servico de Ordem de Servico"
  vpc_id      = data.terraform_remote_state.infra.outputs.vpc_principal_id

  ingress {
    description = "Acesso DocumentDB de dentro da VPC"
    from_port   = var.db_port
    to_port     = var.db_port
    protocol    = "tcp"
    cidr_blocks = [data.terraform_remote_state.infra.outputs.vpc_principal_cidr]
  }

  ingress {
    description = "Acesso DocumentDB das subnets publicas (pods EKS)"
    from_port   = var.db_port
    to_port     = var.db_port
    protocol    = "tcp"
    cidr_blocks = data.terraform_remote_state.infra.outputs.subnet_publica_cidrs
  }

  egress {
    description = "Permitir todo trafego de saida"
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }

  tags = {
    Name = "${var.db_cluster_identifier}-sg"
  }
}

# DB Subnet Group usando as subnets publicas da infraestrutura
resource "aws_docdb_subnet_group" "ordemservico_subnet_group" {
  name       = "${var.db_cluster_identifier}-subnet-group"
  subnet_ids = data.terraform_remote_state.infra.outputs.subnet_publica_ids

  tags = {
    Name = "${var.db_cluster_identifier}-subnet-group"
  }
}

# Cluster Parameter Group para DocumentDB
resource "aws_docdb_cluster_parameter_group" "ordemservico_params" {
  family      = "docdb5.0"
  name        = "${var.db_cluster_identifier}-params"
  description = "DocumentDB cluster parameter group for Ordem de Servico"

  parameter {
    name  = "tls"
    value = var.tls_enabled ? "enabled" : "disabled"
  }

  tags = {
    Name = "${var.db_cluster_identifier}-params"
  }
}

# Cluster DocumentDB dedicado para o microsservico de Ordem de Servico
resource "aws_docdb_cluster" "ordemservico_docdb" {
  cluster_identifier              = var.db_cluster_identifier
  engine                          = "docdb"
  engine_version                  = var.docdb_engine_version
  master_username                 = var.db_master_username
  master_password                 = var.db_master_password
  port                            = var.db_port
  db_subnet_group_name            = aws_docdb_subnet_group.ordemservico_subnet_group.name
  db_cluster_parameter_group_name = aws_docdb_cluster_parameter_group.ordemservico_params.name
  vpc_security_group_ids          = [aws_security_group.ordemservico_docdb_sg.id]

  backup_retention_period      = var.backup_retention_period
  preferred_backup_window      = var.preferred_backup_window
  preferred_maintenance_window = var.preferred_maintenance_window

  skip_final_snapshot       = var.skip_final_snapshot
  final_snapshot_identifier = var.skip_final_snapshot ? null : "${var.db_cluster_identifier}-final-snapshot"

  storage_encrypted = true
  
  # Enable automated backups
  apply_immediately = true

  tags = {
    Name    = var.db_cluster_identifier
    Service = "OrdemServicoService"
  }
}

# Instancia DocumentDB (cluster member)
resource "aws_docdb_cluster_instance" "ordemservico_docdb_instance" {
  identifier         = var.db_instance_identifier
  cluster_identifier = aws_docdb_cluster.ordemservico_docdb.id
  instance_class     = var.docdb_instance_class

  tags = {
    Name    = var.db_instance_identifier
    Service = "OrdemServicoService"
  }
}
