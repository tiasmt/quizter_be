FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["quizter_be.csproj", ""]
RUN dotnet restore "./quizter_be.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "quizter_be.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "quizter_be.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
#ENTRYPOINT ["dotnet", "quizter_be.dll"]
CMD ASPNETCORE_URLS=http://*:$PORT dotnet quizter_be.dll