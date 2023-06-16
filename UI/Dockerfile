#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

#Depending on the operating system of the host machines(s) that will build or run the containers, the image specified in the FROM statement may need to be changed.
#For more information, please see https://aka.ms/containercompat

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
#EXPOSE 80
#EXPOSE 443
#RUN mkdir -p /etc/letsencrypt/live/dreamchat.io
ENV ASPNETCORE_URLS=https://+:7000
COPY aspnetapp.pfx aspnetapp.pfx
ENV ASPNETCORE_Kestrel__Certificates__Default__Password="$aD15~123!@#"
ENV ASPNETCORE_Kestrel__Certificates__Default__Path=/app/aspnetapp.pfx

#ENV ASPNETCORE_URLS=http://+:80;https://+:443
#ENV ASPNETCORE_HTTPS_PORT=443
#ENV ASPNETCORE_Kestrel__Certificates__Default__Path=/etc/letsencrypt/live/dreamchat.io/fullchain.pem
#ENV ASPNETCORE_Kestrel__Certificates__Default__KeyPath=/etc/letsencrypt/live/dreamchat.io/privkey.pem


FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["UI/UI.csproj", "UI/"]
RUN dotnet restore "UI/UI.csproj"
COPY . .
WORKDIR "/src/UI"
RUN dotnet build "UI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "UI.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "UI.dll"]