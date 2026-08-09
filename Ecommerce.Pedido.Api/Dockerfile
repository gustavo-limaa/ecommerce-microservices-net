FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["Ecommerce.Pedido.Api/Ecommerce.Pedido.Api.csproj", "Ecommerce.Pedido.Api/"]
RUN dotnet restore "Ecommerce.Pedido.Api/Ecommerce.Pedido.Api.csproj"
COPY . .
WORKDIR "/src/Ecommerce.Pedido.Api"
RUN dotnet build "Ecommerce.Pedido.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Ecommerce.Pedido.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Ecommerce.Pedido.Api.dll"]