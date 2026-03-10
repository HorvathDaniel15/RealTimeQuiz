# RealTimeQuiz – adatmodell és repository vázlat

> Ez a dokumentum egy tervezési összefoglaló a `RealTimeQuiz` projekthez.
> Célja, hogy később is gyorsan visszanézhető legyen, hogyan érdemes felépíteni a `Model`, `Data` és részben a `Logic` réteget.

---

## 1. Kiindulás

A feladat alapján egy **Kahoot-szerű, valós idejű kvízrendszert** kell készíteni.
A fontosabb backend elvárások:

- ASP.NET WebAPI
- Entity Framework alapú relációs adatbázis
- ASP.NET Identity autentikációhoz és jogosultságkezeléshez
- PIN-kódos csatlakozás belépés nélkül is
- adminisztrációs vezérlés
- kérdések időzített kezelése
- eredmények / ranglista megjelenítése

A modell tervezésénél ezért a domént érdemes **két nagy részre bontani**:

1. **Szerkeszthető kvíz tartalom**
   - mi maga a kvíz
   - milyen kérdések vannak benne
   - milyen válaszlehetőségek tartoznak hozzá

2. **Futó játékfolyamat**
   - melyik konkrét session fut éppen
   - kik csatlakoztak
   - ki mit válaszolt
   - milyen pontszám alakult ki

---

## 2. Javasolt fő entitások

### 2.1. `ApplicationUser`

Az ASP.NET Identity felhasználó entitása.

Feladata:

- regisztráció / bejelentkezés kezelése
- admin oldali tulajdonjog összekötése a kvízekkel
- opcionálisan résztvevői csatlakozás összekötése a belépett felhasználóval

Javasolt plusz kapcsolatok:

- `OwnedQuizzes`
- `Participations`

---

### 2.2. `Quiz`

Ez maga a szerkeszthető kvíz.

Javasolt mezők:

- `Id`
- `Title`
- `Description`
- `OwnerId`
- `Owner`
- `IsPublished`
- `CreatedAtUtc`
- `UpdatedAtUtc`
- `Questions`
- `Sessions`

Szerepe:

- egy admin / tulajdonos által létrehozott kvíz leírása
- ebből lehet később sessiont indítani

---

### 2.3. `QuizQuestion`

A kvíz egy kérdése.

Javasolt mezők:

- `Id`
- `QuizId`
- `Quiz`
- `Text`
- `OrderIndex`
- `TimeLimitSeconds`
- `ImageUrl`
- `Options`
- `Answers`

Szerepe:

- a kérdés szövegét tárolja
- tartalmazza a sorrendet
- itt lehet kérdésenkénti időkorlátot kezelni
- opcionálisan multimédiát is ide lehet kötni

---

### 2.4. `QuestionOption`

A kérdéshez tartozó válaszlehetőség.

Javasolt mezők:

- `Id`
- `QuizQuestionId`
- `QuizQuestion`
- `Text`
- `OrderIndex`
- `IsCorrect`

Szerepe:

- minimum 2 válaszlehetőség / kérdés
- pontosan 1 helyes válasz jelölése

---

### 2.5. `QuizSession`

Ez egy konkrét lejátszás egy adott kvízből.

Javasolt mezők:

- `Id`
- `QuizId`
- `Quiz`
- `JoinPin`
- `State`
- `CurrentQuestionId`
- `CreatedAtUtc`
- `StartedAtUtc`
- `QuestionOpenedAtUtc`
- `QuestionClosedAtUtc`
- `FinishedAtUtc`
- `Participants`
- `Answers`

Szerepe:

- a tényleges játékfolyamatot reprezentálja
- a PIN-kód ehhez a sessionhöz tartozik
- az admin ezt vezérli

---

### 2.6. `SessionParticipant`

Egy adott session résztvevője.

Javasolt mezők:

- `Id`
- `QuizSessionId`
- `QuizSession`
- `UserId` _(nullable, mert lehet vendég is)_
- `User`
- `DisplayName`
- `JoinedAtUtc`
- `TotalScore`
- `Answers`

Szerepe:

- kezeli a PIN-nel belépő játékosokat
- támogatja a belépett felhasználót és a névtelen csatlakozót is
- ranglistához jó hely a session-szintű összpontszámnak

---

### 2.7. `SessionAnswer`

Egy résztvevő konkrét válasza egy sessionben.

Javasolt mezők:

- `Id`
- `QuizSessionId`
- `QuizSession`
- `SessionParticipantId`
- `SessionParticipant`
- `QuizQuestionId`
- `QuizQuestion`
- `QuestionOptionId`
- `QuestionOption`
- `SubmittedAtUtc`
- `Status`
- `IsCorrect`
- `AwardedPoints`

Szerepe:

- naplózza, hogy ki melyik kérdésre mit válaszolt
- ebből lehet eredményt és statisztikát számolni

---

## 3. Enumok

### 3.1. `SessionState`

Javasolt értékek:

```csharp
public enum SessionState
{
    Draft = 0,
    Lobby = 1,
    QuestionOpen = 2,
    QuestionClosed = 3,
    Finished = 4,
    Cancelled = 5
}
```

Feladata:

- jelzi, hogy a session milyen állapotban van
- az API és a SignalR logika is ezt fogja figyelni

---

### 3.2. `SubmissionStatus`

Javasolt értékek:

```csharp
public enum SubmissionStatus
{
    Accepted = 0,
    Late = 1,
    Rejected = 2
}
```

Feladata:

- megkülönbözteti az időben beérkezett, késői vagy érvénytelen válaszokat

---

## 4. Kapcsolatok összefoglalva

### Fő kapcsolatok

- `ApplicationUser 1 - N Quiz`
- `ApplicationUser 1 - N SessionParticipant` _(opcionális kapcsolat)_
- `Quiz 1 - N QuizQuestion`
- `QuizQuestion 1 - N QuestionOption`
- `Quiz 1 - N QuizSession`
- `QuizSession 1 - N SessionParticipant`
- `QuizSession 1 - N SessionAnswer`
- `SessionParticipant 1 - N SessionAnswer`
- `QuizQuestion 1 - N SessionAnswer`

### Fontos megjegyzés

A `Quiz` és a `QuizSession` **nem ugyanaz**.

- `Quiz` = a sablon / szerkeszthető kvíz
- `QuizSession` = a konkrét lejátszás

Ez a szétválasztás nagyon fontos, mert ugyanazt a kvízt többször is el lehet indítani.

---

## 5. Fő üzleti szabályok

Ezeket részben adatbázis, részben repository / service rétegben kell védeni.

### Quiz szabályok

- egy kvízben legalább **1 kérdés** legyen
- egy kérdéshez legalább **2 válaszlehetőség** tartozzon
- egy kérdéshez **pontosan 1 helyes válasz** legyen
- a kérdések sorrendje legyen explicit `OrderIndex` alapján
- a válaszlehetőségek sorrendje is legyen explicit `OrderIndex`

### Session szabályok

- session csak létező quizből indulhat
- a `JoinPin` legyen egyedi
- csak az admin / tulajdonos vezérelhesse a sessiont
- egyszerre csak egy aktuális kérdés legyen aktív

### Résztvevő szabályok

- egy sessionen belül a `DisplayName` legyen egyedi
- ugyanaz a résztvevő egy kérdésre csak egyszer válaszolhasson
- csak az aktuális kérdésre lehessen válaszolni
- lezárt kérdés után a válasz legyen `Late` vagy `Rejected`

---

## 6. Ajánlott repository szerkezet

A `Data` rétegben szerintem **nem generic repository** kell első körben, hanem konkrét use-case közeli repository-k.

### 6.1. `IQuizRepository`

Feladata:

- saját kvízek lekérése tulajdonos szerint
- quiz részletes lekérése kérdésekkel és opciókkal
- új quiz mentése
- quiz módosítása
- quiz törlése

Példaműveletek:

```csharp
Task<List<Quiz>> GetByOwnerAsync(string ownerId, CancellationToken cancellationToken = default);
Task<Quiz?> GetByIdAsync(int quizId, CancellationToken cancellationToken = default);
Task<Quiz?> GetDetailedByIdAsync(int quizId, CancellationToken cancellationToken = default);
Task AddAsync(Quiz quiz, CancellationToken cancellationToken = default);
Task UpdateAsync(Quiz quiz, CancellationToken cancellationToken = default);
Task DeleteAsync(Quiz quiz, CancellationToken cancellationToken = default);
```

---

### 6.2. `IQuizSessionRepository`

Feladata:

- session indítás / lekérés
- session megtalálása PIN alapján
- session állapot frissítése
- aktuális kérdés léptetése

Példaműveletek:

```csharp
Task<QuizSession?> GetByIdAsync(int sessionId, CancellationToken cancellationToken = default);
Task<QuizSession?> GetByPinAsync(string joinPin, CancellationToken cancellationToken = default);
Task<QuizSession?> GetDetailedByIdAsync(int sessionId, CancellationToken cancellationToken = default);
Task AddAsync(QuizSession session, CancellationToken cancellationToken = default);
Task UpdateAsync(QuizSession session, CancellationToken cancellationToken = default);
Task<bool> JoinPinExistsAsync(string joinPin, CancellationToken cancellationToken = default);
```

---

### 6.3. `IParticipantRepository`

Feladata:

- session résztvevők kezelése
- PIN-es csatlakozás előkészítése
- sessionen belüli névütközés ellenőrzése
- ranglista alaplekérés

Példaműveletek:

```csharp
Task<SessionParticipant?> GetByIdAsync(int participantId, CancellationToken cancellationToken = default);
Task<SessionParticipant?> GetBySessionAndNameAsync(int sessionId, string displayName, CancellationToken cancellationToken = default);
Task<List<SessionParticipant>> GetBySessionAsync(int sessionId, CancellationToken cancellationToken = default);
Task AddAsync(SessionParticipant participant, CancellationToken cancellationToken = default);
Task UpdateAsync(SessionParticipant participant, CancellationToken cancellationToken = default);
```

---

### 6.4. `ISessionAnswerRepository`

Feladata:

- válaszok mentése
- ellenőrzés, hogy egy játékos válaszolt-e már
- kérdésenkénti összes válasz lekérdezése
- résztvevő válaszainak lekérdezése

Példaműveletek:

```csharp
Task<SessionAnswer?> GetByParticipantAndQuestionAsync(int participantId, int questionId, CancellationToken cancellationToken = default);
Task<List<SessionAnswer>> GetAnswersForQuestionAsync(int sessionId, int questionId, CancellationToken cancellationToken = default);
Task<List<SessionAnswer>> GetAnswersForParticipantAsync(int participantId, CancellationToken cancellationToken = default);
Task AddAsync(SessionAnswer answer, CancellationToken cancellationToken = default);
```

---

## 7. Mi menjen a service / logic rétegbe?

A repository feladata az adatok kezelése.
Az üzleti szabályok nagy része viszont inkább a `Logic` rétegbe való.

### Példák service szintű logikára

- csak a quiz tulajdonosa indíthat sessiont
- minimum 1 kérdés legyen quiz létrehozáskor
- minimum 2 opció / kérdés
- pontosan 1 helyes válasz / kérdés
- csak `QuestionOpen` állapotban fogadható válasz
- ugyanaz a résztvevő nem válaszolhat kétszer ugyanarra a kérdésre
- válasz csak az aktuális kérdéshez tartozó opcióra érkezhet
- pontszám számítása
- ranglista frissítése

Vagyis:

- **Repository:** adatelérés
- **Logic/Service:** üzleti döntések és szabályok

---

## 8. Ajánlott `DbContext` gondolkodás

A `Data` rétegben érdemes egy központi `AppDbContext` osztályt használni.

Ajánlott öröklés:

```csharp
public class AppDbContext : IdentityDbContext<ApplicationUser>
```

Így egy helyen kezelhető:

- ASP.NET Identity táblák
- saját kvíz entitások
- saját session entitások
- migrációk

### Fontos `DbSet`-ek

```csharp
public DbSet<Quiz> Quizzes => Set<Quiz>();
public DbSet<QuizQuestion> QuizQuestions => Set<QuizQuestion>();
public DbSet<QuestionOption> QuestionOptions => Set<QuestionOption>();
public DbSet<QuizSession> QuizSessions => Set<QuizSession>();
public DbSet<SessionParticipant> SessionParticipants => Set<SessionParticipant>();
public DbSet<SessionAnswer> SessionAnswers => Set<SessionAnswer>();
```

---

## 9. Fontos indexek és megszorítások

### Egyedi indexek

- `QuizQuestions (QuizId, OrderIndex)`
- `QuestionOptions (QuizQuestionId, OrderIndex)`
- `QuizSessions (JoinPin)`
- `SessionParticipants (QuizSessionId, DisplayName)`
- `SessionAnswers (SessionParticipantId, QuizQuestionId)`

### Kötelező mezők

- `Quiz.Title`
- `Quiz.OwnerId`
- `QuizQuestion.Text`
- `QuestionOption.Text`
- `QuizSession.JoinPin`
- `SessionParticipant.DisplayName`

### Törlési viselkedések

Javasolt alapelv:

- `Quiz -> Questions` : Cascade
- `Question -> Options` : Cascade
- `Quiz -> Sessions` : Cascade
- `Session -> Participants` : Cascade
- `Session -> Answers` : Cascade
- `ApplicationUser -> Quiz` : Restrict
- `ApplicationUser -> SessionParticipant` : SetNull

---

## 10. Mappa- és projektstruktúra javaslat

### `RealTimeQuiz.Model`

```text
Entities/
  ApplicationUser.cs
  Quiz.cs
  QuizQuestion.cs
  QuestionOption.cs
  QuizSession.cs
  SessionParticipant.cs
  SessionAnswer.cs
Enums/
  SessionState.cs
  SubmissionStatus.cs
```

### `RealTimeQuiz.Data`

```text
AppDbContext.cs
Repositories/
  IQuizRepository.cs
  IQuizSessionRepository.cs
  IParticipantRepository.cs
  ISessionAnswerRepository.cs
  QuizRepository.cs
  QuizSessionRepository.cs
  ParticipantRepository.cs
  SessionAnswerRepository.cs
Configurations/
  QuizConfiguration.cs
  QuizQuestionConfiguration.cs
  QuestionOptionConfiguration.cs
  QuizSessionConfiguration.cs
  SessionParticipantConfiguration.cs
  SessionAnswerConfiguration.cs
```

### `RealTimeQuiz.Logic`

```text
Services/
  QuizService.cs
  SessionService.cs
Contracts/
  CreateQuizRequest.cs
  JoinSessionRequest.cs
  SubmitAnswerRequest.cs
```

---

## 11. Miért jó ez a felosztás?

Mert tisztán elválasztja:

- a **szerkeszthető tartalmat**
- a **futó session állapotát**
- az **adatelérést**
- az **üzleti szabályokat**

Ez később sokat segít majd:

- REST API endpointok tervezésénél
- SignalR eseményeknél
- integrációs teszteknél
- adatbázis séma diagram készítésénél
- dokumentáció írásánál

---

## 12. Későbbi bővítési lehetőségek

### Ranglista

A mostani modell ezt támogatja.
Lehet:

- mindig újraszámolni a `SessionAnswer` rekordokból
- vagy tárolni a `SessionParticipant.TotalScore` mezőben

### Multimédia

A `QuizQuestion.ImageUrl` már előkészíti.
Később akár külön `QuestionMedia` entitás is készülhet.

### Élő chat

Lehet külön entitás:

```csharp
public class SessionChatMessage
{
    public int Id { get; set; }
    public int QuizSessionId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime SentAtUtc { get; set; }
}
```

### Szerkesztés

A quiz módosítása engedélyezhető addig, amíg nincs aktív vagy már megkezdett session.

### Lapozás

A saját kvízek listázásánál könnyen hozzáadható.

---

## 13. Rövid végső ajánlás

Ha egy mondatban kellene összefoglalni:

> A `Quiz` legyen a sablon, a `QuizSession` a konkrét játék, a `SessionParticipant` a játékos, a `SessionAnswer` pedig a játék során beadott válasz.

Ez a modell:

- elég egyszerű egy beadandóhoz
- jól illeszkedik EF Core-hoz
- kompatibilis ASP.NET Identity-vel
- később bővíthető SignalR-rel, ranglistával, médiával és chat funkcióval is

---

## 14. Következő lépés, ha implementálni kezdjük

Ajánlott sorrend:

1. `Model` entitások létrehozása
2. enumok kitöltése
3. `AppDbContext` létrehozása
4. EF Core konfigurációk és migrációk
5. repository interfészek
6. repository implementációk
7. service réteg üzleti szabályokkal
8. WebAPI endpointok
9. integrációs tesztek

---

## 15. Megjegyzés a jelenlegi projekthez

A jelenlegi solution alapján a backend projektek még nagyrészt vázak, ezért ez a dokumentum **célszerű célmodellt** ír le.
Nem a jelenlegi implementációt dokumentálja, hanem azt, amit érdemes felépíteni a beadandóhoz.
