FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Ecommerce.Auth.Api/Ecommerce.Auth.Api.csproj", "Ecommerce.Auth.Api/"]
RUN dotnet restore "Ecommerce.Auth.Api/Ecommerce.Auth.Api.csproj"
COPY . .
WORKDIR "/src/Ecommerce.Auth.Api"
RUN dotnet build "Ecommerce.Auth.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Ecommerce.Auth.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Ecommerce.Auth.Api.dll"]