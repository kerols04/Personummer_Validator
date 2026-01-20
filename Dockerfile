# ---------- BUILD ----------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

#  FIX: .sln ligger i repo-root
COPY CI_CD_Group_8.sln ./

#  Kopiera csproj-filer separat (bättre cache + korrekt paths)
COPY CI_CD_Group_8/CI_CD_Group_8.csproj CI_CD_Group_8/
COPY CI_CD_Group_8.Tests/CI_CD_Group_8.Tests.csproj CI_CD_Group_8.Tests/

# Restore på solution
RUN dotnet restore ./CI_CD_Group_8.sln

# Kopiera resten av koden
COPY . ./

#  Publish endast console-projektet (inte hela solution)
RUN dotnet publish ./CI_CD_Group_8/CI_CD_Group_8.csproj -c Release -o /app/publish --no-restore

# ---------- RUNTIME ----------
FROM mcr.microsoft.com/dotnet/runtime:9.0
WORKDIR /app

COPY --from=build /app/publish ./

# Starta konsolappen
ENTRYPOINT ["dotnet", "CI_CD_Group_8.dll"]
