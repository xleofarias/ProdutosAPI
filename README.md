# 🛒 ProdutosAPI

Catálogo de produtos em **.NET 10** com arquitetura orientada a eventos, migrado de um stack multi-provedor (Render + CloudAMQP) para **Azure**, com cache gerenciado colocalizado em São Paulo. O projeto é um laboratório prático de nuvem e de tuning de dados.

![.NET](https://img.shields.io/badge/.NET-10.0-purple?style=flat&logo=dotnet)
![Azure SQL](https://img.shields.io/badge/Azure%20SQL-Serverless-0078D4?style=flat&logo=microsoftsqlserver)
![Container Apps](https://img.shields.io/badge/Azure-Container%20Apps-0078D4?style=flat&logo=microsoftazure)
![Service Bus](https://img.shields.io/badge/Azure-Service%20Bus-0078D4?style=flat&logo=microsoftazure)
![Redis](https://img.shields.io/badge/Redis%20Cloud-sa--east--1-DC382D?style=flat&logo=redis)
![Docker Hub](https://img.shields.io/badge/Docker%20Hub-Registry-2496ED?style=flat&logo=docker)
![CI/CD](https://img.shields.io/badge/CI%2FCD-GitHub%20Actions%20%2B%20OIDC-2088FF?style=flat&logo=github-actions)

---

## Sobre

API REST de catálogo com CRUD, autenticação JWT, cache distribuído e processamento assíncrono de eventos. O que o projeto tem de interessante não é o CRUD — é a **migração de nuvem** e as decisões de custo e latência por trás dela.

| Projeto | Papel |
| --- | --- |
| `Produtos.ProdutosAPI` | Web API REST síncrona |
| `Produtos.WorkerService` | Background service que consome a fila |
| `Produtos.Contracts` | Contratos compartilhados entre API e Worker |
| `Produtos.ProdutosAPITests` | Testes unitários (xUnit + Moq) |

O `Contracts` existe para que API e Worker compartilhem a definição das mensagens sem uma depender da outra — publisher e consumer evoluem separados.

```
  POST /api/produtos
         │
         ▼
   ┌──────────┐   grava    ┌──────────────┐
   │   API    │ ─────────► │  Azure SQL   │
   │(Container│            │ (serverless) │
   │   Apps)  │            │ Brazil South │
   └────┬─────┘            └──────────────┘
        │ publica evento
        ▼
  ┌──────────────┐  consome  ┌────────────┐
  │ Service Bus  │ ────────► │   Worker   │
  └──────────────┘           └────────────┘

   GET /api/produtos ──► Redis Cloud (São Paulo) ──► Azure SQL no miss
                         cache-aside, TTL 2 min
```

Logging estruturado com Serilog. Autenticação JWT com hashing PBKDF2 (HMAC-SHA256) nativo do .NET.

## Migração: Render → Azure

O projeto rodava fora do Azure. Cada peça foi mapeada para o serviço gerenciado equivalente:

| Componente | Antes | Depois | Por quê |
| --- | --- | --- | --- |
| Banco relacional | PostgreSQL (Render) | **Azure SQL Database** (free offer, serverless) | O Postgres gratuito do Render expira em 30 dias. Consolidei o banco na mesma nuvem da aplicação e aproveitei para alinhar o projeto com T-SQL |
| Hospedagem | Render | **Azure Container Apps** | Já estava dockerizado; escala a zero |
| Mensageria | RabbitMQ (CloudAMQP) | **Azure Service Bus** | MassTransit suporta como *transport* — troca em configuração, o código de publisher/consumer não muda |
| Cache | Redis (desativado durante a transição) | **Redis Cloud** (AWS `sa-east-1`, São Paulo) | Colocalizado com a aplicação — ver decisões abaixo |
| CI/CD | GitHub Actions → Render | **GitHub Actions → Azure** (login via OIDC) | Sem credencial de longa duração no repositório |

## Decisões de engenharia

**Trocar a implementação de cache nunca tocou a regra de negócio.** Ao longo da migração, a implementação de `IDistributedCache` mudou duas vezes — Redis, depois cache em memória durante a transição, e Redis novamente ao final. `ProductService` e `UserService` não sofreram uma única alteração em nenhuma das trocas: o código sempre dependeu da interface, não da implementação. Vale o registro de que `AddDistributedMemoryCache()` implementa `IDistributedCache` mas guarda tudo na memória do processo — serve para desenvolvimento, mas não sobrevive a escala horizontal, onde cada réplica teria seu próprio cache e um `POST` invalidaria apenas o da réplica que o atendeu.

**Azure Managed Redis foi avaliado e descartado — por latência, não só por preço.** O Azure não tem tier gratuito de Redis e o menor SKU sai em torno de US$ 11–16/mês. Mas o argumento decisivo foi outro: não havia região no Brasil disponível. A round-trip Brazil South → East US é de **120 ms** (tabela oficial da Microsoft). Com o banco na mesma região da aplicação (~10 ms), um cache nos EUA tornaria o *hit* **12x mais lento que a consulta que ele deveria evitar** — e o *miss* custaria ~250 ms. Um cache só faz sentido se for mais rápido que a fonte que ele protege. Optei por Redis Cloud sobre AWS `sa-east-1`: mesma cidade da aplicação, endpoint público, latência de poucos milissegundos.

**O free tier do Redis Cloud não oferece TLS.** O tráfego de cache trafega em texto puro. Aceitável aqui — o catálogo é sintético e o ambiente é de portfólio — e resolvido com o tier pago em qualquer cenário real. Registrado por decisão, não por descuido.

**Governança de custo antes de escalar.** O free offer do Azure SQL dá 100.000 vCore-segundos e 32 GB por mês. Três camadas: o *behavior* do free limit em auto-pause (o freio real — budget **alerta**, não bloqueia), um budget na assinatura com alertas *forecasted* e *actual*, e um alerta de métrica em `Free amount remaining` abaixo de 10% da cota.

**Benchmark roda local, não na nuvem.** Medir p95 contra o Azure SQL serverless misturaria três variáveis: a query, o *resume* do auto-pause e a latência até a região. Os números de performance são colhidos contra SQL Server em container local. A nuvem prova que está no ar; o local prova o número.

**O limite do cache atual já está calculado.** Com 400 produtos, o Redis consome ~340 bytes por produto (descontado o overhead de ~2 MB da própria instância). Contra o teto de 30 MB do tier gratuito, a estratégia atual — todo o catálogo serializado sob uma única chave — satura em torno de **85 mil produtos**. Isso não é limitação do Redis: é consequência de `GET /api/produtos` não ter paginação. O mesmo defeito pressiona o cache, a memória da aplicação e o tempo de resposta. É o próximo item do roadmap, e a previsão será confrontada com a medição.

## Rodando localmente

**Pré-requisitos:** .NET SDK 10.0 e Docker.

```bash
git clone https://github.com/xleofarias/ProdutosAPI.git
cd ProdutosAPI
```

Suba as dependências (ainda não há `docker-compose.yml` — ver roadmap):

```bash
docker run -d -p 1433:1433 --name produtos-sql \
  -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=SuaSenhaForte123!" \
  mcr.microsoft.com/mssql/server:2022-latest

docker run -d -p 6379:6379 --name produtos-redis redis:7
```

Configure e rode a API:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<sua-connection-string>" --project Produtos.ProdutosAPI
dotnet user-secrets set "ConnectionStrings:Redis" "localhost:6379,abortConnect=False" --project Produtos.ProdutosAPI

dotnet run --project Produtos.ProdutosAPI
```

Em outro terminal, para ver a mensageria funcionando:

```bash
dotnet run --project Produtos.WorkerService
```

Faça um `POST /api/produtos` e acompanhe o Worker consumindo o evento.

> **Sobre o ambiente em nuvem:** o Azure SQL usa o free offer com auto-pause. Se a cota mensal de vCore-segundos acabar, o banco pausa até o início do mês seguinte e a API hospedada fica indisponível. É um trade-off deliberado de custo — o caminho local acima funciona sempre.

## Endpoints

| Método | Endpoint | Descrição |
| --- | --- | --- |
| POST | `/v1/auth/login` | Autenticação e emissão de JWT |
| GET | `/api/produtos` | Lista produtos (cache-aside, TTL 2 min) |
| GET | `/api/produtos/{id}` | Detalhe de um produto |
| POST | `/api/produtos` | Cadastro (publica evento no Service Bus) |
| PUT | `/api/produtos/{id}` | Atualização |
| DELETE | `/api/produtos/{id}` | Remoção |

O cache degrada com elegância: falha de Redis é registrada como warning e a requisição segue para o banco. Indisponibilidade de cache não derruba a API.

## Roadmap

- [x] CRUD completo e autenticação JWT
- [x] Estrutura monorepo com contratos compartilhados
- [x] Mensageria assíncrona (produtor/consumidor)
- [x] Testes unitários (xUnit + Moq)
- [x] Migração para Azure (SQL Database, Container Apps, Service Bus)
- [x] CI/CD com GitHub Actions e login OIDC (sem segredos de longa duração)
- [x] Governança de custo (budget + alerta de cota + auto-pause)
- [x] Cache distribuído real (Redis Cloud, colocalizado em São Paulo)
- [ ] Paginação por keyset e filtros avançados
- [ ] `PERFORMANCE.md` com baseline e medições sob carga
- [ ] `docker-compose.yml` para dependências locais
- [ ] Observabilidade com Application Insights

---

**Autor:** Leonardo Farias
