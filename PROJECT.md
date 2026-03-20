# TwinBlade — Tài liệu dự án

## Tổng quan

TwinBlade là backend API cho game multiplayer, xây dựng trên .NET 10 theo chuẩn **Clean Architecture** với 4 layer tách biệt. Triển khai trên AWS sử dụng Cognito (auth), RDS PostgreSQL (database), S3 (storage), và ElastiCache Redis (caching).

---

## Kiến trúc tổng thể

```
TwinBlade.Api              ← HTTP layer (Controllers, Middleware, Extensions)
    ↓ gọi
TwinBlade.Application      ← Business logic (Commands, Queries, Abstractions, DTOs)
    ↓ phụ thuộc interface
TwinBlade.Domain           ← Core entities (không phụ thuộc gì)
    ↑ implement bởi
TwinBlade.Infrastructure   ← AWS, EF Core, Redis (implement các interface của Application)
```

Dependency rule: mỗi layer chỉ phụ thuộc vào layer bên trong, không bao giờ ngược lại.

---

## Cấu trúc thư mục

```
src/
├── TwinBlade.Api/
│   ├── Controllers/
│   │   ├── AuthController.cs
│   │   ├── PlayerController.cs
│   │   ├── RoomController.cs
│   │   ├── MatchController.cs
│   │   └── ItemController.cs
│   ├── Extensions/
│   │   ├── AddAuth.cs          ← JWT Bearer + Cognito config
│   │   └── AddScalar.cs        ← Swagger/OpenAPI config
│   ├── Middleware/
│   │   └── ExceptionMiddleware.cs
│   ├── Program.cs
│   └── appsettings.json
│
├── TwinBlade.Application/
│   ├── Abstractions/
│   │   ├── Auth/ICognitoAuthService.cs
│   │   ├── Caching/IRoomCacheService.cs
│   │   ├── Persistence/
│   │   │   ├── IPlayerRepository.cs
│   │   │   ├── IRoomRepository.cs
│   │   │   ├── IItemRepository.cs
│   │   │   └── IMatchResultRepository.cs
│   │   └── Storage/IAvatarStorageService.cs
│   ├── Commands/
│   │   ├── Auth/   SignInCommand(Handler)
│   │   ├── Player/ RegisterPlayerCommand(Handler)
│   │   ├── Room/   CreateRoomCommand(Handler)
│   │   │           JoinRoomCommand(Handler)
│   │   │           SetPlayerReadyCommand(Handler)
│   │   │           StartGameCommand(Handler)
│   │   └── Match/  SubmitMatchResultCommand(Handler)
│   ├── Queries/
│   │   ├── Player/ GetPlayerQuery(Handler)
│   │   │           GetPlayerProgressQuery(Handler)
│   │   ├── Room/   GetRoomQuery(Handler)
│   │   ├── Match/  GetMatchResultQuery(Handler)
│   │   └── Item/   GetItemsQuery(Handler)
│   └── Dtos/
│       ├── Request/  SignInRequest, RegisterPlayerRequest, CreateRoomRequest,
│       │             JoinRoomRequest, SubmitMatchResultRequest
│       └── Response/ AuthResult, PlayerResponse, PlayerProgressResponse,
│                     RoomResponse, MatchResultResponse, ItemResponse
│
├── TwinBlade.Domain/
│   └── Entities/
│       ├── Player.cs
│       ├── PlayerProgress.cs
│       ├── PlayerItem.cs
│       ├── Item.cs
│       ├── Room.cs
│       ├── RoomPlayer.cs
│       ├── RoomStatus.cs (enum)
│       ├── MatchResult.cs
│       └── PlayerMatchResult.cs
│
└── TwinBlade.Infrastructure/
    ├── Auth/Cognito/CognitoAuthService.cs
    ├── Cache/Redis/RoomCacheService.cs
    ├── Options/
    │   ├── CognitoOptions.cs
    │   ├── S3Options.cs
    │   └── RedisOptions.cs
    ├── Persistence/Rds/
    │   ├── AppDbContext.cs
    │   └── Repositories/
    │       ├── PlayerRepository.cs
    │       ├── RoomRepository.cs
    │       ├── ItemRepository.cs
    │       └── MatchResultRepository.cs
    ├── Storage/S3/S3AvatarStorageService.cs
    └── AddInfrastructure.cs   ← DI registration
```

---

## Domain Entities

### Player
Người chơi trong hệ thống. Được tạo sau khi đăng ký qua Cognito.

| Field        | Type           | Mô tả                          |
|--------------|----------------|--------------------------------|
| Id           | Guid           | Primary key                    |
| Username     | string         | Tên đăng nhập (max 100)        |
| DisplayName  | string         | Tên hiển thị (max 100)         |
| AvatarUrl    | string         | URL ảnh đại diện từ S3         |
| CreatedAt    | DateTime       | Thời điểm tạo                  |
| Progress     | PlayerProgress | 1-1 navigation property        |

### PlayerProgress
Tiến trình của player (gold, inventory). Quan hệ 1-1 với Player.

| Field     | Type             | Mô tả                    |
|-----------|------------------|--------------------------|
| PlayerId  | Guid             | FK → Player (PK)         |
| Gold      | int              | Số vàng hiện có          |
| Inventory | List<PlayerItem> | Owned collection (EF)    |

### PlayerItem
Item trong túi đồ của player. Owned entity của PlayerProgress.

| Field    | Type | Mô tả                  |
|----------|------|------------------------|
| Id       | Guid | PK                     |
| ItemId   | Guid | FK → Item              |
| Quantity | int  | Số lượng               |

### Item
Vật phẩm trong game. Được quản lý thủ công qua DB.

| Field       | Type   | Mô tả                        |
|-------------|--------|------------------------------|
| Id          | Guid   | PK                           |
| Name        | string | Tên item                     |
| Description | string | Mô tả                        |
| ImageUrl    | string | URL ảnh từ S3 (upload thủ công) |
| IsActive    | bool   | Có hiển thị cho player không |

### Room
Phòng chơi. Vòng đời: Waiting → InGame → Finished → Closed.

| Field        | Type             | Mô tả                          |
|--------------|------------------|--------------------------------|
| Id           | Guid             | PK                             |
| RoomCode     | string           | Mã 6 ký tự để join             |
| HostPlayerId | Guid             | Player tạo phòng               |
| Status       | RoomStatus       | Trạng thái phòng               |
| MaxPlayers   | int              | Số người tối đa                |
| Players      | List<RoomPlayer> | Owned collection (EF)          |
| CreatedAt    | DateTime         | Thời điểm tạo                  |

### RoomPlayer
Trạng thái của player trong phòng. Owned entity của Room.

| Field       | Type   | Mô tả                    |
|-------------|--------|--------------------------|
| PlayerId    | Guid   | FK → Player              |
| DisplayName | string | Tên hiển thị trong phòng |
| IsReady     | bool   | Đã sẵn sàng chưa         |

### RoomStatus (enum)
```
Waiting  = 1   ← Đang chờ player vào
InGame   = 2   ← Đang chơi
Finished = 3   ← Đã kết thúc
Closed   = 4   ← Đã đóng
```

### MatchResult
Kết quả trận đấu. Được tạo khi submit kết quả sau game.

| Field      | Type                    | Mô tả                  |
|------------|-------------------------|------------------------|
| Id         | Guid                    | PK                     |
| RoomId     | Guid                    | FK → Room              |
| FinishedAt | DateTime                | Thời điểm kết thúc     |
| Players    | List<PlayerMatchResult> | Owned collection (EF)  |

### PlayerMatchResult
Kết quả cá nhân trong trận. Owned entity của MatchResult.

| Field      | Type | Mô tả              |
|------------|------|--------------------|
| PlayerId   | Guid | FK → Player        |
| Score      | int  | Điểm đạt được      |
| EarnedGold | int  | Gold nhận được     |

---

## Infrastructure — Chi tiết

### 1. AWS Cognito — Authentication

**File:** `Infrastructure/Auth/Cognito/CognitoAuthService.cs`
**Interface:** `Application/Abstractions/Auth/ICognitoAuthService.cs`
**Options:** `Infrastructure/Options/CognitoOptions.cs`

Sử dụng `AdminInitiateAuth` với flow `ADMIN_USER_PASSWORD_AUTH`. Trả về 3 token:
- `AccessToken` — dùng để gọi API (ngắn hạn, ~1h)
- `IdToken` — chứa thông tin user (sub, email, cognito:username)
- `RefreshToken` — dùng để lấy token mới (dài hạn)

JWT validation được cấu hình trong `Api/Extensions/AddAuth.cs`:
- Issuer: `https://cognito-idp.{region}.amazonaws.com/{userPoolId}`
- Audience: `ClientId`
- Claim `sub` → dùng làm PlayerId trong các controller
- Claim `cognito:username` → dùng làm DisplayName khi join room

**appsettings.json:**
```json
"Cognito": {
  "UserPoolId": "ap-southeast-1_Qat2KJgNe",
  "ClientId": "11pm0k75vudmnjlkq2ljm0udur",
  "ClientSecret": "..."
}
```

---

### 2. AWS RDS PostgreSQL — Database

**File:** `Infrastructure/Persistence/Rds/AppDbContext.cs`
**Provider:** Npgsql.EntityFrameworkCore.PostgreSQL

**DbSets:**
| DbSet           | Entity          |
|-----------------|-----------------|
| Players         | Player          |
| PlayerProgresses| PlayerProgress  |
| Items           | Item            |
| Rooms           | Room            |
| MatchResults    | MatchResult     |

**EF Core mapping đáng chú ý:**
- `PlayerProgress` → owned 1-1 với Player, FK là `PlayerId`
- `PlayerProgress.Inventory` → `OwnsMany<PlayerItem>` (lưu trong bảng riêng)
- `Room.Players` → `OwnsMany<RoomPlayer>` (lưu trong bảng riêng)
- `MatchResult.Players` → `OwnsMany<PlayerMatchResult>` (lưu trong bảng riêng)

**Connection string** (appsettings.json):
```json
"ConnectionStrings": {
  "DefaultConnection": "Host=...;Database=TwinBladeDb;Username=...;Password=..."
}
```

**Repositories:**

| Repository            | Interface               | Methods                                          |
|-----------------------|-------------------------|--------------------------------------------------|
| PlayerRepository      | IPlayerRepository       | GetByIdAsync, GetByUsernameAsync, AddAsync, SaveChangesAsync |
| RoomRepository        | IRoomRepository         | GetByIdAsync, GetByCodeAsync, AddAsync, SaveChangesAsync |
| ItemRepository        | IItemRepository         | GetByIdAsync, GetAllActiveAsync                  |
| MatchResultRepository | IMatchResultRepository  | GetByIdAsync, GetByRoomIdAsync, AddAsync, SaveChangesAsync |

---

### 3. AWS S3 — Storage

**File:** `Infrastructure/Storage/S3/S3AvatarStorageService.cs`
**Interface:** `Application/Abstractions/Storage/IAvatarStorageService.cs`
**Options:** `Infrastructure/Options/S3Options.cs`

> Upload file lên S3 được thực hiện **thủ công qua AWS Console**, không qua server.

Service chỉ làm 2 việc:
- `UploadAsync` → trả về URL tĩnh S3 theo pattern `https://{bucket}.s3.{region}.amazonaws.com/avatars/{playerId}`
- `GetAvatarUrlAsync` → tạo pre-signed URL có hiệu lực 1 giờ để đọc file

**Cấu trúc key trên S3:**
```
avatars/{playerId}     ← ảnh đại diện player
```

**appsettings.json:**
```json
"S3": {
  "AvatarBucket": "game-avatar-dev",
  "SaveBucket":   "game-save-dev",
  "Region":       "ap-southeast-1"
}
```

---

### 4. AWS ElastiCache Redis — Caching

**File:** `Infrastructure/Cache/Redis/RoomCacheService.cs`
**Interface:** `Application/Abstractions/Caching/IRoomCacheService.cs`
**Options:** `Infrastructure/Options/RedisOptions.cs`
**Client:** StackExchange.Redis `IConnectionMultiplexer` (Singleton)

Hiện tại Redis chỉ dùng để cache mapping `RoomCode → RoomId`, giúp tránh query DB mỗi lần player join phòng.

**Key pattern:** `room:{roomCode}` (prefix cấu hình qua `RedisOptions.RoomKeyPrefix`)
**TTL:** 2 giờ

**Luồng join room:**
1. Tìm `roomId` từ Redis bằng `roomCode`
2. Nếu cache miss → query DB bằng `roomCode`
3. Nếu không tìm thấy → throw `InvalidOperationException`

**Kết nối** được cấu hình từ section `Cache` trong appsettings:
```json
"Cache": {
  "Endpoint":         "twinblade-o5ht5k.serverless.apse1.cache.amazonaws.com:6379",
  "Ssl":              false,
  "ConnectTimeoutMs": 5000,
  "SyncTimeoutMs":    5000,
  "AbortOnConnectFail": false
}
```

---

### 5. DI Registration — AddInfrastructure.cs

Tất cả infrastructure services được đăng ký tại một điểm duy nhất:

```csharp
services.AddInfrastructure(configuration);
```

Thứ tự đăng ký:
1. Options (Cognito, S3, Redis) — `IOptions<T>`
2. AWS SDK clients — `IAmazonS3`, `IAmazonCognitoIdentityProvider`
3. EF Core DbContext — `AppDbContext` (Scoped)
4. Redis `IConnectionMultiplexer` — Singleton
5. Auth service — `ICognitoAuthService` → `CognitoAuthService` (Scoped)
6. Storage service — `IAvatarStorageService` → `S3AvatarStorageService` (Scoped)
7. Cache service — `IRoomCacheService` → `RoomCacheService` (Scoped)
8. Repositories — tất cả Scoped

---

## API Endpoints

### Auth
| Method | Path              | Auth | Mô tả                    |
|--------|-------------------|------|--------------------------|
| POST   | /api/auth/sign-in | ✗    | Đăng nhập, trả về tokens |

### Player
| Method | Path                      | Auth | Mô tả                         |
|--------|---------------------------|------|-------------------------------|
| POST   | /api/player/register      | ✗    | Tạo player mới sau Cognito signup |
| GET    | /api/player/me            | ✓    | Lấy thông tin player hiện tại |
| GET    | /api/player/{id}          | ✓    | Lấy thông tin player theo id  |
| GET    | /api/player/{id}/progress | ✓    | Lấy gold + inventory          |

### Room
| Method | Path                  | Auth | Mô tả                                    |
|--------|-----------------------|------|------------------------------------------|
| POST   | /api/room             | ✓    | Tạo phòng mới, host tự động vào phòng    |
| POST   | /api/room/join        | ✓    | Vào phòng bằng roomCode                  |
| PUT    | /api/room/{id}/ready  | ✓    | Đặt trạng thái sẵn sàng (`?isReady=true`)|
| POST   | /api/room/{id}/start  | ✓    | Host bắt đầu game (cần tất cả ready)     |
| GET    | /api/room/{id}        | ✓    | Lấy thông tin phòng                      |

### Match
| Method | Path               | Auth | Mô tả                                        |
|--------|--------------------|------|----------------------------------------------|
| POST   | /api/match/submit  | ✓    | Nộp kết quả trận, cộng gold cho player       |
| GET    | /api/match/{id}    | ✓    | Lấy kết quả trận theo id                     |

### Item
| Method | Path       | Auth | Mô tả                    |
|--------|------------|------|--------------------------|
| GET    | /api/item  | ✓    | Lấy danh sách item active|

---

## Luồng nghiệp vụ chính

### Đăng nhập
```
Client → POST /api/auth/sign-in
       → CognitoAuthService.SignInAsync (AdminInitiateAuth)
       → Trả về AccessToken, IdToken, RefreshToken
```

### Tạo và chơi game
```
1. POST /api/room              → Tạo phòng, sinh roomCode 6 ký tự, cache vào Redis
2. POST /api/room/join         → Player khác join bằng roomCode
3. PUT  /api/room/{id}/ready   → Từng player đặt isReady = true
4. POST /api/room/{id}/start   → Host start (validate tất cả non-host đã ready)
5. [Game diễn ra phía client]
6. POST /api/match/submit      → Nộp kết quả, cộng gold, Room.Status → Finished
```

### Submit kết quả
```
SubmitMatchResultCommandHandler:
  1. Validate room tồn tại và đang InGame
  2. Cộng EarnedGold vào Progress.Gold của từng player
  3. Tạo MatchResult với danh sách PlayerMatchResult
  4. Đổi Room.Status → Finished
  5. SaveChanges (MatchResult + Player gold)
```

---

## Packages chính

| Package                              | Version  | Dùng ở         | Mục đích                    |
|--------------------------------------|----------|----------------|-----------------------------|
| MediatR                              | 14.1.0   | Api, Application | CQRS pattern               |
| AWSSDK.CognitoIdentityProvider       | 4.0.6.2  | Infrastructure | Cognito auth                |
| AWSSDK.S3                            | 4.0.19   | Infrastructure | S3 storage                  |
| AWSSDK.Extensions.NETCore.Setup      | 4.0.3.26 | Infrastructure | AWS DI integration          |
| Npgsql.EntityFrameworkCore.PostgreSQL| 10.0.1   | Infrastructure | PostgreSQL provider         |
| StackExchange.Redis                  | 2.12.1   | Infrastructure | Redis client                |
| Microsoft.AspNetCore.Authentication.JwtBearer | 10.0.5 | Api    | JWT validation              |
| Swashbuckle.AspNetCore               | 6.2.2    | Api            | Swagger UI                  |
| AutoMapper                           | 16.1.1   | Api            | (có sẵn, chưa dùng)         |

---

## Cấu hình môi trường

Tất cả config nhạy cảm nên được đưa vào biến môi trường hoặc AWS Secrets Manager khi deploy production. Không commit `ClientSecret` lên git.

```
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=Host=rds-endpoint;...
Cognito__UserPoolId=...
Cognito__ClientId=...
Cognito__ClientSecret=...
Cache__Endpoint=elasticache-endpoint:6379
```
