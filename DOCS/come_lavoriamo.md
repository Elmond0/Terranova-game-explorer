# Come lavoriamo con Git

> Guida del team **Terranova Game Explorer**. Leggila una volta, poi tienila aperta le prime settimane.

**In una frase:** su `main` non si committa mai. Ogni lavoro nasce da una scheda (issue), vive in un branch tuo ed entra in `main` solo con una Pull Request approvata da un compagno.

| Ruolo | GitHub | Territorio (cartelle in cui scrive solo lui) | Label |
|---|---|---|---|
| **Dev A — backend** | [`@K0ff33Blu3`](https://github.com/K0ff33Blu3) | `be/` · `shared/` · `post-man/` · `GameExplorer.slnx` | `BE` · `API` |
| **Dev B — frontend** | [`@fmartini02`](https://github.com/fmartini02) | `fe/` | `FE` |
| **Dev C — database** | [`@Elmond0`](https://github.com/Elmond0) | `db/` · `be/GameExplorer.Api/Data` · `fe/Pages/Account` · `fe/Services/Auth` · `compose.yaml` · `.env.example` · `.github/workflows/database.yml` | `DB` |
| **tutti** | | `DOCS/` · `README.md` · `.gitignore`: modifiche piccole, avvisando gli altri | |

Repository: [`Elmond0/Terranova-game-explorer`](https://github.com/Elmond0/Terranova-game-explorer) · chi fa cosa: [`Suddivisione del lavoro`](Suddivisione%20del%20lavoro.md) · stack: [`adr/adr-01-stack.md`](adr/adr-01-stack.md)

---

## Indice

1. [Il giro completo di una scheda](#1-il-giro-completo-di-una-scheda)
2. [La prima volta (15 minuti, una volta per computer)](#2-la-prima-volta)
3. [Le schede](#3-le-schede)
4. [Branch e commit](#4-branch-e-commit)
5. [La Pull Request](#5-la-pull-request)
6. [`main` si è mosso e i conflitti](#6-main-si-è-mosso-e-i-conflitti)
7. [Le regole del team e il loro perché](#7-le-regole-del-team-e-il-loro-perché)
8. [Quando qualcosa va storto](#8-quando-qualcosa-va-storto)
9. [Protezione di `main`](#9-protezione-di-main)
10. [Fonti](#10-fonti)

---

## 1. Il giro completo di una scheda

Un lavoro vero, da quando prendi la scheda a quando la chiudi. Negli esempi sei `@Elmond0` e lavori sulla scheda **#21** "tabella game_cache"; la PR che ne esce è la **#23**. I numeri sono inventati (la scheda vera di `game_cache` è la #27): issue e PR usano lo **stesso contatore**, e `gh` ti stampa quello vero quando le crei.

```bash
# ── Giorno 1: apro la scheda ───────────────────────────────
git switch main && git pull
gh issue list --assignee @me              # cosa devo fare
gh issue view 21                          # leggo la scheda e il suo "Fatto quando"
git switch -c db/game-cache               # apro il branch

git diff                                  # controllo cosa ho cambiato
git add be/GameExplorer.Api/Data/Entities/GameCacheEntry.cs
git commit -m "feat(db): aggiunge l'entità GameCacheEntry"
git push -u origin db/game-cache          # -u solo la prima volta
gh pr create --draft                      # PR in bozza: gli altri vedono cosa tocco

# ── Giorno 2: continuo sullo stesso branch ─────────────────
git switch db/game-cache
git switch main && git pull && git switch -   # aggiorno main e torno indietro
git merge main                                # mi porto dentro il lavoro degli altri
git add be/GameExplorer.Api/Data/Configurations/GameCacheEntryConfiguration.cs
git commit -m "feat(db): configura la tabella game_cache"
git push

# ── Giorno 3: ho finito ────────────────────────────────────
git switch main && git pull && git switch - && git merge main && git push
gh pr ready                               # tolgo la bozza (nel testo c'è "Closes #21")
# ... un compagno la prova e la approva ...
gh pr merge 23 --squash --delete-branch   # solo ora il branch si cancella
git switch main && git pull               # e passo alla scheda dopo
```

> **Le due regole che non stanno nei comandi**
> 1. **`git diff` prima di ogni `git add`.** Dieci secondi che ti evitano di pushare una riga di prova, il `.env` o la chiave RAWG.
> 2. **Pusha anche a metà lavoro.** Quello che non hai pushato esiste solo sul tuo portatile.

---

## 2. La prima volta

Fai i passi **in quest'ordine**: se cloni prima di esserti collegato, Git risponde `Permission denied (publickey)`.

**Passo 0 — Accetta l'invito.** Il repo è di `@Elmond0`. L'invito arriva per mail o su [github.com/notifications](https://github.com/notifications); finché non lo accetti non puoi pushare né ricevere richieste di revisione.

**Passo 1 — Di' a Git chi sei.**

```bash
git config --global user.name "Nome Cognome"
git config --global user.email "la-tua-email-di-github"
git config --global fetch.prune true      # toglie i branch già cancellati sul server
git config --global pull.ff only          # git pull non inventa merge a sorpresa
```

> ⚠️ **L'email deve essere quella del tuo account GitHub.** GitHub attribuisce ogni commit all'account che possiede quell'email: con un'altra, il tuo lavoro non risulta tuo, e la traccia valuta i contributi individuali. Controlla nella [lista dei commit](https://github.com/Elmond0/Terranova-game-explorer/commits/main) che accanto ai tuoi ci sia il tuo nome utente.

**Passo 2 — Installa `gh`**, il programma di GitHub per il terminale (versione 2.60 o successiva: `gh --version`).

| Sistema | Come |
|---|---|
| Arch / CachyOS | `sudo pacman -S github-cli` |
| macOS | `brew install gh` |
| Ubuntu / Debian | il repository ufficiale di GitHub, con i comandi della [guida di installazione](https://github.com/cli/cli/blob/trunk/docs/install_linux.md). **Non** `sudo apt install gh` senza quel repository: il pacchetto di Ubuntu è vecchio e alcuni comandi falliscono con `GraphQL: Projects (classic) is being deprecated` |
| Senza `sudo` (PC della scuola) | scarica l'archivio da [github.com/cli/cli/releases](https://github.com/cli/cli/releases), copia `bin/gh` in `~/bin` e aggiungi `~/bin` al `PATH` |

**Passo 3 — Collegati.** `gh auth login` e rispondi: *GitHub.com* → *SSH* → *genera una nuova chiave* (la passphrase può restare vuota) → *Login with a web browser*. Questo passo crea anche la chiave SSH, per questo viene prima del clone. Poi scegli un editor che sai usare: `gh config set editor nano` (altrimenti si apre vim).

**Passo 4 — Clona.**

```bash
git clone git@github.com:Elmond0/Terranova-game-explorer.git
cd Terranova-game-explorer
```

**Passo 5 — Controlla.** `gh auth status` dice *Logged in*, `gh issue list` mostra le schede, `git config --get user.email` è l'email di GitHub.

<details>
<summary>Se proprio non puoi usare <code>gh</code>: la chiave SSH a mano</summary>

```bash
ls ~/.ssh/id_ed25519.pub                  # ce l'hai già? Allora salta al cat
ssh-keygen -t ed25519 -C "la-tua-email"   # creala (Invio tre volte)
cat ~/.ssh/id_ed25519.pub                 # copiala tutta
```

Incollala su GitHub in *Settings → SSH and GPG keys → New SSH key*, poi prova con `ssh -T git@github.com`: deve rispondere `Hi <tuonome>!`. La chiave è legata al computer: su ogni macchina ne serve una. Tutto quello che fa `gh` si fa anche dal browser (tabella in §5.4).

</details>

---

## 3. Le schede

Ogni cosa da fare è una **issue**. Non si inventa il lavoro la mattina: **si prende una scheda**. Così gli altri sanno cosa stai toccando e a fine progetto risulta chi ha fatto cosa.

```bash
gh issue list --assignee @me        # le mie (dal browser: filtro  is:open assignee:@me)
gh issue list --label DB            # quelle del database (BE, FE, API...)
gh issue view 21                    # leggerne una
gh issue edit 21 --add-assignee @me # prendermela
gh issue comment 21 --body "Fermo: mi serve GameDetails dalla #19. @K0ff33Blu3"
gh issue create                     # aprirne una nuova (titolo come un commit)
```

**Le label**

| Label | Vuol dire |
|---|---|
| `BE` · `FE` · `DB` | l'area: backend, frontend, database |
| `API` | riguarda gli endpoint, il contratto tra backend e frontend |
| (milestone) `fase 1` … `fase 5` | la fase della `Suddivisione`: non è una label, si imposta in *Milestone* |
| `PO` | urgente: prima delle altre |
| `question` | serve una risposta prima di poter lavorare |
| `invalid` · `wontfix` | scheda sbagliata o che non si farà: si chiude con un commento |

**Quale prendo?** Prima le `PO`, poi la fase più bassa. A parità, quella che sta bloccando un compagno. **Una alla volta:** due schede aperte vogliono dire due branch aperti, e su uno non stai lavorando davvero.

**Il "Fatto quando"** è l'elenco dei criteri per dire che la scheda è finita. Se non lo soddisfi, non è finita, anche se il codice funziona.

**La scheda non si chiude a mano.** Nel testo della PR scrivi `Closes #21` su una riga sua: al merge GitHub chiude la scheda e la collega alla PR per sempre. Valgono anche `Fixes` e `Resolves`. Per più schede ripeti la parola (`Closes #21, closes #22`). Se la PR fa solo una parte del lavoro, scrivi `Rif. #21`: collega senza chiudere. La chiusura funziona solo per le PR verso `main`.

**Ti serve una cosa nel territorio di un altro?** Non modificare i suoi file: aprigli una scheda e assegnala a lui.

---

## 4. Branch e commit

### Il nome del branch: `<area>/<cosa>`

| Area | Quando |
|---|---|
| `be` · `fe` · `db` | lavori nella cartella del backend, del frontend o del database |
| `api` | `post-man/` o il contratto degli endpoint |
| `docs` | `DOCS/`, README |
| `setup` | configurazione del repo (`.gitignore`, soluzione) |
| `fix` | correggi un bug |

Il "cosa" è di 2–4 parole, minuscole, con i trattini: `db/game-cache`, `be/rawg-service`, `fe/search-page`. **No** a `db/modifiche`, `be/fix2`, `prova`, o al tuo nome: branch così restano aperti per mesi. Il numero della scheda non va nel nome: il collegamento lo fa `Closes #21` nella PR.

> Se non sai quale area scrivere, non hai ancora capito quale cartella stai per toccare. Controlla che sia la tua.

### Il commit: `tipo(area): cosa hai fatto`

Seguiamo [Conventional Commits](https://www.conventionalcommits.org/it/v1.0.0/). Il messaggio è **in italiano**, al presente, minuscolo; tutto quello che sta nei file (codice, commenti, nomi) è in inglese.

| Tipo | Quando |
|---|---|
| `feat` | una funzionalità nuova |
| `fix` | la correzione di un bug |
| `test` | test aggiunti o corretti |
| `docs` | solo documentazione |
| `refactor` | codice riorganizzato, stesso comportamento |
| `ci` | workflow di GitHub Actions |
| `chore` | manutenzione e configurazione |

Esempi: `feat(be): aggiunge la ricerca in RawgService` · `ci(db): controlla le migrazioni sulle PR`.

**Il titolo dice cosa, il corpo dice perché.** Il cosa si vede dal codice; il perché fra due mesi non se lo ricorda nessuno:

```bash
git commit -m "fix(db): salva le date in UTC" \
           -m "Npgsql rifiuta le date non UTC nelle colonne timestamptz."
```

**Commit piccoli e frequenti**, e **mai `git add .` alla cieca**: aggiungi i file per nome, così non ti porti dietro `bin/`, `obj/`, file di prova o il `.env`.

---

## 5. La Pull Request

### 5.1 Aprirla

Il branch deve già essere sul server (`git push -u origin <branch>`). Poi:

```bash
gh pr create --draft     # appena hai il primo commit: gli altri vedono cosa tocchi
gh pr ready              # quando hai finito
gh pr create --web       # oppure scrivi il testo nel browser
```

Senza `gh`: dopo il primo `git push` Git stampa il link per aprire la PR; altrimenti, sul repo compare la striscia gialla **Compare & pull request**.

### 5.2 Il testo: una riga di collegamento e quattro sezioni

```markdown
Closes #21

## Cosa contiene
elenco puntato di cosa hai fatto

## Perché così
le scelte che hai preso e il motivo

## Sblocca
chi stava aspettando questo lavoro (se nessuno, togli la sezione)

## Come verificare
i comandi esatti che il compagno deve lanciare per provarla
```

**"Come verificare" è la sezione più importante:** dice al compagno come controllare il tuo lavoro in pochi minuti. Per il backend sono spesso le richieste della collection Postman in `post-man/`.

> **Tieni le PR piccole.** Una PR da 40 file non viene rivista davvero: viene timbrata.

### 5.3 Rivedere la PR di un compagno

È lavoro, non un favore: **entro la giornata**. In tre, se uno non rivede, gli altri due sono fermi.

```bash
gh pr list                                     # le aperte
gh pr diff 23                                  # cosa ha cambiato
gh pr checkout 23                              # provarla sul tuo computer...
git switch -                                   # ...e tornare al tuo lavoro
gh pr review 23 --approve
gh pr review 23 --request-changes -b "Se RAWG risponde 404 qui va in errore"
```

Cosa guardare, in quest'ordine:

1. Fa quello che dice? Soddisfa il "Fatto quando"?
2. Tocca solo le cartelle che doveva toccare?
3. C'è roba che non c'entra (`bin/`, `obj/`, file di prova, `.env`, chiavi)?
4. I nomi si capiscono senza chiedere?

E soprattutto **provala davvero**, seguendo "Come verificare": è anche il modo per capire il codice degli altri, che all'esame ognuno deve saper spiegare. Se il codice funziona ma l'avresti scritto diversamente, approva e parlagliene a voce. **Non approvare mai una PR che non hai guardato.**

### 5.4 Il merge e la pulizia

**Mergia chi ha aperto la PR**, dopo l'approvazione di un compagno:

```bash
gh pr merge 23 --squash --delete-branch   # un solo commit su main, branch cancellato
git switch main && git pull
```

Lo **squash** schiaccia tutti i tuoi commit in uno solo su `main`: la storia resta leggibile, una riga per scheda. Il branch si cancella **solo adesso**, a lavoro finito: finché ci lavori, anche per giorni, resta lì.

**Chiudere una PR senza mergiarla** (lavoro abbandonato): `gh pr close 23 --delete-branch`. Un branch abbandonato senza PR si cancella sul server con `git push origin --delete <branch>`.

Se hai mergiato dal browser (**Squash and merge** → **Delete branch**), il branch sul tuo computer va tolto a mano con `git branch -D <branch>`. Serve la `-D` maiuscola: dopo lo squash su `main` c'è un commit nuovo, diverso dai tuoi, e `-d` risponde `not fully merged`. Non perdi niente: il lavoro è dentro `main` e nella PR. L'unico caso pericoloso è un branch **mai pushato**: se non sei sicuro, prima `git push`.

| Con `gh` | Dal browser |
|---|---|
| `gh pr create` | striscia **Compare & pull request**, o il link stampato da `git push` |
| `gh pr diff <n>` | scheda **Files changed** |
| `gh pr checkout <n>` | `git fetch origin` + `git switch <branch>` |
| `gh pr review --approve` | **Review changes** → *Approve* → *Submit review* |
| `gh pr merge --squash --delete-branch` | **Squash and merge** → **Delete branch**, poi `git branch -D` in locale |
| `gh issue edit <n> --add-assignee @me` | nella scheda: **Assignees** → *assign yourself* |

---

## 6. `main` si è mosso e i conflitti

Mentre lavori, i compagni mergiano altre PR. **Una volta al giorno** porta il loro lavoro nel tuo branch:

```bash
git switch main && git pull
git switch -          # "-" vuol dire il branch di prima
git merge main
```

Così, se un campo di `shared/` cambia nome, te ne accorgi il giorno dopo e sistemi tre righe, non duecento sotto scadenza. I merge dentro il tuo branch non sporcano `main`: lo squash li appiattisce.

### Un conflitto, passo per passo

Un conflitto **non è un errore** e non si perde niente: Git ti dice che tu e un altro avete cambiato le stesse righe dello stesso file, e chiede a te quale versione tenere.

1. `git status`: i file in conflitto sono sotto **Unmerged paths** (`both modified`).
2. Apri il file. Trovi:

   ```text
   <<<<<<< HEAD
   Avvio: docker compose up -d
   =======
   Avvio: docker compose up -d --wait && dotnet run --project be/GameExplorer.Api
   >>>>>>> main
   ```

   Sopra `=======` c'è la tua versione, sotto quella che arriva da `main`.
3. Tieni quello che serve (la tua, la sua, tutte e due o una terza) e **cancella le tre righe di marcatori**.
4. `git add <file>`, poi `git commit` (il messaggio `Merge branch 'main'...` va bene così).
5. Controllo finale: `git grep -n "<<<<<<<"` non deve trovare niente (a parte questa guida), e `dotnet build` deve passare.

**Se ti sei confuso:** `git merge --abort` torna esattamente a prima del merge. Usalo senza paura e chiedi al proprietario del file: un conflitto risolto male cancella il lavoro di un altro, e Git non te lo segnala più.

---

## 7. Le regole del team e il loro perché

**git, GitHub e gh sono tre cose diverse.** `git` è il programma sul tuo computer che tiene la storia dei file. GitHub è il sito che ne tiene una copia e aggiunge issue, Pull Request e revisioni: la Pull Request non esiste in Git. `gh` è il programma che parla con GitHub dal terminale.

**Il territorio.** I conflitti nascono quando due persone toccano le stesse righe dello stesso file, non quando lavorano nello stesso momento. Per questo ognuno scrive solo nelle sue cartelle (tabella in cima).

- **Se devi modificare un file fuori dal tuo territorio, avvisa il proprietario prima di iniziare**, e fai una PR piccola che rivede lui: per esempio i pacchetti del database nel `.csproj` della Web API, o le pagine di Dev C dentro `fe/`.
- **`compose.yaml` lo modifica solo `@Elmond0`:** se ti serve un servizio nuovo, aprigli una scheda.
- **`shared/` è il contratto tra backend e frontend:** se cambia un campo, si rompe il codice degli altri. Ogni modifica si annuncia sul gruppo **prima** della PR.

**Le decisioni si scrivono negli ADR.** Quando decidete insieme qualcosa che tocca più persone (il contratto delle API, dove va un pezzo di codice, come si fa il login), lo scrivete in un ADR in `DOCS/adr/`: contesto, decisione, alternative scartate, conseguenze. Si aggiunge con una PR `docs(adr): ...` rivista dagli altri due. Un ADR accettato non si riscrive: se la decisione cambia, ne serve uno nuovo ([cosa sono gli ADR](https://adr.github.io/)).

**Se sei bloccato, non aspettare.** Chiedi l'interfaccia, non il codice: con i nomi dei metodi e la forma dei dati puoi già scrivere la tua parte, usando una versione finta che restituisce dati fissi. E dillo sul gruppo: restare bloccati in silenzio è la cosa più costosa che si possa fare in tre.

**Le sei cose da non fare mai**

1. Committare su `main`.
2. `git push --force` su un branch condiviso: cancella il lavoro degli altri.
3. Committare il `.env`, una chiave o una password: restano nella cronologia anche se cancelli il file.
4. Toccare la cartella di un altro senza avvisarlo.
5. Approvare una PR senza averla guardata.
6. Lavorare senza una scheda assegnata a te.

**Chi chiedere per cosa**

| Argomento | Chi |
|---|---|
| Web API, RAWG, classi in `shared/`, Postman | `@K0ff33Blu3` |
| Pagine Blazor, MudBlazor, grafica | `@fmartini02` |
| PostgreSQL, EF Core e migrazioni, Docker, `.env`, login | `@Elmond0` |
| Impostazioni del repository GitHub | `@Elmond0` (proprietario) |
| Priorità, scadenze, chi fa cosa | tutti e tre, alla riunione della settimana |

---

## 8. Quando qualcosa va storto

> Quasi tutto si aggiusta. Prima di lanciare un comando con `--force` o `--hard`, chiedi sul gruppo.

| Cosa succede | Cosa fai |
|---|---|
| Non sai su che branch sei | `git status`: te lo dice la prima riga |
| `Your local changes would be overwritten` cambiando branch | hai lavoro non salvato: `git commit -m "wip: ..."`, oppure `git stash` e dopo `git stash pop` |
| `git push` rifiutato: `Updates were rejected` | sul server c'è lavoro che non hai: `git pull`, poi `git push`. **Mai** `--force` |
| Hai committato su `main` (senza pushare) | `git switch -c <area>/<cosa>` (il commit viene con te), `git switch main`, `git reset --hard origin/main`, `git switch -` |
| Vuoi buttare le modifiche a un file | `git restore <file>` (non si recupera) · solo togliere dall'area di staging: `git restore --staged <file>` |
| Messaggio dell'ultimo commit sbagliato (non pushato) | `git commit --amend -m "messaggio giusto"` |
| Hai committato una password o una chiave | **non** basta un altro commit: avvisa subito il gruppo e rigenera la chiave (per RAWG su [rawg.io/apidocs](https://rawg.io/apidocs)). Poi si decide insieme come ripulire |
| `gh`: `GraphQL: Projects (classic) is being deprecated` | `gh` vecchio: aggiornalo (§2, passo 2) |
| `gh`: `No commits between main and <branch>` | non hai committato, o hai dimenticato `git push` |
| `gh`: `Can not approve your own pull request` | serve un compagno |
| Si è aperto vim e non esci | `Esc`, poi `:wq` e Invio; per il futuro `gh config set editor nano` |

---

## 9. Protezione di `main`

Oggi `main` non è protetto: le regole valgono perché le rispettiamo noi. Per farle rispettare a GitHub, `@Elmond0` può creare un **ruleset** in *Settings → Rules → Rulesets → New branch ruleset*:

| Impostazione | Valore |
|---|---|
| Enforcement status | **Active** |
| Target branches | **Include default branch** (`main`) |
| Restrict deletions · Block force pushes | ✅ |
| Require a pull request before merging | ✅, con **1** approvazione |
| Allowed merge methods | solo **Squash** |
| Require status checks to pass | i controlli di `database.yml` (e di `backend.yml` quando esiste) |

Da quel momento un push diretto su `main` viene rifiutato, e il pulsante di merge resta grigio finché un compagno non approva e i controlli automatici non sono verdi.

---

## 10. Fonti

- **Git:** [Pro Git, in italiano](https://git-scm.com/book/it/v2) · [Branch e merge di base, conflitti](https://git-scm.com/book/en/v2/Git-Branching-Basic-Branching-and-Merging) · [`git config`](https://git-scm.com/docs/git-config) (`pull.ff`, `fetch.prune`)
- **GitHub CLI:** [manuale di `gh`](https://cli.github.com/manual/) · [installazione su Linux](https://github.com/cli/cli/blob/trunk/docs/install_linux.md)
- **GitHub:** [Collegare una PR a una issue (`Closes`)](https://docs.github.com/en/issues/tracking-your-work-with-issues/using-issues/linking-a-pull-request-to-an-issue) · [Metodi di merge (squash)](https://docs.github.com/en/repositories/configuring-branches-and-merges-in-your-repository/configuring-pull-request-merges/about-merge-methods-on-github) · [Revisioni delle PR](https://docs.github.com/en/pull-requests/collaborating-with-pull-requests/reviewing-changes-in-pull-requests/about-pull-request-reviews) · [Ruleset](https://docs.github.com/en/repositories/configuring-branches-and-merges-in-your-repository/managing-rulesets/about-rulesets) · [Email dei commit](https://docs.github.com/en/account-and-profile/setting-up-and-managing-your-personal-account-on-github/managing-email-preferences/setting-your-commit-email-address) · [Connettersi con SSH](https://docs.github.com/en/authentication/connecting-to-github-with-ssh)
- **Messaggi di commit:** [Conventional Commits 1.0.0](https://www.conventionalcommits.org/it/v1.0.0/)

### Documenti collegati

| File | Cosa contiene |
|---|---|
| [`Suddivisione del lavoro`](Suddivisione%20del%20lavoro.md) | chi fa cosa, fase per fase |
| [`subject/SUBJECT.md`](subject/SUBJECT.md) | la traccia del progetto |
| [`adr/`](adr/) | le decisioni del team con le motivazioni (ADR): `adr-01-stack.md` e i successivi |
| [`rawg-api.md`](rawg-api.md) | le chiamate dell'API RAWG |
| [`../db/README.md`](../db/README.md) | come si avvia il database locale |
| [Issues](https://github.com/Elmond0/Terranova-game-explorer/issues) | tutte le schede |
