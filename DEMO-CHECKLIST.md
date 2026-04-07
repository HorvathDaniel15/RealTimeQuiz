# DEMO Checklist (MVP REST)

Ez a checklist arra van, hogy ures adatbazisrol is gyorsan, reprodukalhatoan vegig tudj menni a minimum flow-n.

## 1) Elokeszuletek

- [ ] API projekt indul (`RealTimeQuiz.WebAPI`)
- [ ] DB kapcsolat be van allitva az `appsettings*.json` fajlban
- [ ] Swagger elerheto (tipikusan: `https://localhost:xxxx/swagger`)
- [ ] Admin header minden admin hivasnal: `X-Owner-Id: demo-admin-1`
- [ ] Participant optional header (ha kell): `X-User-Id: demo-user-1`

## 2) API inditas

```zsh
cd "/Users/danimester23/Dev/RealTimeQuiz"
dotnet run --project RealTimeQuiz.WebAPI/RealTimeQuiz.WebAPI.csproj
```

## 3) E2E demo flow (admin + participant)

Hasznald ugyanazt az owner headert vegig: `X-Owner-Id: demo-admin-1`.

### 3.1 Quiz letrehozas (Admin)

- Endpoint: `POST /api/admin/quizzes`
- Headers:
  - `X-Owner-Id: demo-admin-1`
  - `Content-Type: application/json`
- Body:

```json
{
  "title": "Demo Quiz",
  "description": "MVP demo quiz",
  "questions": [
    {
      "text": "Mennyi 2+2?",
      "timeLimitSeconds": 20,
      "options": [
        { "text": "3", "isCorrect": false },
        { "text": "4", "isCorrect": true }
      ]
    },
    {
      "text": "Melyik nyelv .NET-hez?",
      "timeLimitSeconds": 20,
      "options": [
        { "text": "C#", "isCorrect": true },
        { "text": "Python", "isCorrect": false }
      ]
    }
  ]
}
```

- Elvart status: `201 Created`
- Mentsd el: `quizId` (`response.id`)

### 3.2 Session letrehozas (Admin)

- Endpoint: `POST /api/admin/sessions`
- Headers: `X-Owner-Id`, `Content-Type: application/json`
- Body:

```json
{ "quizId": 1 }
```

> A bodyban a valos, elozo lepesben kapott `quizId` legyen.

- Elvart status: `201 Created`
- Mentsd el: `sessionId` (`response.id`) es `joinPin` (`response.joinPin`)

### 3.3 Lobby megnyitas (Admin)

- Endpoint: `POST /api/admin/sessions/{sessionId}/open-lobby`
- Headers: `X-Owner-Id`
- Elvart status: `200 OK`
- Elvart allapot: `state = Lobby`

### 3.4 Csatlakozas PIN-nel (Participant)

- Endpoint: `POST /api/participant-sessions/join`
- Headers:
  - `Content-Type: application/json`
  - opcion: `X-User-Id: demo-user-1`
- Body:

```json
{
  "joinPin": "123456",
  "displayName": "Jatekos 1"
}
```

> A `joinPin` legyen az elozo lepesben kapott valos PIN.

- Elvart status: `200 OK`
- Mentsd el: `participantId` (`response.participantId`)

### 3.5 Session inditas (Admin)

- Endpoint: `POST /api/admin/sessions/{sessionId}/start`
- Headers: `X-Owner-Id`
- Elvart status: `200 OK`
- Elvart allapot: `state = QuestionOpen`
- Mentsd el: `currentQuestionId`

### 3.6 Aktualis kerdes lekerese (Participant)

- Endpoint: `GET /api/participant-sessions/{participantId}/current-question`
- Elvart status: `200 OK`
- Mentsd el: `questionId` es egy valaszthato `optionId` az `options` tombbol

### 3.7 Valasz bekuldese (Participant)

- Endpoint: `POST /api/participant-sessions/submit-answer`
- Headers: `Content-Type: application/json`
- Body:

```json
{
  "participantId": 1,
  "questionId": 1,
  "optionId": 1
}
```

> Minden ID legyen az elozo lepesekbol szarmazo valos ertek.

- Elvart status: `200 OK`
- Elvart mezo: `status` (tipikusan `Accepted`)

### 3.8 Kerdes lezarasa (Admin)

- Endpoint: `POST /api/admin/sessions/{sessionId}/close-current-question`
- Headers: `X-Owner-Id`
- Elvart status: `200 OK`
- Elvart allapot: `state = QuestionClosed`

### 3.9 Sajat valasz eredmenyenek lekerese (Participant)

- Endpoint: `GET /api/participant-sessions/{participantId}/results/{questionId}`
- Elvart status: `200 OK`
- Ellenorizd: `isCorrect`, `awardedPoints`, `submittedAtUtc`

### 3.10 Kovetkezo kerdesre leptetes (Admin)

- Endpoint: `POST /api/admin/sessions/{sessionId}/advance`
- Headers: `X-Owner-Id`
- Elvart status: `200 OK`
- Elvart:
  - ha van kovetkezo kerdes: `state = QuestionOpen`
  - ha nincs tobb kerdes: `state = Finished`

### 3.11 Session befejezes (Admin, opcion)

- Endpoint: `POST /api/admin/sessions/{sessionId}/finish`
- Headers: `X-Owner-Id`
- Elvart status: `200 OK`
- Elvart allapot: `state = Finished`

## 4) Minimum smoke tesztek (negativ)

### 4.1 Hibas PIN

- `POST /api/participant-sessions/join` rossz `joinPin` ertekkel
- Elvart: `404 Not Found` vagy `400 Bad Request`
- Body: `application/problem+json`

### 4.2 Submit lezaras utan

- Zarj le egy kerdest (`close-current-question`)
- Probald ujra a `POST /api/participant-sessions/submit-answer` hivast ugyanarra a kerdesre
- Elvart: `403 Forbidden` vagy `400 Bad Request`
- Body: `application/problem+json`

## 5) Done checklist (beadas/demo elott)

- [ ] Ures indulasbol 10-15 percen belul vegigfutott a teljes flow
- [ ] Van quiz kerdesekkel + opciokkal
- [ ] PIN-es csatlakozas mukodik
- [ ] Admin start/close/advance mukodik
- [ ] Participant lat aktualis kerdest es tud valaszolni
- [ ] Lezaras utan participant le tudja kerni az eredmenyet (`isCorrect`)
- [ ] Negativ smoke tesztek legalabb egyszer lefutottak
- [ ] Demo kozben nincs kritikus (blokkolo) hiba

## 6) Gyors hibakereses

- Ha `400`: nezd meg a validacios mezoket (`ValidationProblemDetails.errors`)
- Ha `403`: allapot-atmenet vagy ownership problema
- Ha `404`: rossz ID/PIN vagy nem letezo eroforras
- Ha `500`: nezd a WebAPI logot es a `traceId`-t a `ProblemDetails`-ben

