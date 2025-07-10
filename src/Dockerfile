FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app
COPY PurchaseGateway.Api/PurchaseGateway.Api.csproj PurchaseGateway.Api/
RUN dotnet restore PurchaseGateway.Api/PurchaseGateway.Api.csproj
COPY . ./
RUN dotnet publish ./PurchaseGateway.Api/PurchaseGateway.Api.csproj --no-restore -c Release -o out -p:UseAppHost=false


FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["dotnet", "PurchaseGateway.Api.dll"]
