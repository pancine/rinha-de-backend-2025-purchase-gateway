FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app
COPY ["PurchaseGateway.csproj", "."]
RUN dotnet restore "./PurchaseGateway.csproj"
COPY . .
RUN dotnet publish "./PurchaseGateway.csproj" --no-restore -c Release -o out -p:UseAppHost=false


FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["dotnet", "PurchaseGateway.dll"]
