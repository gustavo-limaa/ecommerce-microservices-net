FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["Ecommerce.Catalogo.Api/Ecommerce.Catalogo.Api.csproj", "Ecommerce.Catalogo.Api/"]
RUN dotnet restore "Ecommerce.Catalogo.Api/Ecommerce.Catalogo.Api.csproj"
COPY . .
WORKDIR "/src/Ecommerce.Catalogo.Api"
RUN dotnet build "Ecommerce.Catalogo.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Ecommerce.Catalogo.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
