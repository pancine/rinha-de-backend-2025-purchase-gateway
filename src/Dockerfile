FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /app
COPY PurchaseGateway.Api/PurchaseGateway.Api.csproj PurchaseGateway.Api/
RUN dotnet restore PurchaseGateway.Api/PurchaseGateway.Api.csproj
COPY . ./
RUN dotnet publish ./PurchaseGateway.Api/PurchaseGateway.Api.csproj --self-contained -r linux-x64 -c Release --no-restore -o out


FROM mcr.microsoft.com/dotnet/runtime-deps:9.0
WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["./PurchaseGateway.Api"]
