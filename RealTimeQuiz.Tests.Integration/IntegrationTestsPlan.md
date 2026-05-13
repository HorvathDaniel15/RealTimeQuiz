# Integrációs Tesztek Tervezete (RealTimeQuiz)

A tesztek célja: Nem az elszigetelt belső metódusok, hanem a rétegek (Controller, Logic, Adatbázis) együttműködésének vizsgálata "Happy Path" illetve "Negatív ág" fókusszal. Az adatbázis EF Core In-Memory lesz beállítva.

## 1. Lépés: Az alapinfrastruktúra (Setup)
A tesztek futtatásához szükséges környezet és eszközök konfigurálása.
* **Cél fájlok:** `RealTimeQuizWebApplicationFactory.cs`, `BaseIntegrationTest.cs` (vagy hasonló ősosztály)
* **Feladatok:**
  * Egy felülírt `WebApplicationFactory` létrehozása, amely a valós adatbázis konfigurációt kicseréli egy In-Memory Database provider-re.
  * Dependency Injection szolgáltatások (pl. Auth, DbContext) szükség szerinti mockolása vagy test-specifikus override-ja.
  * Alap HttpClient generálása a későbbi http kérések szimulálásához.

## 2. Lépés: Autentikáció Tesztek
A regisztrációs és bejelentkezési folyamatok tesztelése, hiszen minden más működéséhez API token szükséges.
* **Cél fájl:** `AuthIntegrationTests.cs`
* **Tesztesetek:**
  * `Register_ShouldCreateUser()`: Egy valid felhasználó regisztrációjakor Siker (200 OK) visszajelzés várható.
  * `Login_ShouldReturnJwtToken()`: Sikeres belépés után a rendszernek egy helyes szerkezetű JWT tokent kell kiadnia.

## 3. Lépés: Kvíz Tesztek
Bejelentkezett admin/felhasználó által történő kvíz manipuláció tesztelése.
* **Cél fájl:** `QuizIntegrationTests.cs`
* **Tesztesetek:**
  * Segédfüggvény használata: Automatikusan regisztrál/bejelentkezik és tokent szerez a kérések Authorization headerjébe.
  * `CreateQuiz_ShouldReturnCreatedQuiz()`: Egy valid kvíz JSON elküldése után a rendszer elmenti azt és visszaadja a kérést az új id-vel.
  * `GetQuizzes_ShouldReturnUserQuizzes()`: Lekérdezi a bejelentkezett felhasználóhoz tartozó kvízeket és ellenőrzi a darabszámot, tartalmat.

## 4. Lépés: Session és Játékmenet Tesztek
A valós idejű futás szimulálása HTTP hívások formájában: meccs indítása, csatlakozás, válaszadás.
* **Cél fájl:** `SessionIntegrationTests.cs`
* **Tesztesetek:**
  * `StartSession_ShouldGeneratePin()`: Kvíz indításakor legenerálódik a 6 számjegyű PIN.
  * `JoinSession_WithValidPin_ShouldReturnSuccess()`: Játékosként a helyes PIN kóddal be lehet lépni a szobába.
  * `JoinSession_WithInvalidPin_ShouldReturnError()`: Hibás PIN kód használatakor a rendszer megfelelő stádusz kóddal (pl. 404/400) dobja vissza a kérést.
  * `SubmitAnswer_ShouldUpdateScore()`: Egy adott kérdésre jó válasz beküldése elmentődik.
  * `GetLeaderboard_ShouldReturnCorrectOrder()`: A végpont visszadja a ranglistát, ahol az előző teszt alapján megfelelő pontszámmal szerepel az adott participant.

