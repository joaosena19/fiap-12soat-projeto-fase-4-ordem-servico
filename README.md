[![Deploy](https://github.com/joaosena19/fiap-12soat-projeto-fase-4-ordem-servico/actions/workflows/deploy.yaml/badge.svg)](https://github.com/joaosena19/fiap-12soat-projeto-fase-4-ordem-servico/actions/workflows/deploy.yaml)

# Identificação

Aluno: João Pedro Sena Dainese  
Registro FIAP: RM365182  

Turma 12SOAT - Software Architecture  
Grupo individual  
Grupo 13  

Discord: joaodainese  
Email: joaosenadainese@gmail.com  

## Sobre este Repositório

Este repositório contém apenas parte do projeto completo da Fase 4. Para visualizar a documentação completa, diagramas de arquitetura, e todos os componentes do projeto, acesse: [Documentação Completa - Fase 4](https://github.com/joaosena19/fiap-12soat-projeto-fase-4-documentacao)

## Descrição

Microsserviço de Ordem de Serviço em .NET, responsável pelo gerenciamento completo de ordens de serviço da oficina mecânica. Orquestra comunicação síncrona com os microsserviços de Cadastro e Estoque, e participa da SAGA coreografada como produtor e consumidor de eventos. Implementado com Clean Architecture, MongoDB no Amazon DocumentDB. Executado em Kubernetes (EKS) com HPA.

## Tecnologias Utilizadas

- **.NET** - Runtime e framework web
- **MongoDB Driver** - Acesso ao banco documental
- **MongoDB/Amazon DocumentDB** - Banco de dados
- **MassTransit + Amazon SQS** - Mensageria assíncrona (SAGA)
- **JWT Authentication** - Autenticação via Forward Token
- **Swagger** - Documentação API
- **Docker** - Containerização
- **Kubernetes** - Orquestração (Amazon EKS)
- **Terraform** - Provisionamento do banco de dados
- **SonarCloud** - Análise estática de qualidade

## Documentação da API

Para consultar a documentação completa dos endpoints da API, acesse: [Documentação de Endpoints - Ordem de Serviço](https://github.com/joaosena19/fiap-12soat-projeto-fase-4-documentacao/blob/main/10.%20Endpoints/1_endpoints_ordem_servico.md)
