# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["CantinaIBJ.WebApi/CantinaIBJ.WebApi.csproj", "CantinaIBJ.WebApi/"]
COPY ["CantinaIBJ.Data/CantinaIBJ.Data.csproj", "CantinaIBJ.Data/"]
COPY ["CantinaIBJ.Model/CantinaIBJ.Model.csproj", "CantinaIBJ.Model/"]
COPY ["CantinaIBJ.Framework/CantinaIBJ.Framework.csproj", "CantinaIBJ.Framework/"]
COPY ["CantinaIBJ.Integration/CantinaIBJ.Integration.csproj", "CantinaIBJ.Integration/"]
RUN dotnet restore "./CantinaIBJ.WebApi/CantinaIBJ.WebApi.csproj"
COPY . .
WORKDIR "/src/CantinaIBJ.WebApi"
RUN dotnet build "./CantinaIBJ.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CantinaIBJ.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CantinaIBJ.WebApi.dll"]