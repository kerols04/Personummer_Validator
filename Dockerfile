# ================================
# STEG 1: BUILD-STAGE
# ================================
# Använder .NET 9 SDK för att bygga applikationen
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

# Sätter arbetskatalog i containern
WORKDIR /app

# --------------------------------
# Kopiera solution-filen först
# (för bättre cache-hantering)
# --------------------------------
COPY *.sln ./

# --------------------------------
# Kopiera projektfiler (csproj)
# Dessa behövs för dotnet restore
# --------------------------------
COPY Ci_CD_Group_8/CI_CD_Group_8/*.csproj ./Ci_CD_Group_8/CI_CD_Group_8/
COPY Ci_CD_Group_8/CI_CD_Group_8.tests/*.csproj ./Ci_CD_Group_8/CI_CD_Group_8.tests/

# --------------------------------
# Återställ NuGet-paket
# --------------------------------
RUN dotnet restore

# --------------------------------
# Kopiera resten av källkoden
# --------------------------------
COPY . .

# --------------------------------
# Bygg och publicera applikationen
# Tester körs i CI – inte här
# --------------------------------
WORKDIR /app/Ci_CD_Group_8/CI_CD_Group_8
RUN dotnet publish -c Release -o /app/publish --no-restore

# ================================
# STEG 2: RUNTIME-STAGE
# ================================
# Använder lättare runtime-image
FROM mcr.microsoft.com/dotnet/runtime:9.0 AS runtime

# Sätter arbetskatalog
WORKDIR /app

# Kopierar endast den färdigbyggda applikationen
COPY --from=build /app/publish .

# --------------------------------
# Startkommando för containern
# --------------------------------
ENTRYPOINT ["dotnet", "CI_CD_Group_8.dll"]

