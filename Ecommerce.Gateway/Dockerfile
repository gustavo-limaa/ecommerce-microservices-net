FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["Ecommerce.Gateway/Ecommerce.Gateway.csproj", "Ecommerce.Gateway/"]
RUN dotnet restore "Ecommerce.Gateway/Ecommerce.Gateway.csproj"
COPY . .
WORKDIR "/src/Ecommerce.Gateway"
RUN dotnet build "Ecommerce.Gateway.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Ecommerce.Gateway.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Ecommerce.Gateway.dll"]