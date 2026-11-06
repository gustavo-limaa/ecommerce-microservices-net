# 🛒 Ecommerce Microservices

Ecossistema de e-commerce construído com **.NET**, organizado em microserviços e integrado por HTTP e mensageria. Este README funciona como um mapa: apresenta a arquitetura, explica onde encontrar cada parte do código e mostra como executar o projeto.

## 🧭 Visão rápida

- **Entrada HTTP:** o API Gateway, baseado em YARP, recebe as chamadas e as encaminha aos serviços.
- **Autenticação:** a Auth API trata usuários e tokens JWT.
- **Pedidos:** a Pedido API aplica regras de negócio e persiste os pedidos.
- **Catálogo:** a API de Catálogo concentra as funcionalidades de produtos.
- **Processamento assíncrono:** a Pedido API publica eventos no RabbitMQ; o Worker de Pagamento consome os eventos.
- **Infraestrutura local:** Docker Compose inicia MySQL, RabbitMQ, APIs, Gateway e Worker.

## 🏗️ Como os componentes se conectam

```text
                         Cliente / ferramenta HTTP
                                   │
                                   ▼
                         Ecommerce.Gateway (YARP)
                        ┌──────────┼───────────┐
                        ▼          ▼           ▼
                 Auth API     Pedido API   Catálogo API
                                  │
                         Pedido criado (evento)
                                  ▼
                              RabbitMQ
                                  │
                                  ▼
                       Pagamento.Worker

          Auth API ──► MySQL ◄── Pedido API / Catálogo API
```

**Fluxo de um pedido:** o cliente chama a API de Pedidos; após o processamento do pedido, a API publica uma mensagem no RabbitMQ. O Worker de Pagamento recebe essa mensagem e executa o processamento em segundo plano. Assim, o trabalho de pagamento não precisa acontecer dentro da mesma chamada HTTP.

> O Gateway está configurado no Compose na porta `5000`. Os serviços também possuem portas diretas para desenvolvimento e diagnóstico; consulte a tabela abaixo.

## 🗺️ Mapa do repositório

```text
ecommerce-microservices-net/
├── EcommerceSolution.slnx          # Solution .NET
├── docker-compose.yml              # Infraestrutura e serviços locais
├── Ecommerce.Gateway/              # Proxy HTTP, rotas e configuração do Gateway
├── Ecommerce.Auth.Api/             # Autenticação, usuários e JWT
├── Ecommerce.Pedido.Api/           # Pedidos, domínio, persistência e eventos
├── Ecommerce.Catalogo.Api/         # Catálogo de produtos
├── Ecommerce.Pagamento.Worker/     # Consumidor assíncrono de eventos de pedido
└── tests/                           # Testes unitários e de integração
```

### Onde procurar no código

| Projeto | Responsabilidade | Pastas/arquivos úteis |
|---|---|---|
| `Ecommerce.Gateway` | Entrada HTTP e encaminhamento para os serviços | `Program.cs`, `appsettings.json`, `Extension/` |
| `Ecommerce.Auth.Api` | Autenticação, usuários e tokens | `Endpoints/`, `Dtos/`, `Entities/`, `Service/`, `Data/` |
| `Ecommerce.Pedido.Api` | Regras e ciclo de vida dos pedidos | `Controllers/`, `Application/`, `Domain/`, `Infrastructure/`, `Mensageria/` |
| `Ecommerce.Catalogo.Api` | Funcionalidades do catálogo | `Controllers/`, `Application/`, `Domain/`, `Infra/` |
| `Ecommerce.Pagamento.Worker` | Consumo de mensagens e processamento de pagamentos | `Worker.cs`, `Events/`, `Service/` |
| `tests/` | Testes unitários e de integração | Projetos de teste por serviço e `EcommerceDataTest/` |

## 🚀 Executar com Docker Compose

### Pré-requisitos

- Docker com Docker Compose
- Git

### 1. Clonar o repositório

```bash
git clone https://github.com/gustavo-limaa/ecommerce-microservices-net.git
cd ecommerce-microservices-net
```

### 2. Configurar as variáveis de ambiente

O Compose lê as configurações de um arquivo `.env` na raiz. Crie-o e defina os valores abaixo. Use uma senha e uma chave JWT próprias para o seu ambiente; não publique segredos reais no repositório.

```dotenv
MYSQL_PORT=3306
MYSQL_ROOT_PASS=escolha-uma-senha-local
MYSQL_DB_NAME=Ecommerce_Pedido_DB
AUTH_DB_NAME=Ecommerce_AuthDb
RABBITMQ_USER=guest
RABBITMQ_PASS=guest
JWT_SECRET=troque-por-uma-chave-secreta-forte
```

`AUTH_DB_NAME` tem valor padrão no Compose e pode ser omitida. `MYSQL_PORT` é a porta publicada no computador; se ela já estiver ocupada, escolha outra porta livre.

### 3. Subir os serviços

```bash
docker compose up -d --build
```

Verificar o estado dos containers:

```bash
docker compose ps
```

Para acompanhar os logs:

```bash
docker compose logs -f
```

### 4. Endereços locais

| Componente | Endereço/porta publicados |
|---|---|
| API Gateway | `http://localhost:5000` |
| Auth API (direta) | `http://localhost:5005` |
| Pedido API (direta) | `http://localhost:8080` |
| Catálogo API (direta) | `http://localhost:7208` |
| RabbitMQ Management | `http://localhost:15672` |
| MySQL | Porta definida em `MYSQL_PORT` (padrão sugerido: `3306`) |

As chamadas destinadas ao uso normal da aplicação devem passar pelo Gateway. Os endereços diretos são úteis para desenvolvimento e diagnóstico. As credenciais do painel do RabbitMQ são as definidas por `RABBITMQ_USER` e `RABBITMQ_PASS` no `.env`.

### Parar o ambiente

```bash
docker compose down
```

Para também apagar os dados persistidos em volumes locais, use `docker compose down -v` — isso remove os dados do banco e do RabbitMQ.

## 🧪 Testes

Com o SDK do .NET compatível com os projetos instalado, execute a suíte a partir da raiz:

```bash
dotnet test EcommerceSolution.slnx
```

Alguns testes de integração podem depender de infraestrutura externa/configuração própria. Consulte o projeto de testes correspondente para seus requisitos.

## 🛠️ Tecnologias e conceitos

- C# e .NET / ASP.NET Core
- YARP como API Gateway
- JWT para autenticação
- MySQL e Entity Framework Core
- RabbitMQ para comunicação assíncrona
- Docker Compose para execução local
- Organização inspirada em DDD, separação de responsabilidades e arquitetura orientada a eventos

## 🔐 Segurança

- Não coloque senhas, tokens ou chaves reais no README, no código ou em commits.
- Mantenha o `.env` local e fora do controle de versão.
- Troque qualquer segredo que tenha sido publicado anteriormente e configure os segredos por ambiente.

## 📌 Nota sobre configuração

As portas e variáveis acima refletem o `docker-compose.yml`. A execução manual pelo `dotnet run` pode exigir configurações adicionais para conexões ao MySQL, RabbitMQ e JWT; confira os arquivos de configuração de cada serviço.
