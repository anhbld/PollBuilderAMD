FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["PollBuilder.API.csproj", "."]
RUN dotnet restore "PollBuilder.API.csproj"

COPY . .
RUN dotnet build "PollBuilder.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PollBuilder.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PollBuilder.API.dll"]