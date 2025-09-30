# ProdutosAPI

# 📦 API de Produtos

Uma API RESTful simples para gerenciamento de produtos, desenvolvida em **ASP.NET Core** utilizando **Entity Framework Core**.

## 🚀 Tecnologias
- .NET 8 / ASP.NET Core
- Entity Framework Core (Code First)
- Swagger (documentação da API)
- SQL Server (pode trocar para outro banco se quiser)

## 📌 Funcionalidades
- Criar produto (POST)
- Listar produtos (GET)
- Buscar produto por ID (GET)
- Atualizar produto (PUT)
- Remover produto (DELETE)

## ⚙️ Como rodar o projeto

1. Clone este repositório:
   ```bash
   git clone https://github.com/seu-usuario/nome-do-repo.git
   ```
2.Acesse a pasta do projeto:
   ```bash
   cd ProdutosAPI
   ```
3.Configure a connection string no arquivo appsettings.json
   ```bash
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=ProdutosDb;Trusted_Connection=True;"
}
   ```
4.Execute as migrations e crie o banco:
   ```bash
   dotnet ef database update
   ```

5.Rode o projeto:
```bash
dotnet run
   ```
6.Acesse a documentação Swagger:
```bash
https://localhost:5001/swagger
   ```

Endpoints principais

GET /api/produtos → Lista todos os produtos

GET /api/produtos/{id} → Busca produto por ID

POST /api/produtos → Cria novo produto

PUT /api/produtos/{id} → Atualiza produto

DELETE /api/produtos/{id} → Remove produto

🔮 Próximos Passos / Melhorias Futuras

 Implementar autenticação e autorização com JWT &#x2705;

 Adicionar camada de testes unitários

 Configurar Docker para facilitar deploy

 Criar logs de auditoria com Serilog

 Adicionar paginação e filtros nas listagens

 Implementar CI/CD com GitHub Actions
