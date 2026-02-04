using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using ProdutosAPI.Data;
using ProdutosAPI.Middlewares;
using ProdutosAPI.Repositories;
using ProdutosAPI.Repositories.Interfaces;
using ProdutosAPI.Services;
using ProdutosAPI.Services.Interfaces;
using System.Reflection;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Microsoft.OpenApi.Validations;
using System.Threading.Tasks;

internal class Program
{
    private static async Task Main(string[] args)
    {
        //Incluído para tratar o problema de data por conta do postgresql
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

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
            a.RequireHttpsMetadata = true;
            a.SaveToken = true;
            // Configura os parâmetros de validação do token
            a.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
            {
                ValidateIssuerSigningKey = true, //Valida a chave de assinatura do token
                IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(key), //Define a chave de assinatura
                ValidateIssuer = false, //Não valida o emissor do token
                ValidateAudience = false //Não valida o destinatário do token
            };
        });

        // Add Cors
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
            {

                    policy.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });

        // Configuração do Rate Limiter Anti-BruteForce
        builder.Services.AddRateLimiter(options =>
        {
            // Quando bloquear, devolve status 429 (Too Many Requests)
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            // Cria uma política chamada "login-limit"
            options.AddFixedWindowLimiter(policyName: "login-limit", options =>
            {
                options.PermitLimit = 5; // Permite apenas 5 tentativas
                options.Window = TimeSpan.FromMinutes(3); // A cada 3 minutos
                options.QueueLimit = 0; // Não deixa ninguém na fila de espera, rejeita na hora
                options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            });
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
                Title = "Products",
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
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("ProdutosAPI"),
                sqlOptions => sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 10,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorCodesToAdd: null//,
                    //errorNumbersToAdd: null
                    )));


        //Add HealthCheck
        builder.Services.AddHealthChecks();

        // Add Repositories
        builder.Services.AddScoped<IProductRepository, ProductRepository>();
        builder.Services.AddScoped<IUserRepository, UserRepository>();

        //Add Services
        builder.Services.AddScoped<IProductService, ProductService>();
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<IAuthService, AuthService>();

        var app = builder.Build();

        //Adiciona o middleware de tratamento de exceções
        app.UseMiddleware<BackofficeExceptionHandlerMiddleware>();

        app.UseSwagger();
        app.UseSwaggerUI();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/error"); 
            app.UseHsts(); // FORÇA USAR HTTPS
        }

        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            try
            {
                var context = services.GetRequiredService<AppDbContext>();
                var configuartion = services.GetRequiredService<IConfiguration>();

                // 1. Aplica as tabelas (Migrations)
                context.Database.Migrate();

                // 2. Criação do admin
                await DbSeeder.SeedAdminUser(context, configuartion);
                
            }
            catch(Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "Ocorreu um erro na criação da instância do banco de dados");
            }
        }

        app.UseHttpsRedirection();

        app.UseCors("AllowFrontend");

        app.UseRateLimiter();

        app.UseAuthentication();
        
        app.UseAuthorization();

        app.MapHealthChecks("/health");

        app.MapControllers();

        app.Run();
    }
}