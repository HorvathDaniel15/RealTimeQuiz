# SessionService helper metódus útmutató

Ez az útmutató abban segít, hogy a `SessionService` private helper metódusai tiszta felelősségekkel készüljenek el.
Cél: a publikus use case-ek (`CreateSessionAsync`, `StartSessionAsync`, stb.) csak orchestráljanak, a szabályok pedig helper szinten legyenek egységesen kezelve.

## Közös alapelvek

- A controller ne döntsön állapotváltásról; ezt mindig a service intézze.
- Input validáció és adatbázis-betöltés legyen külön felelősség.
- Exception típusok legyenek konzisztensen használva:
  - `BusinessValidationException`: hibás bemenet vagy üzleti szabály sérül.
  - `EntityNotFoundException`: nem létező erőforrás.
  - `ForbiddenOperationException`: nem a jelenlegi felhasználóhoz tartozó erőforrás.
- Állapotváltás előtt mindig legyen explicit állapotellenőrzés (`EnsureState`).

---

## 1) ValidateOwnerId

**Cél**
- Ellenőrzi, hogy van-e érvényes aktuális user azonosító.

**Bemenet**
- `ownerId`

**Ellenőrzések**
- Ne legyen null, üres vagy whitespace.

**Hiba esetén**
- `BusinessValidationException`

**Mellékhatás**
- Nincs.

**Hol használd**
- Minden publikus service metódus elején.

---

## 2) ValidateSessionId

**Cél**
- A session azonosító formai validációja.

**Bemenet**
- `sessionId`

**Ellenőrzések**
- Pozitív szám legyen.

**Hiba esetén**
- `BusinessValidationException`

**Mellékhatás**
- Nincs.

**Fontos**
- Itt ne legyen repository hívás és létezés-ellenőrzés.

---

## 3) ValidateCreateSessionRequest

**Cél**
- Session létrehozás request alapellenőrzése.

**Bemenet**
- `request`

**Ellenőrzések**
- A request objektum ne legyen null.
- A `QuizId` legyen pozitív.

**Hiba esetén**
- `BusinessValidationException`

**Mellékhatás**
- Nincs.

**Megjegyzés**
- Most nem szükséges külön "level rules" helper, mert kevés mező van.

---

## 4) LoadOwnedSessionAsync

**Cél**
- Betölti a sessiont, és ellenőrzi a tulajdonjogot.

**Bemenet**
- `sessionId`, `ownerId`, `cancellationToken`

**Várt működés**
- Session betöltése a repository-ból.
- Ha nincs ilyen session: not found.
- Ha nem a jelenlegi user quizéhez tartozik: forbidden.

**Hiba esetén**
- `EntityNotFoundException`
- `ForbiddenOperationException`

**Mellékhatás**
- Nincs, csak beolvasás.

**Hol használd**
- Minden olyan use case-ben, ahol meglévő sessionnel dolgozol.

---

## 5) LoadOwnedQuizAsync

**Cél**
- Betölti a quizt, és ellenőrzi, hogy a jelenlegi felhasználóé.

**Bemenet**
- `quizId`, `ownerId`, `cancellationToken`

**Várt működés**
- Quiz betöltése (részletesen, ha kérdések is kellenek).
- Ha nincs ilyen quiz: not found.
- Ha nem a felhasználóé: forbidden.

**Hiba esetén**
- `EntityNotFoundException`
- `ForbiddenOperationException`

**Mellékhatás**
- Nincs, csak beolvasás.

**Hol használd**
- `CreateSessionAsync`-ban kötelező.

---

## 6) GetOrderedQuestions

**Cél**
- Visszaadja a quiz kérdéseit stabil sorrendben.

**Bemenet**
- `quiz`

**Várt működés**
- Kérdések rendezése `OrderIndex` alapján.

**Hiba esetén**
- Nincs kivétel dobási kényszer, de a hívó oldalon kezeld az üres listát.

**Mellékhatás**
- Nincs.

---

## 7) GetFirstQuestion

**Cél**
- Megadja az első kérdést a rendezett listából.

**Bemenet**
- `quiz`

**Várt működés**
- Az első kérdés visszaadása, ha létezik.
- Ha nincs kérdés: null.

**Mellékhatás**
- Nincs.

---

## 8) GetNextQuestion

**Cél**
- Megadja a következő kérdést az aktuális után.

**Bemenet**
- `quiz`, `currentQuestionId`

**Várt működés**
- Keresse meg az aktuális kérdést a rendezett listában.
- Ha van utána elem: azt adja vissza.
- Ha nincs további kérdés: null.

**Üzleti döntés**
- Ha a `currentQuestionId` nem található, ez jellemzően inkonzisztens állapot; erről dobható üzleti kivétel.

**Mellékhatás**
- Nincs.

---

## 9) EnsureState

**Cél**
- Kizárólag itt dőljön el, hogy egy állapotváltás engedélyezett-e.

**Bemenet**
- `session`, `allowedStates`

**Várt működés**
- Ha a session állapota benne van az engedélyezett állapotok között: továbblépés.
- Ha nincs: tiltott művelet.

**Hiba esetén**
- Általában `ForbiddenOperationException`.

**Mellékhatás**
- Nincs.

**Megjegyzés**
- Itt legyen a legfontosabb állapotgép-szabály központosítva.

---

## 10) GenerateUniqueJoinPinAsync

**Cél**
- Új, egyedi csatlakozási PIN generálása.

**Bemenet**
- `cancellationToken`

**Várt működés**
- PIN formátum előállítása.
- Repository-val ellenőrzés, hogy még nem létezik.
- Ütközés esetén újragenerálás.

**Mellékhatás**
- Adatbázis olvasás (existence check).

**Gyakorlati tipp**
- Legyen próbálkozási limit és egy fallback hiba, ha extrém sok ütközés lenne.

---

## 11) MapToCreateSessionResultDto

**Cél**
- Session entitásból létrehozás utáni DTO-t készít.

**Bemenet**
- `quizSession`

**Várt mezők**
- `Id`, `QuizId`, `JoinPin`, `State`, `CreatedAtUtc`.

**Mellékhatás**
- Nincs.

---

## 12) MapToSessionLifecycleResultDto

**Cél**
- Állapotváltások utáni visszatérési DTO készítése.

**Bemenet**
- `quizSession`

**Várt mezők**
- Állapot és lifecycle timestamp mezők.
- Aktuális kérdés azonosító és (ha kell) alap kérdésinformációk.

**Mellékhatás**
- Nincs.

---

## 13) MapToSessionDetailsDto

**Cél**
- Session részletes lekérdezési DTO összeállítása.

**Bemenet**
- `quizSession` (lehetőleg részletesen betöltve)

**Várt mezők**
- Session metaadatok, állapot, timestamp-ek.
- Résztvevőszám.
- `CurrentQuestion` rész objektum konzisztens kitöltése.

**Mellékhatás**
- Nincs.

---

## Javasolt implementálási sorrend

1. `ValidateOwnerId`, `ValidateSessionId`, `ValidateCreateSessionRequest`
2. `LoadOwnedQuizAsync`, `LoadOwnedSessionAsync`
3. `GetOrderedQuestions`, `GetFirstQuestion`, `GetNextQuestion`
4. `EnsureState`
5. `GenerateUniqueJoinPinAsync`
6. Mapper metódusok

Ezután a publikus metódusok már főleg sorrendvezérlések lesznek.

---

## Minimál teszt-checklist (helper fókusz)

- Hibát dob-e üres `ownerId` esetén.
- Hibát dob-e `sessionId <= 0` esetén.
- Hibát dob-e null request és nem pozitív `QuizId` esetén.
- Not found/forbidden helyesen jön-e a load helper-ekből.
- `EnsureState` tiltott átmenetnél tényleg hibát ad-e.
- PIN generálásnál ütközés esetén tud-e újrapróbálkozni.
- Mapper-ek nem hagynak-e inkonzisztens mezőt.

---

## Rövid használati minta publikus metódusokban (logikai sorrend)

- Input validálás (`ownerId`, `sessionId` vagy request).
- Entitás betöltés + ownership check.
- Állapotellenőrzés (`EnsureState`).
- Domain módosítás (state, current question, timestamp-ek).
- Mentés repository-n keresztül.
- DTO mapping és visszaadás.

