#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["src/kentxxq.Templates.Aspnetcore/kentxxq.Templates.Aspnetcore.Webapi/kentxxq.Templates.Aspnetcore.Webapi.csproj", "src/kentxxq.Templates.Aspnetcore/kentxxq.Templates.Aspnetcore.Webapi/"]
COPY ["src/kentxxq.Templates.Aspnetcore/kentxxq.Templates.Aspnetcore.DB/kentxxq.Templates.Aspnetcore.DB.csproj", "src/kentxxq.Templates.Aspnetcore/kentxxq.Templates.Aspnetcore.DB/"]
COPY ["src/kentxxq.Templates.Aspnetcore/kentxxq.Templates.Aspnetcore.UI/kentxxq.Templates.Aspnetcore.UI.csproj", "src/kentxxq.Templates.Aspnetcore/kentxxq.Templates.Aspnetcore.UI/"]
RUN dotnet restore "src/kentxxq.Templates.Aspnetcore/kentxxq.Templates.Aspnetcore.Webapi/kentxxq.Templates.Aspnetcore.Webapi.csproj"
COPY . .
WORKDIR "/src/src/kentxxq.Templates.Aspnetcore/kentxxq.Templates.Aspnetcore.Webapi"
RUN dotnet build "kentxxq.Templates.Aspnetcore.Webapi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "kentxxq.Templates.Aspnetcore.Webapi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "kentxxq.Templates.Aspnetcore.Webapi.dll"]