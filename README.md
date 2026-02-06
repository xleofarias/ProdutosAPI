# 🛒 ProdutosAPI - Catálogo E-commerce Backend

API RESTful moderna desenvolvida com **.NET 8**, focada em escalabilidade, segurança e integração contínua (CI/CD). O projeto demonstra uma arquitetura preparada para produção, saindo do básico e abordando desafios reais de infraestrutura e segurança.

![Badge .NET](https://img.shields.io/badge/.NET-8.0-purple?style=flat&logo=dotnet)
![Badge Docker](https://img.shields.io/badge/Docker-Enabled-blue?style=flat&logo=docker)
![Badge Postgres](https://img.shields.io/badge/PostgreSQL-Production-336791?style=flat&logo=postgresql)
![Badge CI/CD](https://img.shields.io/badge/CI%2FCD-GitHub%20Actions-2088FF?style=flat&logo=github-actions)
![Badge Status](https://img.shields.io/badge/Status-Concluído-success)

---

## 📋 Sobre o Projeto
Este backend gerencia um catálogo de produtos e usuários, simulando o núcleo de um e-commerce. Diferente de projetos acadêmicos comuns, este repositório foca na **Jornada DevOps e Segurança**:

* **Deploy Automatizado:** Pipeline de CI/CD configurado para deploy automático no **Render**.
* **Segurança Robusta:** * Autenticação via **JWT (Bearer Token)**.
    * **Hashing de Senha Nativo:** Implementação customizada usando `PBKDF2` com `HMACSHA256` (System.Security.Cryptography), eliminando dependências externas e problemas de versionamento (DLL Hell).
    * **Gestão de Segredos:** Uso de Environment Variables e User Secrets para proteger credenciais.
* **Arquitetura:**
    * Tratamento global de erros via **Middleware** personalizado.
    * Seed de dados inteligente (Autocorreção de Admin).
    * Suporte híbrido a Banco de Dados (SQL Server em Dev / PostgreSQL em Prod).

## 🚀 Tecnologias e Ferramentas

* **Core:** C# (.NET 8)
* **ORM:** Entity Framework Core (Code-First)
* **Banco de Dados:** PostgreSQL (Produção) / SQL Server (Local)
* **Infraestrutura:** Docker & Docker Compose
* **CI/CD:** GitHub Actions
* **Cloud:** Render
* **Documentação:** Swagger (OpenAPI)

## ⚙️ Como Executar Localmente

### Pré-requisitos
* .NET SDK 8.0
* Docker (Recomendado) ou SQL Server Local

### Passo a Passo

1. **Clone o repositório:**
   ```bash
   git clone [https://github.com/xleofarias/ProdutosAPI.git](https://github.com/xleofarias/ProdutosAPI.git)
   cd ProdutosAPI
2. Configuração de Ambiente: Configure a connection string no appsettings.json ou utilize User Secrets para maior segurança:
   ```bash
   "ConnectionStrings": { "DefaultConnection": "Server=localhost;Database=ProdutosDb;Trusted_Connection=True;TrustServerCertificate=True;" }
3. Gerando o Banco de Dados:
   ```bash
   dotnet ef database update
4. Executando a API:
   ```bash
   dotnet run
5. Documentação: Acesse o Swagger em:
   ```bash
   https://localhost:5001/swagger

## 🐳 Executando com Docker
Se preferir não instalar o banco localmente, suba todo o ambiente com uma linha:
   ```bash
   docker-compose up -d --build
   ```
## 🔌 Endpoints Principais

Método,Rota,Descrição,Auth
| Método | Endpoint           | Descrição
| ------ | ------------------ | ---------
| POST   | /v1/auth/login     | Autenticação e geração de Token JWT
| GET    | /api/produtos      | Lista todos os produtos (paginação em breve)
| GET    | /api/produtos/{id} | Detalhes de um produto específico
| POST   | /api/produtos      | Cadastro de novo produto
| PUT    | /api/produtos/{id} | Atualização cadastral
| DELETE | /api/produtos/{id} | Remoção lógica/física

## 🔮 Roadmap & Evolução
[x] CRUD Completo de Produtos e Usuários

[x] Autenticação JWT e Roles (Admin/User)

[x] Migração de SQL Server para PostgreSQL (Compatibilidade Linux/Cloud)

[x] Containerização (Dockerfile e Compose)

[x] Pipeline CI/CD (GitHub Actions -> Render)

[x] Refatoração de Segurança (Hash Nativo .NET)

[x] Implementação de Testes Unitários (xUnit)

[ ] Paginação e Filtros Avançados

## 👨‍💻 Autor
Desenvolvido por Leonardo Farias
