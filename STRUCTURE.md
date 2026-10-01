# Template Structure

```
courtyard-auth-template/
├── .template.config/
│   └── template.json          Template settings: name, short name, placeholder namespace
├── Controllers/
│   └── AuthController.cs      HTTP endpoints: signup, login, refresh, logout, me
├── Data/
│   └── AppDbContext.cs        Database access: the Users and RefreshTokens tables
├── Dtos/
│   └── AuthDtos.cs            Shapes of JSON in and out: AuthRequest, RefreshRequest, AuthResponse
├── Models/
│   ├── User.cs                A user row: Id, Email, PasswordHash
│   └── RefreshToken.cs        A refresh token row: hash, owner, expiry, revoked time
├── Options/
│   └── JwtOptions.cs          Typed, validated version of the "Jwt" settings section
├── Services/
│   ├── IPasswordService.cs    Contract: Hash and Verify
│   ├── PasswordService.cs     BCrypt implementation of that contract
│   └── TokenService.cs        Creates JWT access tokens, makes and hashes refresh tokens
├── .gitignore                 Keeps bin, obj and database files out of git
├── appsettings.json           Connection string and JWT settings (never the key)
├── Courtyard.api.csproj       Project file and package references
├── Program.cs                 Startup: registers services, JWT checking, middleware order
├── README.md                  How to install and use the template (not copied to new projects)
└── STRUCTURE.md               This file (not copied to new projects)
```

Folders created later by commands, not part of the template:

```
Migrations/                    Created by dotnet ef migrations add
app.db                         Created by dotnet ef database update
bin/ and obj/                  Build output
```

## What each folder is for

| Folder | Rule of thumb |
|---|---|
| Controllers | Receives HTTP requests and returns responses. No heavy logic |
| Data | The only place that knows about the database layout |
| Dtos | What the outside world sends and receives. Never expose Models directly |
| Models | What is stored in the database |
| Options | Typed settings read from configuration |
| Services | Reusable logic that controllers ask for through their constructors |

## How a request flows

Login:

1. Client sends `POST /api/auth/login` with email and password.
2. `AuthController.Login` finds the user in `AppDbContext`.
3. `IPasswordService.Verify` compares the password with the stored BCrypt hash.
4. `IssueTokensAsync` asks `TokenService` for a JWT and a random refresh token,
   stores only the refresh token's SHA-256 hash, and returns both tokens.

Protected request:

1. Client sends `GET /api/auth/me` with `Authorization: Bearer <accessToken>`.
2. `UseAuthentication` (in `Program.cs`) checks the signature, issuer, audience and expiry.
3. `[Authorize]` lets the request through, or returns 401.
4. `Me()` reads `userId` and `email` from the token's claims.

Refresh:

1. Client sends `POST /api/auth/refresh` with its refresh token.
2. The server hashes it and looks up the hash in `RefreshTokens`.
3. If valid, the old row is marked revoked and a new pair is issued.
4. If a revoked token shows up again, all of that user's tokens are revoked.

## Where to change things

| Goal | File |
|---|---|
| Add a column to users (for example DisplayName) | `Models/User.cs`, then a new migration |
| Add a new table | A class in `Models/`, a `DbSet` in `Data/AppDbContext.cs`, then a migration |
| Change token lifetimes | `appsettings.json` |
| Add more data to the token | `Claim` list in `TokenService.CreateToken` |
| Change the hashing cost | `WorkFactor` in `Services/PasswordService.cs` |
| Add a new feature endpoint | A new controller in `Controllers/` |
| Add a new service | An interface and class in `Services/`, registered in `Program.cs` |
| Switch database | `Program.cs` and the connection string (see README) |

## Order that matters in Program.cs

1. Register services (`AddControllers`, `AddDbContext`, options, services, authentication)
2. `builder.Build()`
3. `UseAuthentication()` before `UseAuthorization()`
4. `MapControllers()`
5. `Run()`
