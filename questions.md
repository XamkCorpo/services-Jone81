# Vaihe 3: Service-kerros, Repository, Result Pattern ja API-dokumentaatio — Teoriakysymykset

Vastaa alla oleviin kysymyksiin omin sanoin. Kirjoita vastauksesi kysymysten alle.

> **Vinkki:** Jos jokin kysymys tuntuu vaikealta, palaa lukemaan teoriamateriaalit:
> - [Service-kerros ja DI](https://github.com/xamk-mire/Xamk-wiki/blob/main/C%23/fin/04-Advanced/WebAPI/Services-and-DI.md)
> - [Repository Pattern](https://github.com/xamk-mire/Xamk-wiki/blob/main/C%23/fin/04-Advanced/Patterns/Repository-Pattern.md)
> - [Result Pattern](https://github.com/xamk-mire/Xamk-wiki/blob/main/C%23/fin/04-Advanced/Patterns/Result-Pattern.md)

---

## Osa 1: Service-kerroshttps://github.com/XamkCorpo/pagination-validation-and-authentication-Jone81.git

### Kysymys 1: Fat Controller -ongelma

Miksi on ongelma jos controller sisältää kaiken logiikan (tietokantakyselyt, muunnokset, validoinnin)? Anna vähintään kaksi konkreettista haittaa.

**Vastaus:**Heikko testattavuus: Yksikkötestien kirjoittaminen on erittäin vaikeaa, koska controllerin testaaminen vaatisi HTTP-kontekstin ja tietokantayhteyksien simulointia.

Koodin uudelleenkäytettävyyden puute: Jos liiketoimintalogiikka on kirjoitettu suoraan controllerin sisään, sitä ei voida kutsua mistään muualta sovelluksesta (esim. toisesta controllerista tai taustaprosessista) ilman koodin kopiointia.
(Lisäksi se rikkoo Single Responsibility -periaatetta, eli yhdellä luokalla pitäisi olla vain yksi syy muuttua, mikä tekee koodista nopeasti sekavaa ja vaikeasti ylläpidettävää).


---

### Kysymys 2: Vastuunjako

Miten vastuut jakautuvat controller:n, service:n ja repository:n välillä tässä harjoituksessa? Kirjoita lyhyt kuvaus kunkin kerroksen tehtävästä.

**Controller vastaa:**HTTP-liikenteen reitityksestä (routing). Se ottaa vastaan HTTP-pyynnöt, delegoi varsinaisen työn Service-kerrokselle ja palauttaa asiakkaalle oikean HTTP-tilakoodin (esim. 200 OK, 404 Not Found) ja vastauksen.

**Service vastaa:**Sovelluksen ydinliiketoimintalogiikasta. Se validoi säännöt, ohjaa tietovirtaa ja huolehtii tietotyyppien muunnoksista (esim. DTO-objektien ja tietokantaentiteettien välillä).

**Repository vastaa:**Yhteydenpidosta tietokantaan. Se piilottaa tietokantakyselyt (esim. Entity Framework Core -logiikan) ja tarjoaa Service-kerrokselle yksinkertaisen rajapinnan datan hakemiseen ja tallentamiseen.


---

### Kysymys 3: DTO-muunnokset servicessä

Miksi DTO ↔ Entity -muunnokset kuuluvat serviceen eikä controlleriin? Mitä hyötyä siitä on, että controller ei tunne `Product`-entiteettiä lainkaan?

**Vastaus:**Tämä takaa kerrosarkkitehtuurin eristyneisyyden. Controllerin tehtävä on käsitellä vain ulospäin näkyvää dataa (DTO = Data Transfer Object), kun taas entiteetit on tarkoitettu vain tietokannan rakenteen kuvaamiseen. Kun controller ei tunne entiteettiä, estetään vahingossa tapahtuvat tietovuot (esim. salasanojen, sisäisten ID-tunnuksien tai aikaleimojen vuotaminen API-vastauksessa). Service tekee muunnoksen ja varmistaa, että controller saa käsiteltäväkseen vain täsmälleen sen datan, joka asiakkaalle on tarkoitus lähettää.


---

## Osa 2: Interface ja Dependency Injection

### Kysymys 4: Interface vs. konkreettinen luokka

Miksi controller injektoi `IProductService`-interfacen eikä suoraan `ProductService`-luokkaa? Mitä hyötyä tästä on?

**Vastaus:**Tämä mahdollistaa löysän sidonnaisuuden (loose coupling). Kun controller riippuu vain rajapinnasta (mitä metodeja on olemassa) eikä toteutuksesta (miten metodit toimivat), sovelluksen ylläpito on helpompaa. Tärkein konkreettinen hyöty on testattavuus: yksikkötestauksessa IProductService voidaan helposti korvata "mock"- eli valeluokalla, jolloin controlleria voidaan testata ilman aitoa service-luokkaa tai tietokantaa.


---

### Kysymys 5: DI-elinkaaret

Selitä ero näiden kolmen elinkaaren välillä ja anna esimerkki milloin kutakin käytetään:

- **AddScoped:**Luodaan kerran jokaisen HTTP-pyynnön (request) aikana. Kaikki komponentit, jotka pyytävät tätä palvelua saman HTTP-pyynnön elinkaaren aikana, saavat saman instanssin. Käyttöesimerkki: Tietokantakonteksti (DbContext) tai sitä käyttävät Servicet.
- **AddSingleton:**Luodaan vain kerran sovelluksen käynnistyessä ja samaa instanssia jaetaan koko sovelluksen elinkaaren ajan kaikille pyynnöille. Käyttöesimerkki: Välimuisti (Cache) tai konfiguraatioasetukset.
- **AddTransient:**Luodaan aina uusi instanssi joka kerta, kun sitä pyydetään. Käyttöesimerkki: Kevyet ja tilattomat (stateless) apuluokat, jotka eivät jaa tietoa komponenttien välillä.

Miksi `AddScoped` on oikea valinta `ProductService`:lle?
Koska ProductService käyttää todennäköisesti DbContext:iä (joko suoraan tai repositoryn kautta), ja Entity Frameworkin DbContext on oletuksena Scoped. DI-sääntöjen mukaan lyhyemmän elinkaaren palvelua ei voi injektoida pidemmän elinkaaren palveluun ilman ongelmia (esim. Singleton ei voi käyttää Scoped-palvelua turvallisesti).

---

### Kysymys 6: DI-kontti

Selitä omin sanoin mitä DI-kontti tekee kun HTTP-pyyntö saapuu ja `ProductsController` tarvitsee `IProductService`:ä. Mitä tapahtuu vaihe vaiheelta?

**Vastaus:**HTTP-pyyntö saapuu oikeaan osoitteeseen, ja framework päättelee, että se pitää reitittää ProductsControllerille.

Framework yrittää luoda ProductsController-olion.

Se huomaa kontrollerin konstruktorista, että luominen vaatii IProductService-rajapinnan toteuttavan olion.

Framework kysyy DI-kontilta (Dependency Injection Container), onko kyseiselle rajapinnalle rekisteröity toteutusta.

DI-kontti löytää Program.cs:ssä tehdyn rekisteröinnin, luo ProductService-olion (ja hakee automaattisesti myös sen tarvitsemat omat riippuvuudet, kuten Repositoryn tai DbContextin).

DI-kontti syöttää (injektoi) valmiin ProductService-olion controllerin konstruktorille, ja pyynnön käsittely voi alkaa.


---

### Kysymys 7: Rekisteröinnin unohtaminen

Mitä tapahtuu jos unohdat rekisteröidä `IProductService`:n `Program.cs`:ssä? Milloin virhe ilmenee ja miltä se näyttää?

**Vastaus:**Sovellus kääntyy (build) onnistuneesti, koska kääntäjä välittää vain siitä, että koodi on syntaktisesti oikein. Virhe tapahtuu ajonaikaisesti (runtime). Kun käyttäjä lähettää ensimmäisen HTTP-pyynnön, DI-kontti heittää virheen tyyppiä InvalidOperationException (esim. "Unable to resolve service for type 'IProductService' while attempting to activate 'ProductsController'"), ja asiakas saa 500 Internal Server Error -vastauksen.


---

## Osa 3: Repository-kerros

### Kysymys 8: Miksi repository?

`ProductService` käytti aluksi `AppDbContext`:ia suoraan. Miksi se refaktoroitiin käyttämään `IProductRepository`:a? Anna vähintään kaksi syytä.

**Vastaus:**Tietokantariippuvuuden eristäminen: Service vapautuu tietämästä, miten data teknisesti haetaan (esim. Entity Framework Core, SQL-kysely vai jopa ulkoinen API). Tämä helpottaa teknologian vaihtamista myöhemmin.

Kyselyjen keskittäminen ja uudelleenkäyttö: Jos sama monimutkainen tietokantakysely tarvitaan monessa eri servicessä, se voidaan kirjoittaa yhteen paikkaan repositoryyn, jolloin koodin toisto vähenee (DRY-periaate).


---

### Kysymys 9: Service vs. Repository

Mikä on `IProductService`:n ja `IProductRepository`:n välinen ero? Mitä tietotyyppejä kumpikin käsittelee (DTO vai Entity)?

**IProductService:**Käsittelee DTO-objekteja. Se ottaa vastaan controllerilta DTO:ita, tekee liiketoimintasäännöt ja palauttaa controllerille DTO:ita.
**IProductRepository:**Käsittelee Entity-luokkia (esim. Product). Se kommunikoi suoraan tietokannan kanssa sovelluksen sisäisillä domeenimalleilla.
---

### Kysymys 10: Controllerin muuttumattomuus

Kun Vaihe 7:ssä lisättiin repository-kerros, `ProductsController` ei muuttunut lainkaan. Miksi? Mitä tämä kertoo rajapintojen (interface) hyödystä?

**Vastaus:**Controller kommunikoi ainoastaan IProductService-rajapinnan kanssa. Koska Service-kerroksen metodien signatuurit (mitä ne ottavat sisään ja palauttavat, eli DTO:t) eivät muuttuneet, controllerin ei tarvinnut tietää mitään siitä, että Service alkoi käyttää Repositorya DbContextin sijaan. Tämä on loistava osoitus interfacen hyödystä: se muodostaa "sopimuksen", joka suojaa ylempiä kerroksia alempien kerroksien teknisiltä muutoksilta.


---

## Osa 4: Exception-käsittely ja lokitus

### Kysymys 11: ILogger

Mikä on `ILogger` ja miksi sitä tarvitaan? Mistä lokit näkee kehitysympäristössä?

**Vastaus:**ILogger on .NETin sisäänrakennettu rajapinta lokitukseen. Sitä tarvitaan tallentamaan tietoa sovelluksen suorituksesta, tapahtumista ja erityisesti virhetilanteista vianetsintää varten. Kehitysympäristössä (Localhost) lokit näkee yleensä suoraan IDE:n konsoli- tai debug-ikkunassa (esim. Visual Studio / VS Code Terminal). Tuotannossa ne ohjataan usein tiedostoon tai ulkoiseen palveluun (esim. Application Insights tai Seq).


---

### Kysymys 12: Odotetut vs. odottamattomat virheet

Selitä ero "odotetun" ja "odottamattoman" virheen välillä. Anna esimerkki kummastakin ja kerro miten ne käsitellään eri tavalla servicessä.

**Odotettu virhe (esimerkki + käsittely):**Esimerkiksi asiakas yrittää hakea tuotetta ID:llä, jota ei ole olemassa (Not Found), tai lähettää puutteellista dataa. Näihin on varauduttu koodissa. Käsittely tapahtuu palauttamalla epäonnistumista kuvaava tulos (esim. Result.Failure) ja ohjaamalla kontrolleri palauttamaan HTTP 400 tai 404. Sovellus ei kaadu, eikä näistä yleensä luoda vakavaa virhelokia (virheilmoitus riittää).

**Odottamaton virhe (esimerkki + käsittely):**Tietokantayhteys katkeaa tai koodissa tapahtuu NullReferenceException-bugin vuoksi. Nämä otetaan kiinni globaalilla virheenkäsittelyllä (esim. middleware tai try-catchin ylin taso). Virheestä kirjoitetaan kriittinen merkintä ILoggerilla (Error/Critical) ja asiakkaalle palautetaan geneerinen HTTP 500 Internal Server Error ilman, että ohjelman sisäisiä yksityiskohtia (stack trace) paljastetaan.


---

## Osa 5: Result Pattern

### Kysymys 13: Miksi null ja bool eivät riitä?

Alla on kaksi esimerkkiä. Selitä miksi ensimmäinen tapa on ongelmallinen ja miten toinen ratkaisee ongelman:

```csharp
// Tapa 1: null
ProductResponse? product = await _service.GetByIdAsync(id);
if (product == null)
    return NotFound();

// Tapa 2: Result
Result<ProductResponse> result = await _service.GetByIdAsync(id);
if (result.IsFailure)
    return NotFound(new { error = result.Error });
```

**Vastaus:**Tapa 1 (null) on ongelmallinen, koska se ei kerro miksi arvo on null. Johtuiko se siitä, että tuotetta ei todella ole olemassa, vai tapahtuiko tietokantahaussa virhe? Tapa 2 (Result-malli) on paljon parempi, koska se on eksplisiittinen: se kertoo tarkalleen onnistuiko toimenpide vai ei (IsFailure) ja mikäli ei, se palauttaa mukanaan selkeän virheviestin (result.Error), jota voidaan hyödyntää suoraan API:n vastauksessa (esim. "Tuotetta ei löytynyt" tai "Nimi on liian lyhyt").


---

### Kysymys 14: Result.Success vs. Result.Failure

Miten `Result Pattern` muutti virheiden käsittelyä servicessä? Vertaa Vaihe 8:n `throw;`-tapaa Vaihe 9:n `Result.Failure`-tapaan: mitä eroa niillä on asiakkaan (API:n kutsuja) näkökulmasta?

**Vastaus:**Exceptions (throw) ovat teknisesti raskaita, ja niitä tulisi käyttää vain poikkeuksellisissa järjestelmävirheissä (Exceptions are for exceptional situations). Liiketoimintalogiikan virheet (esim. "Saldot eivät riitä") ovat normaaleja skenaarioita.

throw -tapa: API-kutsuja saattaa saada yllättävän ja sekavan 500 Internal Server Error -vastauksen, jos virhettä ei erikseen catchata ja muuteta.

Result.Failure -tapa: Liiketoimintavirheet käsitellään normaaleina paluuarvoina. API-kutsuja saa aina rakenteellisen ja ennustettavan JSON-vastauksen yhdistettynä oikeaan HTTP-koodiin (esim. 400 Bad Request), joka sisältää tarkan syyn sille, miksi pyyntö hylättiin.


---

## Osa 6: API-dokumentaatio

### Kysymys 15: IActionResult vs. ActionResult\<T\>

Miksi `ActionResult<ProductResponse>` on parempi kuin `IActionResult`? Anna vähintään kaksi syytä.

**Vastaus:**Tyyppiturvallisuus (Type Safety): Kääntäjä varmistaa, että jos palautat dataa (esim. return Ok(...)), datan on oltava oikeaa tyyppiä (ProductResponse). Pelkkä IActionResult sallii minkä tahansa objektin palauttamisen, mikä on virhealtista.

Automaattinen dokumentaatio: Swagger / OpenAPI osaa päätellä ActionResult<T> -paluutyypistä automaattisesti oikean datascheman API-dokumentaatioon ilman erillisiä lisäattribuutteja.


---

### Kysymys 16: ProducesResponseType

Mitä `[ProducesResponseType]`-attribuutti tekee? Miten se näkyy Swagger UI:ssa?

**Vastaus:**Se kertoo API:a käyttävälle kehittäjälle ja OpenAPI/Swagger-generaattorille tarkalleen, mitä eri HTTP-tilakoodeja (esim. 200 OK, 404 Not Found, 400 Bad Request) kyseinen endpoint voi palauttaa ja minkä tyyppistä dataa kuhunkin koodiin liittyy. Swagger UI:ssa nämä generoituu endpointin "Responses" -osioon selkeäksi listaksi, jolloin API:n kuluttajan on helppo varautua koodissaan kaikkiin mahdollisiin vastausvaihtoehtoihin.


---

### Kysymys 18: Refaktorointi

Sovelluksen toiminnallisuus pysyi täysin samana koko harjoituksen ajan — samat endpointit, samat vastaukset. Mitä refaktorointi tarkoittaa ja miksi se kannattaa, vaikka käyttäjä ei huomaa eroa?

**Vastaus:**Refaktorointi tarkoittaa koodin sisäisen rakenteen siistimistä ja parantamista ilman, että sen ulkoinen toiminnallisuus muuttuu. Se kannattaa, koska:

Koodista tulee selkeämpää ja helpompaa lukea uuden kehittäjän näkökulmasta.

Arkkitehtuurista tulee joustavampi: kun logiikka on hajautettu oikein (Controller, Service, Repository), uusien ominaisuuksien lisääminen on jatkossa huomattavasti nopeampaa ja turvallisempaa.

Tekninen velka pienenee ja koodin testattavuus nousee, mikä vähentää tulevien bugien määrää ohjelmistoa jatkokehitettäessä.


---
