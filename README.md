# StartCheckerApp

**Mobilní aplikace pro správu startovních listin a sledování závodníků v orientačním běhu**  
Autor: Jan Nechanický  
Vedoucí: doc. Ing. Vítězslav Beran, Ph.D.  
Datum vytvoření: 1. 2. 2025  

---
# Postup pro otevření a spuštění řešení

## Požadavky
- **Visual Studio 2022** (v17.4+) nebo **Visual Studio 2022 for Mac**  
- **.NET 9 SDK** (nebo vyšší)  
- Workload **Mobile development with .NET** (MAUI)  
- **Android SDK** + emulátor nebo připojené Android zařízení  
- **Windows 10** (pro cílení Windows Desktop)

---

## 1. Získání kódu
1. Obdržíte ZIP s projektem.  
2. Rozbalte ZIP do libovolného adresáře.

## 2. Otevření řešení
1. Spusťte **Visual Studio**.  
2. **File → Open → Project/Solution**  
3. Vyberte `StartCheckerApp.sln`.

## 3. Obnovení závislostí
1. V **Solution Explorer** klikněte pravým tlačítkem na řešení.  
2. Zvolte **Restore NuGet Packages**.  
3. Počkejte na dokončení (ve výstupu se zobrazí „Restore completed“).

## 4. Nastavení startup-projektu
1. V **Solution Explorer** vyberte projekt:
   - Pro Android: **StartCheckerApp (net9.0-android)**
   - Pro Windows: **StartCheckerApp (net9.0-windows10.0.19041.0)**
2. Pravým tlačítkem → **Set as Startup Project**

## 5. Volba cílové platformy / emulátoru
- **Android**  
  - V rozbalovacím seznamu vedle tlačítka ▶ vyberte Android Emulátor nebo připojené zařízení.
- **Windows**  
  - V rozbalovacím seznamu zvolte **(Windows Machine)**.

## 6. Build & Run
1. Klikněte na ▶ (**Start Debugging**)  
   nebo stiskněte **Ctrl + F5** (**Start Without Debugging**).  
2. Aplikace se zkompiluje, nasadí a spustí na zvolené platformě.

---

### Tipy
- Při chybě **„SDK not found“** spusťte **Visual Studio Installer** a nainstalujte .NET MAUI workload.  
- Pro Android: v **Android SDK Manager** nainstalujte API Level 31+.  
- Pro Windows Desktop: povolte komponentu **.NET Windows Desktop** v “Individual Components”.

---

## Popis klíčových komponent

### Models
- **Runner.cs**  
  - Vlastnosti běžce (ID, jméno, čip, časy, flagy)  
  - `ComputedStatus` pro UI  
- **RaceData.cs**  
  - Metadata závodu (ID, název, seznam běžců, status, timestamp)  

### Services
- **RaceDataService**  
  - `AddRaceAsync` – založení závodu na serveru  
  - `UploadStartlistAsync` – nahrání IOF XML  
  - CRUD v paměti + synchronizace (`SyncRunnersWithServer`)  
- **RunnerDatabaseService**  
  - `InitializeAsync` – vytvoření tabulky  
  - CRUD v SQLite + indexy + řazení  

### Views
- **MainPage** – navigační menu  
- **GetStartlistPage** – stránka „Načíst startovní listinu“  
- **FullListPage** – kompletní startovní listina  
- **CurrentMinutePage** – startovní listina pro aktuální minutu  
- **RunnerDetailPage** – detail závodníka  
- **SettingsPage** – nastavení: vytvoření závodu, upload, časový posun, USB zařízení 

---
## Struktura složek a souborů
Pozn. Veškeré metody a třídy jsou důkladně komentovány v jednotlivých souborech – najdete je v horní části.

/Messages/
└─ RunnerUpdatedMessage.cs
/Models/
├─ Runner.cs
├─ RunnerGroup.cs
└─ RaceData.cs
/Platforms/
/Properties/
/Resources/
/Services/
├─ IMessageService.cs
├─ RaceDataService.cs
├─ RunnerDatabaseService.cs
├─ SportIdentFrameParser.cs
├─ StatusToColorConverter.cs
└─ UsbCommunicationService.cs
/Views/
├─ BaseRunnersPage.xaml(.cs)
├─ CurrentMinutePage.xaml(.cs)
├─ FullListPage.xaml(.cs)
├─ GetStartlistPage.xaml(.cs)
├─ MainPage.xaml(.cs)
├─ RunnerDetailPage.xaml(.cs)
└─ SettingsPage.xaml(.cs)
/App.xaml(.cs)
/AppShell.xaml(.cs)
/Constants.cs
/MauiProgram.cs
/README.md
/StartCheckerApp_TemporaryKey.pfx
/StartCheckerApp.csproj
/StartCheckerApp.sln


| Složka / Soubor                             | Popis                                                                   |
|---------------------------------------------|-------------------------------------------------------------------------|
| **StartCheckerApp.sln**                     | Visual Studio řešení                                                    |
| **StartCheckerApp.csproj**                  | Projektová konfigurace (.NET MAUI)                                      |
| **Constants.cs**                            | Sdílené konstanty napříč aplikací                                       |
| **MauiProgram.cs**                          | Bootstrap .NET MAUI, DI kontejner, registrace služeb                    |
| **App.xaml(.cs)**                           | Globální zdroje a spuštění aplikace                                     |
| **AppShell.xaml(.cs)**                      | Definice hlavní navigační struktury (Shell)                             |
| **Messages/RunnerUpdatedMessage.cs**        | Mediatorová zpráva pro notifikace změny Runneru                         |
| **Models/Runner.cs**                        | Datový model závodníka                                                  |
| **Models/RunnerGroup.cs**                   | Skupina Runnerů sdružená podle minuty startu                            |
| **Models/RaceData.cs**                      | Model metadat závodu (ID, název, startovní listina, status, čas)        |
| **Services/IMessageService.cs**             | Abstrakce pro zobrazování systémových zpráv (alert/toast)               |
| **Services/RaceDataService.cs**             | Správa REST volání, stav závodu a synchronizace dat                     |
| **Services/RunnerDatabaseService.cs**       | SQLite CRUD operace pro `Runner` + indexování a řazení                  |
| **Services/SportIdentFrameParser.cs**       | Parser rámců dat ze SportIdent čtečky                                   |
| **Services/StatusToColorConverter.cs**      | Převod stavu Runneru na barvu pro UI                                    |
| **Services/UsbCommunicationService.cs**     | Komunikace s USB čtečkami (SportIdent)                                  |
| **Views/BaseRunnersPage.xaml(.cs)**         | Sdílená logika a layout pro stránky s výpisem Runnerů                   |
| **Views/GetStartlistPage.xaml(.cs)**        | Načtení startovní listiny ze serveru a její uložení                     |
| **Views/FullListPage.xaml(.cs)**            | Zobrazení kompletního seznamu závodníků                                 |
| **Views/CurrentMinutePage.xaml(.cs)**       | Zobrazení běžců startujících v aktuální minutě                          |
| **Views/MainPage.xaml(.cs)**                | Úvodní obrazovka s navigačními tlačítky                                 |
| **Views/RunnerDetailPage.xaml(.cs)**        | Detailní úprava a zobrazení informací o jednom závodníkovi              |
| **Views/SettingsPage.xaml(.cs)**            | Vytvoření závodu, upload XML, časový posun, detekce USB                 |
| **Resources/**                              | Grafika, fonty, styly aplikace                                          |
| **Platforms/**                              | Platform-specifické zdroje a konfigurace                                |
| **Properties/**                             | Projektové vlastnosti (.NET MAUI manifest apod.)                        |
| **StartCheckerApp_TemporaryKey.pfx**        | Dočasný certifikát pro podpis APK                                       |
| **README.md**                               | Tento soubor – přehled struktury, instalace a spuštění                  |



