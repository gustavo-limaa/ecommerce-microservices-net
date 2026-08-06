# 🛒 Ecommerce Microservices - Pedido API

![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)
![C#](https://img.shields.io/badge/C%23-13-239120?logo=csharp)
![MySQL](https://img.shields.io/badge/MySQL-8.0-4479A1?logo=mysql)
![xUnit](https://img.shields.io/badge/Tests-xUnit%20%26%20FluentAssertions-512BD4)

Microserviço robusto para gerenciamento de Pedidos desenvolvido com **ASP.NET Core**, seguindo as práticas de **Domain-Driven Design (DDD)**, **Clean Architecture** e engenharia orientada a testes (**TDD**).

---

## 🚀 Principais Funcionalidades

* **Order Lifecycle Management**: Gerenciamento do ciclo de vida dos pedidos (criação, busca e cancelamento de status).
* **Input Validation**: Validações de entrada nos DTOs utilizando **FluentValidation** para garantir a integridade dos contratos de requisição.
* **Domain Protections**: Regras e invariantes de negócio protegidas diretamente dentro das Entidades de Domínio (ex: impedir pedidos sem itens ou transições de status inválidas).
* **Global Error Handling**: Tratamento centralizado de exceções retornando respostas padronizadas no formato **RFC 7807 (ProblemDetails)**.
* **Resilient Testing Suite**: Suíte de testes de integração cobrindo fluxos HTTP com execução isolada e massa de dados fake.

---

## 🛠️ Tech Stack

* **Framework**: .NET 10.0
* **Persistence**: Entity Framework Core 9.0 + MySQL
* **Validations**: FluentValidation.AspNetCore
* **Testing Stack**:
  * **xUnit** (Testing Framework)
  * **FluentAssertions** (Fluent Assertions)
  * **Bogus** (Data Generation / Fakers)
  * **Microsoft.AspNetCore.Mvc.Testing** (`WebApplicationFactory`)

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

## 🧪 Arquitetura dos Testes de Integração

A suíte de testes de integração (`Ecommerce.Integration.Tests`) valida os endpoints contra um banco de dados real em ambiente de teste utilizando `WebApplicationFactory`.

### Estratégias de Teste Utilizadas:
* **Test Isolation**: Adoção do atributo `[Collection("Integration Tests")]` para garantir a execução sequencial e evitar concorrência no banco de dados.
* **Resilient Assertions**: Validação baseada em presença de IDs (`.Should().Contain(...)`) em vez de checagem global de contagem de linhas no banco.
* **Data Factories**: Geração automatizada de dados falsos com a biblioteca **Bogus**.

---

## ⚙️ Como Executar o Projeto

### Pré-requisitos
* .NET 10 SDK instalado
* Instância do MySQL rodando localmente ou via Docker

### 1. Executar as Migrations do Banco de Dados
```bash
dotnet ef database update --project Ecommerce.Pedido.Api