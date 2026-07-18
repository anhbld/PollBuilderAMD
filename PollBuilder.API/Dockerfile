FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["PollBuilder.API/PollBuilder.API.csproj", "PollBuilder.API/"]
RUN dotnet restore "PollBuilder.API/PollBuilder.API.csproj"

COPY . .

WORKDIR "/src/PollBuilder.API"
RUN dotnet publish "PollBuilder.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "PollBuilder.API.dll"]