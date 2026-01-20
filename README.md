Personnummerkontroll i C#

Projektbeskrivning

Detta projekt är en C#-baserad konsolapplikation som kontrollerar om ett svenskt personnummer är giltigt enligt svenska regler.
Projektet innehåller även enhetstester med xUnit, automatiserad testning via GitHub Actions samt en Docker-container för att kunna köra applikationen i en isolerad miljö.

Syftet med projektet är att visa:

* Grundläggande C#-programmering
* Enhetstestning
* CI/CD med GitHub Actions
* Docker-containerisering


Svenska regler för personnummer 

Ett svenskt personnummer består av:

* Födelsedatum** (YYMMDD eller YYYYMMDD)
* Tre individnummer**
* En kontrollsiffra**

Regler som kontrolleras i applikationen:

* Personnumret får inte vara tomt
* Bindestreck och mellanslag ignoreras
* Endast siffror tillåts
* Längden måste vara 10 eller 12 siffror
* Datumdelen måste vara ett giltigt datum
* Kontrollsiffran verifieras med **Luhn-algoritmen**



Köra applikationen lokalt (utan Docker)

 Krav:

* .NET SDK 8.0 (LTS)
* Visual Studio eller .NET CLI

Steg:

1. Klona repot:

bash
git clone https://github.com/<ditt-användarnamn>/CI_CD_Group_8.git


2. Gå till projektmappen:

bash
cd CI_CD_Group_8


3. Kör applikationen:

bash
dotnet run --project CI_CD_Group_8


4. Ange ett personnummer när programmet frågar:

text
Skriv personnummer (YYMMDD-XXXX):


Programmet skriver ut om personnumret är giltigt eller inte.



Köra tester lokalt

Projektet använder **xUnit** för enhetstester.

 Kör alla tester:

bash
dotnet test


Vad som testas:

* Giltiga personnummer
* Ogiltiga personnummer (fel datum, fel kontrollsiffra, bokstäver, tom input)

Om alla tester passerar visas:

text
Passed: X, Failed: 0



  CI – GitHub Actions

Projektet använder GitHub Actions för kontinuerlig integration.

 Vad som sker automatiskt:

* Projektet byggs
* Alla enhetstester körs
* Workflow stoppas om tester misslyckas

CI-konfigurationen finns i:

text
.github/workflows/ci.yml



 Docker – Köra applikationen i container

Krav:

Docker installerat

Bygg Docker-imagen lokalt:

bash
docker build -t personnummervalidator .


 Kör containern:

bash
docker run -it personnummervalidator


Applikationen startar och ber om ett personnummer precis som lokalt.


DockerHub

Docker-imagen byggs och publiceras automatiskt till DockerHub via GitHub Actions.

Autentisering sker via **GitHub Secrets** (användarnamn och token), vilket gör lösningen säker.


 Projektstruktur 

¨¨
CI_CD_Group_8
├── CI_CD_Group_8
│   ├── Program.cs
│   ├── PersonnummerValidator.cs
│   └── CI_CD_Group_8.csproj
├── CI_CD_Group_8.Tests
│   ├── PersonnummerValidatorTests.cs
│   └── CI_CD_Group_8.Tests.csproj
├── .github/workflows/ci.yml
├── Dockerfile
└── README.md
¨¨¨¨


Projektet uppfyller samtliga krav i uppgiften:

* C#-konsolapplikation för personnummerkontroll
* Enhetstester med xUnit
* CI med GitHub Actions
* Docker-container och publicering till DockerHub
* Dokumentation i README.md



