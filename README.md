# RealTimeQuiz

Egy real-time kvíz alkalmazás, amely lehetővé teszi interaktív kvízek létrehozását és lebonyolítását.

## Projekt Struktúra

A projekt tiszta architektúrát követ több rétegre osztva:

```
RealTimeQuiz/
├── RealTimeQuiz.Model/          # Domain entitások és enumok
├── RealTimeQuiz.Data/           # Adatbázis hozzáférés és repository-k
├── RealTimeQuiz.Logic/          # Üzleti logika és use case-ek
├── RealTimeQuiz.WebAPI/         # ASP.NET Core Web API
├── RealTimeQuiz.Client/         # Angular frontend
├── RealTimeQuiz.Tests.Integration/  # Integrációs tesztek
└── RealTimeQuiz.Tests.E2E/      # End-to-end tesztek
```

## Technológiai Stack

### Backend
- **.NET 8.0** - Backend framework
- **ASP.NET Core Web API** - REST API
- **Entity Framework Core** - ORM
- **PostgreSQL** - Adatbázis
- **ASP.NET Core Identity** - Autentikáció

### Frontend
- **Angular 20** - Frontend framework
- **TypeScript** - Programozási nyelv

## Fejlesztői Dokumentáció

📖 **[ARCHITECTURE.md](./ARCHITECTURE.md)** - Részletes architektúra dokumentáció, amely tartalmazza:
- Rétegek felelősségeinek leírását
- MVP funkciólista
- Főbb use case-ek és réteg-hozzárendelések
- Fejlesztési irányelvek
- Best practice-ek és példák

## Gyors Kezdés

### Előfeltételek

- .NET 8.0 SDK
- Node.js és npm (Angular-hoz)
- PostgreSQL

### Backend Futtatása

```bash
cd RealTimeQuiz.WebAPI
dotnet restore
dotnet run
```

A Web API elérhető lesz a `https://localhost:5001` címen (vagy a konfigurált porton).

### Frontend Futtatása

```bash
cd RealTimeQuiz.Client
npm install
ng serve
```

Az Angular alkalmazás elérhető lesz a `http://localhost:4200` címen.

## MVP Funkciók

Az alkalmazás első működő verziója (MVP) a következő funkciókat támogatja:

### Quiz Kezelés
- ✅ Quiz létrehozása és szerkesztése
- ✅ Kérdések és válaszopciók hozzáadása
- ✅ Quiz publikálása

### Session Management
- ✅ Kvíz session indítása egyedi PIN-nel
- ✅ Résztvevők csatlakozása PIN alapján
- ✅ Real-time session vezérlés

### Kvíz Lebonyolítás
- ✅ Kérdések egymás utáni megjelenítése
- ✅ Résztvevők válaszainak fogadása
- ✅ Időbélyegzett válaszok (késő/elfogadott)
- ✅ Eredmények megjelenítése

### Autentikáció
- ✅ Regisztráció és bejelentkezés kvíz tulajdonosoknak
- ✅ Név alapú csatlakozás résztvevőknek

## Tesztelés

```bash
# Integrációs tesztek futtatása
dotnet test RealTimeQuiz.Tests.Integration

# E2E tesztek futtatása
dotnet test RealTimeQuiz.Tests.E2E
```

## Fejlesztési Irányelvek

Kérjük, olvasd el az [ARCHITECTURE.md](./ARCHITECTURE.md) dokumentumot, mielőtt hozzákezdel a fejlesztéshez. Ez a dokumentum tisztázza:

- Hogy melyik kód melyik rétegbe tartozik
- Hogyan kell új funkciókat implementálni
- Milyen felelősségei vannak az egyes rétegeknek
- Milyen anti-pattern-öket kell elkerülni

## Hozzájárulás

1. Olvasd el az [ARCHITECTURE.md](./ARCHITECTURE.md) dokumentumot
2. Hozz létre egy feature branch-et
3. Commit-old a változtatásokat
4. Nyiss egy Pull Request-et

## Licensz

[Licensz információ]
