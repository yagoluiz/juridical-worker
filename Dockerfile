FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
COPY ["src/Juridical.Worker/Juridical.Worker.csproj", "src/Juridical.Worker/"]
RUN dotnet restore "src/Juridical.Worker/Juridical.Worker.csproj"
COPY . .
WORKDIR "/src/Juridical.Worker"
RUN dotnet build "Juridical.Worker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Juridical.Worker.csproj" -c Release -o /app/publish

FROM base AS final
ENV TZ=America/Sao_Paulo
RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Juridical.Worker.dll"]
