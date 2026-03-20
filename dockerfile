FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY certs/global-bundle.pem /app/certs/global-bundle.pem
COPY ["src/TwinBlade.Api/TwinBlade.Api.csproj", "src/TwinBlade.Api/"]
COPY ["src/TwinBlade.Application/TwinBlade.Application.csproj", "src/TwinBlade.Application/"]
COPY ["src/TwinBlade.Domain/TwinBlade.Domain.csproj", "src/TwinBlade.Domain/"]
COPY ["src/TwinBlade.Infrastructure/TwinBlade.Infrastructure.csproj", "src/TwinBlade.Infrastructure/"]

RUN dotnet restore "src/TwinBlade.Api/TwinBlade.Api.csproj"

COPY . .
WORKDIR /src/src/TwinBlade.Api
RUN dotnet publish "TwinBlade.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false --no-restore
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "TwinBlade.Api.dll"]