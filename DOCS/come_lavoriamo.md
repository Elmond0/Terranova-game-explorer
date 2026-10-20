# Come lavoriamo con Git

> Guida per il team — ft_transcendence
> Leggila una volta. Poi tienila aperta le prime due settimane.

**In una frase:** `main` è sacro e non si tocca mai. Tutto quello che fai vive
in un branch tuo, ed entra in `main` solo passando da una Pull Request che un
compagno ha approvato.

Se ti ricordi solo questo, il 90% è fatto.

---

# 🚀 Il riassunto: una scheda dall'inizio alla fine

**Se hai fretta, leggi solo questo.** È un giorno di lavoro vero, dal momento
in cui prendi una scheda a quando la chiudi.

Guarda che il giorno 2 e il giorno 3 **non** hanno `switch -c` né
`--delete-branch`: il branch esiste già e resta aperto finché il lavoro non è
finito.

```bash
# ── GIORNO 1: apro la scheda ────────────────────────────────
git switch main && git pull

gh issue list --label dev2 --label P0        # cosa devo fare
gh issue view 47                             # leggo la scheda
gh issue edit 47 --add-assignee @me          # me la prendo
git switch -c <login>/auth/jwt-sessions        # apro il branch

git diff
git add services/api/src/auth/jwt.ts
git commit -m "feat(auth): aggiunge le sessioni JWT"
git push -u origin <login>/auth/jwt-sessions   # -u solo stavolta
```

```bash
# ── GIORNO 2: continuo ──────────────────────────────────────
git switch <login>/auth/jwt-sessions           # torno sul branch di ieri

git switch main && git pull
git switch -
git merge main

git add services/api/src/auth/middleware.ts
git commit -m "feat(auth): protegge le rotte con il middleware"
git push                                     # senza -u, e' gia' collegato
```

```bash
# ── GIORNO 3: ho finito ─────────────────────────────────────
git switch main && git pull
git switch -
git merge main
git push

gh pr create                                 # nel corpo: Closes #47
gh pr checks --watch                         # aspetto il verde
# ... un compagno approva ...
gh pr merge 52 --squash --delete-branch      # SOLO ADESSO si cancella
git switch main && git pull                  # e passo alla scheda dopo
```

> ### Le due regole che non stanno nei comandi
> 1. **`git diff` prima di ogni `git add`.** Dieci secondi che ti evitano di
>    pushare un `console.log` o una chiave.
> 2. **Pusha anche a metà lavoro.** Quello che non hai pushato non esiste: se
>    il portatile muore, è perso.

> **👤 Negli esempi:** sei **`@kriboss`**, il **Dev2** (autenticazione). Stai
> lavorando sulla scheda **#47** — *"login, sessioni JWT e middleware di
> protezione"* — e la Pull Request che ne esce è la **#52**.
> Sostituisci con i tuoi valori.
>
> **Perché la PR ha un numero più alto della scheda:** su GitHub issue e Pull
> Request pescano dallo **stesso contatore**. Il numero non dice che tipo di
> cosa sia, dice solo in che ordine è nata. La PR arriva giorni dopo la scheda
> #47, quindi ha per forza un numero più grande. Non inventarlo: `gh pr create`
> te lo stampa quando la apre, e `gh pr status` te lo ricorda dopo.
>
> `<login>` è il **login 42**, il primo pezzo del nome di ogni branch. Quello di
> `@kriboss` non lo sappiamo ancora, quindi negli esempi resta così: tu al suo
> posto scrivi il tuo. Chi ce l'ha già: Dev1 `elmondo`, Dev3 `miricci`.

---

## Indice

### 🚀 [Il riassunto: una scheda dall'inizio alla fine](#-il-riassunto-una-scheda-dallinizio-alla-fine)
*Se hai fretta, leggi solo questo.*

### 1. [La prima volta: installare gh e collegarsi](#1-la-prima-volta-installare-gh-e-collegarsi)
*Una volta sola per computer, 15 minuti.*

- [Passo 0 — accetta l'invito](#passo-0--accetta-linvito)
- [Passo 1 — di' a git chi sei](#passo-1--di-a-git-chi-sei)
- [Passo 2 — installa gh](#passo-2--installa-gh)
- [Passo 3 — collegati](#passo-3--collegati)
- [Passo 4 — clona il repo](#passo-4--clona-il-repo)
- [Passo 5 — controlla che funzioni tutto](#passo-5--controlla-che-funzioni-tutto)
- [Se non puoi installare gh (macchine della scuola)](#se-non-puoi-installare-gh-macchine-della-scuola)
  · [La chiave SSH a mano](#la-chiave-ssh-a-mano)

### 2. [Con gh — tutto dal terminale](#2-con-gh--tutto-dal-terminale)
*Quello che userai ogni giorno.*

- [2.1 Le schede](#21-le-schede) — trovarle, prendersele, aprirne di nuove
- [2.2 Il codice](#22-il-codice) — branch, commit, push, `merge main`
- [2.3 La Pull Request](#23-la-pull-request)
  · [Aprirla — tre modi](#aprirla--tre-modi-dal-più-facile)
  · [Aspettare la CI](#aspettare-la-ci)
  · [Rivedere la PR di un compagno](#rivedere-la-pr-di-un-compagno)
  · [Mergiare](#mergiare)
- [2.4 Cancellare un branch a mano](#24-cancellare-un-branch-a-mano)
- [2.5 I comandi che userai davvero](#25-i-comandi-che-userai-davvero)
- [2.6 Errori tipici di gh](#26-errori-tipici-di-gh)

### 3. [Dal browser — senza gh](#3-dal-browser--senza-gh)
*Sulle macchine della scuola, dove non puoi installare niente.*

- [3.1 Le schede dal browser](#31-le-schede-dal-browser)
- [3.2 Aprire una PR dal browser](#32-aprire-una-pr-dal-browser)
- [3.3 Guardare la CI dal browser](#33-guardare-la-ci-dal-browser)
- [3.4 Rivedere la PR di un altro dal browser](#34-rivedere-la-pr-di-un-altro-dal-browser)
- [3.5 Mergiare dal browser e pulire in locale](#35-mergiare-dal-browser-e-pulire-in-locale)
- [3.6 Tabella delle corrispondenze](#36-tabella-delle-corrispondenze) — ogni comando `gh` e il suo pulsante

### 4. [Il perché di ogni cosa](#4-il-perché-di-ogni-cosa)
*Da leggere quando qualcosa non torna.*

- [4.1 git, GitHub, gh e la CI sono quattro cose diverse](#41-git-github-gh-e-la-ci-sono-quattro-cose-diverse)
  · [Come funziona la CI, passo passo](#come-funziona-la-ci-passo-passo)
  · [La CI e il blocco del merge sono due cose separate](#la-ci-e-il-blocco-del-merge-sono-due-cose-separate)
- [4.2 Il tuo territorio](#42-il-tuo-territorio) — chi scrive dove, e cosa fa davvero CODEOWNERS
- [4.3 Le schede: perché si parte sempre da una](#43-le-schede-perché-si-parte-sempre-da-una)
  · [`Closes #47` — la riga che fa tutto](#closes-47--la-riga-che-fa-tutto)
- [4.4 Come si chiama un branch](#44-come-si-chiama-un-branch)
- [4.5 Come si scrive un commit e una PR](#45-come-si-scrive-un-commit-e-una-pr)
- [4.6 Come si rivede la PR di un altro](#46-come-si-rivede-la-pr-di-un-altro)
- [4.7 `main` si è mosso mentre lavoravi](#47-main-si-è-mosso-mentre-lavoravi)
- [4.8 I conflitti — guida passo passo](#48-i-conflitti--guida-passo-passo)
  · [Se ti sei confuso](#se-ti-sei-confuso)
  · [Come non averli più](#come-non-averli-più)
- [4.9 Se sei bloccato](#49-se-sei-bloccato)
- [4.10 Le sei cose da non fare mai](#410-le-sei-cose-da-non-fare-mai)
- [4.11 Ho sbagliato — come torno indietro](#411-ho-sbagliato--come-torno-indietro)
- [4.12 Chi chiedere per cosa](#412-chi-chiedere-per-cosa)

### [Documenti collegati](#documenti-collegati)

---

# 1. La prima volta: installare gh e collegarsi

> ⏱️ 15 minuti, si fa una volta sola **per computer**.

**Fallo in quest'ordine.** Se salti un passo, quello dopo non funziona. In
particolare: **non clonare prima di esserti autenticato**, altrimenti ti
risponde `Permission denied (publickey)`.

## Passo 0 — accetta l'invito

Ti arriva una mail da GitHub, oppure lo trovi su
[github.com/notifications](https://github.com/notifications). Finché non lo
accetti **non risulti nel repo**: non puoi pushare e non ricevi le richieste
di review.

## Passo 1 — di' a git chi sei

```bash
git config --global user.name "Nome Cognome"
git config --global user.email "la-tua-email-di-github"
git config --global fetch.prune true
git config --global pull.ff only
```

> ⚠️ **L'email deve essere quella del tuo account GitHub.** Se ne metti
> un'altra, i tuoi commit non risultano tuoi sul sito: a fine progetto, nella
> sezione *Individual Contributions*, il tuo lavoro non compare. Si sistema,
> ma è una rottura.

`fetch.prune` cancella dal tuo computer i branch che sul server non esistono
più. Senza, fra un mese hai 40 branch fantasma e non capisci più quali sono
vivi.

`pull.ff only` fa sì che `git pull` accetti solo aggiornamenti lineari. Se la
tua linea e quella del server si sono separate, si ferma e te lo dice invece di
inventarsi un merge a sorpresa.

## Passo 2 — installa gh

> ⚠️ **Non usare `sudo apt install gh` su Ubuntu.** Il pacchetto di Ubuntu è
> fermo alla versione 2.45, di oltre due anni fa, e `gh issue view` **non
> funziona**: esce solo `GraphQL: Projects (classic) is being deprecated` e
> nessun output. La versione vecchia chiede a GitHub i *Projects classic*, che
> ora GitHub rifiuta con un errore — quindi il comando muore lì.

**Su Ubuntu/Debian, aggiungi il repository ufficiale di GitHub.** È l'unico
modo di avere l'ultima versione e di riceverne gli aggiornamenti:

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

**Su macOS** basta `brew install gh`.

**Controlla di avere una versione recente** — deve essere 2.60 o superiore:

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
git clone git@github.com:<owner>/ft_transcendence.git
cd ft_transcendence
```

## Passo 5 — controlla che funzioni tutto

```bash
git log --oneline -3        # deve mostrarti dei commit
gh auth status              # deve dire Logged in
gh pr list                  # deve rispondere senza errori
git config --get user.email # deve essere l'email di GitHub
```

## Se non puoi installare gh (macchine della scuola)

Niente `sudo`? Non sei bloccato: **i comandi `git` non cambiano di una
virgola.** `gh` serve solo per Pull Request e schede, e ogni cosa che fa ha il
suo pulsante su github.com ([sezione 3](#3-dal-browser--senza-gh)).

**Prima di arrenderti, prova lo stesso.** `gh` è un singolo eseguibile: puoi
tenerlo in casa tua senza installare niente a livello di sistema.

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

La prima riga chiede a GitHub qual è l'ultima versione: serve perché il nome
del file cambia a ogni rilascio.

> ⚠️ **Non** funziona scrivere `gh_*_linux_amd64.tar.gz` nell'URL: `curl` non
> espande gli asterischi e ti torna un errore 404.

### La chiave SSH a mano

Se proprio non riesci a installare `gh`, la chiave SSH che creava lui la fai
tu, una volta sola **per macchina**.

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
> su quelle della scuola, ne servono due, una per computer. Si possono avere
> quante se ne vuole.

---

# 2. Con gh — tutto dal terminale

> Qui c'è **cosa digitare**. Il perché di ogni scelta sta nella
> [sezione 4](#4-il-perché-di-ogni-cosa).

## 2.1 Le schede

**Quali sono le mie:**

```bash
gh issue list --label dev2 --label P0   # le mie, urgenti
gh issue list --label dev2 --label P1   # le mie, dopo
gh issue list --label blocca            # chi sta fermando gli altri
gh issue list --search "argon2"         # cercarne una
gh issue status                         # cosa mi riguarda adesso
```

**Leggerla** — stima, dipendenze, "fatto quando":

```bash
gh issue view 47
```

**Prendersela**, così nessun altro la inizia:

```bash
gh issue edit 47 --add-assignee @me
```

E sulla board trascina la scheda da **Ready** a **In progress**.

**Dire che sei bloccato** — mai lasciarla in *In progress* in silenzio:

```bash
gh issue comment 47 --body "Fermo: mi serve il contratto di #16 per sapere la forma dello stato. @K0ff33Blu3"
```

Poi riportala in **Ready** e prendine un'altra.

**Aprirne una nuova** — serve ogni volta che trovi una cosa da fare che non è
sulla board:

```bash
gh issue create          # ti fa le domande a video
```

Oppure in un comando solo:

```bash
gh issue create --title "fix(web): la mano non si aggiorna dopo la pesca" \
  --body "Quando il mazzo pesca, la mano resta quella di prima finche' non
ricarico la pagina.

Fatto quando: la carta pescata compare senza ricaricare." \
  --label dev4 --label P1 --label area:web
```

**Caso frequente: ti serve una cosa dal territorio di un altro.** Non
modificare i suoi file — aprigli una scheda.

```bash
gh issue create --title "chore(infra): aggiungere bot-service al compose" \
  --body "Il Dockerfile e' pronto in services/bot/.
Serve il blocco nel docker-compose.yml.

Fatto quando: make up alza anche il bot." \
  --label dev5 --label area:infra --label P2
```

## 2.2 Il codice

**Ogni mattina, prima di qualunque cosa:**

```bash
git switch main && git pull
```

**Apro il branch della scheda** — il nome è `<login>/<area>/<cosa>`
([come si sceglie](#44-come-si-chiama-un-branch)):

```bash
git switch -c <login>/auth/jwt-sessions
```

**Mentre lavoro** — commit piccoli e frequenti, non uno alla fine:

```bash
git status                  # dove sono, cosa ho toccato
git diff                    # cosa ho cambiato: SEMPRE prima di add
git add services/api/src/auth/jwt.ts
git commit -m "feat(auth): aggiunge le sessioni JWT"
```

**Mando su:**

```bash
git push -u origin <login>/auth/jwt-sessions   # la PRIMA volta di questo branch
git push                                       # tutte le altre
```

**Il giorno dopo riprendo il branch di ieri.** Non ne creo uno nuovo:

```bash
git switch <login>/auth/jwt-sessions
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
| Body | si apre l'editor col template: riempi le quattro sezioni, salva ed esci (nano: `Ctrl+O`, `Invio`, `Ctrl+X`) |
| What's next? | scegli **Submit** |

Alla fine ti stampa il link e **il numero** della PR.

**Modo 2 — precompilato dal commit:**

```bash
gh pr create --fill
```

> ⚠️ Così la PR **non** avrà le quattro sezioni. Usalo solo quando il commit si
> spiega da solo.

**Modo 3 — tutto in un comando:**

```bash
gh pr create --base main --head <tuo-branch> \
  --title "feat(auth): login con Google" \
  --body "Closes #47

## Cosa contiene
- rotta /auth/google e callback
- collegamento automatico se l'email coincide

## Perche' cosi'
Evita di creare account doppi per la stessa persona.

## Sblocca
Dev4, che deve mettere il pulsante nella pagina di login.

## Come verificare
make up e prova ad accedere con Google"
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

### Aspettare la CI

```bash
gh pr checks --watch
```

Resta lì e si aggiorna da solo finché la CI non finisce.

| | |
|---|---|
| 🟢 **verde** | puoi mergiare |
| 🔴 **rosso** | il merge è **bloccato**, per tutti, anche per chi ha creato il repo |

**Se è rossa:** apri il log, sistema sul **tuo** branch, `git push`. La CI
riparte da sola: non serve chiudere e riaprire la PR, non serve avvisare
nessuno.

```bash
gh run list --limit 3
gh run view --log-failed
```

**Se dice `no checks reported on the '<branch>' branch`:** la CI non è ancora
partita, ci mette qualche secondo a registrarsi. Aspetta e rilancia.

Poi avvisa sul gruppo: «PR #52 pronta, la rivede qualcuno?»

### Rivedere la PR di un compagno

Succede ogni giorno. È lavoro, non un favore — [perché](#46-come-si-rivede-la-pr-di-un-altro).

```bash
gh pr list                   # quali sono aperte
gh pr view 31                # titolo, testo, stato
gh pr diff 31                # cosa ha cambiato

gh pr checkout 31            # provarla sul tuo computer
git switch -                 # tornare al tuo lavoro

gh pr review 31 --approve
gh pr review 31 --comment -b "Una domanda su X..."
gh pr review 31 --request-changes -b "Qui si rompe se il token e' scaduto"
```

`-b` è il testo del commento; se lo ometti, `gh` apre l'editor. Per commentare
**una riga precisa** conviene il browser: `gh pr view --web`, poi passa il
mouse sul numero di riga.

### Mergiare

```bash
gh pr merge 52 --squash --delete-branch
git switch main && git pull
```

Fa tre cose insieme: mergia schiacciando tutto in un commit, cancella il branch
sul server **e** in locale, e ti riporta su `main`.

In una riga sola, aspettando prima il verde — `&&` vuol dire *"esegui il
secondo solo se il primo è andato bene"*:

```bash
gh pr checks --watch && gh pr merge 52 --squash --delete-branch
```

> ### ⚠️ Il branch si cancella una volta sola, alla fine
> `--delete-branch` si usa **solo nel momento in cui la PR viene mergiata**,
> cioè quando il lavoro su quella scheda è finito davvero.
>
> Finché ci stai ancora lavorando — anche se pushi dieci volte, anche a
> distanza di giorni — **il branch resta lì.** Non si cancella la sera e non
> si ricrea la mattina: la mattina ci torni sopra con `git switch`.

**Se si rifiuta:**

| Messaggio | Vuol dire |
|---|---|
| `Required status check ... is expected` | la CI non è verde, o non è ancora finita |
| `Pull request is not mergeable` | serve un'approvazione, oppure ci sono [conflitti](#48-i-conflitti--guida-passo-passo) |

**Non è `gh` che si rifiuta: è il server.** Non c'è modo di forzare.

**Chiudere una PR senza mergiarla:**

```bash
gh pr close 52 --delete-branch
```

## 2.4 Cancellare un branch a mano

> Con `gh pr merge --delete-branch` non serve: lo fa lui. Questi comandi
> servono quando **`gh` non l'ha fatto** — per esempio hai mergiato dal
> browser — oppure quando **abbandoni un branch** senza mergiarlo.

```bash
git branch -a                                    # che branch ho
git switch main                                  # esco da quello da cancellare
git branch -D <login>/auth/jwt-sessions          # lo cancello in locale
git push origin --delete <login>/auth/jwt-sessions   # e sul server
git fetch --prune                                # ripulisco i fantasmi
```

I branch che iniziano con `remotes/origin/` stanno sul server, gli altri solo
sul tuo computer: sono due cose separate e si cancellano separatamente.

> ### Perché `-D` maiuscola e non `-d` minuscola
> Con `-d` git ti risponde, alla lettera:
>
> ```
> error: the branch `'<nome>'` is not fully merged.
> If you are sure you want to delete it, run 'git branch -D ...'
> ```
>
> Sembra un errore grave. Non lo è. Noi usiamo lo **squash**: su `main` è
> finito **un commit nuovo**, con un codice diverso dai tuoi. Per git i tuoi
> commit originali non risultano dentro `main`, quindi `-d` (che è prudente)
> si rifiuta. `-D` cancella e basta.
>
> **Non perdi niente:** il tuo lavoro è dentro `main`, schiacciato in quel
> commit, ed è anche nella PR su GitHub per sempre.
>
> ⚠️ L'unico caso in cui `-D` fa perdere del lavoro è un branch **mai pushato
> e mai mergiato**. Se non sei sicuro, fai prima `git push`.

## 2.5 I comandi che userai davvero

```bash
# le schede
gh issue list --label devN --label P0   # quali sono le mie urgenti
gh issue view 47                        # leggerne una
gh issue edit 47 --add-assignee @me     # prendermela
gh issue comment 47 --body "..."        # dire che sono bloccato
gh issue create                         # aprirne una nuova

# il codice
git switch main && git pull             # ogni mattina
git switch -c <login>/<area>/<cosa>     # nuovo task
git status                              # dove sono, cosa ho toccato
git diff                                # cosa ho cambiato
git add <file>                          # scelgo cosa committare
git commit -m "tipo(area): ..."         # sigillo
git push                                # mando su
git switch -                            # torno al branch di prima
git merge main                          # mi porto dentro gli altri

# la pull request
gh pr create                            # apro la PR (con "Closes #47")
gh pr checks --watch                    # aspetto la CI
gh pr status                            # cosa mi riguarda
gh pr diff <n>                          # cosa ha cambiato un altro
gh pr checkout <n>                      # provo la sua PR
gh pr review <n> --approve              # approvo
gh pr merge <n> --squash --delete-branch  # mergio
```

> Sono venticinque. Non ne servono altri per il 95% del tempo.

## 2.6 Errori tipici di gh

*(messaggi verificati sul campo, sono questi alla lettera)*

| Messaggio | Cosa fare |
|---|---|
| `GraphQL: Projects (classic) is being deprecated` e nessun output | Il tuo `gh` è vecchio. Aggiornalo: [passo 2](#passo-2--installa-gh). |
| `failed to run git: fatal: not a git repository` | Non sei dentro la cartella del progetto. Fai `cd`. |
| `must be on a branch named differently than "main"` | Stai aprendo una PR da `main`. Crea un branch. |
| `pull request create failed: GraphQL: No commits between main and <branch>` | Non hai committato niente, o hai dimenticato `git push`. |
| `a pull request for branch "<branch>" into branch "main" already exists:` | Non è un errore: ce l'hai già e `gh` ti ridà il link. Ogni push la aggiorna da solo. |
| `failed to create review: GraphQL: Review Can not approve your own pull request` | **Non puoi approvare la tua stessa PR.** Lo vieta GitHub: ogni PR ha bisogno di un **altro** che la approvi. |
| Si è aperto un editor strano e non riesco a uscire | È vim: `Esc`, poi `:wq` e Invio. Per non ritrovartelo più: `gh config set editor nano` |

---

# 3. Dal browser — senza gh

> **I comandi `git` restano identici.** Cambia solo quello che faceva `gh`:
> schede e Pull Request. Puoi anche lavorare metà e metà — git dal terminale,
> le PR dal browser. È quello che fanno in tanti.

## 3.1 Le schede dal browser

Le schede stanno in due posti, e sono la stessa cosa vista in due modi: il tab
**Issues** del repo (l'elenco) e la **board** del progetto (gli stessi dati
come cartoncini).

**Trovare le mie** — casella di ricerca in cima al tab Issues:

```
label:dev2 label:P0        le mie, urgenti
label:dev2                 tutte le mie
assignee:@me               quelle che mi sono già preso
label:blocca               chi sta fermando gli altri
is:open no:assignee        schede libere, senza padrone
```

Sulla board è la stessa cosa: casella **Filter by keyword or by field** in
alto, stessi filtri, stessa sintassi.

**Prendersela:**

1. Aprila (clic sul titolo)
2. Colonna di destra → **Assignees** → *assign yourself*
3. Sempre a destra → **Projects** → campo **Status** → *In progress*
   (oppure trascina il cartoncino sulla board: è lo stesso gesto)

**Scriverci sopra:** riquadro dei commenti in fondo alla scheda. Usalo quando
sei bloccato, con `@nome` per far arrivare la notifica. Poi rimetti lo
**Status** su *Ready* e prendi un'altra scheda.

**Aprirne una nuova:**

1. Tab **Issues** → pulsante verde **New issue**
2. **Titolo:** `tipo(area): cosa`, scritto come un commit
3. **Corpo:** cosa succede, e una riga *"Fatto quando: …"*
4. Colonna di destra → **Labels**: chi la prende (`devN`), l'area
   (`area:web`) e la priorità (`P1`)
5. **Create**

La scheda compare sulla board da sola: il workflow *Auto-add to project* è
acceso.

**Chiuderla: non si chiude a mano.** Nel corpo della PR scrivi `Closes #47` e
al merge GitHub la chiude e la sposta in *Done*. Il pulsante **Close issue** si
usa solo quando la scheda non serve più e non ci sarà nessuna PR (duplicato,
cosa decisa diversamente): in quel caso scrivi prima un commento che dice
perché.

## 3.2 Aprire una PR dal browser

**La scorciatoia:** quando fai `git push` di un branch nuovo, git stampa già il
link giusto.

```
remote: Create a pull request for 'miricci/game/mock' on GitHub by visiting:
remote:   https://github.com/<owner>/<repo>/pull/new/miricci/game/mock
```

**Se hai perso il link:** vai su `github.com/<owner>/<repo>`, in alto compare
una striscia gialla col tuo branch e il pulsante **Compare & pull request**.
Se non c'è: tab **Pull requests** → **New pull request** → base `main`,
compare il tuo branch.

Poi: titolo come il commit, riempi le quattro sezioni che trovi già nel
riquadro del testo, pulsante verde **Create pull request**.

## 3.3 Guardare la CI dal browser

In fondo alla pagina della PR c'è un riquadro:

| | |
|---|---|
| 🟡 *Some checks haven't completed yet* | sta ancora girando, aspetta |
| 🟢 *All checks have passed* | puoi mergiare |
| 🔴 *Some checks were not successful* | il merge è **bloccato** |

**Se è rosso:** clicca **Details** accanto al controllo fallito. Si apre il
log, e lo step rosso è quello che non è passato: cliccalo per leggere
l'errore. Poi sistemi sul tuo branch e pushi — la CI riparte da sola.

## 3.4 Rivedere la PR di un altro dal browser

1. Tab **Pull requests** → apri la sua PR
2. Tab **Files changed** → vedi tutte le sue modifiche

**Per provarla sul tuo computer** (sostituisce `gh pr checkout`):

```bash
git fetch origin
git switch <nome-del-suo-branch>
# ... provala, segui la sua sezione "Come verificare" ...
git switch -                # torna al tuo lavoro
```

`git fetch` scarica **tutti** i branch del server, quindi il suo c'è già anche
se non l'hai mai visto.

**Per rispondere:** tab **Files changed** → **Review changes** in alto a destra
→ **Comment** (una domanda) / **Approve** (via libera) / **Request changes**
(c'è da sistemare) → **Submit review**.

> Per commentare **una riga precisa**: passa il mouse sul numero della riga,
> compare un `+` azzurro, cliccalo e scrivi. È il modo migliore di fare una
> review: il commento resta attaccato alla riga.

## 3.5 Mergiare dal browser e pulire in locale

1. In fondo alla PR, pulsante verde **Squash and merge**
   *(se è grigio: la CI non è verde o manca un'approvazione — il riquadro
   sopra ti dice cosa manca)*
2. **Confirm squash and merge**
3. Compare **Delete branch**: **cliccalo.** Cancella il branch dal server.

Il branch sul **tuo computer** resta: quello lo cancelli tu.

```bash
git switch main
git pull
git branch -D <nome-del-tuo-branch>
git branch -a                  # devi vedere solo main e origin/main
```

> `-D` maiuscola, non `-d`: il perché è
> [nella sezione 2.4](#24-cancellare-un-branch-a-mano).

## 3.6 Tabella delle corrispondenze

| Terminale | Browser |
|---|---|
| `gh pr create` | striscia gialla **Compare & pull request**, o il link che stampa `git push` |
| `gh pr checks --watch` | riquadro in fondo alla PR / tab **Checks** |
| `gh pr view` | la pagina della PR |
| `gh pr diff` | tab **Files changed** |
| `gh pr checkout <n>` | `git fetch origin` + `git switch <nome-del-branch>` |
| `gh pr review --approve` | **Review changes** → Approve → Submit |
| `gh pr merge --squash --delete-branch` | **Squash and merge** + **Delete branch** + `git branch -D` in locale |
| `gh pr list` | tab **Pull requests** del repo |
| `gh pr close <n>` | in fondo alla PR: **Close pull request** |
| `gh issue list --label ...` | tab **Issues** + filtro `label:dev2 label:P0` |
| `gh issue view <n>` | clic sul titolo della scheda |
| `gh issue edit <n> --add-assignee @me` | colonna destra → Assignees → *assign yourself* |
| `gh issue create` | tab **Issues** → **New issue** |
| `gh issue comment <n>` | riquadro dei commenti in fondo alla scheda |
| *(spostare di colonna)* | trascina il cartoncino, o Projects → Status |

---

# 4. Il perché di ogni cosa

## 4.1 git, GitHub, gh e la CI sono quattro cose diverse

Hanno nomi simili e all'inizio si confondono. Cinque minuti qui ti risparmiano
due settimane di *"non ho capito cosa sto facendo"*.

| | Cos'è |
|---|---|
| **git** | Un programma **sul tuo computer**. Gestisce la storia dei file in locale. Non sa cosa sia GitHub. Funziona anche senza internet. |
| **GitHub** | Un **sito** (un server). Tiene una copia del repo e aggiunge cose che git non ha: Pull Request, review, CI, issue. |
| **gh** | Un programma sul tuo computer che **parla con GitHub** dal terminale, così non devi aprire il browser. |
| **CI** | Un **computer di GitHub** che fa dei controlli al posto tuo, da solo, ogni volta che apri o aggiorni una PR. |

> **git non sa cosa sia una Pull Request.** Non esiste nel suo vocabolario:
> `git pull-request` non è un comando. La Pull Request è un'invenzione di
> GitHub e vive sul loro server, non nel tuo repo.

**Cos'è davvero una Pull Request.** È una richiesta scritta su GitHub che dice:

> *"Ho il branch `miricci/game/mock`. Confrontalo con `main` e mostra le
> differenze. Voglio che entri in `main`. Chi vuole può commentare, provarlo e
> approvarlo."*

Una pagina web con tre cose sopra: il confronto tra due branch, una
discussione, e un pulsante Merge. Nient'altro.

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

 gh pr create  -------------------->  "apri una richiesta di merge"
                                            |
                                      crea la pagina della PR
                                            |
                                      fa partire la CI DA SOLO

 gh pr checks  <-------------------   "com'è andata la CI?"
                                      verde / rosso

 gh pr merge  --------------------->  "mergia"
                                            |
                                      GitHub controlla le regole:
                                      CI verde? approvazioni?
                                        sì  -> mergia
                                        no  -> RIFIUTA

 git pull  <-----------------------   scarica il main aggiornato
```

**`gh` non decide niente.** Manda richieste e riporta risposte. Quando
`gh pr merge` ti dice di no, non è `gh` che si rifiuta: è il server.

### Come funziona la CI, passo passo

Quando apri una PR, o pushi altro codice su una PR già aperta:

1. GitHub guarda se esiste `.github/workflows/ci.yml`
2. Accende una macchina virtuale **vuota** (Ubuntu pulito)
3. Ci clona dentro il repo
4. Esegue gli step del file, uno per uno
5. Se uno fallisce → **rosso** e si ferma lì. Se passano tutti → **verde**
6. Spegne la macchina e ti manda il risultato

**Perché una macchina pulita e non il tuo computer.** Sul tuo portatile il
progetto funziona anche perché hai installato delle cose mesi fa e te ne sei
dimenticato. Sulla macchina della CI non c'è niente: se funziona lì, funziona
anche per il valutatore che clona il repo la mattina dell'esame.

**Tu dentro la CI non devi scrivere niente.** Il file `ci.yml` lo scrive una
volta il tech lead; parte da sola e tu aspetti il verde. Due sole eccezioni: se
diventa rossa la sistemi tu (la CI non ripara niente, ti dice cosa non va), e
`ci.yml` crescerà nel tempo con gli step di install, lint, build e test — sarà
una PR come le altre.

**Cosa controlla oggi:**

- che nessun file `.env` sia finito nel repository
- che il `Makefile` sia indentato con i TAB (senza, `make` non parte)
- che i file obbligatori del progetto ci siano tutti

Sono gli errori che fanno **rigettare il progetto** in valutazione e che
nessuno nota rileggendo il proprio codice.

### La CI e il blocco del merge sono due cose separate

- **La CI** dice solo verde o rosso. È un **semaforo**.
- **Il ruleset** è la **sbarra**: gli abbiamo detto *"non mergiare se il
  controllo non è verde"*.

Senza il ruleset avresti la CI rossa e il merge che passa lo stesso. È la
differenza tra un avviso e una regola.

## 4.2 Il tuo territorio

Ognuno ha delle cartelle dove è **l'unico a scrivere**. È così che cinque
persone lavorano insieme senza pestarsi i piedi: i conflitti Git nascono quando
due persone toccano **le stesse righe dello stesso file**, non quando lavorano
nello stesso momento.

| Dev | Cartelle |
|---|---|
| **Dev1** | `services/bot/` · `frontend/src/locales/` · `frontend/src/pages/legal/` · `docs/` |
| **Dev2** | `services/api/src/auth/` |
| **Dev3** | `packages/contract/` · `services/engine/` |
| **Dev4** | `frontend/` (tutto il resto) |
| **Dev5** | `services/api/` · `infra/` · `docker-compose.yml` · `Makefile` |

Sta scritto anche in `.github/CODEOWNERS`: quando apri una PR, GitHub chiama in
review il proprietario delle cartelle che hai toccato.

> **CODEOWNERS non decide chi può scrivere.** Tutti possono committare
> ovunque: decide solo **chi deve approvare**. Il territorio è un accordo tra
> voi, con un controllo all'uscita.

> **Regola d'oro.** Se stai per modificare un file fuori dal tuo territorio,
> **fermati e avvisa il proprietario prima di iniziare.** Non è burocrazia: è
> lui che dovrà approvare la tua PR, e se non se lo aspetta resterai fermo due
> giorni.

> ⚠️ **Eccezione assoluta:** `docker-compose.yml` lo tocca **solo Dev5**. Se ti
> serve un servizio nuovo lì dentro, apri una issue e chiediglielo. Due
> persone su quel file = conflitto a ogni merge.

## 4.3 Le schede: perché si parte sempre da una

Tutto il lavoro del progetto è già scritto: ogni cosa da fare è una **issue**
sul repo, e ogni issue compare come **scheda** sulla board. Non si inventa il
lavoro la mattina: **si prende una scheda.**

> ⚠️ **Non si inizia mai a scrivere codice senza una scheda.** Se stai per fare
> una cosa che non è sulla board, apri prima la scheda. Serve a due cose: gli
> altri sanno cosa stai toccando, e a fine progetto risulta chi ha fatto cosa
> — che è un requisito esplicito della valutazione.

**Come si legge una scheda:**

```
#47  feat(auth): registrazione email/password con hash argon2
     label: dev2  major  area:auth  P1
```

| Label | Vuol dire |
|---|---|
| `dev2` | è tua |
| `major` | vale 2 pt, è un modulo importante |
| `area:auth` | tocca `services/api/src/auth/` — il tuo territorio |
| `P1` | importante, ma prima vengono le `P0` |
| `blocca` | se c'è: qualcun altro è fermo finché non chiudi |

Dentro la scheda trovi stima, dipendenze, il branch da usare e soprattutto il
**"fatto quando"**: i criteri per dire che è finita.

> **Se non soddisfi il "fatto quando", la scheda non è finita**, anche se il
> codice funziona.

**Quale prendo?** Prima le P0, poi le P1, poi le P2. Dentro lo stesso livello,
prima quelle con `blocca`: stai fermando qualcuno.

**Una alla volta.** Due schede aperte insieme vuol dire due branch aperti
insieme, e su uno dei due non stai davvero lavorando.

### `Closes #47` — la riga che fa tutto

Nel corpo della Pull Request, su una riga sua. Non è un commento: è un
**comando che GitHub capisce**. Quando la PR viene mergiata:

- l'issue `#47` si chiude da sola
- la scheda si sposta da sola in **Done**
- la PR e la issue restano collegate per sempre

> **È il motivo per cui la board non muore.** Nei team che aggiornano la board
> a mano, dopo tre settimane nessuno la aggiorna più e diventa inutile. Con
> `Closes #N` non c'è niente da ricordarsi.

Funzionano anche `Fixes #47` e `Resolves #47`: sono la stessa cosa.

- **Più schede in una PR:** `Closes #47, closes #48` — la parola va ripetuta
  davanti a ogni numero, altrimenti chiude solo la prima.
- **Se la PR riguarda la scheda ma non la chiude:** scrivi `Rif. #47`, collega
  senza chiudere.

## 4.4 Come si chiama un branch

```
<tuologin>/<area>/<cosa>
```

**Le aree sono le cartelle**, non etichette a caso:

| Area | Cartella |
|---|---|
| `setup` | configurazione del repo, CI |
| `contract` | `packages/contract/` |
| `game` | `services/engine/` |
| `api` | `services/api/` |
| `auth` | `services/api/src/auth/` |
| `bot` | `services/bot/` |
| `web` | `frontend/` |
| `infra` | `infra/`, docker, nginx, Makefile |
| `docs` | `docs/`, README |
| `fix` | stai correggendo un bug |

> Se non sai quale area scrivere, non hai ancora capito quale cartella stai per
> toccare. Fermati e controlla che sia la tua.

**Il "cosa": 2-4 parole in inglese, minuscolo, con i trattini.**

| ✅ | ❌ |
|---|---|
| `auth/avatar-upload` | `auth/modifiche` |
| `web/card-components` | `auth/fix2` |
| `fix/race-condition` | `prova` |

> ⚠️ **Mai** un branch col tuo nome e basta (`miricci`, `miricci2`,
> `miricci-save`). Quelli restano aperti per mesi ed è così che il progetto
> esplode l'ultima settimana.

> Nel nome del branch **non va il numero della scheda**. Il collegamento si fa
> nella PR, con `Closes #47`.

## 4.5 Come si scrive un commit e una PR

**Commit:**

```
tipo(area): cosa hai fatto, all'imperativo, minuscolo
```

Tipi: `feat` · `fix` · `chore` · `docs` · `refactor` · `test`
Esempio: `feat(auth): aggiunge il login con Google`

Se c'è un perché non ovvio, aggiungilo con un secondo `-m`:

```bash
git commit -m "fix(game): serializza gli eventi della partita" \
           -m "Due mosse simultanee potevano corrompere lo stato delle pile.
Ora gli eventi passano da una coda per partita."
```

> **Il titolo dice cosa, il corpo dice perché.** Il cosa si vede già dal
> codice; il perché fra due mesi non se lo ricorda nessuno.

**Pull Request: una riga di collegamento + quattro sezioni.**

```markdown
Closes #47

## Cosa contiene
elenco puntato di cosa hai fatto

## Perche' cosi'
le scelte che hai preso e il motivo

## Sblocca
chi stava aspettando questo lavoro (se nessuno, togli la sezione)

## Come verificare
i comandi ESATTI che il compagno deve digitare per provarla
```

**"Come verificare" è la sezione più importante delle quattro.** Stai dicendo
al tuo compagno come controllare il tuo lavoro in trenta secondi. Le PR facili
da rivedere vengono approvate in giornata; le altre restano ferme.

> ⚠️ **Tieni le PR piccole.** Una PR da 40 file non viene rivista davvero:
> viene timbrata. E allora tanto vale non farle.

## 4.6 Come si rivede la PR di un altro

Metà del tuo lavoro su Git è questo. Non è un favore: è il motivo per cui il
progetto non si rompe. **Quando ti arriva una richiesta di review, entro la
giornata.**

**Cosa guardare, in quest'ordine:**

1. Fa quello che dice di fare?
2. Tocca solo le cartelle che doveva toccare?
3. C'è finita dentro roba che non c'entra? (file di prova, `console.log`, un
   `.env`, una chiave)
4. Il nome delle cose si capisce senza chiedere?

E soprattutto: **provala davvero**, seguendo la sua sezione "Come verificare".

> **Non riscrivere il codice del compagno nella review.** Se non ti piace come
> l'ha fatto ma funziona, approva e parlagliene a voce. Bloccare una PR per
> gusto personale fa fermare tutto il team.

> **Approva solo se l'hai davvero guardata.** Un'approvazione automatica vale
> zero e ti rende corresponsabile di quello che si rompe.

## 4.7 `main` si è mosso mentre lavoravi

Succede sempre: apri un branch, ci lavori, e nel frattempo qualcuno ha mergiato
altre tre PR. Normale.

```bash
git switch main && git pull
git switch -                  # torna al tuo branch (- = "quello di prima")
git merge main
```

**La direzione è una sola:** stai **tirando dentro** il lavoro degli altri. Il
tuo codice resta invisibile a tutti finché non apri la PR. Puoi fare
`git merge main` dieci volte al giorno: nessuno vede niente di tuo.

Non serve a *condividere*, serve a **non restare indietro**: se Dev3 rinomina
un campo del contratto e te ne accorgi il giorno dopo, sistemi tre righe in
cinque minuti; se te ne accorgi dopo due settimane, ne sistemi duecento sotto
scadenza.

> **I merge dentro il tuo branch non sporcano `main`.** Al momento della PR,
> `--squash` li appiattisce tutti in un commit solo. Per questo puoi mergiare
> ogni giorno senza pensarci.

## 4.8 I conflitti — guida passo passo

> ### 🟢 Prima di tutto: non hai rotto niente.
> Un conflitto non è un errore e non si perde niente. Git ti sta solo dicendo:
> *"tu e un altro avete cambiato le stesse righe dello stesso file, decidete
> voi quale versione tenere"*. Succede a tutti. Si risolve in cinque minuti.

Quasi sempre esce qui, dopo `git merge main`:

```
CONFLICT (content): Merge conflict in frontend/src/App.jsx
Automatic merge failed; fix conflicts and then commit the result.
```

**Passo 1 — vedi quali file:**

```bash
git status
```

Sono elencati sotto **Unmerged paths**, come `both modified: <file>`. Con
`git status --short` hanno il simbolo `UU`. Di solito è **uno**.

**Passo 2 — apri quel file.** Dentro trovi una cosa così:

```
<<<<<<< HEAD
const titolo = "The Game";
=======
const titolo = "The Game - 42";
>>>>>>> main
```

| Tra… | C'è… |
|---|---|
| `<<<<<<< HEAD` e `=======` | **la tua** versione |
| `=======` e `>>>>>>> main` | quella che arriva da `main` (il lavoro di un altro) |

**Passo 3 — decidi e pulisci.** Tieni quello che serve e **cancella le tre
righe di marcatori** (`<<<<<<<`, `=======`, `>>>>>>>`). Devono sparire tutte e
tre. Puoi tenere la tua versione, la sua, tutte e due, o scriverne una terza
che le mette insieme. Nell'esempio, se la versione giusta è la sua, il file
diventa:

```js
const titolo = "The Game - 42";
```

e basta. Nessun `<<<<<<<` rimasto.

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
...`). Va benissimo così: salva ed esci — nano: `Ctrl+O`, `Invio`, `Ctrl+X`;
vim: `:wq` e Invio.

**Controllo finale — fallo sempre:**

```bash
grep -rn "<<<<<<<" . --exclude-dir=.git
```

Se non stampa niente sei a posto. Un file con dentro `<<<<<<<` **non compila e
non funziona**: è l'errore più comune dopo un conflitto risolto di fretta.

> **Unica eccezione:** se ti segnala `docs/come_lavoriamo.md`, ignoralo. Sono
> i marcatori d'esempio di questa guida, non un conflitto.

### Se ti sei confuso

```bash
git merge --abort
```

Torna esattamente a prima del merge, come se non fosse successo niente. Il tuo
lavoro è intatto. **Usalo senza paura**, in tre casi:

- **non sai quale versione tenere** → abort, e scrivi al proprietario di quel
  file ([chi chiedere](#412-chi-chiedere-per-cosa)). Un conflitto **risolto
  male** è molto peggio di uno non risolto: cancelli il lavoro di un altro e
  Git non te lo segnala più.
- **sono dieci file e non ci capisci niente** → abort, e scrivi sul gruppo.
  Non è colpa tua: dieci file in conflitto vogliono dire che quel branch è
  rimasto aperto troppo a lungo. Si risolve in due davanti allo stesso schermo.
- **ti sei perso a metà** → abort, riprovi dopo con più calma.

### Come non averli più

1. **`git merge main` dentro il tuo branch ogni giorno.** Un conflitto di un
   giorno tocca dieci righe. Uno di tre settimane tocca trecento file, e arriva
   sempre nella settimana della consegna.
2. **Resta nella tua cartella** ([territorio](#42-il-tuo-territorio)).
3. **Se devi toccare il file di un altro, avvisalo prima.**

## 4.9 Se sei bloccato

Ti serve una cosa che un altro non ha ancora finito? **Non aspettare.**

- Chiedigli **l'interfaccia, non il codice**: i nomi delle funzioni, la forma
  dei dati. Con quelli puoi già scrivere la tua parte.
- Se ti serve già funzionante, usa un finto (mock) che restituisce dati fissi,
  e vai avanti.
- **Dillo al sync del gruppo.** Restare bloccati in silenzio è la cosa più
  costosa che si possa fare in cinque.

## 4.10 Le sei cose da non fare mai

1. **Committare su `main`.** (Il server ti dirà di no comunque.)
2. **`git push --force` su un branch condiviso.** Mai. Cancella il lavoro degli
   altri e non si torna indietro.
3. **Committare il file `.env`, un certificato o una chiave.** Restano nella
   cronologia per sempre, anche se cancelli il file dopo. Prima di ogni
   `git add`: `git diff`.
4. **Toccare la cartella di un altro senza avvisarlo.**
5. **Approvare una PR senza averla guardata.**
6. **Iniziare a lavorare senza una scheda, o senza assegnartela.** Due persone
   che fanno la stessa cosa senza saperlo è successo in tutti i team che non se
   le assegnano.

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
git commit -m "wip: salvo il lavoro a meta'"   # poi continui
git stash                                      # oppure lo metti da parte
git stash pop                                  # per riprenderlo
```

**`git push` viene rifiutato: `Updates were rejected`** — sul server c'è roba
che tu non hai (hai lavorato da due computer, o qualcuno ha pushato sul tuo
branch):

```bash
git pull
git push
```

> ⚠️ **Non usare `--force` per farlo passare:** cancelli il lavoro altrui.

> **Eccezione:** se il rifiuto dice `non-fast-forward` ed è arrivato **dopo**
> un `git commit --amend` o un reset di un commit già pushato, `git pull` non è
> la soluzione (ti duplica i commit). Fermati e chiedi sul gruppo. Per questo
> l'amend si fa solo **prima** di pushare.

**`fatal: Need to specify how to reconcile divergent branches`** — la tua linea
e quella del server si sono separate e git non vuole indovinare. Quasi sempre
non ti serve davvero quel `pull`: controlla con `git log --oneline -3` cosa hai
e cosa c'è sul server. Se devi solo recuperare un commit fatto dal browser:

```bash
git pull --ff-only origin <nome-del-tuo-branch>
```

**Ho pushato ma la PR sembra vecchia** — non è vecchia: si aggiorna da sola a
ogni push. Ricarica la pagina, e controlla con `git status` di aver pushato sul
branch giusto.

**gh non trova la mia PR**

```bash
gh pr list      # le PR aperte
gh pr status    # quelle che riguardano te
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

**Ho committato sul branch sbagliato**

```bash
git switch -c <branch-giusto>          # porta con te il commit
git reset --hard origin/<branch>       # poi, sul branch sbagliato
```

**Ho fatto un casino, voglio il branch come sul server** — butta via **tutto**
il lavoro locale non pushato:

```bash
git fetch origin
git reset --hard origin/<nome-branch>
```

**Ho committato una password o una chiave**

> 🚨 **Non cancellarla con un altro commit: resta nella cronologia.**
> Avvisa **subito** il gruppo, invalida la chiave e generane una nuova. Poi si
> decide insieme come ripulire.

> **In dubbio:** fermati e chiedi sul gruppo **prima** di digitare qualcosa con
> `--force` o `--hard` dentro. Un messaggio costa trenta secondi, un branch
> distrutto costa una giornata a qualcun altro.

## 4.12 Chi chiedere per cosa

| Argomento | Chi |
|---|---|
| Architettura, contratto tra i servizi, WebSocket | **Dev3** (tech lead) |
| Docker, compose, nginx, database, API | **Dev5** |
| Login, utenti, amici, OAuth | **Dev2** |
| Frontend, componenti, styling | **Dev4** |
| Priorità, scadenze, board, chi fa cosa | **Dev1** (PO/PM) |

Se non sai a chi chiedere, chiedi sul canale del gruppo. Meglio una domanda in
più che mezza giornata persa.

---

## Documenti collegati

| File | Cosa contiene |
|---|---|
| [`divisione_compiti.md`](divisione_compiti.md) | Chi fa cosa, i moduli e i 22 punti |
| `backlog.md` | Le 90 schede della board — *solo in locale, non nel repo: sulla board ci sono già tutte* |
| `workflow_git.md` | Il riferimento completo col setup del repo da zero — *solo in locale* |
