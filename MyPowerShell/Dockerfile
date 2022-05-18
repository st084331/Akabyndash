FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["MyPowerShell/MyPowerShell.csproj", "MyPowerShell/"]
RUN dotnet restore "MyPowerShell/MyPowerShell.csproj"
COPY . .
WORKDIR "/src/MyPowerShell"
RUN dotnet build "MyPowerShell.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MyPowerShell.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MyPowerShell.dll"]
