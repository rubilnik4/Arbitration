FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081
EXPOSE 5005

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Debug 
WORKDIR /src

COPY . /src
WORKDIR "/src/Arbitration"

RUN dotnet restore "Arbitration.fsproj"
RUN dotnet build "Arbitration.fsproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Debug
RUN dotnet publish "Arbitration.fsproj" -c $BUILD_CONFIGURATION -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

CMD ["dotnet", "Arbitration.dll"]
