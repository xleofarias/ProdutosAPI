# 🛒 ProdutosAPI

Catálogo de produtos em **.NET 8** com arquitetura orientada a eventos, migrado de um stack multi-provedor (Render + CloudAMQP) para **Azure**. O projeto serve como laboratório prático de nuvem e de tuning de dados.

![.NET](https://img.shields.io/badge/.NET-8.0-purple?style=flat&logo=dotnet)
![Azure SQL](https://img.shields.io/badge/Azure%20SQL-Serverless-0078D4?style=flat&logo=microsoftsqlserver)
![Container Apps](https://img.shields.io/badge/Azure-Container%20Apps-0078D4?style=flat&logo=microsoftazure)
![Service Bus](https://img.shields.io/badge/Azure-Service%20Bus-0078D4?style=flat&logo=microsoftazure)
![Redis](https://img.shields.io/badge/Redis-Cache-DC382D?style=flat&logo=redis)
![CI/CD](https://img.shields.io/badge/CI%2FCD-GitHub%20Actions-2088FF?style=flat&logo=github-actions)

---

## Sobre

Uma API REST de catálogo com CRUD, autenticação JWT, cache distribuído e processamento assíncrono. O que o projeto tem de interessante não é o CRUD — é a **migração completa de nuvem** e as decisões de custo por trás dela.

O ecossistema tem três projetos:

| Projeto | Papel |
| --- | --- |
| `Produtos.ProdutosAPI` | Web API REST síncrona |
| `Produtos.WorkerService` | Background service que consome a fila |
| `Produtos.Contracts` | Contratos compartilhados entre API e Worker |
| `Produtos.ProdutosAPITests` | Testes unitários (xUnit) |

O `Contracts` existe para que API e Worker compartilhem a definição das mensagens sem uma depender da outra — publisher e consumer evoluem separados.

```
  POST /api/produtos
         │
         ▼
   ┌──────────┐   grava    ┌──────────────┐
   │   API    │ ─────────► │  Azure SQL   │
   │(Container│            │ (serverless) │
   │   Apps)  │            └──────────────┘
   └────┬─────┘
        │ publica evento
        ▼
  ┌──────────────┐  consome  ┌────────────┐
  │ Service Bus  │ ────────► │   Worker   │
  └──────────────┘           └────────────┘

   GET /api/produtos ──► Redis (cache-aside) ──► Azure SQL no miss
```

## Migração: Render → Azure

O projeto rodava inteiro fora do Azure. Cada peça foi mapeada para o serviço gerenciado equivalente:

| Componente | Antes | Depois | Por quê |
| --- | --- | --- | --- |
| Banco relacional | PostgreSQL (Render) | **Azure SQL Database** (free offer, serverless) | Alinha o projeto com T-SQL, que é minha praia no dia a dia |
| Hospedagem | Render | **Azure Container Apps** | Já estava dockerizado; escala a zero |
| Mensageria | RabbitMQ (CloudAMQP) | **Azure Service Bus** | MassTransit suporta como *transport* — troca em configuração |
| CI/CD | GitHub Actions → Render | **GitHub Actions → Azure** | Só retargetar o deploy |
| Cache | Redis (tier gratuito externo) | **mantido** | Ver decisão abaixo |

A migração do banco não foi só trocar o provider do EF Core. O SQL Server **não aceita comparação de tupla**, então a paginação por keyset precisou sair da forma do Postgres:

```sql
-- PostgreSQL
WHERE (preco, id) > (@ultimoPreco, @ultimoId)

-- SQL Server (forma expandida)
WHERE preco > @ultimoPreco OR (preco = @ultimoPreco AND id > @ultimoId)
```

## Decisões de engenharia

**Redis: avaliei o Azure Managed Redis e decidi não migrar.** O Azure não oferece tier gratuito de Redis, e o menor SKU fica em torno de US$ 11–16/mês. Dá para desabilitar a alta disponibilidade e baratear, mas isso implica perda de dados e downtime — aceitável só em dev/teste. Para um projeto de portfólio, o custo não se justifica e o ganho técnico seria mínimo: o `StackExchange.Redis` conecta ao serviço gerenciado sem configuração especial. Mantive o cache num tier gratuito externo. **Migrar tudo por completude é fácil; decidir o que não migrar é o trabalho.**

**Governança de custo antes de escalar.** O free offer do Azure SQL dá 100.000 vCore-segundos e 32 GB por mês. Configurei três camadas: o *behavior* do free limit em auto-pause (o freio real — budget alerta, não bloqueia), um budget na assinatura com alerta *forecasted* e *actual*, e um alerta de métrica em `Free amount remaining` abaixo de 10% da cota.

**Benchmark roda local, não na nuvem.** Medir p95 contra o Azure SQL serverless mistura três variáveis: a query, o resume do auto-pause e a latência da internet até a região. Os números de performance são colhidos contra SQL Server em container local, com a metodologia documentada. A nuvem prova que está no ar; o local prova o número.

## Rodando localmente

**Pré-requisitos:** .NET SDK 8.0 e Docker.

```bash
git clone https://github.com/xleofarias/ProdutosAPI.git
cd ProdutosAPI
```

Suba as dependências locais (SQL Server e Redis):

```bash
docker compose up -d
```

Configure os secrets e rode a API:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<sua-connection-string>" --project Produtos.ProdutosAPI
dotnet run --project Produtos.ProdutosAPI
```

Para ver a mensageria funcionando, rode o Worker em outro terminal:

```bash
dotnet run --project Produtos.WorkerService
```

Faça um `POST /api/produtos` e acompanhe o Worker consumindo o evento.

> **Sobre o ambiente em nuvem:** o Azure SQL usa o free offer com auto-pause. Se a cota mensal de vCore-segundos acabar, o banco pausa até o início do mês seguinte e a API hospedada fica indisponível. É um trade-off deliberado de custo — o caminho local acima funciona sempre.

## Endpoints

| Método | Endpoint | Descrição |
| --- | --- | --- |
| POST | `/v1/auth/login` | Autenticação e emissão de JWT |
| GET | `/api/produtos` | Lista produtos (cache-aside via Redis) |
| GET | `/api/produtos/{id}` | Detalhe de um produto |
| POST | `/api/produtos` | Cadastro (publica evento no Service Bus) |
| PUT | `/api/produtos/{id}` | Atualização |
| DELETE | `/api/produtos/{id}` | Remoção |

Autenticação por JWT, com hashing de senha via PBKDF2 (HMAC-SHA256) nativo do .NET.

## Roadmap

- [x] CRUD completo e autenticação JWT
- [x] Cache distribuído (Redis, cache-aside)
- [x] Estrutura monorepo com contratos compartilhados
- [x] Mensageria assíncrona (produtor/consumidor)
- [x] Testes unitários (xUnit)
- [x] Migração para Azure (SQL Database, Container Apps, Service Bus)
- [x] CI/CD com GitHub Actions fazendo deploy no Azure
- [x] Governança de custo (budget + alerta de cota + auto-pause)
- [ ] Paginação por keyset e filtros avançados
- [ ] `PERFORMANCE.md` com medições antes/depois sob carga
- [ ] `docker-compose.yml` para dependências locais
- [ ] Observabilidade com Application Insights

---

**Autor:** Leonardo Farias
