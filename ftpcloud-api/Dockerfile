FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY FtpCloud.sln ./
COPY src/FtpCloud.Domain/FtpCloud.Domain.csproj src/FtpCloud.Domain/
COPY src/FtpCloud.Application/FtpCloud.Application.csproj src/FtpCloud.Application/
COPY src/FtpCloud.Infrastructure/FtpCloud.Infrastructure.csproj src/FtpCloud.Infrastructure/
COPY src/FtpCloud.Api/FtpCloud.Api.csproj src/FtpCloud.Api/
RUN dotnet restore FtpCloud.sln

COPY src/ src/
RUN dotnet publish src/FtpCloud.Api/FtpCloud.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app .

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "FtpCloud.Api.dll"]
