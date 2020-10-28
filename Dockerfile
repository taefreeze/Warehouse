ROM utarn/aspnetcore3.1-centos8:latest AS builder
WORKDIR /Warehouse
COPY . .
RUN dotnet publish --output /app --configuration Release

FROM utarn/aspnetcore3.1-centos8:latest
WORKDIR /app
COPY --from=builder /app .
ENTRYPOINT ["dotnet", "Warehouse.dll"]