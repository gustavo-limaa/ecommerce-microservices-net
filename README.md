# 🛒 Arquitetura de Microsserviços Orientada a Eventos para E-Commerce

Este repositório contém uma **Solução de Microsserviços Orientada a Eventos** construída com **.NET**, **RabbitMQ**, **MySQL** e **ASP.NET Core**.

O objetivo deste projeto é simular o ecossistema backend de um e-commerce real, trabalhando com estruturas complexas de dados de domínio, processamento assíncrono de eventos, isolamento de banco de dados por serviço e monitoramento com Health Checks em nível de produção.

---

## 🏗️ Arquitetura do Sistema

A solução implementa o padrão **Database per Service** (Um Banco de Dados por Serviço) e se comunica de forma assíncrona através de um Message Broker (RabbitMQ):

                  [ Cliente / Postman ]
                            │
                            ▼
     ┌──────────────────────────────────────────────┐
     │            Ecommerce.Pedido.Api              │
     │  - API REST para criação de Pedidos          │
     │  - Entidades complexas (Itens, Endereço)     │
     │  - Banco MySQL Isolado (`orders_db`)        │
     └──────────────────────┬───────────────────────┘
                            │
                            ▼
                   ┌─────────────────┐
                   │ RabbitMQ Broker │
                   │ (Fila de Event) │
                   └────────┬────────┘
                            │
                            ▼
     ┌──────────────────────────────────────────────┐
     │          Ecommerce.Pagamento.Worker          │
     │  - Background Service (Consumidor)           │
     │  - Processamento Assíncrono de Pagamento     │
     │  - Banco de Dados Isolado (`payments_db`)    │
     └──────────────────────────────────────────────┘

     ---

## 🚀 Principais Recursos e Práticas de Engenharia

* **Arquitetura Orientada a Eventos (EDA):** Comunicação assíncrona entre serviços via RabbitMQ, garantindo alta disponibilidade e resiliência.
* **Database per Service:** Desacoplamento total da camada de persistência (`orders_db` e `payments_db`).
* **Domínio com Dados Complexos:** Objetos ricos contendo coleções aninhadas (Itens do Pedido), Value Objects (Endereço de Entrega) e subtotais calculados.
* **Observabilidade & Health Checks:** Endpoints customizados (`/healthz`) que retornam JSON estruturado com o status individual do MySQL, EF Core e RabbitMQ.
* **Clean Code & SOLID:** Injeção de dependência modularizada, separação por casos de uso e middlewares globais para tratamento de exceções.

---

## 🛠️ Tecnologias Utilizadas

* **Linguagem:** C# / .NET 8+
* **Framework Web:** ASP.NET Core Web API & Worker Services (`BackgroundService`)
* **ORMs & Banco de Dados:** Entity Framework Core, MySQL
* **Mensageria:** RabbitMQ (`RabbitMQ.Client`)
* **Observabilidade:** `AspNetCore.HealthChecks`
     
