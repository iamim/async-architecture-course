FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["AsyncArch.Services.Tasks/AsyncArch.Services.Tasks.csproj", "AsyncArch.Services.Tasks/"]
RUN dotnet restore "AsyncArch.Services.Tasks/AsyncArch.Services.Tasks.csproj"
COPY . .
WORKDIR "/src/AsyncArch.Services.Tasks"
RUN dotnet build "AsyncArch.Services.Tasks.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AsyncArch.Services.Tasks.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AsyncArch.Services.Tasks.dll"]
