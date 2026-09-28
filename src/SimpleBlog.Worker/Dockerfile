FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY SimpleBlog.sln ./
COPY src/SimpleBlog.Core/SimpleBlog.Core.csproj src/SimpleBlog.Core/
COPY src/SimpleBlog.Application/SimpleBlog.Application.csproj src/SimpleBlog.Application/
COPY src/SimpleBlog.Infrastructure/SimpleBlog.Infrastructure.csproj src/SimpleBlog.Infrastructure/
COPY src/SimpleBlog.Worker/SimpleBlog.Worker.csproj src/SimpleBlog.Worker/
RUN dotnet restore src/SimpleBlog.Worker/SimpleBlog.Worker.csproj

COPY . .
RUN dotnet publish src/SimpleBlog.Worker/SimpleBlog.Worker.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "SimpleBlog.Worker.dll"]
