FROM utarn/aspnetcore3.1-centos8:latest AS builder 
WORKDIR /src
COPY Warehouse.csproj /Warehouse
RUN dotnet restore Warehouse.csproj /Warehouse
COPY . .
WORKDIR /src/aspnetproject
RUN dotnet publish --output /app --configuration Release

FROM utarn/aspnetcore3.1-centos8:latest
WORKDIR /app
COPY --from=builder /app .
ENTRYPOINT ["dotnet", "aspnetproject.dll"]