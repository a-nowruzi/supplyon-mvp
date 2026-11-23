FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
RUN mkdir -p /app/data && chown -R $APP_UID:$APP_UID /app
USER $APP_UID
ENV ASPNETCORE_URLS=http://+:8083
ENV ASPNETCORE_HTTP_PORTS=8083
EXPOSE 8083
ENTRYPOINT ["dotnet", "O2Ban.ValidationWorkspace.dll"]
