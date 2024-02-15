#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

#Depending on the operating system of the host machines(s) that will build or run the containers, the image specified in the FROM statement may need to be changed.
#For more information, please see https://aka.ms/containercompat

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
COPY ["ProjetoCantinaIBJ/CantinaIBJ.WebApi/CantinaIBJ.WebApi.csproj", "CantinaIBJ.WebApi/"]
COPY ["ProjetoCantinaIBJ/CantinaIBJ.Data/CantinaIBJ.Data.csproj", "CantinaIBJ.Data/"]
COPY ["ProjetoCantinaIBJ/CantinaIBJ.Model/CantinaIBJ.Model.csproj", "CantinaIBJ.Model/"]
COPY ["ProjetoCantinaIBJ/CantinaIBJ.Framework/CantinaIBJ.Framework.csproj", "CantinaIBJ.Framework/"]
COPY ["ProjetoCantinaIBJ/CantinaIBJ.Integration/CantinaIBJ.Integration.csproj", "CantinaIBJ.Integration/"]
RUN dotnet restore "ProjetoCantinaIBJ/CantinaIBJ.WebApi/CantinaIBJ.WebApi.csproj"
COPY . .
RUN dotnet build "ProjetoCantinaIBJ/CantinaIBJ.WebApi/CantinaIBJ.WebApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ProjetoCantinaIBJ/CantinaIBJ.WebApi/CantinaIBJ.WebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CantinaIBJ.WebApi.dll"]