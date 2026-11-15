# Git workflow

Come lavoriamo con Git e GitHub nel team **Terranova Game Explorer**. Chi fa cosa: [`Suddivisione del lavoro`](Suddivisione%20del%20lavoro.md). Avvio del progetto: [`README.md`](../README.md).

**In una frase:** niente commit su `main`. Ogni lavoro parte da una issue, vive in un branch ed entra in `main` con una PR approvata su GitHub da un compagno.

## Territori

Ognuno scrive solo nei suoi file. Fuori dal proprio territorio: PR piccola, rivista dal proprietario.

| Ruolo | GitHub | Territorio | Label |
|---|---|---|---|
| **A – backend** | [`@K0ff33Blu3`](https://github.com/K0ff33Blu3) | `be/` (tranne le righe di C) · `shared/` · `postman/` dei giochi · `GameExplorer.slnx` | `BE` · `API` |
| **B – frontend** | [`@fmartini02`](https://github.com/fmartini02) | `fe/` (anche `Pages/Account` e `Services/Auth`) · `.github/workflows/frontend.yml` | `FE` |
| **C – database e account** | [`@Elmond0`](https://github.com/Elmond0) | `db/` · `be/GameExplorer.Api/Data/` · `be/GameExplorer.Api/Auth/` · `postman/` dell'account · `compose.yaml` · `Makefile` · `.env.example` · `.github/workflows/database.yml` · `.github/dependabot.yml` | `DB` |
| **tutti** | | `DOCS/` · `README.md` · `.gitignore`, con modifiche piccole e avvisando gli altri | |

File condivisi: in `Program.cs` e nel `.csproj` della Web API, C aggiunge solo le righe del login e del database, in un blocco commentato. `shared/` è il contratto tra le due app: ogni cambio si annuncia sul gruppo **prima** della PR.

## Prima configurazione (una volta per computer)

| | Linux | macOS | Windows |
|---|---|---|---|
| Git | gestore di pacchetti | `xcode-select --install` | [Git for Windows](https://git-scm.com/download/win) (installa anche **Git Bash**) |
| `gh` ≥ 2.60 | [repository ufficiale](https://github.com/cli/cli/blob/trunk/docs/install_linux.md) (non il pacchetto di Ubuntu: è vecchio) · Arch: `pacman -S github-cli` | `brew install gh` | `winget install --id GitHub.cli` |
| Terminale | quello di sistema | quello di sistema | **Git Bash**: tutti i comandi di questa guida e `make` ([README](../README.md)) |

Su Windows, dopo un'installazione apri un terminale nuovo. Git Bash si apre dal menu Start oppure con tasto destro nella cartella → *Open Git Bash here*. Usa i percorsi così: `C:\Users\nome` → `/c/Users/nome`.

```bash
git config --global user.name "Nome Cognome"
git config --global user.email "email-del-tuo-account-github"
git config --global fetch.prune true
git config --global pull.ff only
gh auth login        # GitHub.com → SSH → genera una chiave → Login with a web browser
gh config set editor nano     # Windows: gh config set editor notepad
git clone git@github.com:Elmond0/Terranova-game-explorer.git
```

- **L'email deve essere quella dell'account GitHub**, altrimenti i commit vanno a un altro account e il lavoro non risulta tuo. Controllo: nella [lista dei commit](https://github.com/Elmond0/Terranova-game-explorer/commits/main) accanto ai tuoi deve comparire il tuo username.
- `gh auth login` crea la chiave SSH: fallo prima del clone, se no `Permission denied (publickey)`.
- Fine riga: [`.gitattributes`](../.gitattributes) forza LF su `*.sh` e `Makefile`, così funzionano anche clonati su Windows. Non cambiarlo.
- Verifica: `gh auth status` → *Logged in*; `gh issue list` mostra le issue.

## Il giro di un lavoro

```bash
git switch main && git pull
gh issue list --assignee @me
gh issue develop 42 --base main --checkout   # branch collegato alla issue #42 (oppure: git switch -c db/nome-breve)
# ... lavoro ...
git diff                                      # sempre, prima di add
git add <file per nome>                       # mai "git add ." alla cieca
git commit -m "feat(db): aggiunge la tabella dei preferiti"
git push -u origin HEAD                       # -u solo la prima volta
gh pr create --draft                          # appena c'è il primo commit
# ogni giorno:  git switch main && git pull && git switch - && git merge main
gh pr ready                                   # a lavoro finito
# dopo l'approvazione di un compagno:
gh pr merge --squash --delete-branch
git switch main && git pull
```

## Issue

- Niente lavoro senza issue assegnata (`gh issue edit 42 --add-assignee @me`). Una aperta alla volta.
- Ogni issue ha label d'area (`BE` `FE` `DB` `API`) e milestone (`fase 0`…`fase 2`). `PO` = urgente, `question` = serve una risposta prima di lavorare.
- Ordine: prima `PO`, poi la fase più bassa, poi quella che blocca un compagno.
- Il **"Fatto quando"** nella issue è il criterio di fine.
- Si chiude dalla PR: `Closes #42` su una riga sua (anche `Fixes`, `Resolves`); `Rif. #42` collega senza chiudere. Funziona solo per PR verso `main`.
- Serve qualcosa nel territorio di un altro: issue assegnata a lui, non modifiche ai suoi file.

## Branch

Due forme ammesse:

| Forma | Esempio | Quando |
|---|---|---|
| `<area>/<cosa>` | `db/favorites`, `be/studio-entity`, `docs/git-workflow` | branch aperto a mano |
| `<n>-<titolo>` | `68-featfe-accesso-registrazione` | creato da `gh issue develop` o dal pulsante *Create a branch* della issue |

Aree: `be` `fe` `db` `api` `docs` `ci`. Il tipo (`fix`, `feat`) va nel commit, non nel branch: niente `be/fix/...`.

## Commit

[Conventional Commits](https://www.conventionalcommits.org/it/v1.0.0/): `tipo(area): cosa`, in italiano, al presente, minuscolo. Codice, commenti e nomi dei file in inglese.

| Tipo | Uso |
|---|---|
| `feat` · `fix` · `test` · `refactor` | codice nuovo, bug, test, stessa logica riorganizzata |
| `docs` · `ci` · `chore` | documentazione, GitHub Actions, configurazione e dipendenze |

- Il corpo spiega il **perché**: `git commit -m "fix(db): salva le date in UTC" -m "Npgsql rifiuta le date non UTC in timestamptz."`
- Da evitare (presenti nella storia): `eureka`, `tmp`, `modificati modelli`, `add:DOC`, `be(fix): ...` al posto di `fix(be): ...`.

## Pull Request

Testo:

```markdown
Closes #42

## Cosa contiene
## Perché così
## Come verificare
comandi esatti (dotnet test, richieste Postman, pagina da aprire)
```

- **Piccola**: una issue, poche decine di file. Una PR enorme non viene rivista davvero.
- Prima di `gh pr ready`: `dotnet build` e `dotnet test GameExplorer.slnx` (o `make test`) in locale.
- CI su GitHub Actions: `frontend.yml` (test del client su `fe/` e `shared/`) e `database.yml` (migrazioni su `db/` e `Data/`). Non c'è CI del backend e la Web API non ha test automatici: si prova a mano con Postman.
- Le PR di **Dependabot** (azioni e immagini Docker) le rivede e mergia C.

### Revisione

Entro la giornata. Il compagno prova la PR seguendo "Come verificare", poi **approva su GitHub**: un "ok" a voce non lascia traccia nella PR.

```bash
gh pr list
gh pr checkout 42 && dotnet test GameExplorer.slnx
gh pr review 42 --approve        # oppure --request-changes -b "motivo"
git switch -
```

Cosa guardare: fa quello che dice la issue? Tocca solo il suo territorio? Contiene `bin/`, `obj/`, `.env`, `secrets/`, chiavi o file di prova?

### Merge

Mergia **l'autore**, dopo l'approvazione, sempre con **squash**: un commit per PR su `main`, con il titolo della PR. Il titolo quindi segue anch'esso `tipo(area): cosa`.

```bash
gh pr merge 42 --squash --delete-branch
```

Dal browser: **Squash and merge**, poi **Delete branch** e in locale `git branch -D <branch>` (`-D` maiuscola: dopo lo squash Git non riconosce il branch come mergiato).

## Conflitti

1. `git status` → i file in conflitto sono sotto *Unmerged paths*.
2. Nel file, tra `<<<<<<<` e `=======` c'è la tua versione, tra `=======` e `>>>>>>>` quella di `main`. Tieni quello che serve e togli i marcatori.
3. `git add <file>` → `git commit` → `git grep -n "<<<<<<<"` vuoto → `dotnet build`.

Se non è chiaro: `git merge --abort` e chiedi al proprietario del file.

## Mai

1. Commit o push diretto su `main`.
2. `git push --force` su un branch condiviso.
3. `.env`, `secrets/`, chiavi o password in un commit: restano nella storia. Se succede, avvisa e rigenera la chiave (IGDB: [console Twitch](https://dev.twitch.tv/console)).
4. Modificare file di un altro senza avvisarlo.
5. Approvare una PR senza averla provata.

## Problemi frequenti

| Sintomo | Soluzione |
|---|---|
| `Updates were rejected` al push | `git pull`, poi `git push`. Mai `--force` |
| `Your local changes would be overwritten` | `git stash`, cambi branch, `git stash pop` |
| Commit su `main` non pushato | `git switch -c <area>/<cosa>`, `git switch main`, `git reset --hard origin/main` |
| Messaggio sbagliato, non pushato | `git commit --amend -m "..."` |
| `make`: `f non atteso` (Windows) | `make` lanciato da PowerShell o cmd: aprilo da Git Bash |
| `$'\r': command not found` in uno script | lo script ha CRLF: `git checkout -- <file>` (`.gitattributes` lo riporta a LF) |
| `gh`: `Projects (classic) is being deprecated` | `gh` vecchio: aggiornalo |
| `Can not approve your own pull request` | serve un compagno |
| Si apre vim | `Esc`, `:wq`, Invio; poi `gh config set editor nano` (Windows: `notepad`) |

## Protezione di `main`

Oggi `main` non è protetto (nessun ruleset). Per far rispettare le regole a GitHub, `@Elmond0` crea un ruleset in *Settings → Rules → Rulesets*:

| Impostazione | Valore |
|---|---|
| Target | default branch (`main`) |
| Restrict deletions · Block force pushes | ✅ |
| Require a pull request | ✅, 1 approvazione |
| Allowed merge methods | solo **Squash** |
| Require status checks | no: le CI partono solo su alcune cartelle, un check obbligatorio che non parte blocca le altre PR |

E in *Settings → General → Pull Requests*: togliere *Allow merge commits* e *Allow rebase merging*, attivare *Automatically delete head branches*.

## Fonti

- [Pro Git](https://git-scm.com/book/it/v2) · [Conventional Commits](https://www.conventionalcommits.org/it/v1.0.0/) · [manuale di `gh`](https://cli.github.com/manual/) · [`gh` su Windows](https://github.com/cli/cli/blob/trunk/docs/install_windows.md)
- GitHub: [collegare PR e issue](https://docs.github.com/en/issues/tracking-your-work-with-issues/using-issues/linking-a-pull-request-to-an-issue) · [squash merge](https://docs.github.com/en/repositories/configuring-branches-and-merges-in-your-repository/configuring-pull-request-merges/configuring-commit-squashing-for-pull-requests) · [ruleset](https://docs.github.com/en/repositories/configuring-branches-and-merges-in-your-repository/managing-rulesets/about-rulesets) · [email dei commit](https://docs.github.com/en/account-and-profile/setting-up-and-managing-your-personal-account-on-github/managing-email-preferences/setting-your-commit-email-address)
