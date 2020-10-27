FROM utarn/aspnetcore3.1-centos8:latest AS builder 
COPY Warehouse.csproj Warehouse.csproj
RUN dotnet restore Warehouse.csproj Warehouse.csproj
COPY . .
RUN dotnet publish --output /app --configuration Release

FROM utarn/aspnetcore3.1-centos8:latest
WORKDIR /app
COPY --from=builder /app .
ENTRYPOINT ["dotnet", "Warehouse.dll"]