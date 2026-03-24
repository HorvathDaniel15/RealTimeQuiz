# Resztvevoi csatlakozas es valaszbekuldes terv

## 1. Cel es scope

Ez a feladat a resztvevoi oldal ket fo use case-et valositja meg backend oldalon:

- PIN alapu session csatlakozas display nevvel
- valasz bekuldese az aktualisan nyitott kerdesre

A session allapotgep mar kesz, erre epitunk. A controller/minimal API csak tovabbit, a szabalyok a logic retegben legyenek.

---

## 2. Javasolt uj service

### 2.1 Uj interface

`RealTimeQuiz.Logic/Services/Interfaces/IParticipantSessionService.cs`

Javasolt publikus muveletek:

- `JoinByPinAsync(request, userId, cancellationToken)`
- `GetCurrentQuestionForParticipantAsync(request, cancellationToken)`
- `SubmitAnswerAsync(request, cancellationToken)`

Megjegyzes:

- `userId` lehet `null` (anonymous jatekos), ez teljesen valid.
- ownership ellenorzes itt nem kell, ez nem admin flow.

### 2.2 Uj implementacio

`RealTimeQuiz.Logic/Services/Implementations/ParticipantSessionService.cs`

Felelossege:

- session PIN alapjan betoltes
- display nev validalas + sessionon beluli egyediseg
- aktualis kerdes visszaadasa
- valasz bekuldes szabalyellenorzessel
- concurrency konfliktusok kezelese (dupla valasz)

---

## 3. Javasolt DTO-k

Hely: `RealTimeQuiz.Logic/Contracts/Sessions/Requests` es `.../Responses`

### 3.1 Request DTO-k

- `JoinSessionByPinRequest`
  - `JoinPin` (string)
  - `DisplayName` (string)

- `GetCurrentQuestionRequest`
  - `ParticipantId` (int)

- `SubmitAnswerRequest`
  - `ParticipantId` (int)
  - `QuestionId` (int)
  - `OptionId` (int)

### 3.2 Response DTO-k

- `JoinSessionResultDto`
  - `ParticipantId`
  - `SessionId`
  - `JoinPin`
  - `DisplayName`
  - `SessionState`
  - `JoinedAtUtc`

- `ParticipantCurrentQuestionDto`
  - `SessionId`
  - `QuestionId`
  - `OrderIndex`
  - `Text`
  - `ImageUrl`
  - `TimeLimitSeconds`
  - `OpenedAtUtc`
  - `Option` lista (ID + szoveg + order)

- `SubmitAnswerResultDto`
  - `SessionId`
  - `ParticipantId`
  - `QuestionId`
  - `OptionId`
  - `Status` (`SubmissionStatus`)
  - `IsCorrect`
  - `AwardedPoints`
  - `SubmittedAtUtc`

---

## 4. Repository oldali kiegeszites (minimal)

A jelenlegi repository-k jo alapot adnak, de ket ponton erdemes boviteni.

### 4.1 IParticipantRepository

Jelenlegi `GetBySessionAndNameAsync` pontos egyezesre megy. Erdemes donteni:

- case-sensitive nev egyediseg legyen ("Anna" es "anna" kulonbozo), vagy
- case-insensitive nev egyediseg legyen (javasolt UX)

Ha case-insensitive kell, repository/query oldalon normalizalni kell.

### 4.2 ISessionAnswerRepository

A dupla valasz tiltashoz service ellenorzes mar jo, de DB oldali versenyhelyzet miatt kell robust insert kezeles.

Javasolt uj metodus:

- `TryAddAsync(SessionAnswer answer, CancellationToken ct)` -> `bool`

Mukodes:

- true: sikeres mentes
- false: unique constraint serult (`IX_SessionAnswers_SessionParticipantId_QuizQuestionId`)

Ez ugyanaz a minta, mint amit mar hasznalsz a join PIN-nel.

---

## 5. Uzleti szabalyok (egyetlen helyen: service)

### 5.1 Join szabalyok

- PIN kotelezo, trim, hossze ellenorzes
- DisplayName kotelezo, trim, max hossz (modell szerint 100)
- PIN-hez session talalhato legyen
- Session allapota legyen csatlakozhato (javasolt: `Lobby`)
- ugyanabban a sessionben display nev ne utkozzon
- resztvevo letrehozas `JoinedAtUtc = UtcNow`, `TotalScore = 0`

Hiba esetek:

- rossz PIN -> `EntityNotFoundException`
- rossz allapot -> `ForbiddenOperationException`
- ervenytelen input / nevutkozes -> `BusinessValidationException`

### 5.2 Aktualis kerdes lekerdezes szabalyok

- participant ID ervenyes legyen
- participant letezzen
- participant sessione betoltodjon quiz + questions + options navokkal
- session state `QuestionOpen` legyen
- `CurrentQuestionId` legyen kitoltve
- current question tenyleg a quizhez tartozzon

### 5.3 Valasz bekuldes szabalyok

- participant letezzen
- session state `QuestionOpen` legyen
- session `CurrentQuestionId` ne legyen null
- request `QuestionId` egyezzen a session `CurrentQuestionId`-val
- `OptionId` a megadott kerdes opcioi kozul legyen
- ugyanaz a participant ugyanarra a kerdesre csak egyszer valaszolhat
  - service oldali pre-check + repository `TryAddAsync` konfliktuskezeles

Allapotkezeles valasznal:

- ha kerdes nyitott -> `SubmissionStatus.Accepted`
- ha kerdes mar zarult -> tiltott muvelet (nem mentunk valaszt)

---

## 6. Segedmetodusok terve (ParticipantSessionService belso)

Javasolt private helper-ek:

- `ValidateJoinRequest(...)`
- `ValidateSubmitAnswerRequest(...)`
- `LoadSessionByPinAsync(...)`
- `LoadParticipantWithSessionGraphAsync(...)`
- `EnsureJoinableState(session)`
- `EnsureQuestionOpenState(session)`
- `EnsureCurrentQuestionConsistency(session, requestQuestionId)`
- `FindOptionInCurrentQuestion(session, optionId)`
- `EnsureDisplayNameIsUniqueAsync(sessionId, displayName, ct)`
- `NormalizeDisplayName(displayName)`
- `MapTo...Dto(...)`

Cel: a publikus metodusok olvashatok maradjanak, es minden szabaly explicit legyen.

---

## 7. Adatbazis- es versenyhelyzet vedelem

### 7.1 Ami mar megvan

- `QuizSessions.JoinPin` unique index
- `SessionAnswers (SessionParticipantId, QuizQuestionId)` unique index

### 7.2 Ami kell a service-ben

- retry logika ott, ahol random generalas van (join pinnel mar megoldottad)
- `SessionAnswer` insertnel `TryAddAsync` konfliktuskezeles
- sikertelen insert utan entity detach, hogy tiszta maradjon a context

---

## 8. API vegpontok (ha WebAPI-ba kotod)

Javasolt vegpontok:

- `POST /api/participant-sessions/join`
- `GET /api/participant-sessions/{participantId}/current-question`
- `POST /api/participant-sessions/submit-answer`

Megjegyzes:

- participant flownal ne koveteljen admin authot.
- anonymous user tamogatasnal a `userId` opcionis.

---

## 9. DI es wiring checklist

- uj service interface + implementation regisztralasa DI-be
- repository interface-ek kiegeszitese implementaciokkal
- endpoint/controller osszekotes
- cancellation token atadas vegig

---

## 10. Tesztelési terv (minimum)

### 10.1 Join

- sikeres join `Lobby` allapotban
- hibas PIN -> not found
- nevutkozes ugyanabban a sessionben -> hiba
- rossz allapotban (pl. `Finished`) join tiltva

### 10.2 Current question

- `QuestionOpen` allapotban visszaadja a kerdest
- nincs current question -> hiba
- nem nyitott allapot -> hiba

### 10.3 Submit answer

- sikeres valasz nyitott kerdesre
- dupla valasz tiltva (service pre-check)
- dupla valasz tiltva konkurens esetben (DB unique + TryAddAsync)
- rossz option (nem az aktualis kerdeshez tartozik) -> hiba
- rossz questionId (nem current) -> hiba
- session lezart/allapot nem megfelelo -> hiba

---

## 11. Megvalositas sorrend (ajanlott)

- [x] 1. DTO-k + service interface letrehozas
- [x] 2. service helper-ek megirasa (validalas, load, ensure)
- [x] 3. Join use case implementacio
- [x] 4. Current question use case implementacio
- [ ] 5. Submit answer use case implementacio
- [ ] 6. repository `TryAddAsync` a `SessionAnswer`-hoz
- [ ] 7. endpoint wiring
- [ ] 8. integration tesztek

Folytatas innen: **5. Submit answer use case implementacio**

---

## 12. Definition of Done

- PIN alapu csatlakozas mukodik
- display nev egyediseg kezelt sessionon belul
- aktualis kerdes visszaadhato biztonsagosan
- valasz bekuldes csak nyitott kerdesre megy
- ugyanarra a kerdesre egy participant csak egyszer valaszol
- option-kerdes kapcsolat validalva van
- tiltott allapotokban ertelmes hiba uzenet jon
- service + DB szinten is van vedelem versenyhelyzetek ellen

