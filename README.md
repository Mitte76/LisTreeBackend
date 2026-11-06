# LisTreeBackend

Lösningen innehåller ett ASP.NET Core Web API med .NET 10 och PostgreSQL, projekten `Application`, `Domain` och `Infrastructure`, samt den fristående Blazor WebAssembly-appen `LisTreeWeb` med PWA-stöd.

## Förutsättningar på en ny Windows-dator

Installera följande innan du börjar:

1. **Git** – för att hämta källkoden.
2. **.NET 10 SDK** – installera SDK:n, inte bara .NET Runtime. Kontrollera installationen i PowerShell:
   ```powershell
   dotnet --list-sdks
   ```
   Listan ska innehålla en version som börjar med `10.`.
3. **PostgreSQL** – databasservern kör på en separat dator. Ta reda på datorns nätverksnamn eller IP-adress, port (vanligen `5432`), databasnamn och PostgreSQL-användarens inloggningsuppgifter.
4. **Visual Studio 2026** (valfritt) med arbetsbelastningen **ASP.NET och webbutveckling** om du vill arbeta i Visual Studio. Det går också att bygga och köra projektet från terminalen.

Datorn som kör API:t måste kunna nå databasservern över nätverket. PostgreSQL måste acceptera TCP-anslutningar från API-datorn, serverns brandvägg måste tillåta PostgreSQL-porten från den datorn, och PostgreSQLs `pg_hba.conf` måste tillåta API-datorns IP-adress för databasanvändaren. Begränsa åtkomsten till nödvändiga datorer; undvik att öppna porten för alla adresser.

## Hämta och öppna lösningen

Öppna PowerShell och kör:

```powershell
git clone https://github.com/Mitte76/LisTreeBackend.git
cd LisTreeBackend
```

Öppna lösningen i Visual Studio 2026 genom att öppna `LisTreeBackend.slnx` i den här mappen. Visual Studio återställer NuGet-paketen automatiskt när lösningen läses in. Från terminalen kan du återställa dem med:

```powershell
dotnet restore .\LisTreeBackend.slnx
```

## Skapa och konfigurera databasen

API:t är konfigurerat för PostgreSQL-servern `192.168.1.150` på port `5432` och databasen `listree`. Det här är nätverksadressen till datorn där PostgreSQL kör, inte `localhost`.

Spara anslutningen lokalt med .NET User Secrets så att lösenordet inte checkas in. Kör från lösningens rot och ersätt `<DATABASANVÄNDARE>` och `<DITT_LOSENORD>` med dina egna inloggningsuppgifter:

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=192.168.1.150;Port=5432;Database=listree;Username=<DATABASANVÄNDARE>;Password=<DITT_LOSENORD>" --project .\LisTreeBackend\API.csproj
```

Behåll citattecknen runt anslutningssträngen. User Secrets är lokal konfiguration för din dator och behöver inte checkas in.

## Skapa databasschemat

Projektet innehåller Entity Framework Core-migreringar, men API:t kör dem inte automatiskt vid start. Installera EF Core-kommandoradsverktyget en gång om du inte redan har det:

```powershell
dotnet tool install --global dotnet-ef --version 10.0.12
```

Kör sedan följande i samma PowerShell-fönster. Sätt miljövariabeln till samma värden som i User Secrets, inklusive databasserverns värdnamn eller IP-adress. Design-time-fabriken för migreringar läser `ConnectionStrings__DefaultConnection` från miljövariabler:

```powershell
$env:ConnectionStrings__DefaultConnection = "Host=192.168.1.150;Port=5432;Database=listree;Username=<DATABASANVÄNDARE>;Password=<DITT_LOSENORD>"
dotnet ef database update --project .\Infrastructure\Infrastructure.csproj --startup-project .\LisTreeBackend\API.csproj
```

Kommandot ansluter till databasservern och skapar tabellerna i databasen. Miljövariabeln gäller bara det aktuella PowerShell-fönstret. Om du stänger det innan migreringen är klar behöver du sätta variabeln igen.

## Bygg och starta API:t

Från lösningens rot:

```powershell
dotnet build .\LisTreeBackend.slnx
dotnet run --project .\LisTreeBackend\API.csproj --launch-profile https
```

Startprofilen `https` använder utvecklingsmiljön och adresserna `https://localhost:7164` och `http://localhost:5226`. I utvecklingsmiljön finns Swagger UI på:

- <https://localhost:7164/swagger>
- <http://localhost:5226/swagger>

Om webbläsaren varnar för utvecklingscertifikatet kan du skapa och lita på .NET:s lokala HTTPS-certifikat med:

```powershell
dotnet dev-certs https --trust
```

API:ts användarändpunkter finns under `/api/Users`.

## Starta Blazor-appen

Starta `LisTreeWeb` med HTTPS-profilen i Visual Studio. Första gången behöver appen laddas klart online. När service workern har aktiverats, ladda om sidan en gång medan den fortfarande körs; de Blazor-resurser som laddas sparas då i webbläsarens cache. Därefter kan du stoppa appen i Visual Studio och öppna eller ladda om samma adress offline i samma webbläsare. Statusindikatorn kontrollerar om webbservern för `LisTreeWeb` kan nås och uppdateras ungefär var femte sekund; den visar inte om API:t eller internet i övrigt är tillgängligt.
