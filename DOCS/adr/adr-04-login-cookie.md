# ADR-04 — Login: cookie e account gestiti da noi

- **Stato:** Accettato
- **Data:** 26/10/2026 (cookie). Aggiornato il 12/11/2026: tolto ASP.NET Core Identity, il login lo scriviamo noi
- **Decisori:** Elmond0, K0ff33Blu3, fmartini02

## Contesto e problema

Preferiti, wishlist e profilo appartengono a un utente, quindi serve un login. Le domande erano due:

1. Come fa il client Blazor WebAssembly a dimostrare alla Web API chi è l'utente collegato?
2. Chi gestisce account e password: ASP.NET Core Identity o codice nostro?

All'inizio avevamo risposto alla prima e, per la seconda, avevamo preso Identity con `MapIdentityApi`. Dopo due settimane di uso abbiamo cambiato idea sulla seconda.

## Opzioni considerate

**Come si riconosce l'utente**

- **A. Cookie `HttpOnly`.** Il browser lo conserva e lo rimanda da solo a ogni richiesta; JavaScript non può leggerlo.
- **B. Token** salvato dal client (di solito nel `localStorage`) e aggiunto a mano a ogni richiesta. Comodo per i client che non sono un browser.

**Chi gestisce gli account**

- **A. ASP.NET Core Identity** con `MapIdentityApi`: registrazione, login, hash delle password e molto altro già pronti.
- **B. Codice nostro**: una tabella `users`, l'hash delle password fatto con la libreria standard di .NET e pochi endpoint scritti da noi.

## Decisione

**Cookie `HttpOnly` (A).** Un token nel `localStorage` può essere letto da qualunque script in pagina, quindi anche da uno iniettato (XSS); un cookie `HttpOnly` no. È anche la strada che la guida Microsoft indica per Blazor WebAssembly.

**Account gestiti da noi (B).** Identity funzionava, ma per il nostro caso era troppo:

- creava sette tabelle (`AspNetUsers`, `AspNetRoles`, `AspNetUserClaims`…) e ce ne servivano una;
- esponeva una dozzina di endpoint (conferma email, reset password, autenticazione a due fattori) che non usiamo e che senza un servizio email non funzionano;

A noi servono cinque operazioni: registrarsi, entrare, uscire, sapere chi è collegato e cambiare la password. Scritte a mano sono un file solo (`Auth/AuthEndpoints.cs`) più l'hash (`Auth/Passwords.cs`).

La parte delicata, cioè come si salvano le password, non l'abbiamo inventata: usiamo `Rfc2898DeriveBytes.Pbkdf2` di .NET, PBKDF2 con SHA-256 e 600.000 iterazioni, il valore che OWASP raccomanda oggi. Ogni password ha il suo salt casuale e il confronto avviene a tempo costante (`CryptographicOperations.FixedTimeEquals`).

### Come funziona

| Cosa | Come |
|---|---|
| Endpoint | `POST /api/account/register`, `/login`, `/logout`; `GET /api/account/manage/info` (chi sono); `POST /api/account/manage/info` (cambio password) |
| Password | da 8 a 128 caratteri, almeno una maiuscola e un carattere speciale |
| Email | unica senza distinguere maiuscole e minuscole (collation ICU in PostgreSQL, vedi ADR-05) |
| Cookie | `terranova.auth`, `HttpOnly`, persistente |
| Sessione | nel cookie c'è un "timbro" (`SessionStamp`). A ogni richiesta la Web API controlla che sia ancora quello salvato nel database: quando si cambia password il timbro cambia e gli altri dispositivi vengono scollegati |
| Email inesistente | il login calcola comunque un hash finto, così la risposta impiega lo stesso tempo e non si capisce quali email sono registrate |
| Troppi tentativi | al massimo 10 richieste al minuto per indirizzo IP su registrazione, login e cambio password, poi `429` |
| Non collegato | la Web API risponde `401` (o `403`) invece di fare un redirect a una pagina di login che non esiste |

### Conseguenze

- **Web API:** CORS con `AllowCredentials` e un'origine precisa (`Cors:FrontendOrigin` in `appsettings.Development.json`); con le credenziali il browser non accetta `*`. `UseAuthentication` e `UseAuthorization` vengono dopo `UseCors`.
- **Client:** `CookieHandler` aggiunge `credentials: include` a ogni chiamata; `CookieAuthenticationStateProvider` chiede `/api/account/manage/info` per sapere chi è collegato. Gli indirizzi sono rimasti quelli di Identity, così passando al codice nostro il client non è cambiato.
- **Database:** tabella `users`. Cancellando un utente spariscono anche i suoi preferiti e la sua wishlist (`ON DELETE CASCADE`). Il passaggio ha cancellato gli utenti di prova creati con Identity (migrazione `ReplaceIdentityWithUsers`).
- **Postman:** la collection `GameExplorer - Account` prova registrazione, login, password sbagliata e logout; Postman conserva il cookie tra una richiesta e l'altra.
- **Cosa non c'è:** conferma dell'email, recupero della password dimenticata e autenticazione a due fattori. Servirebbe un servizio email e non rientra nella traccia.
- **Pubblicazione:** in locale client (`localhost:5100`) e Web API (`localhost:5290`) sono lo stesso sito per il browser, quindi il cookie passa. Se un giorno li pubblicassimo su domini diversi andrebbe rivisto (`SameSite`), ma la consegna è in locale.

## Fonti

- [OWASP Password Storage Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Password_Storage_Cheat_Sheet.html) · [`Rfc2898DeriveBytes.Pbkdf2`](https://learn.microsoft.com/dotnet/api/system.security.cryptography.rfc2898derivebytes.pbkdf2) · [Autenticazione con cookie senza Identity](https://learn.microsoft.com/aspnet/core/security/authentication/cookie) · [Blazor WebAssembly con Identity (cookie e `CookieHandler`)](https://learn.microsoft.com/aspnet/core/blazor/security/webassembly/standalone-with-identity/) · [Rate limiting in ASP.NET Core](https://learn.microsoft.com/aspnet/core/performance/rate-limit)
