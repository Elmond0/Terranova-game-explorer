# Come lavoriamo con Git

> Guida per il team — **Terranova Game Explorer**
> Leggila una volta. Poi tienila aperta le prime due settimane.

**In una frase:** su `main` non si committa mai direttamente. Tutto quello che
fai vive in un branch tuo, ed entra in `main` solo passando da una Pull Request
che un compagno ha approvato.

Se ti ricordi solo questo, il 90% è fatto.

---

## Il team

| Ruolo | GitHub | Cartelle | Label delle schede |
|---|---|---|---|
| **Dev A — backend** | [`@K0ff33Blu3`](https://github.com/K0ff33Blu3) | `be/` · `shared/` · `post-man/` | `BE` · `API` |
| **Dev B — frontend** | [`@fmartini02`](https://github.com/fmartini02) | `fe/` | `FE` |
| **Dev C — database** | [`@Elmond0`](https://github.com/Elmond0) | `db/` · `docker-compose.yml` · `.env.example` | `DB` |

Repository: [`github.com/Elmond0/Terranova-game-explorer`](https://github.com/Elmond0/Terranova-game-explorer)
· stack e motivazioni in [`adr/adr-01-stack.md`](adr/adr-01-stack.md).

---

# 🚀 Il riassunto: una scheda dall'inizio alla fine

**Se hai fretta, leggi solo questo.** È un lavoro vero, dal momento in cui
prendi una scheda a quando la chiudi.

Il giorno 2 e il giorno 3 **non** hanno `switch -c` né `--delete-branch`: il
branch esiste già e resta aperto finché il lavoro non è finito.

```bash
# ── GIORNO 1: apro la scheda ────────────────────────────────
git switch main && git pull

gh issue list --assignee @me                 # cosa devo fare
gh issue view 8                              # leggo la scheda
git switch -c db/cache-giochi                # apro il branch

git diff
git add db/Terranova.Data/GiocoCache.cs
git commit -m "feat(db): aggiunge l'entità GiocoCache"
git push -u origin db/cache-giochi           # -u solo stavolta
gh pr create --draft                         # PR in bozza: gli altri vedono cosa tocco
```

```bash
# ── GIORNO 2: continuo ──────────────────────────────────────
git switch db/cache-giochi                   # torno sul branch di ieri

git switch main && git pull
git switch -
git merge main

git add db/Terranova.Data/AppDbContext.cs
git commit -m "feat(db): aggiunge AppDbContext e la prima migrazione"
git push                                     # senza -u, è già collegato
```

```bash
# ── GIORNO 3: ho finito ─────────────────────────────────────
git switch main && git pull
git switch -
git merge main
git push

gh pr ready                                  # tolgo la bozza (nel corpo: Closes #8)
# ... un compagno la prova e approva ...
gh pr merge 12 --squash --delete-branch      # SOLO ADESSO si cancella
git switch main && git pull                  # e passo alla scheda dopo
```

> ### Le due regole che non stanno nei comandi
> 1. **`git diff` prima di ogni `git add`.** Dieci secondi che ti evitano di
>    pushare un `Console.WriteLine` di prova, il `.env` o la chiave RAWG.
> 2. **Pusha anche a metà lavoro.** Quello che non hai pushato non esiste: se
>    il portatile muore, è perso.

> **👤 Negli esempi:** sei **`@Elmond0`**, Dev C. Stai lavorando sulla scheda
> **#8** — *"tabella di cache dei giochi nel database"* — e la Pull Request che
> ne esce è la **#12** (numero inventato). Sostituisci con i tuoi valori.
>
> **Perché la PR ha un numero più alto della scheda:** su GitHub issue e Pull
> Request pescano dallo **stesso contatore**. Il numero dice solo in che ordine
> è nata. Non inventarlo: `gh pr create` te lo stampa quando la apre, e
> `gh pr status` te lo ricorda dopo.

---

## Indice

### 🚀 [Il riassunto: una scheda dall'inizio alla fine](#-il-riassunto-una-scheda-dallinizio-alla-fine)

### 1. [La prima volta: installare gh e collegarsi](#1-la-prima-volta-installare-gh-e-collegarsi)
*Una volta sola per computer, 15 minuti.*

- [Passo 0 — accetta l'invito](#passo-0--accetta-linvito)
- [Passo 1 — di' a git chi sei](#passo-1--di-a-git-chi-sei)
- [Passo 2 — installa gh](#passo-2--installa-gh)
- [Passo 3 — collegati](#passo-3--collegati)
- [Passo 4 — clona il repo](#passo-4--clona-il-repo)
- [Passo 5 — controlla che funzioni tutto](#passo-5--controlla-che-funzioni-tutto)
- [Se non puoi installare gh (macchine della scuola)](#se-non-puoi-installare-gh-macchine-della-scuola)

### 2. [Con gh — tutto dal terminale](#2-con-gh--tutto-dal-terminale)
- [2.1 Le schede](#21-le-schede)
- [2.2 Il codice](#22-il-codice)
- [2.3 La Pull Request](#23-la-pull-request)
- [2.4 Cancellare un branch a mano](#24-cancellare-un-branch-a-mano)
- [2.5 I comandi che userai davvero](#25-i-comandi-che-userai-davvero)
- [2.6 Errori tipici di gh](#26-errori-tipici-di-gh)

### 3. [Dal browser — senza gh](#3-dal-browser--senza-gh)
- [3.1 Le schede dal browser](#31-le-schede-dal-browser)
- [3.2 Aprire una PR dal browser](#32-aprire-una-pr-dal-browser)
- [3.3 Rivedere la PR di un altro dal browser](#33-rivedere-la-pr-di-un-altro-dal-browser)
- [3.4 Mergiare dal browser e pulire in locale](#34-mergiare-dal-browser-e-pulire-in-locale)
- [3.5 Tabella delle corrispondenze](#35-tabella-delle-corrispondenze)

### 4. [Il perché di ogni cosa](#4-il-perché-di-ogni-cosa)
- [4.1 git, GitHub e gh sono tre cose diverse](#41-git-github-e-gh-sono-tre-cose-diverse)
- [4.2 Il tuo territorio](#42-il-tuo-territorio)
- [4.3 Le schede: perché si parte sempre da una](#43-le-schede-perché-si-parte-sempre-da-una)
- [4.4 Come si chiama un branch](#44-come-si-chiama-un-branch)
- [4.5 Come si scrive un commit e una PR](#45-come-si-scrive-un-commit-e-una-pr)
- [4.6 Come si rivede la PR di un altro](#46-come-si-rivede-la-pr-di-un-altro)
- [4.7 `main` si è mosso mentre lavoravi](#47-main-si-è-mosso-mentre-lavoravi)
- [4.8 I conflitti — guida passo passo](#48-i-conflitti--guida-passo-passo)
- [4.9 Se sei bloccato](#49-se-sei-bloccato)
- [4.10 Le sei cose da non fare mai](#410-le-sei-cose-da-non-fare-mai)
- [4.11 Ho sbagliato — come torno indietro](#411-ho-sbagliato--come-torno-indietro)
- [4.12 Chi chiedere per cosa](#412-chi-chiedere-per-cosa)
- [4.13 Protezione di `main` (da attivare)](#413-protezione-di-main-da-attivare)

### [Documenti collegati](#documenti-collegati)

---

# 1. La prima volta: installare gh e collegarsi

> ⏱️ 15 minuti, si fa una volta sola **per computer**.

**Fallo in quest'ordine.** Se salti un passo, quello dopo non funziona. In
particolare: **non clonare prima di esserti autenticato**, altrimenti ti
risponde `Permission denied (publickey)`.

## Passo 0 — accetta l'invito

Il repository è di `@Elmond0`: gli altri due vengono aggiunti come
collaboratori. Ti arriva una mail da GitHub, oppure lo trovi su
[github.com/notifications](https://github.com/notifications). Finché non lo
accetti **non risulti nel repo**: non puoi pushare e non ricevi le richieste di
review.

## Passo 1 — di' a git chi sei

```bash
git config --global user.name "Nome Cognome"
git config --global user.email "la-tua-email-di-github"
git config --global fetch.prune true
git config --global pull.ff only
```

> ⚠️ **L'email deve essere quella dell'account GitHub che usi per il
> progetto.** GitHub attribuisce ogni commit all'account che possiede
> quell'email. Se ne metti un'altra, i tuoi commit risultano di un altro
> account (o di nessuno) e, a fine progetto, il tuo lavoro non compare tra i
> contributi individuali — che la traccia valuta.
>
> Controllo: apri [la lista dei commit](https://github.com/Elmond0/Terranova-game-explorer/commits/main)
> e verifica che accanto ai tuoi ci sia **il tuo** nome utente.

`fetch.prune` cancella dal tuo computer i branch che sul server non esistono
più. Senza, fra un mese hai 40 branch fantasma e non capisci più quali sono
vivi.

`pull.ff only` fa sì che `git pull` accetti solo aggiornamenti lineari. Se la
tua linea e quella del server si sono separate, si ferma e te lo dice invece di
inventarsi un merge a sorpresa.

## Passo 2 — installa gh

| Sistema | Comando |
|---|---|
| Arch / CachyOS | `sudo pacman -S github-cli` |
| macOS | `brew install gh` |
| Ubuntu / Debian | repository ufficiale di GitHub, vedi sotto |

> ⚠️ **Su Ubuntu non usare `sudo apt install gh`.** Il pacchetto di Ubuntu è
> vecchio e alcuni comandi (`gh issue view`) falliscono con
> `GraphQL: Projects (classic) is being deprecated`.

```bash
(type -p wget >/dev/null || sudo apt install wget -y) \
&& sudo mkdir -p -m 755 /etc/apt/keyrings \
&& wget -qO- https://cli.github.com/packages/githubcli-archive-keyring.gpg \
   | sudo tee /etc/apt/keyrings/githubcli-archive-keyring.gpg > /dev/null \
&& sudo chmod go+r /etc/apt/keyrings/githubcli-archive-keyring.gpg \
&& echo "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/githubcli-archive-keyring.gpg] https://cli.github.com/packages stable main" \
   | sudo tee /etc/apt/sources.list.d/github-cli.list > /dev/null \
&& sudo apt update && sudo apt install gh -y
```

Controlla di avere una versione recente (2.60 o superiore):

```bash
gh --version
```

## Passo 3 — collegati

```bash
gh auth login
```

| Domanda | Risposta |
|---|---|
| Where? | **GitHub.com** |
| Protocol? | **SSH** |
| Generate a new SSH key? | **Sì** (premi Invio se chiede una passphrase, va bene vuota) |
| How to authenticate? | **Login with a web browser** → ti dà un codice, lo incolli nella pagina che si apre |

> Questo passo configura **anche la chiave SSH**. È il motivo per cui si fa
> prima di clonare.

Metti anche un editor che sai usare: `gh` a volte te ne apre uno per scrivere
il testo della PR, e di default è vim.

```bash
gh config set editor nano
```

## Passo 4 — clona il repo

```bash
git clone git@github.com:Elmond0/Terranova-game-explorer.git
cd Terranova-game-explorer
```

## Passo 5 — controlla che funzioni tutto

```bash
git log --oneline -3        # deve mostrarti dei commit
gh auth status              # deve dire Logged in
gh issue list               # deve elencare le schede
git config --get user.email # deve essere l'email di GitHub
```

## Se non puoi installare gh (macchine della scuola)

Niente `sudo`? Non sei bloccato: **i comandi `git` non cambiano di una
virgola.** `gh` serve solo per Pull Request e schede, e ogni cosa che fa ha il
suo pulsante su github.com ([sezione 3](#3-dal-browser--senza-gh)).

**Prima di arrenderti, prova lo stesso.** `gh` è un singolo eseguibile: puoi
tenerlo nella tua home senza installare niente a livello di sistema
(su Mac usa il file `macOS_arm64.zip` della stessa pagina dei rilasci).

```bash
mkdir -p ~/bin && cd /tmp
VER=$(curl -s https://api.github.com/repos/cli/cli/releases/latest \
      | grep -oP '"tag_name": *"v\K[^"]+')
curl -sL "https://github.com/cli/cli/releases/download/v${VER}/gh_${VER}_linux_amd64.tar.gz" -o gh.tgz
tar xzf gh.tgz && cp gh_${VER}_linux_amd64/bin/gh ~/bin/
echo 'export PATH="$HOME/bin:$PATH"' >> ~/.zshrc    # o ~/.bashrc
source ~/.zshrc
gh --version
```

### La chiave SSH a mano

Se proprio non riesci a installare `gh`, la chiave SSH la crei tu, una volta
sola **per macchina**.

```bash
ls ~/.ssh/id_ed25519.pub                  # 1. ce l'hai già? Se sì, salta al 3
ssh-keygen -t ed25519 -C "la-tua-email"   # 2. creala (Invio tre volte)
cat ~/.ssh/id_ed25519.pub                 # 3. copiala TUTTA
```

4. Incollala su GitHub: **Settings → SSH and GPG keys → New SSH key**
   Title: *"macchina scuola"*, Key: incolla → **Add SSH key**
5. Verifica con `ssh -T git@github.com`: deve rispondere
   `Hi <tuonome>! You've successfully authenticated...`
   (*"does not provide shell access"* alla fine è normale.)

> ⚠️ **La chiave è legata alla macchina.** Se lavori sia sul tuo portatile sia
> su quelle della scuola, ne servono due, una per computer.

---

# 2. Con gh — tutto dal terminale

> Qui c'è **cosa digitare**. Il perché di ogni scelta sta nella
> [sezione 4](#4-il-perché-di-ogni-cosa).

## 2.1 Le schede

**Quali sono le mie:**

```bash
gh issue list --assignee @me            # assegnate a me
gh issue list --label DB                # tutte quelle del database (BE, FE, API...)
gh issue list --label PO                # le urgenti
gh issue list --search "cache"          # cercarne una
gh issue status                         # cosa mi riguarda adesso
```

**Leggerla** — cosa fare, dipendenze, "fatto quando":

```bash
gh issue view 8
```

**Prendersela**, se non è già assegnata a te:

```bash
gh issue edit 8 --add-assignee @me
```

**Dire che sei bloccato** — mai restare fermo in silenzio:

```bash
gh issue comment 8 --body "Fermo: mi serve la classe DettaglioGioco della #2 per sapere cosa salvare. @K0ff33Blu3"
```

**Aprirne una nuova** — ogni volta che trovi una cosa da fare che non è già una
scheda:

```bash
gh issue create          # ti fa le domande a video
```

Oppure in un comando solo:

```bash
gh issue create --title "fix(fe): la ricerca non si svuota cambiando pagina" \
  --body "Torno alla Home e la barra di ricerca mostra ancora il testo di prima.

Fatto quando: tornando alla Home la barra è vuota." \
  --label FE --assignee fmartini02
```

**Caso frequente: ti serve una cosa nel territorio di un altro.** Non
modificare i suoi file — aprigli una scheda.

```bash
gh issue create --title "chore(db): aggiungere il servizio api al docker-compose" \
  --body "Il Dockerfile del backend è pronto in be/.
Serve il blocco nel docker-compose.yml.

Fatto quando: docker compose up avvia anche il backend." \
  --label DB --assignee Elmond0
```

## 2.2 Il codice

**Ogni volta che inizi a lavorare, prima di qualunque cosa:**

```bash
git switch main && git pull
```

**Apro il branch della scheda** — il nome è `<area>/<cosa>`
([come si sceglie](#44-come-si-chiama-un-branch)):

```bash
git switch -c db/cache-giochi
```

**Mentre lavoro** — commit piccoli e frequenti, non uno alla fine:

```bash
git status                  # dove sono, cosa ho toccato
git diff                    # cosa ho cambiato: SEMPRE prima di add
git add db/Terranova.Data/GiocoCache.cs
git commit -m "feat(db): aggiunge l'entità GiocoCache"
```

> ⚠️ **Mai `git add .` alla cieca.** Aggiungi i file per nome: così non ti
> porti dietro cartelle `bin/`, `obj/`, file di prova o il `.env`.

**Mando su:**

```bash
git push -u origin db/cache-giochi   # la PRIMA volta di questo branch
git push                             # tutte le altre
```

**Il giorno dopo riprendo il branch di ieri.** Non ne creo uno nuovo:

```bash
git switch db/cache-giochi
```

**Una volta al giorno mi porto dentro il lavoro degli altri** — il trattino
vuol dire *"il branch di prima"*:

```bash
git switch main && git pull
git switch -
git merge main
```

> Se esce `CONFLICT` non hai rotto niente:
> [guida ai conflitti](#48-i-conflitti--guida-passo-passo).

## 2.3 La Pull Request

Prima di tutto il branch deve esistere sul server: non puoi aprire una PR su un
branch che GitHub non conosce.

```bash
git push -u origin <nome-del-tuo-branch>
```

### Aprirla — tre modi, dal più facile

**Modo 1 — interattivo**, consigliato le prime volte:

```bash
gh pr create
```

| Domanda | Cosa fare |
|---|---|
| Where should we push the branch? | conferma |
| Title | scrivilo e Invio |
| Body | si apre l'editor: scrivi la riga `Closes #N` e le quattro sezioni ([modello](#45-come-si-scrive-un-commit-e-una-pr)), salva ed esci (nano: `Ctrl+O`, `Invio`, `Ctrl+X`) |
| What's next? | scegli **Submit** |

Alla fine ti stampa il link e **il numero** della PR.

**Modo 2 — precompilato dal commit:**

```bash
gh pr create --fill
```

> ⚠️ Così la PR **non** avrà le quattro sezioni né `Closes #N`. Usalo solo per
> modifiche minuscole che si spiegano da sole.

**Modo 3 — tutto in un comando:**

```bash
gh pr create --base main \
  --title "feat(db): PostgreSQL in Docker" \
  --body "Rif. #8

## Cosa contiene
- servizio db nel docker-compose.yml (postgres:18, volume, healthcheck)
- .env.example con le variabili richieste

## Perché così
Il volume su /var/lib/postgresql è il percorso richiesto da PostgreSQL 18.

## Sblocca
@K0ff33Blu3, che può collegare il backend al database.

## Come verificare
cp .env.example .env, impostare la password, poi docker compose up -d e docker compose ps: lo stato deve essere healthy"
```

> ⚠️ Dentro `--body` **non usare virgolette doppie**, o il comando si spezza a
> metà. Usa gli apici singoli, o niente.

**In bozza (draft)** — per aprirla appena hai il primo commit, così gli altri
vedono cosa stai toccando. Nessuno la mergia finché è in bozza:

```bash
gh pr create --draft
gh pr ready              # quando hai finito
```

**Riempire il testo nel browser**, con tutto già precompilato:

```bash
gh pr create --web
```

Poi avvisa sul gruppo: «PR #12 pronta, la rivede qualcuno?»

### Rivedere la PR di un compagno

Succede ogni giorno. È lavoro, non un favore — [perché](#46-come-si-rivede-la-pr-di-un-altro).

```bash
gh pr list                   # quali sono aperte
gh pr view 12                # titolo, testo, stato
gh pr diff 12                # cosa ha cambiato

gh pr checkout 12            # provarla sul tuo computer
git switch -                 # tornare al tuo lavoro

gh pr review 12 --approve
gh pr review 12 --comment -b "Una domanda su X..."
gh pr review 12 --request-changes -b "Se RAWG risponde 404 qui va in errore"
```

`-b` è il testo del commento; se lo ometti, `gh` apre l'editor. Per commentare
**una riga precisa** conviene il browser: `gh pr view --web`, poi passa il
mouse sul numero di riga.

### Mergiare

**Mergia chi ha aperto la PR**, dopo l'approvazione di un compagno.

```bash
gh pr merge 12 --squash --delete-branch
git switch main && git pull
```

Mergia schiacciando tutti i commit in uno solo, cancella il branch sul server
**e** in locale, e ti riporta su `main`.

> ### ⚠️ Il branch si cancella una volta sola, alla fine
> `--delete-branch` si usa **solo nel momento in cui la PR viene mergiata**,
> cioè quando il lavoro su quella scheda è finito davvero.
>
> Finché ci stai ancora lavorando — anche se pushi dieci volte, anche a
> distanza di giorni — **il branch resta lì.** La mattina ci torni sopra con
> `git switch`.

**Se si rifiuta con `Pull request is not mergeable`:** ci sono
[conflitti](#48-i-conflitti--guida-passo-passo) con `main`. Fai `git merge main`
sul tuo branch, risolvi, pusha e riprova.

**Chiudere una PR senza mergiarla:**

```bash
gh pr close 12 --delete-branch
```

## 2.4 Cancellare un branch a mano

> Con `gh pr merge --delete-branch` non serve: lo fa lui. Questi comandi
> servono quando hai mergiato dal browser, oppure quando **abbandoni un
> branch** senza mergiarlo.

```bash
git branch -a                                # che branch ho
git switch main                              # esco da quello da cancellare
git branch -D db/cache-giochi                # lo cancello in locale
git push origin --delete db/cache-giochi     # e sul server
git fetch --prune                            # ripulisco i fantasmi
```

I branch che iniziano con `remotes/origin/` stanno sul server, gli altri solo
sul tuo computer: sono due cose separate e si cancellano separatamente.

> ### Perché `-D` maiuscola e non `-d` minuscola
> Con `-d` git risponde `error: the branch '<nome>' is not fully merged`.
> Sembra grave, non lo è. Noi usiamo lo **squash**: su `main` è finito **un
> commit nuovo**, diverso dai tuoi. Per git i tuoi commit originali non
> risultano dentro `main`, quindi `-d` (che è prudente) si rifiuta. `-D`
> cancella e basta.
>
> **Non perdi niente:** il tuo lavoro è dentro `main`, schiacciato in quel
> commit, ed è anche nella PR su GitHub per sempre.
>
> ⚠️ L'unico caso in cui `-D` fa perdere del lavoro è un branch **mai pushato
> e mai mergiato**. Se non sei sicuro, fai prima `git push`.

## 2.5 I comandi che userai davvero

```bash
# le schede
gh issue list --assignee @me            # le mie
gh issue view 8                         # leggerne una
gh issue edit 8 --add-assignee @me      # prendermela
gh issue comment 8 --body "..."         # dire che sono bloccato
gh issue create                         # aprirne una nuova

# il codice
git switch main && git pull             # prima di iniziare
git switch -c <area>/<cosa>             # nuovo lavoro
git status                              # dove sono, cosa ho toccato
git diff                                # cosa ho cambiato
git add <file>                          # scelgo cosa committare
git commit -m "tipo(area): ..."         # sigillo
git push                                # mando su
git switch -                            # torno al branch di prima
git merge main                          # mi porto dentro gli altri

# la pull request
gh pr create                            # apro la PR (con "Closes #8")
gh pr status                            # cosa mi riguarda
gh pr diff <n>                          # cosa ha cambiato un altro
gh pr checkout <n>                      # provo la sua PR
gh pr review <n> --approve              # approvo
gh pr merge <n> --squash --delete-branch  # mergio
```

> Non ne servono altri per il 95% del tempo.

## 2.6 Errori tipici di gh

| Messaggio | Cosa fare |
|---|---|
| `GraphQL: Projects (classic) is being deprecated` e nessun output | Il tuo `gh` è vecchio. Aggiornalo: [passo 2](#passo-2--installa-gh). |
| `failed to run git: fatal: not a git repository` | Non sei dentro la cartella del progetto. Fai `cd`. |
| `must be on a branch named differently than "main"` | Stai aprendo una PR da `main`. Crea un branch. |
| `pull request create failed: GraphQL: No commits between main and <branch>` | Non hai committato niente, o hai dimenticato `git push`. |
| `a pull request for branch "<branch>" into branch "main" already exists:` | Non è un errore: ce l'hai già e `gh` ti ridà il link. Ogni push la aggiorna da solo. |
| `Review Can not approve your own pull request` | **Non puoi approvare la tua stessa PR.** Serve un compagno. |
| Si è aperto un editor strano e non riesco a uscire | È vim: `Esc`, poi `:wq` e Invio. Per non ritrovartelo più: `gh config set editor nano` |

---

# 3. Dal browser — senza gh

> **I comandi `git` restano identici.** Cambia solo quello che faceva `gh`:
> schede e Pull Request. Puoi anche lavorare metà e metà — git dal terminale,
> le PR dal browser.

## 3.1 Le schede dal browser

Tab **Issues** del repo. **Trovare le mie** — casella di ricerca in cima:

```
is:open assignee:@me       le mie
is:open label:DB           tutte quelle del database
is:open label:PO           le urgenti
is:open no:assignee        schede libere, senza padrone
```

**Prendersela:** apri la scheda → colonna di destra → **Assignees** →
*assign yourself*.

**Scriverci sopra:** riquadro dei commenti in fondo alla scheda. Usalo quando
sei bloccato, con `@nome` per far arrivare la notifica.

**Aprirne una nuova:**

1. Tab **Issues** → pulsante verde **New issue**
2. **Titolo:** `tipo(area): cosa`, scritto come un commit
3. **Corpo:** cosa va fatto, e una riga *"Fatto quando: …"*
4. Colonna di destra → **Labels** (`BE`, `FE`, `DB`, …) e **Assignees**
5. **Create**

**Chiuderla: non si chiude a mano.** Nel corpo della PR scrivi `Closes #8` e
al merge GitHub la chiude da solo. Il pulsante **Close issue** si usa solo
quando la scheda non serve più e non ci sarà nessuna PR (duplicato, cosa
decisa diversamente): in quel caso scrivi prima un commento che dice perché.

## 3.2 Aprire una PR dal browser

**La scorciatoia:** quando fai `git push` di un branch nuovo, git stampa già il
link giusto.

```
remote: Create a pull request for 'db/cache-giochi' on GitHub by visiting:
remote:   https://github.com/Elmond0/Terranova-game-explorer/pull/new/db/cache-giochi
```

**Se hai perso il link:** vai sul repo, in alto compare una striscia gialla
col tuo branch e il pulsante **Compare & pull request**. Se non c'è: tab
**Pull requests** → **New pull request** → base `main`, compare il tuo branch.

Poi: titolo come il commit, `Closes #N` e le quattro sezioni nel testo,
pulsante verde **Create pull request** (o la freccia accanto → **Create draft
pull request** per la bozza).

## 3.3 Rivedere la PR di un altro dal browser

1. Tab **Pull requests** → apri la sua PR
2. Tab **Files changed** → vedi tutte le sue modifiche

**Per provarla sul tuo computer** (sostituisce `gh pr checkout`):

```bash
git fetch origin
git switch <nome-del-suo-branch>
# ... provala, segui la sua sezione "Come verificare" ...
git switch -                # torna al tuo lavoro
```

**Per rispondere:** tab **Files changed** → **Review changes** in alto a destra
→ **Comment** (una domanda) / **Approve** (via libera) / **Request changes**
(c'è da sistemare) → **Submit review**.

> Per commentare **una riga precisa**: passa il mouse sul numero della riga,
> compare un `+` azzurro, cliccalo e scrivi.

## 3.4 Mergiare dal browser e pulire in locale

1. In fondo alla PR, pulsante verde **Squash and merge**
   (se il pulsante dice *Merge pull request*, apri la freccia accanto e scegli
   **Squash and merge**)
2. **Confirm squash and merge**
3. Compare **Delete branch**: **cliccalo.** Cancella il branch dal server.

Il branch sul **tuo computer** resta: quello lo cancelli tu.

```bash
git switch main
git pull
git branch -D <nome-del-tuo-branch>
git branch -a                  # devi vedere solo main e origin/main
```

## 3.5 Tabella delle corrispondenze

| Terminale | Browser |
|---|---|
| `gh pr create` | striscia gialla **Compare & pull request**, o il link che stampa `git push` |
| `gh pr view` | la pagina della PR |
| `gh pr diff` | tab **Files changed** |
| `gh pr checkout <n>` | `git fetch origin` + `git switch <nome-del-branch>` |
| `gh pr review --approve` | **Review changes** → Approve → Submit |
| `gh pr merge --squash --delete-branch` | **Squash and merge** + **Delete branch** + `git branch -D` in locale |
| `gh pr list` | tab **Pull requests** del repo |
| `gh pr close <n>` | in fondo alla PR: **Close pull request** |
| `gh issue list --assignee @me` | tab **Issues** + filtro `assignee:@me` |
| `gh issue view <n>` | clic sul titolo della scheda |
| `gh issue edit <n> --add-assignee @me` | colonna destra → Assignees → *assign yourself* |
| `gh issue create` | tab **Issues** → **New issue** |
| `gh issue comment <n>` | riquadro dei commenti in fondo alla scheda |

---

# 4. Il perché di ogni cosa

## 4.1 git, GitHub e gh sono tre cose diverse

| | Cos'è |
|---|---|
| **git** | Un programma **sul tuo computer**. Gestisce la storia dei file in locale. Non sa cosa sia GitHub. Funziona anche senza internet. |
| **GitHub** | Un **sito** (un server). Tiene una copia del repo e aggiunge cose che git non ha: Pull Request, review, issue. |
| **gh** | Un programma sul tuo computer che **parla con GitHub** dal terminale, così non devi aprire il browser. |

> **git non sa cosa sia una Pull Request.** `git pull-request` non è un
> comando. La Pull Request è un'invenzione di GitHub e vive sul loro server,
> non nel tuo repo.

**Cos'è davvero una Pull Request.** È una richiesta scritta su GitHub che dice:

> *"Ho il branch `db/cache-giochi`. Confrontalo con `main` e mostra le
> differenze. Voglio che entri in `main`. Chi vuole può commentare, provarlo e
> approvarlo."*

**Cosa fa ogni comando:**

```
 IL TUO COMPUTER                      GITHUB (il server)
 ---------------                      ------------------

 git switch -c mio-branch
 git add / git commit
      |
 [i commit stanno qui,
  solo tu li vedi]

 git push  ------------------------>  copia i commit sul server
                                      (ora li vedono anche gli altri)

 gh pr create  -------------------->  crea la pagina della PR

 gh pr review  -------------------->  un compagno approva

 gh pr merge  --------------------->  "mergia": il branch entra in main

 git pull  <-----------------------   scarica il main aggiornato
```

**`gh` non decide niente.** Manda richieste e riporta risposte.

## 4.2 Il tuo territorio

Ognuno ha delle cartelle dove è **l'unico a scrivere**. È così che tre persone
lavorano insieme senza pestarsi i piedi: i conflitti Git nascono quando due
persone toccano **le stesse righe dello stesso file**, non quando lavorano
nello stesso momento.

| Dev | Cartelle e file |
|---|---|
| **`@K0ff33Blu3`** (backend) | `be/` · `shared/` (classi condivise) · `post-man/` · il file della soluzione `.slnx` |
| **`@fmartini02`** (frontend) | `fe/` |
| **`@Elmond0`** (database) | `db/` · `docker-compose.yml` · `.env.example` |
| **tutti** | `DOCS/` · `README.md` · `.gitignore` — modifiche piccole, avvisando gli altri |

> **Regola d'oro.** Se stai per modificare un file fuori dal tuo territorio,
> **fermati e avvisa il proprietario prima di iniziare.** È lui che dovrà
> approvare la tua PR, e se non se lo aspetta resterai fermo due giorni.

> ⚠️ **`docker-compose.yml` lo modifica solo `@Elmond0`.** Se ti serve un
> servizio nuovo lì dentro (per esempio il backend), apri una scheda e
> chiediglielo. Due persone su quel file = conflitto a ogni merge.

> ⚠️ **`shared/` è il contratto tra frontend e backend.** Le classi dei dati
> (`Gioco`, `DettaglioGioco`, …) le usano tutti: se `@K0ff33Blu3` cambia il
> nome di un campo, si rompe il codice di `@fmartini02` e di `@Elmond0`.
> Ogni modifica lì va annunciata sul gruppo **prima** della PR.

## 4.3 Le schede: perché si parte sempre da una

Ogni cosa da fare è una **issue** sul repo. Non si inventa il lavoro la
mattina: **si prende una scheda.**

> ⚠️ **Non si inizia mai a scrivere codice senza una scheda.** Se stai per fare
> una cosa che non è una issue, aprila prima. Serve a due cose: gli altri sanno
> cosa stai toccando, e a fine progetto risulta chi ha fatto cosa — la traccia
> chiede che ognuno sappia spiegare il proprio codice.

**Come si legge una scheda:**

```
#8  feat(db): tabella di cache dei giochi nel database, per ridurre le chiamate a RAWG
    label: DB    assegnata a: Elmond0
```

**Le label del repo:**

| Label | Vuol dire |
|---|---|
| `BE` | backend — `@K0ff33Blu3` |
| `FE` | frontend — `@fmartini02` |
| `DB` | database — `@Elmond0` |
| `API` | riguarda gli endpoint REST (contratto tra backend e frontend) |
| `FASE_0` | le basi: da fare per prime |
| `FASE_1` | prime funzionalità |
| `PO` | urgente: prima delle altre |
| `question` | serve una risposta prima di poter lavorare |
| `invalid` · `wontfix` | scheda sbagliata o che non si farà: si chiude con un commento |

Dentro la scheda trovi cosa fare, le dipendenze e soprattutto il
**"fatto quando"**: i criteri per dire che è finita.

> **Se non soddisfi il "fatto quando", la scheda non è finita**, anche se il
> codice funziona.

**Quale prendo?** Prima le `PO`, poi le `FASE_0`, poi le `FASE_1`. Dentro lo
stesso livello, prima quelle da cui dipende un compagno: stai fermando
qualcuno.

**Una alla volta.** Due schede aperte insieme vuol dire due branch aperti
insieme, e su uno dei due non stai davvero lavorando.

### `Closes #8` — la riga che fa tutto

Nel corpo della Pull Request, su una riga sua. Non è un commento: è un
**comando che GitHub capisce**. Quando la PR viene mergiata:

- l'issue `#8` si chiude da sola
- la PR e la issue restano collegate per sempre

Funzionano anche `Fixes #8` e `Resolves #8`: sono la stessa cosa.

- **Più schede in una PR:** `Closes #1, closes #2` — la parola va ripetuta
  davanti a ogni numero, altrimenti chiude solo la prima.
- **Se la PR riguarda la scheda ma non la chiude** (una parte del lavoro):
  scrivi `Rif. #8`, collega senza chiudere.

## 4.4 Come si chiama un branch

```
<area>/<cosa>
```

**Le aree sono le cartelle**, non etichette a caso:

| Area | Cartella |
|---|---|
| `be` | `be/`, `shared/` |
| `fe` | `fe/` |
| `db` | `db/`, `docker-compose.yml` |
| `api` | `post-man/` e contratto degli endpoint |
| `docs` | `DOCS/`, README |
| `setup` | configurazione del repo (`.gitignore`, soluzione) |
| `fix` | stai correggendo un bug |

> Se non sai quale area scrivere, non hai ancora capito quale cartella stai per
> toccare. Fermati e controlla che sia la tua.

**Il "cosa": 2-4 parole, minuscolo, con i trattini.**

| ✅ | ❌ |
|---|---|
| `db/cache-giochi` | `db/modifiche` |
| `be/rawg-service` | `be/fix2` |
| `fe/pagina-ricerca` | `prova` |

> ⚠️ **Mai** un branch col tuo nome e basta (`elmondo`, `elmondo2`,
> `elmondo-save`). Quelli restano aperti per mesi ed è così che il progetto
> esplode l'ultima settimana.

> Nel nome del branch **non va il numero della scheda**. Il collegamento si fa
> nella PR, con `Closes #8`.

## 4.5 Come si scrive un commit e una PR

**Commit:**

```
tipo(area): cosa hai fatto, al presente, minuscolo
```

Tipi: `feat` (funzionalità nuova) · `fix` (bug) · `chore` (manutenzione,
configurazione) · `docs` · `refactor` · `test`
Esempi: `feat(be): aggiunge RawgService.CercaGiochi` ·
`chore(db): aggiunge PostgreSQL al docker-compose`

Se c'è un perché non ovvio, aggiungilo con un secondo `-m`:

```bash
git commit -m "fix(db): salva le date in UTC" \
           -m "Npgsql rifiuta le date con fuso orario diverso da UTC nelle colonne timestamptz."
```

> **Il titolo dice cosa, il corpo dice perché.** Il cosa si vede già dal
> codice; il perché fra due mesi non se lo ricorda nessuno.

**Pull Request: una riga di collegamento + quattro sezioni.**

```markdown
Closes #8

## Cosa contiene
elenco puntato di cosa hai fatto

## Perché così
le scelte che hai preso e il motivo

## Sblocca
chi stava aspettando questo lavoro (se nessuno, togli la sezione)

## Come verificare
i comandi ESATTI che il compagno deve digitare per provarla
```

**"Come verificare" è la sezione più importante delle quattro.** Stai dicendo
al tuo compagno come controllare il tuo lavoro in trenta secondi. Per il
backend sono spesso le richieste della collection Postman in `post-man/`.

> ⚠️ **Tieni le PR piccole.** Una PR da 40 file non viene rivista davvero:
> viene timbrata.

## 4.6 Come si rivede la PR di un altro

Metà del tuo lavoro su Git è questo. **Quando ti arriva una richiesta di
review, entro la giornata.** In tre, se uno non rivede, gli altri due sono
fermi.

**Cosa guardare, in quest'ordine:**

1. Fa quello che dice di fare? Soddisfa il "fatto quando" della scheda?
2. Tocca solo le cartelle che doveva toccare?
3. C'è finita dentro roba che non c'entra? (`bin/`, `obj/`, file di prova, un
   `.env`, la chiave RAWG)
4. Il nome delle cose si capisce senza chiedere?

E soprattutto: **provala davvero**, seguendo la sua sezione "Come verificare".
È anche il modo per capire il codice degli altri: in valutazione ognuno deve
saper spiegare tutto il progetto, non solo la propria parte.

> **Non riscrivere il codice del compagno nella review.** Se non ti piace come
> l'ha fatto ma funziona, approva e parlagliene a voce.

> **Approva solo se l'hai davvero guardata.** Un'approvazione automatica vale
> zero e ti rende corresponsabile di quello che si rompe.

## 4.7 `main` si è mosso mentre lavoravi

Succede sempre: apri un branch, ci lavori, e nel frattempo un compagno ha
mergiato altre PR. Normale.

```bash
git switch main && git pull
git switch -                  # torna al tuo branch (- = "quello di prima")
git merge main
```

**La direzione è una sola:** stai **tirando dentro** il lavoro degli altri. Il
tuo codice resta invisibile a tutti finché non apri la PR.

Serve a **non restare indietro**: se `@K0ff33Blu3` rinomina un campo in
`shared/` e te ne accorgi il giorno dopo, sistemi tre righe in cinque minuti;
se te ne accorgi dopo due settimane, ne sistemi duecento sotto scadenza.

> **I merge dentro il tuo branch non sporcano `main`.** Al momento della PR,
> `--squash` li appiattisce tutti in un commit solo.

## 4.8 I conflitti — guida passo passo

> ### 🟢 Prima di tutto: non hai rotto niente.
> Un conflitto non è un errore e non si perde niente. Git ti sta solo dicendo:
> *"tu e un altro avete cambiato le stesse righe dello stesso file, decidete
> voi quale versione tenere"*. Si risolve in cinque minuti.

Quasi sempre esce qui, dopo `git merge main`:

```
CONFLICT (content): Merge conflict in README.md
Automatic merge failed; fix conflicts and then commit the result.
```

**Passo 1 — vedi quali file:**

```bash
git status
```

Sono elencati sotto **Unmerged paths**, come `both modified: <file>`. Di
solito è **uno**.

**Passo 2 — apri quel file.** Dentro trovi una cosa così:

```
<<<<<<< HEAD
Avvio: docker compose up -d
=======
Avvio: docker compose up -d && dotnet run --project be/Terranova.Api
>>>>>>> main
```

| Tra… | C'è… |
|---|---|
| `<<<<<<< HEAD` e `=======` | **la tua** versione |
| `=======` e `>>>>>>> main` | quella che arriva da `main` (il lavoro di un altro) |

**Passo 3 — decidi e pulisci.** Tieni quello che serve e **cancella le tre
righe di marcatori** (`<<<<<<<`, `=======`, `>>>>>>>`). Puoi tenere la tua
versione, la sua, tutte e due, o scriverne una terza che le mette insieme.

**Passo 4 — di' a Git che hai finito:**

```bash
git add <il file che hai sistemato>
git status                          # controlla di non averne dimenticati
```

**Passo 5 — chiudi:**

```bash
git commit
```

Si apre un editor con un messaggio già scritto (`Merge branch 'main' into
...`). Va benissimo così: salva ed esci.

**Controllo finale — fallo sempre:**

```bash
grep -rn "<<<<<<<" . --exclude-dir=.git
dotnet build                        # se hai toccato codice C#
```

Se `grep` non stampa niente sei a posto. Un file con dentro `<<<<<<<` **non
compila**.

> **Unica eccezione:** se ti segnala `DOCS/come_lavoriamo.md`, ignoralo. Sono
> i marcatori d'esempio di questa guida.

### Se ti sei confuso

```bash
git merge --abort
```

Torna esattamente a prima del merge. Il tuo lavoro è intatto. **Usalo senza
paura** quando non sai quale versione tenere: scrivi al proprietario di quel
file ([chi chiedere](#412-chi-chiedere-per-cosa)). Un conflitto **risolto
male** è molto peggio di uno non risolto: cancelli il lavoro di un altro e Git
non te lo segnala più.

### Come non averli più

1. **`git merge main` dentro il tuo branch ogni giorno.**
2. **Resta nella tua cartella** ([territorio](#42-il-tuo-territorio)).
3. **Se devi toccare il file di un altro, avvisalo prima.**

## 4.9 Se sei bloccato

Ti serve una cosa che un altro non ha ancora finito? **Non aspettare.**

- Chiedigli **l'interfaccia, non il codice**: i nomi dei metodi, la forma dei
  dati. Con quelli puoi già scrivere la tua parte.
- Se ti serve già funzionante, usa un finto (mock) che restituisce dati fissi,
  e vai avanti. Esempio: `@fmartini02` può costruire le pagine con un elenco
  di giochi scritto a mano finché le API di `@K0ff33Blu3` non sono pronte.
- **Dillo sul gruppo.** Restare bloccati in silenzio è la cosa più costosa che
  si possa fare in tre.

## 4.10 Le sei cose da non fare mai

1. **Committare su `main`.** Oggi GitHub non lo impedisce
   ([vedi 4.13](#413-protezione-di-main-da-attivare)): è una regola tra noi.
2. **`git push --force` su un branch condiviso.** Mai. Cancella il lavoro degli
   altri e non si torna indietro.
3. **Committare il `.env`, la chiave RAWG o una password.** Restano nella
   cronologia per sempre, anche se cancelli il file dopo. Prima di ogni
   `git add`: `git diff`.
4. **Toccare la cartella di un altro senza avvisarlo.**
5. **Approvare una PR senza averla guardata.**
6. **Iniziare a lavorare senza una scheda, o senza assegnartela.**

## 4.11 Ho sbagliato — come torno indietro

> Quasi tutto si aggiusta. Niente panico.

**Non so su che branch sono**

```bash
git status      # la prima riga te lo dice
git branch      # l'asterisco sei tu
```

**Non mi fa cambiare branch** — `error: Your local changes would be
overwritten by checkout`. Hai lavoro non committato:

```bash
git commit -m "wip: salvo il lavoro a metà"   # poi continui
git stash                                     # oppure lo metti da parte
git stash pop                                 # per riprenderlo
```

**`git push` viene rifiutato: `Updates were rejected`** — sul server c'è roba
che tu non hai (hai lavorato da due computer, o qualcuno ha pushato sul tuo
branch):

```bash
git pull
git push
```

> ⚠️ **Non usare `--force` per farlo passare:** cancelli il lavoro altrui.

**Ho committato su `main` per sbaglio** (e non ho ancora pushato)

```bash
git switch -c <area>/<cosa>            # il commit viene con te nel nuovo branch
git switch main
git reset --hard origin/main           # main torna com'è sul server
git switch -                           # e continui sul branch
```

**Voglio buttare via delle modifiche**

```bash
git restore <file>            # un file solo
git restore .                 # tutto (ATTENZIONE: non si recupera)
git restore --staged <file>   # solo togliere dallo staging, il file resta
```

**Ho sbagliato il messaggio dell'ultimo commit** — solo se **non** hai ancora
pushato:

```bash
git commit --amend -m "messaggio giusto"
```

**Ho committato una password o la chiave RAWG**

> 🚨 **Non cancellarla con un altro commit: resta nella cronologia.**
> Avvisa **subito** il gruppo, rigenera la chiave su
> [rawg.io/apidocs](https://rawg.io/apidocs) (o cambia la password). Poi si
> decide insieme come ripulire.

> **In dubbio:** fermati e chiedi sul gruppo **prima** di digitare qualcosa con
> `--force` o `--hard` dentro.

## 4.12 Chi chiedere per cosa

| Argomento | Chi |
|---|---|
| API REST, RAWG, chiave API, classi in `shared/`, Postman | **`@K0ff33Blu3`** |
| Pagine Blazor, componenti MudBlazor, grafica | **`@fmartini02`** |
| PostgreSQL, Entity Framework, Docker, `docker-compose.yml`, `.env` | **`@Elmond0`** |
| Impostazioni del repository GitHub (collaboratori, regole) | **`@Elmond0`** (proprietario del repo) |
| Priorità, scadenze, chi fa cosa | **tutti e tre**, al punto della settimana |

Se non sai a chi chiedere, chiedi sul gruppo. Meglio una domanda in più che
mezza giornata persa.

## 4.13 Protezione di `main` (da attivare)

**Oggi `main` non è protetto:** GitHub accetta push diretti e merge senza
approvazione. Le regole di questa guida valgono perché le rispettiamo noi.

Per farle rispettare a GitHub, il proprietario del repo (`@Elmond0`) può
attivare un **ruleset**: *Settings → Rules → Rulesets → New branch ruleset*.

| Impostazione | Valore |
|---|---|
| Enforcement status | **Active** |
| Target branches | **Include default branch** (`main`) |
| Restrict deletions | ✅ |
| Block force pushes | ✅ |
| Require a pull request before merging | ✅ — *Required approvals*: **1** |
| Allowed merge methods | solo **Squash** |

Da quel momento un push diretto su `main` viene rifiutato e il pulsante di
merge resta grigio finché un compagno non approva.

> Più avanti si può aggiungere una **CI** (GitHub Actions) che a ogni PR
> esegue `dotnet build` e `dotnet test` su una macchina pulita, e renderla
> obbligatoria nello stesso ruleset. Oggi non c'è.

---

## Documenti collegati

| File | Cosa contiene |
|---|---|
| [`subject/SUBJECT.md`](subject/SUBJECT.md) | La traccia del progetto |
| [`adr/adr-01-stack.md`](adr/adr-01-stack.md) | Stack tecnologico e motivazioni |
| [`piano-backend.md`](piano-backend.md) | Piano di lavoro di `@K0ff33Blu3` |
| [`piano-database.md`](piano-database.md) | Piano di lavoro di `@Elmond0` |
| [`rawg-api.md`](rawg-api.md) | Elenco delle chiamate dell'API RAWG |
| [Issues](https://github.com/Elmond0/Terranova-game-explorer/issues) | Tutte le schede |
