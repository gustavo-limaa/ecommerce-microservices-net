# 🛒 Ecommerce Microservices - Pedido API & Pagamento Worker

![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)
![C%23](https://img.shields.io/badge/C%23-13-239120?logo=csharp)
![MySQL](https://img.shields.io/badge/MySQL-8.0-4479A1?logo=mysql)
![RabbitMQ](https://img.shields.io/badge/RabbitMQ-3.12-FF6600?logo=rabbitmq)
![Docker](https://img.shields.io/badge/Docker-Containers-2496ED?logo=docker)
![xUnit](https://img.shields.io/badge/Tests-xUnit%20%26%20FluentAssertions-512BD4)

Ecossistema de microserviços assíncrono e distribuído para gestão de **Pedidos** e processamento assíncrono de **Pagamentos**, construído com **ASP.NET Core .NET 10**, **Domain-Driven Design (DDD)**, **Clean Architecture**, **Event-Driven Architecture (EDA)** e engenharia orientada a testes (**TDD**).

---

## 🔄 Fluxo e Arquitetura do Sistema

```text
               +-----------------------------------+
               |   Cliente / Scalar UI / Postman   |
               +-----------------------------------+
                                 | (HTTP POST)
                                 v
               +-----------------------------------+
               |        Ecommerce.Pedido.Api       |
               +-----------------------------------+
                 /                                               / (Persistência)                  \ (Publicação de Evento)
               v                                   v
    +--------------------+              +-----------------------+
    |   MySQL Database   |              |  RabbitMQ Broker      |
    | (Ecommerce_Pedido) |              | (pedido-criado-queue) |
    +--------------------+              +-----------------------+
                                                    |
                                                    | (Consumo Assíncrono)
                                                    v
                                        +-----------------------+
                                        |  Pagamento.Worker     |
                                        | (Processa Pagamento)  |
                                        +-----------------------+
```

---

## 🚀 Principais Funcionalidades

* **Order Lifecycle Management**: Gerenciamento do ciclo de vida dos pedidos (criação, busca e cancelamento de status).
* **Event-Driven Architecture (EDA)**: Publicação de eventos assíncronos via RabbitMQ após o armazenamento do pedido para consumo em tempo real por background workers.
* **Input Validation**: Validações de entrada nos DTOs utilizando **FluentValidation** para garantir a integridade dos contratos de requisição.
* **Domain Protections**: Regras e invariantes de negócio protegidas diretamente dentro das Entidades de Domínio (ex: impedir pedidos sem itens ou transições de status inválidas).
* **Global Error Handling**: Tratamento centralizado de exceções retornando respostas padronizadas no formato **RFC 7807 (ProblemDetails)**.
* **OpenAPI & Interactive Docs**: Documentação de APIs utilizando **Scalar API Reference** (`/scalar/v1`).
* **Containerized Ecosystem**: Ambiente 100% orquestrado via **Docker Compose** com suporte a variáveis de ambiente protegidas (`.env`).

---

## 🛠️ Tech Stack & Bibliotecas

| Categoria | Tecnologia / Biblioteca |
| :--- | :--- |
| **Framework & Runtime** | .NET 10.0 (ASP.NET Core & .NET Worker Service) |
| **Linguagem** | C# 13 |
| **Persistência de Dados** | Entity Framework Core 9.0 + Pomelo MySQL |
| **Mensageria & Broker** | RabbitMQ.Client (AMQP) |
| **Documentação API** | Scalar.AspNetCore / OpenAPI 3.0 |
| **Validações** | FluentValidation.AspNetCore |
| **Orquestração / Infra** | Docker & Docker Compose |
| **Testing Stack** | xUnit, FluentAssertions, Bogus, Microsoft.AspNetCore.Mvc.Testing (`WebApplicationFactory`) |

---

## 🏛️ Hierarquia de Exceções e HTTP Status Mapping

Os erros capturados pelo `GlobalExceptionHandler` são mapeados de forma limpa para os códigos de resposta HTTP padronizados:

| Custom Exception | HTTP Status | Descrição |
| :--- | :--- | :--- |
| `BadRequestException` / `DomainException` | **400 Bad Request** | Erros de sintaxe na requisição ou violação de regras de negócio. |
| `UnauthorizedException` | **401 Unauthorized** | Falta de identificação ou Token JWT expirado/inválido. |
| `ForbiddenException` | **403 Forbidden** | Usuário autenticado, mas sem permissão/role para a ação. |
| `NotFoundException` | **404 Not Found** | O recurso ou ID do pedido solicitado não existe no banco. |
| `ConflictException` | **409 Conflict** | Conflito de estado do recurso (ex: tentar cancelar pedido já cancelado). |
| `Unhandled Exceptions` | **500 Internal Server Error** | Erros não previstos no servidor. |

---

## 📂 Estrutura do Repositório

```text
ecommerce-microservices-net/
├── .env.example                      ← Modelo de variáveis de ambiente (público)
├── .gitignore                        ← Regras de ignorados do Git (incluindo .env)
├── docker-compose.yml                ← Orquestração dos serviços (MySQL, RabbitMQ, API e Worker)
├── EcommerceSolution.slnx            ← Solution principal (.NET 10)
├── README.md                         ← Documentação do projeto
│
├── Ecommerce.Pedido.Api/             ← Microserviço de Pedidos (Web API)
│   ├── Dockerfile                    ← Build do container da API
│   ├── Program.cs                    ← Bootstrapping & Pipeline HTTP
│   ├── DependencyInjection.cs        ← Injeção de dependências modular
│   ├── Controllers/                  ← Endpoints HTTP
│   ├── Domain/                       ← Entidades, Value Objects e Invariantes de Negócio
│   ├── Infrastructure/               ← AppDbContext, Repositórios e Persistência EF Core
│   └── Mensageria/                   ← Processador e eventos RabbitMQ (PedidoCriadoEvent)
│
├── Ecommerce.Pagamento.Worker/       ← Worker de Pagamentos (Background Service)
│   ├── Dockerfile                    ← Build do container do Worker
│   ├── Program.cs                    ← Host e configurações do Worker
│   └── Worker.cs                     ← Consumer da fila "pedido-criado-queue"
│
└── tests/                            ├── Suíte de Testes
    ├── Ecommerce.Unitario.Tests/     ← Testes unitários de regras de domínio
    ├── Ecommerce.Integration.Tests/  ← Testes de integração HTTP (WebApplicationFactory)
    └── EcommerceDataTest/            ← Massas de dados e Fakers com Bogus
```

---

## 🧪 Arquitetura dos Testes de Integração

A suíte de testes de integração (`Ecommerce.Integration.Tests`) valida os endpoints contra um banco de dados real em ambiente de teste utilizando `WebApplicationFactory`.

* **Test Isolation**: Adoção do atributo `[Collection("Integration Tests")]` para garantir a execução sequencial e evitar corrida no banco de dados.
* **Resilient Assertions**: Validação baseada na presença de IDs (`.Should().Contain(...)`) em vez de checagem global de contagem de linhas no banco.
* **Data Factories**: Geração automatizada de dados falsos com a biblioteca **Bogus**.

---

## ⚙️ Como Executar o Projeto

### 🐳 Opção 1: Via Docker Compose (Recomendado - 1 Comando)

1. Crie o arquivo `.env` na raiz baseado no `.env.example`:
   ```bash
   cp .env.example .env
   ```
2. Suba todos os microsserviços e infraestrutura (MySQL + RabbitMQ + API + Worker):
   ```bash
   docker compose up --build
   ```
3. Acesse a documentação **Scalar** no navegador:  
   👉 `http://localhost:8080/scalar/v1` ou `http://localhost:8080/swagger`

---

### 💻 Opção 2: Execução Manual via CLI / Visual Studio

#### Pré-requisitos
* .NET 10 SDK instalado
* Instância do MySQL (porta `3306` ou `3308`) e RabbitMQ ativas

#### 1. Aplicar Migrations do Banco de Dados
```bash
dotnet ef database update --project Ecommerce.Pedido.Api
```

#### 2. Executar a API de Pedidos
```bash
dotnet run --project Ecommerce.Pedido.Api
```

#### 3. Executar o Worker de Pagamentos
```bash
dotnet run --project Ecommerce.Pagamento.Worker
```

#### 4. Executar a Suíte Completa de Testes
```bash
dotnet test
```
