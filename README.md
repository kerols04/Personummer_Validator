
---

# Personnummerkontroll – C# Konsolapplikation (Grupp 8)

#Översikt

Detta projekt är en **C#-baserad konsolapplikation** som verifierar **svenska personnummer** enligt gällande regler.
Applikationen kan:

* kontrollera om ett personnummer är giltigt
* hantera olika format (`YYMMDD-XXXX`, `YYYYMMDDXXXX`, `+`, `-`)
* verifiera födelsedatum
* utföra kontrollsiffra med **Luhn-algoritmen**
* ge tydlig feedback om varför ett personnummer är ogiltigt

Projektet innehåller även:

* **enhetstester (xUnit)**
* **CI/CD med GitHub Actions**
* **Docker-container** som byggs och publiceras automatiskt till DockerHub

---

#Syfte med uppgiften

Målet med uppgiften är att:

* bygga en fungerande **C#-konsolapplikation**
* skriva **enhetstester** för applikationen
* använda **GitHub Actions** för automatiserad testning
* paketera applikationen i en **Docker-container**
* dokumentera hela lösningen tydligt i `README.md`

---

#Svenska regler för personnummer (kortfattat)

Ett svenskt personnummer består av:

```
ÅÅMMDD-XXXX  (eller YYYYMMDD-XXXX)
```

* **Datumdel**: måste vara ett giltigt datum
* **Separator**:

  * `-` → personen är under 100 år
  * `+` → personen är 100 år eller äldre
* **De sista fyra siffrorna**:

  * näst sista siffran används ofta som könsindikator (heuristik)
  * sista siffran är **kontrollsiffra** (Luhn)

Mer information:
[https://sv.wikipedia.org/wiki/Personnummer_i_Sverige](https://sv.wikipedia.org/wiki/Personnummer_i_Sverige)

---

# Hur applikationen fungerar

# 1. Inmatning

Användaren matar in ett personnummer i konsolen.

Exempel:

```
19900101-0017
9001010017
19900101+0017
```

#2. Validering

Applikationen kontrollerar:

1. Tom inmatning / fel längd
2. Att datumdelen är giltig
3. Rätt sekel (1800 / 1900 / 2000)
4. Kontrollsiffra med **Luhn-algoritmen**

#3. Resultat

* Vid **giltigt personnummer**:

  * normaliserat format visas
  * födelsedatum visas
  * kön (heuristik) visas
* Vid **ogiltigt personnummer**:

  * tydligt felmeddelande visas

---

# Enhetstester

Projektet använder **xUnit** för testning.

Tester finns för:

* tom inmatning
* fel längd
* ogiltigt datum
* ogiltig kontrollsiffra
* olika separatorer (`-`, `+`, inget)
* 10 vs 12 siffror
* normalisering av personnummer
* köns-heuristik

Tester körs automatiskt via **GitHub Actions** vid:

* push till `main`
* pull request mot `main`

---

# CI/CD – GitHub Actions

Projektet använder GitHub Actions för:

# Build & Test

* återställer NuGet-paket
* bygger applikationen
* kör alla enhetstester

#Docker Build & Deploy

* körs **endast om testerna lyckas**
* bygger Docker-image
* pushar image till **DockerHub**
* använder **GitHub Secrets** för säkra credentials

---

# Docker

### Dockerfile

Projektet använder **multi-stage build**:

1. **Build-stage**

   * använder `.NET SDK`
   * återställer paket
   * bygger och publicerar applikationen
2. **Runtime-stage**

   * använder lättare `.NET runtime`
   * innehåller endast färdig applikation

Detta ger:

* mindre image
* bättre säkerhet
* snabbare start

---

# Köra applikationen lokalt

# Krav

* .NET 9 SDK

# Kör lokalt

```bash
dotnet restore
dotnet build
dotnet run --project Ci_CD_Group_8/CI_CD_Group_8
```

---

#Köra tester lokalt

```bash
dotnet test
```

---

 Köra med Docker

 Bygg image

```bash
docker build -t swedish-personnummer-validator .
```

Kör container

```bash
docker run -it swedish-personnummer-validator
```

---

## 📦 DockerHub

Docker-imagen publiceras automatiskt till DockerHub via GitHub Actions:

```
<dockerhub-användarnamn>/swedish-personnummer-validator:latest
```

(Inloggning sker via GitHub Secrets)

---

 Projektstruktur (förenklad)

```
.
├── Ci_CD_Group_8/
│   ├── CI_CD_Group_8/          # Konsolapplikation
│   └── CI_CD_Group_8.tests/    # xUnit-tester
├── .github/workflows/ci.yml    # GitHub Actions
├── Dockerfile
├── CI_CD_Group_8.sln
└── README.md
```

---


Grupp 8

Redovisning sker **gruppvis fredag den 16:e**.

---

Sammanfattning

Detta projekt uppfyller samtliga krav i uppgiften:

* ✔ C#-konsolapplikation
* ✔ Enhetstester (xUnit)
* ✔ GitHub Actions (CI/CD)
* ✔ Docker-container
* ✔ Dokumentation i README

---


