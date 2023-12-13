FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["MKS6-Orders/MKS6-Orders/MKS6-Orders.csproj", "MKS6-Orders/"]
RUN dotnet restore "MKS6-Orders/MKS6-Orders.csproj"
COPY . .
WORKDIR "/src/MKS6-Orders"
RUN dotnet build "MKS6-Orders.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MKS6-Orders.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MKS6-Orders-Backend.dll"]
