FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Juridical.Core/Juridical.Core.csproj", "Juridical.Core/"]
COPY ["Juridical.LegalProcess.Worker/Juridical.LegalProcess.Worker.csproj", "Juridical.LegalProcess.Worker/"]
RUN dotnet restore "Juridical.LegalProcess.Worker/Juridical.LegalProcess.Worker.csproj"
COPY . .
WORKDIR "/src/Juridical.LegalProcess.Worker"
RUN dotnet build "Juridical.LegalProcess.Worker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Juridical.LegalProcess.Worker.csproj" -c Release -o /app/publish

FROM base AS final
ENV TZ=America/Sao_Paulo
RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Juridical.LegalProcess.Worker.dll"]
