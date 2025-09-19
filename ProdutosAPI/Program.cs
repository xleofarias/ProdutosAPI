using Microsoft.EntityFrameworkCore;
using ProdutosAPI.Datas;
using ProdutosAPI.Services;
using System.Reflection;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllers();

        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddSwaggerGen(c => 
        {
            //Descobre o nome do arquivo XML que contém os comentários
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            //Monta o caminho completo até esse XML (normalmente na pasta bin/Debug/netX.X/)
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

            //Informa ao Swagger que deve carregar os comentários desse arquivo XML
            c.IncludeXmlComments(xmlPath);
        });

        //Add DbContext
        builder.Services.AddDbContext<ProdutosDBContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("ProdutosAPI")));

        //Add Services
        builder.Services.AddScoped<IProdutosService, ProdutosService>();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}