using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using DotNetEnv;

namespace ProdutosAPI.Data;

    // Fábrica de contexto do banco de dados
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        Env.Load("../.env"); // Carrega as variáveis de ambiente do arquivo .env

        var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING") ?? throw new InvalidOperationException("CONNECTION_STRING não encontrada nas variáveis de ambiente.");

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure(
                maxRetryCount: 5, // Número máximo de tentativas
                maxRetryDelay: TimeSpan.FromSeconds(10), // Tempo máximo de espera entre tentativas
                errorNumbersToAdd: null) // Números de erro adicionais para considerar como falhas transitórias
            )// Configura o contexto para usar SQL Server
            .Options;

        return new AppDbContext(options); // Retorna uma nova instância do contexto do banco de dados
    }
}