# RealTimeQuiz Backend Architektúra és MVP Scope

## Áttekintés

Ez a dokumentum definiálja a RealTimeQuiz alkalmazás backend rétegeit, azok felelősségeit, és az MVP (Minimum Viable Product) funkciókészletét. A cél, hogy tiszta separáció legyen a rétegek között, és egyértelmű legyen, hogy melyik funkció melyik rétegbe tartozik.

## Rétegek és Felelősségeik

### 1. Model Réteg (`RealTimeQuiz.Model`)

**Felelősség:**
- Domain objektumok, entitások és enumok definiálása
- Üzleti domain szabályainak reprezentálása
- Adatstruktúrák leírása

**Mit tartalmaz:**
- `Entities/` - Adatbázis entitások (Quiz, QuizSession, QuizQuestion, QuestionOption, SessionParticipant, SessionAnswer, ApplicationUser)
- `Enums/` - Domain specifikus enumerációk (SessionState, SubmissionStatus)

**Mit NEM tartalmaz:**
- ❌ Adatbázis műveletek logikája
- ❌ Üzleti döntések végrehajtása
- ❌ HTTP/API specifikus kód
- ❌ Perzisztencia specifikus implementációk

**Példák:**
```csharp
// ✅ JÓ - Domain objektum tulajdonságokkal
public class Quiz
{
    public int Id { get; set; }
    public string Title { get; set; }
    public ICollection<QuizQuestion> Questions { get; set; }
}

// ✅ JÓ - Domain enum
public enum SessionState
{
    Draft, Lobby, QuestionOpen, QuestionClosed, Finished
}
```

### 2. Data Réteg (`RealTimeQuiz.Data`)

**Felelősség:**
- Adatbázis hozzáférés absztrakciója
- CRUD műveletek implementálása
- Entity Framework Core konfiguráció
- Repository minta implementálása

**Mit tartalmaz:**
- `Repositories/AppDbContext.cs` - EF Core DbContext és entitás konfigurációk
- `Repositories/` - Repository implementációk (QuizRepository, QuizSessionRepository, ParticipantRepository, SessionAnswerRepository)
- `Interfaces/` - Repository interfészek

**Mit NEM tartalmaz:**
- ❌ Üzleti logika (pl. "Ki tud csatlakozni egy sessionhöz?")
- ❌ Validációs szabályok (pl. "Ez a válasz pontosan időben érkezett-e?")
- ❌ Több repository-t használó koordináció
- ❌ HTTP/API specifikus kód

**Példák:**
```csharp
// ✅ JÓ - Egyszerű adathozzáférés
public async Task<Quiz?> GetByIdAsync(int quizId)
{
    return await _context.Quizzes
        .FirstOrDefaultAsync(q => q.Id == quizId);
}

// ✅ JÓ - Include használata lekérdezéshez
public async Task<Quiz?> GetDetailedByIdAsync(int quizId)
{
    return await _context.Quizzes
        .Include(q => q.Questions)
        .ThenInclude(q => q.Options)
        .FirstOrDefaultAsync(q => q.Id == quizId);
}

// ❌ ROSSZ - Üzleti döntés a repository-ban
public async Task<bool> CanUserJoinSession(int sessionId, string userId)
{
    var session = await GetSessionByIdAsync(sessionId);
    return session.State == SessionState.Lobby; // Ez üzleti logika!
}
```

### 3. Logic Réteg (`RealTimeQuiz.Logic`)

**Felelősség:**
- Üzleti szabályok és use case-ek implementálása
- Több repository koordinálása
- Domain logika végrehajtása
- Validációk és döntések

**Mit tartalmaz:**
- Use case service osztályok (QuizService, SessionService, ParticipantService)
- Üzleti logika és szabályok
- Komplex műveletek koordinálása

**Mit NEM tartalmaz:**
- ❌ HTTP request/response kezelés
- ❌ Autentikáció/autorizáció (az a WebAPI-ban van)
- ❌ Direkt adatbázis hozzáférés (csak repository-kon keresztül)
- ❌ UI/Frontend specifikus logika

**Példák:**
```csharp
// ✅ JÓ - Üzleti logika service-ben
public class SessionService
{
    public async Task<bool> CanJoinSession(int sessionId, string userId)
    {
        var session = await _sessionRepository.GetByIdAsync(sessionId);
        if (session == null) return false;
        
        // Üzleti szabály: csak Lobby státuszú sessionhöz lehet csatlakozni
        if (session.State != SessionState.Lobby) return false;
        
        // Üzleti szabály: maximális résztvevők száma
        var participantCount = await _participantRepository.GetCountBySessionAsync(sessionId);
        return participantCount < MaxParticipants;
    }
    
    public async Task<SessionAnswer> SubmitAnswer(int sessionId, int participantId, int questionId, int optionId)
    {
        var session = await _sessionRepository.GetByIdAsync(sessionId);
        
        // Üzleti döntés: válasz elfogadása vagy elutasítása
        var status = DetermineSubmissionStatus(session);
        
        var answer = new SessionAnswer
        {
            SessionParticipantId = participantId,
            QuizQuestionId = questionId,
            QuestionOptionId = optionId,
            Status = status,
            SubmittedAtUtc = DateTime.UtcNow
        };
        
        await _answerRepository.AddAsync(answer);
        return answer;
    }
    
    private SubmissionStatus DetermineSubmissionStatus(QuizSession session)
    {
        if (session.State != SessionState.QuestionOpen)
            return SubmissionStatus.Rejected;
            
        if (session.QuestionClosedAtUtc.HasValue)
            return SubmissionStatus.Late;
            
        return SubmissionStatus.Accepted;
    }
}

// ✅ JÓ - Több repository koordinálása
public async Task<QuizSession> CreateSessionAsync(int quizId, string ownerId)
{
    // Ellenőrzés, hogy a quiz létezik-e és a user tulajdonosa-e
    var quiz = await _quizRepository.GetByIdAsync(quizId);
    if (quiz == null || quiz.OwnerId != ownerId)
        throw new UnauthorizedAccessException();
    
    // Session létrehozása egyedi PIN-nel
    var pin = await GenerateUniquePinAsync();
    
    var session = new QuizSession
    {
        QuizId = quizId,
        JoinPin = pin,
        State = SessionState.Draft,
        CreatedAtUtc = DateTime.UtcNow
    };
    
    await _sessionRepository.AddAsync(session);
    return session;
}
```

### 4. WebAPI Réteg (`RealTimeQuiz.WebAPI`)

**Felelősség:**
- HTTP endpoint-ok definiálása
- Request/Response kezelés és mapping
- Autentikáció és autorizáció
- Validációs hibák HTTP formában való visszaadása
- SignalR hub-ok (real-time kommunikáció)

**Mit tartalmaz:**
- `Controllers/` - API controller-ek
- `Hubs/` - SignalR hub-ok (ha van)
- `DTOs/` - Data Transfer Object-ek (ha szükséges)
- `Program.cs` - Alkalmazás konfiguráció és szolgáltatások beállítása

**Mit NEM tartalmaz:**
- ❌ Üzleti logika végrehajtása (az a Logic rétegben van)
- ❌ Több service-t igénylő komplex műveletek koordinálása
- ❌ Direkt repository hívások

**Példák:**
```csharp
// ✅ JÓ - Controller delegál a service-nek
[ApiController]
[Route("api/[controller]")]
public class SessionController : ControllerBase
{
    private readonly SessionService _sessionService;
    
    [HttpPost("{sessionId}/join")]
    [Authorize]
    public async Task<IActionResult> JoinSession(int sessionId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        // Autorizáció és validáció a controllerben
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();
        
        // Üzleti logika a service-ben
        var canJoin = await _sessionService.CanJoinSession(sessionId, userId);
        if (!canJoin)
            return BadRequest("Cannot join this session");
        
        var participant = await _sessionService.JoinSessionAsync(sessionId, userId);
        return Ok(participant);
    }
}

// ❌ ROSSZ - Üzleti logika a controllerben
[HttpPost("submit-answer")]
public async Task<IActionResult> SubmitAnswer(SubmitAnswerRequest request)
{
    // NE ezt csináld! Ez üzleti logika, ami a Logic rétegbe tartozik
    var session = await _sessionRepository.GetByIdAsync(request.SessionId);
    if (session.State != SessionState.QuestionOpen)
        return BadRequest();
    
    var status = session.QuestionClosedAtUtc.HasValue 
        ? SubmissionStatus.Late 
        : SubmissionStatus.Accepted;
    
    // ... továbbiak
}
```

### 5. Client Réteg (`RealTimeQuiz.Client`)

**Felelősség:**
- Felhasználói felület megjelenítése
- API hívások végrehajtása
- Frontend validáció (UX, nem biztonság)
- Real-time kommunikáció (SignalR client)

**Mit tartalmaz:**
- Angular komponensek, service-ek
- UI logika és megjelenítés
- HTTP kliensek API-hívásokhoz
- Routing és navigáció

**Mit NEM tartalmaz:**
- ❌ Üzleti döntések (pl. "Ez a válasz elfogadható-e?")
- ❌ Autorizációs logika (csak UI elrejtése, a backend mindig ellenőriz)
- ❌ Adatbázis hozzáférés

**Példák:**
```typescript
// ✅ JÓ - Frontend csak meghívja az API-t
async submitAnswer(questionId: number, optionId: number) {
    try {
        const result = await this.apiService.submitAnswer(
            this.sessionId,
            questionId,
            optionId
        );
        
        if (result.status === 'Accepted') {
            this.showSuccessMessage();
        } else {
            this.showLateSubmissionMessage();
        }
    } catch (error) {
        this.showErrorMessage(error.message);
    }
}

// ❌ ROSSZ - Frontend dönt üzleti kérdésről
async submitAnswer(questionId: number, optionId: number) {
    // NE ezt! A backend dönt arról, hogy elfogadható-e a válasz
    if (this.isQuestionClosed) {
        this.showMessage('Too late!');
        return;
    }
    
    await this.apiService.submitAnswer(...);
}
```

## MVP Funkciólista

### Alapfunkciók (Kötelező az MVP-ben)

1. **Quiz Kezelés**
   - Quiz létrehozása (tulajdonos által)
   - Kérdések hozzáadása/szerkesztése
   - Opciók megadása, helyes válasz kijelölése
   - Quiz publikálása

2. **Session Management**
   - Session létrehozása egy quizhez
   - Egyedi JOIN PIN generálása
   - Résztvevők csatlakozása PIN-nel
   - Session indítása (Lobby → QuestionOpen)

3. **Real-time Quiz Flow**
   - Tulajdonos vezérli a quiz menetét
   - Kérdések egymás utáni megjelenítése
   - Résztvevők válaszolnak
   - Kérdés lezárása (későbbi válaszok már nem számítanak)
   - Következő kérdésre lépés

4. **Eredmények**
   - Válaszok rögzítése (helyes/helytelen)
   - Egyszerű statisztika (ki hány jót válaszolt)
   - Session végén eredménylista

5. **Autentikáció**
   - Regisztráció/bejelentkezés (Quiz tulajdonosoknak)
   - Session-ökhöz csatlakozás név megadásával (nem kell regisztráció)

### Opcionális Funkciók (MVP után)

- ⏱️ Időlimit kérdésenként
- 🏆 Pontszámítás (gyorsabb válasz = több pont)
- 📊 Részletes statisztikák és grafikonok
- 🖼️ Képek feltöltése kérdésekhez
- 🎨 Testreszabható quiz témák
- 📱 PWA támogatás
- 💾 Quiz export/import
- 🔄 Quiz duplikálás/template-ek
- 👥 Csapatmód
- 📈 历史 eredmények tárolása

## Főbb Use Case-ek és Réteg-hozzárendelés

### Use Case 1: Quiz létrehozása

| Réteg | Felelősség |
|-------|-----------|
| **Client** | Űrlap megjelenítése, frontend validáció |
| **WebAPI** | HTTP POST fogadása, autentikáció, request parsing |
| **Logic** | Quiz létrehozása, tulajdonos beállítása, validáció |
| **Data** | Quiz mentése az adatbázisba |
| **Model** | Quiz entitás struktúra |

### Use Case 2: Session indítása és PIN generálás

| Réteg | Felelősség |
|-------|-----------|
| **Client** | "Start Session" gomb, PIN megjelenítés |
| **WebAPI** | HTTP POST fogadása, autorizáció (tulajdonos-e) |
| **Logic** | Egyedi PIN generálás, ütközés ellenőrzés, session létrehozás |
| **Data** | Session mentése, PIN egyediség ellenőrzése |
| **Model** | QuizSession entitás |

### Use Case 3: Résztvevő csatlakozása

| Réteg | Felelősség |
|-------|-----------|
| **Client** | PIN beviteli űrlap, név megadása |
| **WebAPI** | POST request fogadása |
| **Logic** | Session keresése PIN alapján, állapot ellenőrzés (Lobby?), résztvevő létrehozása |
| **Data** | Session lekérdezése, résztvevő mentése |
| **Model** | SessionParticipant entitás |

### Use Case 4: Válasz beküldése

| Réteg | Felelősség |
|-------|-----------|
| **Client** | Válasz opció kiválasztása, küldés gomb |
| **WebAPI** | POST request fogadása, résztvevő azonosítás |
| **Logic** | Session állapot ellenőrzés, időbélyeg alapú döntés (elfogadott/késő/elutasított), válasz mentés |
| **Data** | SessionAnswer mentése |
| **Model** | SessionAnswer entitás, SubmissionStatus enum |

### Use Case 5: Következő kérdésre lépés

| Réteg | Felelősség |
|-------|-----------|
| **Client** | "Next Question" gomb (csak tulajdonosnál) |
| **WebAPI** | POST request fogadása, autorizáció |
| **Logic** | Session állapot módosítása (QuestionClosed → QuestionOpen), következő kérdés ID beállítása, időbélyegek frissítése |
| **Data** | Session frissítése |
| **Model** | QuizSession, SessionState enum |

### Use Case 6: Eredmények lekérése

| Réteg | Felelősség |
|-------|-----------|
| **Client** | Eredménylista megjelenítése |
| **WebAPI** | GET request fogadása |
| **Logic** | Résztvevők válaszainak összesítése, helyes válaszok számlálása, rangsor készítése |
| **Data** | Résztvevők, válaszok és helyes opciók lekérdezése JOIN-okkal |
| **Model** | SessionAnswer, SessionParticipant, QuestionOption |

## Fejlesztési Irányelvek

### 1. Repository Pattern

**DO ✅**
- Egyszerű CRUD műveletek
- Szűrés és rendezés
- Include-ok az eager loading-hoz
- Async/await használata

**DON'T ❌**
- Üzleti döntések
- Több entitást érintő tranzakciós logika
- Validációs szabályok

### 2. Logic Layer

**DO ✅**
- Use case-ek implementálása
- Több repository koordinálása
- Üzleti szabályok végrehajtása
- Exception dobása hibás esetben
- Validációk

**DON'T ❌**
- HTTP specifikus kód (status code-ok, stb.)
- Direkt DbContext használat
- UI logika

### 3. WebAPI Controller

**DO ✅**
- HTTP request/response kezelés
- Autentikáció/autorizáció
- DTO mapping (ha van)
- HTTP status code-ok visszaadása
- Model state validáció

**DON'T ❌**
- Komplex üzleti logika
- Több service koordinálása (ha lehetséges)
- Direkt repository hívás

### 4. Session Szabályok - Egy Helyen!

Az összes session állapot-váltási logika a `SessionService`-ben van:
- Ki csatlakozhat?
- Mikor lehet válaszolni?
- Mikor számít egy válasz későnek?
- Mikor lehet következő kérdésre lépni?

**NEM szabad:**
- A controllerben dönteni ezekről
- A frontenden ellenőrizni (csak UX célból rejteni elemeket)
- A repository-ban üzleti szabályokat érvényesíteni

## Mikor melyik réteget kell módosítani?

| Ha... | Akkor... |
|-------|----------|
| Új entitást kell létrehozni | `Model` réteg |
| Új adatbázis lekérdezés kell | `Data` réteg (repository) |
| Új üzleti szabály | `Logic` réteg (service) |
| Új API endpoint | `WebAPI` réteg (controller) |
| Új UI funkció | `Client` réteg |
| Módosítani kell, hogy ki mit csinálhat | `Logic` réteg (service) + `WebAPI` (autorizáció) |
| Real-time kommunikáció | `WebAPI` (SignalR hub) + `Client` (SignalR client) |

## Kérdések és Válaszok

**K: Hol legyen a validáció?**
**V:** Több helyen is, különböző célokból:
- **Client:** UX javítás (azonnali visszajelzés)
- **WebAPI:** Input validáció (ModelState, DataAnnotations)
- **Logic:** Üzleti szabályok validációja (pl. lehet-e válaszolni)

**K: SignalR kód hova kerül?**
**V:** 
- Hub osztályok: `WebAPI/Hubs/`
- Service hívások a hub-okból: `Logic` réteg
- SignalR client: `Client` réteg

**K: Hol történjen az entitások közötti mapping?**
**V:**
- Ha DTO-kat használunk: `WebAPI` rétegben (AutoMapper vagy manuális)
- Ha közvetlenül entitásokat adjuk vissza: a `Logic` réteg adja vissza az entitást, és a `WebAPI` serialize-álja

**K: Transaction kezelés hol van?**
**V:**
- Ha egy use case több repository műveletet igényel, a `Logic` rétegben
- EF Core alapból transaction-t használ a `SaveChanges()` hívásnál

## Ellenőrző lista új funkció implementálásakor

- [ ] Eldöntöttem, hogy melyik use case-hez tartozik
- [ ] Tisztában vagyok, hogy melyik réteg mit csinál
- [ ] Model entitások szükségesek? → `Model`
- [ ] Új repository metódus? → `Data/Interfaces` + `Data/Repositories`
- [ ] Üzleti logika? → `Logic` (service osztály)
- [ ] API endpoint? → `WebAPI` (controller)
- [ ] UI? → `Client` (Angular komponens)
- [ ] A repository nem tartalmaz üzleti döntést
- [ ] A controller csak delegál a service-nek
- [ ] A frontend nem dönt üzleti kérdésről

---

**Utolsó frissítés:** 2026-03-10  
**Verzió:** 1.0  
**Állapot:** Elfogadott architektúra
