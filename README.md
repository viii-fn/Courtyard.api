# Courtyard Auth API Template

A ready-made ASP.NET Core Web API with a complete login system:
signup, login, JWT access tokens, rotating refresh tokens, logout, and a
protected `/me` endpoint. Password hashing uses BCrypt. Database is SQLite
through EF Core with migrations.

- Template name: `Courtyard Auth API`
- Short name (the command you type): `courtyardauth`
- Template identity: `Courtyard.AuthApi.Template`
- Placeholder namespace: `Courtyard.api`

## The namespace

Every file in this template uses the namespace `Courtyard.api`
(for example `namespace Courtyard.api.Services;`). That text is the
placeholder. When you create a project, it is replaced everywhere, in
namespaces, `using` lines, the `.csproj` file name, and the `appsettings.json`
Issuer/Audience, by the name you pass with `-n`.

```
dotnet new courtyardauth -n Acme.Api
```

turns `Courtyard.api.Services` into `Acme.Api.Services`, and
`Courtyard.api.csproj` into `Acme.Api.csproj`.

Rules for the name you pass:

- Letters, numbers, and dots only. No hyphens or spaces.
- It is case-sensitive. `Acme.Api` and `Acme.api` give different namespaces.
- Do not name a project `BCrypt`, `Options`, or anything that clashes with a library.

## One-time setup (install the template)

1. Clone the template
   ```
   git clone https://github.com/viii-fn/Courtyard.api
   ```.
3. Install it:

```
dotnet new install Courtyard.api
```

3. Check it is listed:

```
dotnet new list courtyardauth
```

If you edit the template later, run the install command again to update it.
To remove it: `dotnet new uninstall ~/templates/courtyard-auth-template`

## Creating a new project

```
cd ~
dotnet new courtyardauth -n Acme.Api
cd Acme.Api
```

Then, inside the new project:

```
dotnet user-secrets init
dotnet user-secrets set "Jwt:Key" "$(openssl rand -base64 64 | tr -d '\n')"
dotnet build
dotnet ef migrations add InitialCreate
dotnet ef database update
dotnet watch
```

What each line does:

| Command | Purpose |
|---|---|
| `dotnet user-secrets init` | Gives this project its own private secret store |
| `dotnet user-secrets set "Jwt:Key" ...` | Creates the signing key for tokens (single line, 88 characters) |
| `dotnet build` | Compiles. Migrations need a clean build first |
| `dotnet ef migrations add InitialCreate` | Writes the migration for Users and RefreshTokens |
| `dotnet ef database update` | Creates `app.db` with those tables |
| `dotnet watch` | Runs the API and restarts when you save |

The first restore needs internet. After that the packages are cached and
everything works offline.

Never put a real key in `appsettings.json` or in the template folder.
Use a different key for every project and every environment. In production,
set it as an environment variable: `Jwt__Key=...`

## Endpoints

| Method | Route | Body | Result |
|---|---|---|---|
| POST | `/api/auth/signup` | `{ "email", "password" }` | `{ accessToken, refreshToken }` or 409 |
| POST | `/api/auth/login` | `{ "email", "password" }` | `{ accessToken, refreshToken }` or 401 |
| POST | `/api/auth/refresh` | `{ "refreshToken" }` | New token pair, old refresh token revoked |
| POST | `/api/auth/logout` | `{ "refreshToken" }` | 204 |
| GET | `/api/auth/me` | none, needs `Authorization: Bearer <accessToken>` | `{ userId, email }` |

## Quick test

Replace `5000` with the port `dotnet watch` prints.

```
curl -X POST http://localhost:5000/api/auth/signup \
  -H "Content-Type: application/json" \
  -d '{"email":"elvis@test.com","password":"password123"}'

curl -i http://localhost:5000/api/auth/me

curl http://localhost:5000/api/auth/me -H "Authorization: Bearer PASTE_ACCESS_TOKEN"

curl -X POST http://localhost:5000/api/auth/refresh \
  -H "Content-Type: application/json" \
  -d '{"refreshToken":"PASTE_REFRESH_TOKEN"}'
```

Expected: signup returns two tokens, `/me` without a token returns 401, `/me`
with the token returns your email, refresh returns a new pair. Sending the
same old refresh token a second time returns 401 and revokes all of that
user's refresh tokens.

## Settings (appsettings.json)

| Setting | Meaning |
|---|---|
| `ConnectionStrings:Default` | Database location. `Data Source=app.db` is a SQLite file |
| `Jwt:Issuer` / `Jwt:Audience` | Labels stamped into tokens and checked back. Any non-empty text |
| `Jwt:AccessTokenMinutes` | Lifetime of the access token (1 to 60) |
| `Jwt:RefreshTokenDays` | Lifetime of the refresh token (1 to 90) |
| `Jwt:Key` | Not stored here. Set with user-secrets or an environment variable |

## Moving to Postgres later

1. `dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL`
2. In `Program.cs`, change `options.UseSqlite(...)` to `options.UseNpgsql(...)`
3. Put the Postgres connection string in `ConnectionStrings__Default`
4. Delete the `Migrations` folder, then run `dotnet ef migrations add InitialCreate`
   and `dotnet ef database update` against the new database

## Common errors and what they mean

| Message | Cause and fix |
|---|---|
| `DataAnnotation validation failed for 'JwtOptions' members: 'Key'` | The secret is not set or is shorter than 64 characters. Run the `user-secrets set` command |
| Same error for `Issuer` or `Audience` | The `Jwt` block is missing from `appsettings.json` or sits outside the outer braces |
| `Build failed` when running `dotnet ef` | Run `dotnet build` and fix the first error it lists |
| `already contains a definition for 'X'` | Two files declare the same class. Delete the duplicate |
| `no such table: Users` | You skipped `dotnet ef database update` |
| 401 on every request after a while | The access token expired. Call `/refresh` |

## Before using this for a real product

- Keep the key out of source control and rotate it if it ever leaks.
- Add rate limiting on `/login` and `/signup`.
- Serve only over HTTPS.
- Add CORS rules for your frontend's address.
- For password reset, email verification, and 2FA, consider ASP.NET Core Identity.
