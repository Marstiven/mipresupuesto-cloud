# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar csproj y restaurar dependencias
COPY ["MiPresupuesto-api.csproj", "./"]
RUN dotnet restore "MiPresupuesto-api.csproj"

# Copiar el resto del código y compilar en Release
COPY . .
RUN dotnet build "MiPresupuesto-api.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "MiPresupuesto-api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MiPresupuesto-api.dll"]

