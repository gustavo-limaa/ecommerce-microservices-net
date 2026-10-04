🛒 Ecommerce Microservices - Ecosystem & API GatewayEcossistema distribuído e assíncrono de microserviços composto por API Gateway (YARP), Autenticação JWT (Auth API), Gestão de Pedidos, e Processamento de Pagamentos (Worker). 

O projeto adota princípios de Domain-Driven Design (DDD), Clean Architecture, Event-Driven Architecture (EDA) e testes de integração com banco isolado e proxies virtuais.🔄 Arquitetura do EcossistemaPlaintext  

+-----------------------------------+
                              
                               
                               |   Cliente / Scalar UI / Postman   |
                               +-----------------------------------+
                                                 |
                                                 v
                               +-----------------------------------+
                               |         Ecommerce.Gateway         |
                               |      (YARP + JWT + RateLimit)     |
                               +-----------------------------------+
                                 /               |               \
                +---------------+                |                +---------------+
                | (Auth Route)                   | (Pedidos)                      | (Catálogo)
                v                                v                                v
    +-----------------------+        +-----------------------+        +-----------------------+
    |   Ecommerce.Auth.Api  |        |  Ecommerce.Pedido.Api |        |   Catalogo.Api (Mock) |
    +-----------------------+        +-----------------------+        +-----------------------+
        |                |                       |     \
        v                v                       v      \ (Publica Evento)
    +-------+       +----------+             +-------+   +-----------------------+
    | JWT   |       | MySQL DB |             | MySQL |   |    RabbitMQ Broker    |
    | Auth  |       |  (Auth)  |             | (Ped) |   | (pedido-criado-queue) |
    +-------+       +----------+             +-------+   +-----------------------+
                                                                     |
                                                                     v
                                                         +-----------------------+
                                                         |   Pagamento.Worker    |
                                                         +-----------------------+
                                                         
🚀 Módulos e ComponentesEcommerce.Gateway (YARP): Ponto único de entrada (Reverse Proxy) responsável pelo roteamento dinâmico, validação centralizada de tokens JWT Bearer, políticas de autorização e Rate Limiting.Ecommerce.Auth.Api: Microserviço responsável pelo cadastro de usuários, autenticação, verificação de credenciais e emissão de tokens JWT 

seguros.Ecommerce.Pedido.Api: Microserviço focado no gerenciamento do ciclo de vida dos pedidos com validações de domínio e persistência em banco isolado.Ecommerce.Pagamento.Worker: Background Service assíncrono que consome mensagens da fila do RabbitMQ e simula o processamento de pagamentos.🛠️️ Tech StackCategoriaTecnologia / BibliotecaFrameworks.NET 8.0 / .NET 10.0 (ASP.NET Core & Worker Service)Reverse ProxyYARP (Yet Another Reverse Proxy)Segurança & TokenJWT (JSON Web Token) + BCrypt / ASP.NET IdentityPersistência de DadosEntity Framework Core + Pomelo MySQLMensageriaRabbitMQ (AMQP)Testes Unitários & IntegraçãoxUnit, FluentAssertions, WireMock.Net, Respawn, BogusInfraestruturaDocker, Docker Compose, MySQL 8.0🧪 Estratégia de Testes (Unitários & Integração)A suíte de testes foi projetada para garantir isolamento e alta fidelidade ao ambiente de produção:1. Testes do Gateway (Ecommerce.UnitarioTests)Mock de Serviços de Destino: Uso de WireMock.Net para simular as respostas dos microserviços de Auth, Pedidos e Catálogo em portas locais isoladas (5999, 5998, 5997).In-Memory YARP Routing: Substituição das rotas via InMemoryConfigProvider em C# para validar o redirecionamento e tratamento de erros (ex: 502 Bad Gateway quando um microserviço fica indisponível).Autorização JWT: Testes de rotas protegidas garantindo o retorno de 401 Unauthorized na ausência de tokens válidos.2. Testes da Auth API (Ecommerce.Integration.Tests)Banco de Dados de Teste Dedicado: Conexão com uma instância MySQL isolada rodando na porta 3308 (ecommerce_auth_testes_db).Limpeza Automática com Respawn: Utilização da biblioteca Respawn para resetar as tabelas entre a execução de cada teste sem recriar o schema do banco.⚙️ Configuração dos Ambientes e Credenciais de TesteCredenciais Padrão do Ambiente de TesteAs credenciais e chaves abaixo estão configuradas nas WebApplicationFactory dos testes de integração:String de Conexão MySQL (Testes - Porta 3308):PlaintextServer=127.0.0.1;Port=3308;Database=ecommerce_auth_testes_db;Uid=test_user;Pwd=test_password_123;
Chave Secreta JWT (Testes):PlaintextS3cr3t_K3y_S3cur3_T3st_Envir0nm3nt_2026!


ecommerce-microservices-net/
├── .env.example                            ← Modelo de variáveis de ambiente
├── docker-compose.yml                      ← Orquestração dos serviços (MySQL, RabbitMQ, APIs e Worker)
├── EcommerceSolution.slnx                  ← Solution principal (.NET)
├── README.md                               ← Documentação do ecossistema
│
├── Ecommerce.Auth.Api/                     ← Microserviço de Autenticação e Usuários
│   ├── Data/                               ← DbContext e mapeamentos do MySQL
│   ├── Dtos/                               ← Data Transfer Objects (Login/Registro)
│   ├── Endpoints/                          ← Minimal API Endpoints
│   ├── Entities/                           ← Entidades do domínio de Auth
│   ├── Migrations/                         ← Migrações do Entity Framework Core
│   ├── Service/                            ← Regras de negócio e geração de Tokens JWT
│   ├── Dockerfile                          ← Build do container da Auth API
│   └── Program.cs                          ← Bootstrapping e Middlewares
│
├── Ecommerce.Catalogo.Api/                 ← Microserviço de Catálogo de Produtos
│   ├── Application/                        ← Casos de uso e Handlers
│   ├── Controllers/                        ← Endpoints HTTP
│   ├── Domain/                             ← Entidades e Invariantes do Catálogo
│   ├── Infra/                              ← Repositórios e Acesso a Dados
│   ├── Mensageria/                         ← Publicadores/Consumidores de eventos
│   ├── Migrations/                         ← Migrações do EF Core
│   ├── ICatalogoAssemblyMarker.cs          ← Marker interface para Injeção de Dependência/Testes
│   └── Program.cs                          ← Bootstrapping da API de Catálogo
│
├── Ecommerce.Gateway/                      ← API Gateway Principal (YARP)
│   ├── Extension/                          ← Extensões de RateLimiting, Resiliência e JWT
│   ├── appsettings.json                    ← Configurações de rotas e clusters do YARP
│   ├── Dockerfile                          ← Build do container do Gateway
│   └── Program.cs                          ← Bootstrapping e pipeline do Proxy
│
├── Ecommerce.Pedido.Api/                   ← Microserviço de Gestão de Pedidos
│   ├── Application/                        ← Serviços da aplicação e DTOs
│   ├── Controllers/                        ← Endpoints HTTP
│   ├── Domain/                             ← Entidades e Regras de Negócio de Pedidos
│   ├── Infrastructure/                     ← Persistência de dados (EF Core)
│   ├── Mensageria/                         ← Publicação no RabbitMQ (PedidoCriadoEvent)
│   ├── Migrations/                         ← Migrações do EF Core
│   ├── DependencyInjection.cs              ← Registro de serviços em DI
│   ├── IPedidoAssemblyMarker.cs            ← Marker interface para testes
│   └── Program.cs                          ← Bootstrapping da API de Pedidos
│
├── Ecommerce.Pagamento.Worker/             ← Worker de Processamento de Pagamentos (Background Service)
│   ├── Dtos/                               ← DTOs de integração
│   ├── Events/                             ← Eventos do RabbitMQ consumidos
│   ├── Service/                            ← Processamento de pagamentos
│   ├── utility/                            ← Utilitários auxiliares
│   ├── Worker.cs                           ← Consumer da fila "pedido-criado-queue"
│   └── Program.cs                          ← Host e configurações do Worker
│
└── tests/                                  ← Suíte Completa de Testes
    ├── Ecommerce.Integration.Tests/        ← Testes de Integração HTTP (WebApplicationFactory)
    │   ├── Auth.Integration/               ← Testes de integração da Auth API
    │   ├── Catalogo.Integration/           ← Testes de integração do Catálogo
    │   ├── Pagamento.Integration/          ← Testes de integração de Pagamentos
    │   ├── Pedido.Integration/             ← Testes de integração de Pedidos
    │   └── Setup/                          ← Factories de teste (AuthWebApplicationFactory) e Respawner
    │
    ├── Ecommerce.UnitarioTests/            ← Testes Unitários e Mocks
    │   ├── Gateway.Unitario/               ← Testes do YARP Gateway com WireMock
    │   ├── Pagamento.Unitario/             ← Testes unitários do Worker
    │   └── Pedido.Unitario/                ← Testes unitários de Regras de Domínio
    │
    └── EcommerceDataTest/                  ← Projeto de dados de teste compartilhados
        └── DataFactory.cs                  ← Geradores de dados mocados usando Bogus

⚙️ Como Executar os Testes Localmente1. Subir a Infraestrutura de Teste (MySQL 3308)Certifique-se de que o container do MySQL para os testes esteja rodando na porta 3308:Bashdocker compose up -d mysql-testes

2. Executar a Suíte Completa via CLIPara rodar todos os testes de integração do Gateway e da Auth API:Bashdotnet test

3. ⚙️ Como Executar o Projeto na Sua Máquina
Existem duas formas de rodar o ecossistema: a recomendada (via Docker Compose), que já sobe todos os bancos, RabbitMQ, Gateway e Microserviços com 1 comando, ou a manual (via CLI).

📋 Pré-requisitos
Antes de começar, garante que tens instalado na tua máquina:

Docker Desktop ativo.

.NET 8.0 SDK e .NET 10.0 SDK (caso fores rodar via CLI).

🐳 Opção 1: Via Docker Compose (Recomendado - 1 Comando)
Esta é a forma mais rápida. O Docker vai subir o MySQL de Produção/Dev, o MySQL de Testes, o RabbitMQ, o API Gateway, os Microserviços e o Worker de Pagamento automaticamente.

1. Clonar o Repositório:
Bash
git clone https://github.com/seu-usuario/ecommerce-microservices-net.git
cd ecommerce-microservices-net
2. Criar o Ficheiro de Variáveis de Ambiente:
Cria um ficheiro .env na raiz do projeto baseado no .env.example:

Bash
cp .env.example .env
3. Subir Todo o Ecossistema:
Bash
docker compose up -d --build
4. Testar os Endpoints Mapeados:
Após os containers subirem, podes aceder aos serviços através das portas expostas:

🌐 API Gateway (YARP): http://localhost:5000 (Ponto de entrada principal para todas as chamadas)

🔑 Auth API (Direta): http://localhost:5001

📦 Pedido API (Direta): http://localhost:5002

🏷️ Catálogo API (Direta): http://localhost:5003

🐇 Painel do RabbitMQ: http://localhost:15672 (Login/Senha: guest / guest)

📜 Documentação Scalar/Swagger: http://localhost:5000/scalar/v1

💻 Opção 2: Execução Manual via CLI / Visual Studio
Se preferires rodar os projetos diretamente pela IDE ou Terminal para depurar o código:

1. Subir Apenas a Infraestrutura (Bancos e Mensageria):
Bash
docker compose up -d mysql-dev mysql-testes rabbitmq
2. Aplicar as Migrations nos Bancos de Dados:
Bash
# Migrations da Auth API
dotnet ef database update --project Ecommerce.Auth.Api

# Migrations da Pedido API
dotnet ef database update --project Ecommerce.Pedido.Api
3. Rodar os Microserviços (Em Terminais Separados):
Bash
# Terminal 1: Auth API
dotnet run --project Ecommerce.Auth.Api

# Terminal 2: Pedido API
dotnet run --project Ecommerce.Pedido.Api

# Terminal 3: Catálogo API
dotnet run --project Ecommerce.Catalogo.Api

# Terminal 4: Pagamento Worker
dotnet run --project Ecommerce.Pagamento.Worker

# Terminal 5: API Gateway (Subir por último)
dotnet run --project Ecommerce.Gateway
