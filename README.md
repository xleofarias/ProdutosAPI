# 🛒 ProdutosAPI - Gestão de Catálogo E-commerce

API RESTful desenvolvida com **.NET 8**, focada em performance, arquitetura limpa e boas práticas de manipulação de dados.

![Badge .NET](https://img.shields.io/badge/.NET-8.0-purple)
![Badge Status](https://img.shields.io/badge/Status-Em_Desenvolvimento-yellow)

## 📋 Sobre o Projeto
Este projeto simula o backend de um catálogo de e-commerce. O objetivo principal não é apenas o CRUD, mas a demonstração de uma arquitetura robusta pronta para integração com front-ends modernos.

Diferenciais técnicos:
* **Clean Code:** Separação clara de responsabilidades.
* **Performance:** Consultas ao banco de dados (SQL Server) otimizadas via Entity Framework Core.
* **Segurança:** Implementação planejada de Autenticação JWT.

## 🚀 Tecnologias Utilizadas
* **Linguagem:** C# (.NET 8)
* **ORM:** Entity Framework Core (Abordagem Code-First com Migrations)
* **Banco de Dados:** SQL Server
* **Documentação:** Swagger (OpenAPI)
* **Validações:** Data Annotations / FluentValidation (se tiver)

## ⚙️ Como Executar Localmente

### Pré-requisitos
* .NET SDK 8.0
* SQL Server (LocalDB ou Container Docker)

### Passo a Passo
1. **Clone o repositório:**
   ```bash
   git clone [https://github.com/xleofarias/ProdutosAPI.git](https://github.com/xleofarias/ProdutosAPI.git)
   ```
2. Acesse a pasta do projeto:
   ```bash
   cd ProdutosAPI
   ```
3. Configure a connection string no arquivo appsettings.json
   ```bash
   "ConnectionStrings": {
     "DefaultConnection": "Server=localhost;Database=ProdutosDb;Trusted_Connection=True;"
   }   
   ```
4. Execute as migrations e crie o banco:
   ```bash
   dotnet ef database update
   ```

5. Rode o projeto:
   ```bash
   dotnet run
   ```
6. Acesse a documentação Swagger:
   ```bash
   https://localhost:5001/swagger
   ```

🔌Endpoints Principais

| Método | Endpoint           | Descrição
| ------ | ------------------ | ---------
| GET    | /api/produtos      | Lista todos os produtos (paginação em breve)
| GET    | /api/produtos/{id} | Detalhes de um produto específico
| POST   | /api/produtos      | Cadastro de novo produto
| PUT    | /api/produtos/{id} | Atualização cadastral
| DELETE | /api/produtos/{id} | Remoção lógica/física

🔮 Roadmap & Melhorias
[x] Implementação de CRUD Básico
[ ] Autenticação e Autorização com JWT (Bearer Token)
[ ] Implementação de Testes Unitários (xUnit)
[ ] Containerização com Docker
[ ] Pipeline de CI/CD (GitHub Actions)

---
Desenvolvido por Leonardo Farias LinkedIn | Portfólio
