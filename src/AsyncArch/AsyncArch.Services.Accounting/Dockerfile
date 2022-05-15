FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["AsyncArch.Services.Accounting/AsyncArch.Services.Accounting.csproj", "AsyncArch.Services.Accounting/"]
RUN dotnet restore "AsyncArch.Services.Accounting/AsyncArch.Services.Accounting.csproj"
COPY . .
WORKDIR "/src/AsyncArch.Services.Accounting"
RUN dotnet build "AsyncArch.Services.Accounting.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AsyncArch.Services.Accounting.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AsyncArch.Services.Accounting.dll"]
