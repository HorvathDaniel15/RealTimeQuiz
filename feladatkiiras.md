# Modern webalkalmazások fejlesztése .NET környezetben
**2025/2026 tavaszi félév**

## 1. Feladat összefoglalása
Készítsünk egy online, valós idejű kvízrendszert (Kahoot jellegű alkalmazást). A rendszerben a felhasználók által létrehozott kvízekben a résztvevők kérdésekre válaszolhatnak meghatározott időkereten belül.

- **Architektúra:** kliens-szerver felépítés
- **Vezérlés:** központilag irányított, valós idejű frissítésekkel
- **Kapacitás:** tetszőleges számú résztvevő támogatása

---

## 2. Általános követelmények
- **Backend:** REST WebAPI ASP.NET-ben, C# nyelven, MVC architektúra szerint
- **Kliens:** résztvevői és adminisztrációs felület (web, asztali vagy mobil)
- **Adattárolás:** relációs adatbázis, Entity Framework (ORM)
- **Biztonság:**
  - ASP.NET Identity az autentikációhoz és jogosultságkezeléshez
  - Biztonságos jelszótárolás (hashing)
  - A szervernek kell garantálnia az adatok biztonságát
- **Kódminőség:** jól szervezett, fordítási hiba és figyelmeztetés nélküli forráskód
  - Kötelező szabályrendszer: `ELTE.FI.SARuleset`
- **Tesztelés:** megfelelő számú integrációs teszt, legalább az alapfunkciókra

---

## 3. Dokumentáció követelményei
A dokumentációt **PDF formátumban** kell leadni, az alábbi tartalommal:

- Fejlesztő adatai, feladatleírás és funkcionális elemzés
- **Fejlesztői rész:**
  - statikus terv (UML komponens- és osztálydiagram)
  - adatbázis séma diagram
  - WebAPI interfész leírás
  - tesztesetek
- **Tiltás:** ne tartalmazzon kódrészleteket vagy képernyőképeket

---

## 4. Pontozás és funkcionalitás

### 4.1. REST alapfeladat (2 pont)
- **Résztvevői oldal:**
  - regisztráció / bejelentkezés
  - PIN-kódos csatlakozás (belépés nélkül is)
  - aktuális kérdés megválaszolása az időkereten belül
  - eredmények megtekintése
- **Adminisztrációs oldal:**
  - kvízek listázása
  - indítása
  - vezérlése (aktiválás, lezárás, léptetés)
- **Kvíz létrehozása:**
  - dinamikus kérdésszám (minimum 1)
  - minimum 2 válasz / kérdés
  - helyes válasz megjelölése

### 4.2. SignalR alapfeladat (1 pont)
- Valós idejű frissítés a résztvevői felületen (megjelenítés, lezárás, léptetés) oldalújratöltés nélkül

### 4.3. Választható részfeladatok
| Feladat | Pontszám | Leírás |
| :--- | :---: | :--- |
| Időzített kérdések | 0,5 pont | Kérdésenkénti időkorlát és valós idejű visszaszámláló |
| Ranglista | 1 pont | Rangsorszámítás és megjelenítés a kérdések között és a végén |
| Multimédia | 0,5 pont | Képek vagy videók csatolása (minimum kérdésenként egy kép) |
| OAuth2 | 0,5 pont | JWT és Refresh Token Flow alapú hitelesítés |
| 2FA | 0,5 pont | Kétfaktoros (email alapú) hitelesítés |
| Elfelejtett jelszó | 0,5 pont | Emailben küldött lejáró jelszó-visszaállító link vagy kód |
| Szerkesztés | 0,5 pont | Kiírt, de el nem kezdett kvízek módosítása |
| Lapozás | 0,5 pont | Szerveroldali lapozás nagy mennyiségű kvíz esetén |
| Élő chat | 1 pont | SignalR chat, ahol a korábbi üzenetek is látszanak |
| Rendszeradmin | 1 pont | Összes kvíz törlése, felhasználók kezelése, RBAC jogosultságok |

---

*Az értékelés az elfogadott részfeladatok pontszámának egész része.*

