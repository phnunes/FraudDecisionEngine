FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/FraudAnalysis.Worker/FraudAnalysis.Worker.csproj", "src/FraudAnalysis.Worker/"]
COPY ["src/FraudAnalysis.Application/FraudAnalysis.Application.csproj", "src/FraudAnalysis.Application/"]
COPY ["src/FraudAnalysis.Domain/FraudAnalysis.Domain.csproj", "src/FraudAnalysis.Domain/"]
COPY ["src/FraudAnalysis.Infrastructure/FraudAnalysis.Infrastructure.csproj", "src/FraudAnalysis.Infrastructure/"]
RUN dotnet restore "src/FraudAnalysis.Worker/FraudAnalysis.Worker.csproj"
COPY . .
WORKDIR "/src/src/FraudAnalysis.Worker"
RUN dotnet build "FraudAnalysis.Worker.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "FraudAnalysis.Worker.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FraudAnalysis.Worker.dll"]
