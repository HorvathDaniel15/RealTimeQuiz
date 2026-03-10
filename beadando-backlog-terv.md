# RealTimeQuiz — backlog és GitHub issue terv

## Mi ez a dokumentum?

Ez a fájl a `RealTimeQuiz` beadandó következő fejlesztési szakaszainak tervezett backlogja. A célja nem az, hogy technikai implementációs részletekbe menjen le, hanem hogy később is egyértelműen vissza lehessen nézni:

- mi a következő feladat,
- miért fontos,
- milyen részfeladatokat foglal magába,
- mire kell figyelni,
- és mikor tekinthető késznek.

A terv a `feladatkiiras.md` követelményeire épül, és a jelenlegi projektállapotból indul ki:

- a `Model` és a `Data` réteg már jó alapot ad,
- a `Logic` réteg még gyakorlatilag üres,
- a `WebAPI` még kezdeti állapotú,
- az integrációs tesztek és az Angular kliens még csak váz szinten vannak.

---

## Hogyan érdemes használni?

Ezt a dokumentumot úgy érdemes kezelni, mint egy közepesen részletes roadmapet:

- GitHub issue-k alapjának teljesen jó,
- egy hét múlva visszaolvasva is emlékeztet arra, hogy mit miért akartál megcsinálni,
- védésen is jól használható, mert látszik belőle a tudatos tervezés.

Fontos: ez nem azt jelenti, hogy egyszerre mind a 12 issue-val kell foglalkozni. Inkább egy jól átgondolt sorrend, amin végig lehet menni úgy, hogy közben ne csússzon szét a projekt.

---

# Issue 01 — Backend architektúra és MVP scope véglegesítése

## Cél
Le kell zárni, hogy a backend rétegeknek pontosan mi a feladata, és az első működő verzióba pontosan milyen funkciók kerülnek bele.

## Miért fontos?
Ha ez nincs előre tisztázva, akkor nagyon könnyen összefolyik:

- mi tartozik a repository-ba,
- mi tartozik a business logicba,
- mi tartozik a controllerbe,
- és mi marad a frontend feladata.

Ez az a pont, ahol el lehet kerülni a későbbi spagettikódot.

## Mit tartalmazzon?
- A rétegek felelősségének rögzítése:
  - `Model` = entitások, enumok, domain objektumok
  - `Data` = EF Core, `AppDbContext`, repository-k
  - `Logic` = üzleti szabályok és use case-ek
  - `WebAPI` = HTTP endpointok, authentikáció, request/response kezelés
  - `Client` = UI és API-hívások
- Az MVP scope lezárása a beadandó kötelező része alapján.
- Annak eldöntése, hogy mi számít alapfunkciónak és mi opcionális extrának.
- A fő use case-ek rövid felsorolása.

## Mire kell figyelni?
- A repository ne tartalmazzon üzleti döntéseket.
- A controller ne végezzen több repository-n átívelő logikát.
- A frontend ne döntsön arról, hogy egy kérdés megválaszolható-e.
- A session szabályok egy helyen legyenek, ne szétszórva.

## Mikor kész?
- Le van írva a réteghatár.
- Le van írva az MVP funkciólista.
- Egyértelmű, hogy melyik fő use case melyik rétegben valósul meg.
- Később visszaolvasva is egyből látszik, hogy mit hova kell tenni.

## Miért ez az első?
Mert minden további issue erre épül. Ha a szerkezeti gondolkodás nincs rendben, akkor később sokkal nehezebb lesz tisztán fejleszteni.

---

# Issue 02 — EF Core migráció és az adatbázis létrehozásának ellenőrzése

## Cél
A már megtervezett entitásokból és `DbContext` konfigurációból tényleges adatbázis-sémát kell létrehozni, majd ezt ki is kell próbálni.

## Miért fontos?
Most a modell és a `Data` réteg már elég jól áll, de a rendszer csak akkor tekinthető valós alapnak, ha ebből ténylegesen létrejön a relációs adatbázis.

A beadandó egyik kulcspontja, hogy az Entity Framework ne csak formálisan legyen jelen, hanem valóban működő adatbázis-kezelést adjon.

## Mit tartalmazzon?
- Az EF Core provider végleges bekötése.
- Connection string beállítása.
- Az első migráció létrehozása.
- Az adatbázis tényleges létrehozása.
- Az identity táblák és a saját táblák ellenőrzése.
- A kulcsok, idegen kulcsok, cascade szabályok és indexek átnézése.

## Mire kell figyelni?
Különösen ezekre a szabályokra:
- a session PIN legyen egyedi,
- a kérdések sorrendje egy kvízen belül legyen egyedi,
- az opciók sorrendje egy kérdésen belül legyen egyedi,
- a résztvevő neve egy sessionön belül legyen egyedi,
- ugyanaz a résztvevő ugyanarra a kérdésre csak egyszer válaszolhasson.

Ha itt valami hibásan van leképezve, akkor később a Logic rétegben olyan hibák jöhetnek elő, amik valójában adatbázis-szintű problémák.

## Mikor kész?
- Létrejön az első migration.
- A DB hiba nélkül felépül.
- A fő táblák megjelennek.
- A kapcsolatok ellenőrizve vannak.
- Biztosan látszik, hogy az adatmodell működőképes.

## Miért most jön?
Mert a business logicot sokkal nyugodtabban lehet megírni, ha a perzisztencia már stabil. Így egyszerre nem két fronton kell hibát keresni.

---

# Issue 03 — Kvízkezelési üzleti logika megvalósítása

## Cél
A kvízekkel kapcsolatos fő use case-eket a `Logic` rétegben kell összerakni úgy, hogy a szabályok ne a controllerben és ne a repository-ban legyenek.

## Miért fontos?
A teljes rendszer alapja a kvíz. Ha a kvíz létrehozása és lekérdezése nincs stabilan megoldva, akkor nem lehet rá megbízható session logikát építeni.

A `QuizRepository` jelenlegi iránya jó: adatot kér le és ment. A fölé kell most egy üzleti logikai réteg.

## Mit tartalmazzon?
- `QuizService` vagy ennek megfelelő service réteg megtervezése.
- Saját kvízek listázásának use case-e.
- Kvíz részletes lekérdezése.
- Új kvíz létrehozása.
- A létrehozás üzleti validációi, például:
  - minimum 1 kérdés,
  - kérdésenként minimum 2 válaszlehetőség,
  - pontosan 1 helyes válasz,
  - sorrendiség helyes kezelése,
  - tulajdonos hozzárendelése.

## Mire kell figyelni?
- A repository csak adatot kezeljen.
- A service ellenőrizze a domain szabályokat.
- A későbbi API-hoz érdemes DTO-kban gondolkodni, ne közvetlen EF entitásokban.
- A saját kvízek listázása tényleg userhez kötött legyen.

## Mikor kész?
- Van működő service a kvízkezelésre.
- A kvíz létrehozás validációkkal együtt működik.
- A saját kvízek listázása működik.
- A részletes lekérdezés rendezett kérdésekkel és opciókkal jön vissza.

## Miért ez jön a migráció után?
Mert a kvíz a session kiindulópontja. Előbb kell stabilan létrehozni és lekérni a kvízt, és csak utána érdemes ráépíteni a játékfolyamatot.

---

# Issue 04 — Session állapotgép és a központi játékmenet-logika megvalósítása

## Cél
A kvízjáték életciklusát egy jól meghatározott állapotgépként kell megtervezni és implementálni.

## Miért fontos?
Ez az egész rendszer legkényesebb része. Ha a session állapotai és az állapotváltások nincsenek egy helyen, akkor könnyen szétszóródik a logika:

- kicsit a controllerben,
- kicsit a repository-ban,
- kicsit a frontendben,
- kicsit a service-ben.

Ezt mindenképp el kell kerülni.

## Mit tartalmazzon?
- Session állapotok meghatározása, például:
  - `Draft`
  - `Lobby`
  - `QuestionOpen`
  - `QuestionClosed`
  - `Finished`
- Az állapotok közötti lehetséges átmenetek rögzítése.
- `SessionService` vagy ennek megfelelő service kialakítása.
- Fő use case-ek:
  - session létrehozás,
  - session indítás,
  - kérdés megnyitása,
  - kérdés lezárása,
  - továbblépés a következő kérdésre,
  - session befejezése.
- Az időbélyegek tudatos kezelése:
  - `StartedAtUtc`
  - `QuestionOpenedAtUtc`
  - `QuestionClosedAtUtc`
  - `FinishedAtUtc`

## Mire kell figyelni?
- Ne a controller döntse el, hogy egy állapotváltás megengedett-e.
- Ne legyenek „rejtett” átmenetek.
- A `CurrentQuestionId` kezelése konzisztens legyen.
- Ha később időzített kérdéseket vagy ranglistát akarsz, azok is erre az állapotgépre fognak épülni.

## Mikor kész?
- Le van írva és kódolva az állapotgép.
- A tiltott átmenetek hibát adnak.
- A session vezérlés service-szinten működik.
- Egyértelmű, hogy egy session éppen melyik állapotban van és mit lehet vele csinálni.

## Miért ez jön most?
Mert a résztvevői folyamatok és a válaszbeküldés teljesen ettől függenek. Előbb a játékmenetet kell stabilizálni, és csak utána érdemes ráengedni a játékosokat.

---

# Issue 05 — Résztvevő csatlakozás és válaszbeküldési logika megvalósítása

## Cél
A résztvevői oldal két legfontosabb use case-ének backend oldali megvalósítása:

- csatlakozás PIN-kóddal,
- válasz beküldése egy aktuális kérdésre.

## Miért fontos?
Ettől lesz a rendszer ténylegesen használható kvízjátékként. A résztvevő szempontjából ez a legfontosabb üzleti folyamat.

## Mit tartalmazzon?
- PIN alapú session keresés.
- Csatlakozás display névvel.
- Display név egyediségének ellenőrzése sessionön belül.
- Az aktuális kérdés lekérdezési logikája.
- Válasz beküldésének logikája.
- Szabályok ellenőrzése:
  - csak nyitott kérdésre lehessen válaszolni,
  - csak egyszer lehessen válaszolni ugyanarra a kérdésre,
  - csak az adott kérdéshez tartozó opció legyen választható,
  - zárt vagy befejezett sessionben ne lehessen szabálytalanul válaszolni.

## Mire kell figyelni?
- A dupla válasz tiltása legyen service- és adatbázis-szinten is védve.
- A beküldött opció tényleg az aktuális kérdéshez tartozzon.
- A résztvevő lehet anonymous is, ezt ne keverd össze az admin authentikációval.
- Ez a rész nagyon érzékeny a versenyhelyzetekre, főleg később valós idejű működés mellett.

## Mikor kész?
- A résztvevő PIN alapján csatlakozni tud.
- A névütközés kezelve van.
- Érvénytelen vagy rossz állapotú session esetén hibát kap.
- Ugyanarra a kérdésre csak egyszer tud választ küldeni.
- A válasz csak érvényes kérdés/opció kombinációra mehet be.

## Miért a session logika után jön?
Mert a résztvevői műveletek csak akkor értelmesek, ha már létezik stabil játékmenet, aminek van állapota és szabályrendszere.

---

# Issue 06 — ASP.NET Identity integráció és authentikációs endpointok

## Cél
Az adminisztrációs oldal és a tulajdonosi műveletek mögé valódi felhasználókezelést és hitelesítést kell tenni.

## Miért fontos?
Ez nem extra funkció, hanem a beadandó kötelező része. A rendszernek biztosítania kell, hogy a kvízek kezelése valódi felhasználókhoz legyen kötve, biztonságos jelszókezeléssel.

## Mit tartalmazzon?
- ASP.NET Identity bekötése a `WebAPI` projektben.
- `ApplicationUser` használata.
- Regisztráció endpoint.
- Bejelentkezés endpoint.
- Védett admin végpontok.
- Tulajdonos-ellenőrzés a kvízekhez és sessionökhöz.

## Mire kell figyelni?
- Az admin auth és a résztvevői PIN-es csatlakozás két külön dolog.
- A tulajdonosi ellenőrzést a backend garantálja, ne a frontend.
- Érdemes átgondolni, hogy a minimum beadandóhoz cookie-s vagy tokenes megoldás lesz-e egyszerűbb.

## Mikor kész?
- Lehet regisztrálni.
- Lehet bejelentkezni.
- A védett végpontok authentikációt igényelnek.
- Egy user csak a saját kvízeit és sessionjeit tudja kezelni.

## Miért itt jön?
Mert mostanra már körvonalazódik, milyen backend műveleteket kell védeni. Így az authot nem légüres térbe vezeted be, hanem már valós use case-ekhez kötöd.

---

# Issue 07 — REST API controllerek megvalósítása a fő backend funkciókhoz

## Cél
A már létező logikai réteget HTTP endpointokon keresztül használhatóvá kell tenni.

## Miért fontos?
A `Logic` réteg önmagában még nem elég. Az admin felület, a résztvevői kliens és az integrációs tesztek is API-n keresztül fogják használni a rendszert.

## Mit tartalmazzon?
- Quiz controller a fő műveletekkel:
  - saját kvízek listázása,
  - kvíz lekérése,
  - kvíz létrehozása.
- Session controller a fő műveletekkel:
  - session indítása,
  - session állapot lekérése,
  - kérdés megnyitása,
  - kérdés lezárása,
  - léptetés,
  - befejezés.
- Participant / Answer endpointok:
  - csatlakozás PIN alapján,
  - válasz beküldése,
  - eredmény lekérése.
- Egységes hibaformátum kialakítása.

## Mire kell figyelni?
- A controller ne valósítson meg business logicot.
- A controller lehetőleg DTO-kat fogadjon és adjon vissza.
- A hibaüzenetek következetesek legyenek.
- A Swagger legyen átlátható, mert a fejlesztés során sokat segít.

## Mikor kész?
- A fő use case-ekhez tartozik működő endpoint.
- A controllerek service réteget használnak.
- A hibák kezelése következetes.
- A rendszer backend oldala HTTP-n keresztül használható.

## Miért az auth után jön?
Mert így már rögtön védetten lehet felépíteni az admin endpointokat, és nem kell később visszamenni az összes végpontot újragondolni.

---

# Issue 08 — Integrációs tesztek a fő backend folyamatokra

## Cél
Automatikus ellenőrzésekkel igazolni, hogy a fő backend folyamatok tényleg együtt működnek.

## Miért fontos?
A beadandó integrációs teszteket is kér, de ezen túl ez lesz az a pont, ahol kiderül, hogy az összes réteg együtt is stabil-e:

- adatbázis,
- identity,
- API,
- logic,
- repository.

## Mit tartalmazzon?
Legalább a következő flow-kat érdemes lefedni:
- regisztráció,
- bejelentkezés,
- kvíz létrehozása,
- saját kvízek lekérése,
- session indítása,
- PIN join,
- válasz beküldése,
- eredmény lekérése.

Érdemes negatív teszteket is írni:
- dupla válasz tiltása,
- zárt kérdésre válasz tiltása,
- idegen kvíz vezérlésének tiltása,
- hibás PIN kezelése.

## Mire kell figyelni?
- Ez ne unit teszt legyen, hanem tényleges integrációs teszt.
- A teljes láncot kell nézni, nem csak egyetlen service-t.
- A negatív esetek legalább annyira fontosak, mint a happy path.

## Mikor kész?
- Van működő integrációs teszt infrastruktúra.
- A fő backend flow-k le vannak fedve.
- Van néhány fontos negatív eset is.
- A tesztek újrafuttathatók és segítenek regressziók ellen.

## Miért most jön?
Mert mostanra már van értelmes backend funkcionalitás, amit végponttól végpontig lehet tesztelni. Ezzel még a frontend előtt el lehet csípni sok hibát.

---

# Issue 09 — Angular admin MVP felület elkészítése

## Cél
Az adminisztrációs oldal minimálisan működő kliensoldali felületének elkészítése.

## Miért fontos?
A beadandóban szükség van admin felületre is, ahol a user a saját kvízeit kezeli és sessiont indít. Ezzel válik a backend ténylegesen használható rendszerré.

## Mit tartalmazzon?
- Admin login felület.
- Saját kvízek listázása.
- Új kvíz létrehozó nézet.
- Kvíz részletes nézete.
- Session indítás.
- Session vezérlő nézet az alap műveletekhez.

## Mire kell figyelni?
- Első körben nem a design a lényeg, hanem a működő flow.
- A kliens ne találjon ki backend szabályokat.
- A felület legyen egyszerű, de a fő műveletek jól követhetők.

## Mikor kész?
- Az admin be tud jelentkezni.
- Látja a saját kvízeit.
- Tud új kvízt létrehozni.
- Tud sessiont indítani és legalább alap szinten vezérelni.

## Miért csak most jön?
Mert előbb legyen stabil backend és tesztelt API. Így a frontend már egy viszonylag kész szerveroldalra épülhet.

---

# Issue 10 — Angular résztvevői MVP felület elkészítése

## Cél
A résztvevői oldal minimálisan működő kliensoldali felületének elkészítése.

## Miért fontos?
A rendszer másik fő szereplője a résztvevő. A játék csak akkor teljes, ha a user be tud lépni PIN-nel, látja az aktuális kérdést, válaszolni tud és kap visszajelzést.

## Mit tartalmazzon?
- PIN megadása.
- Display name megadása.
- Sessionhöz csatlakozás.
- Aktuális kérdés megjelenítése.
- Válaszopciók listázása.
- Válasz beküldése.
- Eredmény vagy legalább alap visszajelzés megjelenítése.

## Mire kell figyelni?
- A résztvevői flow legyen gyors és egyszerű.
- A hibahelyzetek érthetőek legyenek.
- A frontend itt se legyen üzleti szabályok forrása, csak a backend állapotát jelenítse meg.

## Mikor kész?
- A résztvevő PIN-nel és névvel csatlakozni tud.
- Látja az aktuális kérdést.
- Tud választ beküldeni.
- Kap valamilyen eredmény- vagy státuszvisszajelzést.

## Miért az admin után jön?
Mert az admin oldal általában több üzleti műveletet érint, így érdemes azt előbb stabilizálni. Utána a résztvevői oldal már egy készebb API-ra tud támaszkodni.

---

# Issue 11 — SignalR alapú valós idejű frissítések bevezetése

## Cél
A résztvevői felület valós időben reagáljon a session változásaira, oldalfrissítés nélkül.

## Miért fontos?
Ez a beadandó külön pontozott része, és egy Kahoot-jellegű alkalmazásnál kifejezetten sokat ad az élményhez. Ettől lesz a rendszer valóban „real-time”.

## Mit tartalmazzon?
- SignalR hub kialakítása.
- Sessionhöz kötött kommunikációs csoportok.
- Események kezelése, például:
  - kérdés megnyitása,
  - kérdés lezárása,
  - következő kérdés,
  - session befejezése.
- Frontend oldali kapcsolódás és eseményfigyelés.

## Mire kell figyelni?
- A SignalR ne helyettesítse a backend üzleti logikát.
- A hub csak közvetítse a változásokat.
- Előbb legyen stabil REST működés, és utána jöjjön a valós idejű réteg.
- Érdemes figyelni arra, mi történik újracsatlakozás vagy kapcsolatvesztés esetén.

## Mikor kész?
- Az admin műveleteire a résztvevői felület automatikusan reagál.
- Nem kell kézzel újratölteni az oldalt.
- A fő sessionváltozások valós időben megjelennek.

## Miért a frontend MVP után jön?
Mert előbb legyen egy működő alap flow. A real-time működés erre ráépülő plusz, nem az elsődleges stabilitási réteg.

---

# Issue 12 — Opcionális extra funkciók külön backlogba rendezése

## Cél
Az alap beadandó funkcionalitás fölé tervezett extrákat külön kezelni, hogy ne zavarják meg az MVP elkészítését.

## Miért fontos?
Az extrák jók pontszerzésre, de nagyon könnyen elviszik az időt és a fókuszt. Ha túl korán belekeverednek a kötelező funkciók közé, a projekt könnyen szétcsúszhat.

## Mit tartalmazzon?
A feladatkiírás alapján ilyen extra irányok jöhetnek szóba:
- időzített kérdések,
- ranglista,
- multimédia,
- OAuth2 / JWT / refresh token,
- 2FA,
- elfelejtett jelszó,
- kvíz szerkesztés,
- lapozás,
- élő chat,
- rendszeradmin funkciók.

## Mire kell figyelni?
- Csak akkor érdemes extra funkciót vállalni, ha az alap rendszer már működik.
- Érdemes külön priorizálni az extrákat:
  - mi ad sok pontot,
  - mi egyszerű,
  - mi kockázatos.
- Inkább 1-2 jól befejezett extra legyen, mint 5 félkész.

## Mikor kész?
- Az extrák külön backlogban szerepelnek.
- El van döntve, melyik kerül ténylegesen megvalósításra.
- Az MVP önmagában is lezárható marad.

## Miért ez az utolsó?
Mert ez már nem az alap működésről szól, hanem a bővítésről. Előbb a kötelező részt kell biztonsággal lezárni.

---

# Rövid összefoglalás

A 12 issue valójában 4 nagyobb fejlesztési szakaszra bontható:

## 1. Alapozás
- Backend architektúra és scope tisztázása
- Migráció és adatbázis ellenőrzése

## 2. Backend mag
- Kvízlogika
- Session logika
- Résztvevői folyamatok
- Identity
- REST API

## 3. Minőségbiztosítás
- Integrációs tesztek

## 4. Kliens és bővítések
- Admin felület
- Résztvevői felület
- SignalR
- Extra funkciók

---

# Védésre jól használható záró gondolat

A projektet érdemes úgy bemutatni, hogy a fejlesztés nem véletlenszerűen, hanem tudatos rétegezéssel és funkcionális blokkok mentén történt. Először az adatmodellt és az architektúrát kellett stabilizálni, utána az üzleti logikát, majd az API-t és a teszteket, és csak ezután a kliensoldali és valós idejű funkciókat.

Ez a sorrend segít abban, hogy a rendszer ne széteső, hanem jól védhető, fokozatosan felépített beadandó legyen.

