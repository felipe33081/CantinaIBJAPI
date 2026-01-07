# 1. Adicionando o bloco "base" que estava faltando (Runtime)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# 2. Build (SDK)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# CORREÇÃO: Ajustei de "CantinaIBJ" (I) para "CantinalBJ" (L) para bater com o GitHub
COPY ["CantinalBJ.WebApi/CantinalBJ.WebApi.csproj", "CantinalBJ.WebApi/"]
COPY ["CantinalBJ.Data/CantinalBJ.Data.csproj", "CantinalBJ.Data/"]
COPY ["CantinalBJ.Model/CantinalBJ.Model.csproj", "CantinalBJ.Model/"]
COPY ["CantinalBJ.Framework/CantinalBJ.Framework.csproj", "CantinalBJ.Framework/"]
COPY ["CantinalBJ.Integration/CantinalBJ.Integration.csproj", "CantinalBJ.Integration/"]

# Restaura dependências
RUN dotnet restore "./CantinalBJ.WebApi/CantinalBJ.WebApi.csproj"

# Copia todo o resto
COPY . .
WORKDIR "/src/CantinalBJ.WebApi"

# Build
RUN dotnet build "./CantinalBJ.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

# 3. Publish
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CantinalBJ.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# 4. Final (Agora funciona porque o "base" foi definido lá em cima)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CantinalBJ.WebApi.dll"]