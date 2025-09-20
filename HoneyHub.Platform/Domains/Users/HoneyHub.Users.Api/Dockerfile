FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

# Copy published output (from CI/local publish step)
COPY . .

# Bind and expose
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Run as non-root (after files exist)
USER 64198

ENTRYPOINT ["dotnet", "HoneyHub.Users.Api.dll"]
