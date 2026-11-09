API     := be/GameExplorer.Api
CLIENT  := fe/GameExplorer.Client
RUN     := dotnet run --launch-profile http --project
COMPOSE := docker compose --profile tools
SHELL   := bash

SECRET_FILES := secrets/postgres_password.txt secrets/app_db_password.txt \
	secrets/api_db_password.txt secrets/twitch_id.txt secrets/twitch_key.txt

GREEN  := \033[0;32m
YELLOW := \033[0;33m
RED    := \033[0;31m
RESET  := \033[0m

BACKEND_ENV = ConnectionStrings__Default="Host=localhost;Port=$(or $(POSTGRES_PORT),5432);Database=$(or $(POSTGRES_DB),terranova);Username=$(or $(API_DB_USER),terranova_api);Password=$$(< secrets/api_db_password.txt)" \
	Igdb__ClientId="$$(< secrets/twitch_id.txt)" \
	Igdb__ClientSecret="$$(< secrets/twitch_key.txt)"

-include .env

all: up migrate

.env:
	@cp .env.example $@
	@printf "$(YELLOW)Created .env from .env.example$(RESET)\n"

secrets:
	@for f in $(SECRET_FILES); do \
		[ -s $$f ] || { printf "$(RED)Missing or empty: $$f$(RESET)\n"; exit 1; }; \
	done

up: .env secrets
	@printf "$(GREEN)Starting the database...$(RESET)\n"
	@$(COMPOSE) up -d --wait db

migrate: secrets
	@printf "$(GREEN)Applying migrations...$(RESET)\n"
	@dotnet tool restore
	@dotnet restore $(API)
	@$(BACKEND_ENV) ./db/scripts/migrate.sh

dev: secrets
	@printf "$(GREEN)Starting API and client (Ctrl+C stops both)...$(RESET)\n"
	@trap 'trap - INT TERM; kill 0' INT TERM; $(BACKEND_ENV) $(RUN) $(API) & $(RUN) $(CLIENT) & wait

test:
	@dotnet test GameExplorer.slnx

adminer: up
	@printf "$(GREEN)Starting Adminer: http://localhost:$(or $(ADMINER_PORT),8080)$(RESET)\n"
	@$(COMPOSE) up -d --wait adminer

adminer-stop:
	@printf "$(YELLOW)Stopping Adminer...$(RESET)\n"
	@$(COMPOSE) rm -sf adminer

ps:
	@$(COMPOSE) ps

logs:
	@$(COMPOSE) logs -f

clean:
	@printf "$(YELLOW)Stopping and removing containers (data is kept)...$(RESET)\n"
	@$(COMPOSE) down

fclean:
	@read -rp "Delete all local data? Type yes: " a; [ "$$a" = yes ]
	@printf "$(RED)Removing containers and database data...$(RESET)\n"
	@$(COMPOSE) down -v

re: fclean all

.PHONY: all secrets up migrate dev test adminer adminer-stop ps logs clean fclean re
.NOTPARALLEL:
