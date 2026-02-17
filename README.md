# SalesAssistant MVP (Telegram + OpenAI + PostgreSQL)

Production-oriented MVP using Clean Architecture and ASP.NET Core Web API (.NET 8).

## Folder Structure

```
src/
  SalesAssistant.Api/              # Presentation layer (controllers, DI bootstrap, host)
  SalesAssistant.Application/      # Use cases contracts and abstractions
  SalesAssistant.Domain/           # Core entities and enums
  SalesAssistant.Infrastructure/   # EF Core persistence + external integrations
Dockerfile
docker-compose.yml
.env.example
```

## Core Flow

1. Telegram sends update to `POST /api/webhooks/telegram/{storeId}`.
2. `TelegramService` loads/creates conversation and stores incoming message.
3. `LeadService` checks order intent and runs name/phone capture workflow.
4. If not lead capture:
   - `ConversationService` tries FAQ matching from PostgreSQL.
   - fallback to `OpenAiService` with store-specific `SystemPrompt`.
5. Assistant response is persisted and sent back to Telegram.
6. On lead creation, owner receives Telegram notification.

## Environment Variables

- `ConnectionStrings__Postgres`
- `OpenAI__ApiKey`
- `OpenAI__Model`
- `OpenAI__BaseUrl`

## Run with Docker Compose

```bash
docker compose --env-file .env.example up --build
```

## Telegram Webhook Setup Example

```bash
curl -X POST "https://api.telegram.org/bot<YOUR_BOT_TOKEN>/setWebhook" \
  -H "Content-Type: application/json" \
  -d '{"url":"https://<YOUR_DOMAIN>/api/webhooks/telegram/<STORE_ID_GUID>"}'
```

## Example System Prompt (stored per Store)

> You are a sales assistant for an e-commerce Telegram shop. Answer politely, push toward order completion, and collect missing details.
