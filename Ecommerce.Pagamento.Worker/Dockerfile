FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["Ecommerce.Pagamento.Worker/Ecommerce.Pagamento.Worker.csproj", "Ecommerce.Pagamento.Worker/"]
RUN dotnet restore "Ecommerce.Pagamento.Worker/Ecommerce.Pagamento.Worker.csproj"
COPY . .
WORKDIR "/src/Ecommerce.Pagamento.Worker"
RUN dotnet build "Ecommerce.Pagamento.Worker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Ecommerce.Pagamento.Worker.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Ecommerce.Pagamento.Worker.dll"]
