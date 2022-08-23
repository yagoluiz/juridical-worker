FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
COPY ["src/Juridical.Core/Juridical.Core.csproj", "src/Juridical.Core/"]
COPY ["src/Juridical.Message.Worker/Juridical.Message.Worker.csproj", "src/Juridical.Message.Worker/"]
RUN dotnet restore "src/Juridical.Message.Worker/Juridical.Message.Worker.csproj"
COPY . .
WORKDIR "/src/Juridical.Message.Worker"
RUN dotnet build "Juridical.Message.Worker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Juridical.Message.Worker.csproj" -c Release -o /app/publish

FROM base AS final
ENV TZ=America/Sao_Paulo
RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Juridical.Message.Worker.dll"]
