using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using ProdutosAPI.Datas;
using ProdutosAPI.Middlewares;
using ProdutosAPI.Services;
using ProdutosAPI.Services.Interfaces;
using System.Reflection;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllers();

        builder.Services.AddEndpointsApiExplorer();


        // Add Authentication
        var key = System.Text.Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Key"]);
        builder.Services.AddAuthentication(a =>
        {
            // Define o esquema de autenticação
            a.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            a.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(a =>
        {
            // Configura os parâmetros de validação do token
            a.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
            {
                ValidateIssuerSigningKey = true, //Valida a chave de assinatura do token
                IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Key"])), //Define a chave de assinatura
                ValidateIssuer = false, //Não valida o emissor do token
                ValidateAudience = false //Não valida o destinatário do token
            };
        });

        builder.Services.AddSwaggerGen(c =>
        {
            //Descobre o nome do arquivo XML que contém os comentários
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            //Monta o caminho completo até esse XML (normalmente na pasta bin/Debug/netX.X/)
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

            //Informa ao Swagger que deve carregar os comentários desse arquivo XML
            c.IncludeXmlComments(xmlPath);

            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "ProdutosAPI",
                Version = "v1"
            });
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
            {
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 12345abcdef\"",
            });
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {   
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new string[] {}
                }
            });
        });

        //Add DbContext
        builder.Services.AddDbContext<ProdutosDBContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("ProdutosAPI")));

        //Add Services
        builder.Services.AddScoped<IProdutosService, ProdutosService>();
        builder.Services.AddScoped<IUsuarioService, UsuariosService>();
        builder.Services.AddTransient<IAuthService, AuthService>();

        var app = builder.Build();

        //Adiciona o middleware de tratamento de exceções
        app.UseMiddleware<BackofficeExceptionHandlerMiddleware>();

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