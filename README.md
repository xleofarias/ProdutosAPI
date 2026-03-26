# 🛒 ProdutosAPI - Ecossistema E-commerce Cloud-Native

API RESTful moderna e distribuída desenvolvida com **.NET 8**, focada em escalabilidade, mensageria e alta disponibilidade. O projeto abandonou dependências locais para demonstrar uma arquitetura **100% Cloud-Native** preparada para cenários reais de produção.

![Badge .NET](https://img.shields.io/badge/.NET-8.0-purple?style=flat&logo=dotnet)
![Badge Postgres](https://img.shields.io/badge/PostgreSQL-Render%20Cloud-336791?style=flat&logo=postgresql)
![Badge Redis](https://img.shields.io/badge/Redis-Cache-DC382D?style=flat&logo=redis)
![Badge RabbitMQ](https://img.shields.io/badge/RabbitMQ-CloudAMQP-FF6600?style=flat&logo=rabbitmq)
![Badge CI/CD](https://img.shields.io/badge/CI%2FCD-GitHub%20Actions-2088FF?style=flat&logo=github-actions)
![Badge Status](https://img.shields.io/badge/Status-Concluído-success)

---

## 📋 Sobre o Projeto
Este ecossistema gerencia um catálogo de produtos e usuários, simulando o núcleo de um e-commerce de alto tráfego. O grande diferencial deste repositório é a sua arquitetura moderna distribuída em nuvem:

* **Arquitetura Orientada a Eventos (EDA):** Integração com **RabbitMQ (CloudAMQP)** via **MassTransit** para processamento assíncrono em background, garantindo *Temporal Decoupling* e tolerância a falhas.
* **Estrutura Monorepo corporativa:** * `Produtos.API`: Web API RESTful Síncrona (Hospedada no Render).
    * `Produtos.Worker`: Background Service para processamento de filas.
    * `Produtos.Contracts`: Biblioteca de Classes (*Shared Contracts*) garantindo a Fonte Única da Verdade.
* **Performance e Cache:** Utilização de **Redis** para alta performance e alívio de carga no banco principal.
* **Infraestrutura 100% Nuvem:** Zero dependência de *localhost*. O ecossistema conecta API, Mensageria, Cache e Banco de Dados rodando inteiramente em serviços PaaS/SaaS.
* **Segurança Robusta:** Autenticação via JWT, Hashing Nativo `.NET` (PBKDF2 HMACSHA256) e gestão estrita de variáveis de ambiente.

## 🚀 Tecnologias e Serviços Cloud

* **Core:** C# (.NET 8)
* **Mensageria:** RabbitMQ via CloudAMQP & MassTransit v8
* **Banco de Dados (Relacional):** PostgreSQL (Hospedado no Render)
* **Banco de Dados (NoSQL/Cache):** Redis 
* **CI/CD & Hosting:** GitHub Actions integrado com deploy automatizado no Render

## ⚙️ Como Testar e Executar

Como a infraestrutura é nativa em nuvem, a API e os bancos de dados já estão online e operantes. 

### 1. Testando a API na Nuvem
Você pode interagir diretamente com a API em produção através do Swagger.

### 2. Executando o Worker Service Localmente
Para presenciar a mensageria em tempo real, você pode rodar apenas o Consumidor (Worker) na sua máquina. Ele se conectará à nuvem e escutará os eventos gerados pela API ao vivo.

**Pré-requisitos:**
* .NET SDK 8.0
* Solicitar as credenciais de ambiente (URI do RabbitMQ/Redis/Postgres) ao mantenedor do projeto.

**Passo a Passo:**
1. **Clone o repositório:**
   ```bash
   git clone [https://github.com/xleofarias/ProdutosAPI.git](https://github.com/xleofarias/ProdutosAPI.git)
   cd ProdutosAPI
2. **Configure os Secrets no Worker:**
   ```bash
   dotnet user-secrets set "RabbitMq:Host" "amqps://sua-uri-do-cloudamqp" --project Produtos.Worker
3. **Inicie o Consumidor:**
   ```bash
   dotnet run --project Produtos.Worker/Produtos.Worker.csproj
4. Veja a mágica: Faça um POST no Swagger (Render) para criar um produto, e observe o terminal local do Worker consumindo o evento da nuvem em milissegundos!

| Método | Endpoint | Descrição |
| ---- | -------  | --------- |
| POST | /v1/auth/login | Autenticação e geração de Token JWT |
| GET  | /api/produtos  | Lista produtos com altíssima velocidade (Redis Cache) |
| GET  | /api/produtos/{id} | Detalhes de um produto específico |
| POST  | /api/produtos | Cadastro (Dispara evento assíncrono para o CloudAMQP) |
| PUT  | /api/produtos/{id} | Atualização cadastral |
| DELETE | /api/produtos/{id} | Remoção lógica/física |


🔮 Roadmap & Evolução
[x] CRUD Completo e Autenticação JWT

[x] Migração para PostgreSQL (Cloud)

[x] Pipeline CI/CD (GitHub Actions -> Render)

[x] Integração de Cache Distribuído (Redis)

[x] Arquitetura Monorepo e Shared Contracts

[x] Mensageria Assíncrona Cloud com RabbitMQ (Produtor/Consumidor)

[ ] Paginação e Filtros Avançados

[x] Implementação de Testes Unitários (xUnit)

👨‍💻 Autor
Desenvolvido por Leonardo Farias
