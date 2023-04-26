FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Poseidon/Poseidon.csproj", "Poseidon/"]
RUN dotnet restore "Poseidon/Poseidon.csproj"
COPY . .
WORKDIR "/src/Poseidon"
RUN dotnet build "Poseidon.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Poseidon.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Poseidon.dll"]
