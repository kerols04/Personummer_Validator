# ---------- BUILD ----------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY CI_CD_Group_8/CI_CD_Group_8.sln .
COPY CI_CD_Group_8/CI_CD_Group_8/ CI_CD_Group_8/
COPY CI_CD_Group_8/CI_CD_Group_8.Tests/ CI_CD_Group_8.Tests/

RUN dotnet restore CI_CD_Group_8.sln

COPY . .
WORKDIR /src/CI_CD_Group_8/CI_CD_Group_8
RUN dotnet publish -c Release -o /app/publish

# ---------- RUNTIME ----------
FROM mcr.microsoft.com/dotnet/runtime:9.0
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "CI_CD_Group_8.dll"]
