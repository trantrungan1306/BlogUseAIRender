FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY SimpleBlog.sln ./
COPY src/SimpleBlog.Core/SimpleBlog.Core.csproj src/SimpleBlog.Core/
COPY src/SimpleBlog.Application/SimpleBlog.Application.csproj src/SimpleBlog.Application/
COPY src/SimpleBlog.Infrastructure/SimpleBlog.Infrastructure.csproj src/SimpleBlog.Infrastructure/
COPY src/SimpleBlog.Api/SimpleBlog.Api.csproj src/SimpleBlog.Api/
RUN dotnet restore src/SimpleBlog.Api/SimpleBlog.Api.csproj

COPY . .
RUN dotnet publish src/SimpleBlog.Api/SimpleBlog.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "SimpleBlog.Api.dll"]
