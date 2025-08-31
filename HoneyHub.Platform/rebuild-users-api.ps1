# Rebuild and restart HoneyHub.Users.Api container
Set-Location "$PSScriptRoot\.."

Write-Host "Publishing project..."
dotnet publish .\HoneyHub.Users.Api\HoneyHub.Users.Api.csproj -c Release -o .\publish\users-api

Write-Host "Building Docker image..."
docker build -t honeyhub/users-api:dev -f .\HoneyHub.Users.Api\Dockerfile .\publish\users-api

Write-Host "Stopping old container (if running)..."
docker stop honeyhub-users-api 2>$null
docker rm honeyhub-users-api 2>$null

Write-Host "Starting new container..."
docker run -d --name honeyhub-users-api -p 5173:8080 -e ASPNETCORE_ENVIRONMENT=Development honeyhub/users-api:dev

Write-Host "Done! Container running at http://localhost:5173"
