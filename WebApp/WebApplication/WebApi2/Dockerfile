#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["WebApi2/WebApi2.csproj", "WebApi2/"]
RUN dotnet restore "WebApi2/WebApi2.csproj"
COPY . .
WORKDIR "/src/WebApi2"
RUN dotnet build "WebApi2.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WebApi2.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV TEXT=TestText
ENTRYPOINT ["dotnet", "WebApi2.dll"]