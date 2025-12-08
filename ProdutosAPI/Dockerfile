# Etapa 1 - Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia apenas arquivos necessários para cache
COPY *.sln ./
COPY ProdutosAPI.csproj ./
RUN dotnet restore

# Copia tudo pro container
COPY . .

# Vai até a pasta onde está o .csproj
WORKDIR /src/

RUN dotnet publish -c Release -o /app/out

# Etapa 2 - Runtime (imagem leve)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copia apenas os arquivos publicados da etapa anterior
COPY --from=build /app/out .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "ProdutosAPI.dll"]
