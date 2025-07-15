# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Set environment variables for the API
ENV DATA__DataConnStr="Server=mysql_db;Database=pharmamoov;User ID=admin;Password=AWSsourire25!"
ENV ASPNETCORE_ENVIRONMENT="Production"

ENV PAYMENTCONFIG__CLIENTID=pharmamoovtest
ENV PAYMENTCONFIG__APIKEY=rWBV2tbSrY3xjEdikSR70eANEhzHKq71ge83QxujVrY7J50w1z
ENV PAYMENTCONFIG__CREDITEDID=
ENV PAYMENTCONFIG__WALLETID=
ENV PAYMENTCONFIG__BASEURL=https://api.sandbox.mangopay.com
ENV PAYMENTCONFIG__SANDBOXREFERENCE=https://api.sandbox.mangopay.com
ENV PAYMENTCONFIG__PRODUCTIONREFERENCE=https://api.mangopay.com
ENV PAYMENTCONFIG__PAYMENTRETURNURL=https://pharmamoov.fr/home/PaymentWaiting/
ENV PAYMENTCONFIG__WEBRETURLURL=https://pharmamoov.fr/home/PaymentWaitingWeb/

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["PharmaMoov.API/PharmaMoov.API.csproj", "PharmaMoov.API/"]
COPY ["PharmaMoov.Models/PharmaMoov.Models.csproj", "PharmaMoov.Models/"]
RUN dotnet restore "./PharmaMoov.API/PharmaMoov.API.csproj"
COPY . .
WORKDIR "/src/PharmaMoov.API"
RUN dotnet build "./PharmaMoov.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./PharmaMoov.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PharmaMoov.API.dll"]
