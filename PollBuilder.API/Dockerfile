# 🟢 Step 1: Use the .NET 10.0 SDK for building
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy the csproj using the relative path from the root context
COPY ["PollBuilder.API/PollBuilder.API.csproj", "PollBuilder.API/"]
RUN dotnet restore "PollBuilder.API/PollBuilder.API.csproj"

# Copy the rest of the source code
COPY . .
WORKDIR "/src/PollBuilder.API"
RUN dotnet build "PollBuilder.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PollBuilder.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 🟢 Step 2: Use the .NET 10.0 ASP.NET Runtime for the final image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PollBuilder.API.dll"]