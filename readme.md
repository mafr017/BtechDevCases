# Auth with JWT — ASP.NET Core + React + PostgreSQL

Register, log in, and open a protected page that shows `Hello [email], welcome back`.
Users are logged out after 15 minutes of inactivity. Each user has a wallet and can transfer money to other users.
The app is built to keep working on very bad connections.

**Stack:** .NET 10 Web API, PostgreSQL 17, React + Vite + TypeScript, Docker Compose, xUnit.

## How to build and run

Requirements: Docker with Docker Compose.

```bash
cp .env.example .env
```

Set `Jwt__Secret` in `.env` (at least 32 characters), for example with the output of:

```bash
openssl rand -base64 48
```

Build and start everything:

```bash
docker compose up --build
```

| Service | URL |
| --- | --- |
| Web app | <http://localhost:8080> |
| API + Swagger UI | <http://localhost:8000/swagger> |
| PostgreSQL | `localhost:5433` |

The database schema is created from `db/init/*.sql` on the first start. To reset the database: `docker compose down -v`.
If you already ran an older version of this project, reset the database once so the wallet tables are created.

## Environment variables

`docker-compose.yml` reads them from `.env`. Empty values use the default.

| Variable | Default | Required | Purpose |
| --- | --- | --- | --- |
| `Jwt__Secret` | — | **yes** | Key used to sign the JWT (min 32 characters) |
| `Jwt__Issuer` | `auth-api` | | `iss` claim, validated on every request |
| `Jwt__Audience` | `auth-web` | | `aud` claim, validated on every request |
| `Jwt__AccessTokenMinutes` | `15` | | Token lifetime, which is also the inactivity limit |
| `POSTGRES_USER` | `app` | | Database user (used by the database and the API) |
| `POSTGRES_PASSWORD` | `app` | | Database password (used by the database and the API) |
| `POSTGRES_DB` | `authdb` | | Database name (used by the database and the API) |
| `CORS_ORIGIN` | `http://localhost:8080` | | Web app origin allowed to call the API |

## API

| Method | Path | Auth | Result |
| --- | --- | --- | --- |
| `POST` | `/api/auth/register` | — | `201 { id, email }` |
| `POST` | `/api/auth/login` | — | `200 { accessToken, tokenType, expiresAt, expiresIn }` |
| `POST` | `/api/auth/refresh` | Bearer | New token for a still-valid token |
| `GET` | `/api/me` | Bearer | `{ userId, email, message: "Hello [email], welcome back" }` |
| `GET` | `/api/wallet` | Bearer | `{ balance, transfers }` (latest 20, sent and received) |
| `POST` | `/api/wallet/transfers` | Bearer + `Idempotency-Key` | `{ recipient, amount, notes }` → `201` with the updated wallet |

Sample requests are in [`AuthApi/AuthApi/AuthApi.http`](AuthApi/AuthApi/AuthApi.http).

## Tests

```bash
cd AuthApi && dotnet test
```
