# Stage 1: Build & Publish
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy toàn bộ file .csproj để restore dependencies (tận dụng Docker caching)
COPY ["NexusOS.API/NexusOS.API.csproj", "NexusOS.API/"]
COPY ["NexusOS.BLL/NexusOS.BLL.csproj", "NexusOS.BLL/"]
COPY ["NexusOS.DAL/NexusOS.DAL.csproj", "NexusOS.DAL/"]
COPY ["NexusOS.MB/NexusOS.MB.csproj", "NexusOS.MB/"]
COPY ["NexusOS.Util/NexusOS.Util.csproj", "NexusOS.Util/"]

RUN dotnet restore "NexusOS.API/NexusOS.API.csproj"

# Copy toàn bộ source code
COPY . .
WORKDIR "/src/NexusOS.API"

# Build và publish ứng dụng
RUN dotnet publish "NexusOS.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# .NET 8/9/10 mặc định dùng port 8080 cho HTTP
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "NexusOS.API.dll"]

