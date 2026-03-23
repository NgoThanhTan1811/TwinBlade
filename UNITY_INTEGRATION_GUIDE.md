# Twin Blade Backend Integration Guide for Unity Mirror

> Complete guide for Unity developers to integrate Twin Blade multiplayer dungeon crawler backend with Unity Mirror networking.

## Table of Contents

1. [Architecture Overview](#architecture-overview)
2. [Authentication](#authentication)
3. [Session Management API](#session-management-api)
4. [Mirror Game Server Integration](#mirror-game-server-integration)
5. [Backend Integration Points](#backend-integration-points)
6. [Data Models](#data-models)
7. [Complete Game Flow](#complete-game-flow)
8. [Best Practices](#best-practices)

---

## Architecture Overview

### Hybrid Architecture: Backend Session Server + Mirror Game Server

```
┌─────────────────────────────────────────────────────────────────┐
│                     ARCHITECTURE OVERVIEW                        │
└─────────────────────────────────────────────────────────────────┘

Unity Client
    │
    ├──[Session Layer]──────────> Twin Blade Backend
    │                                 │
    │  • Authentication              ├─ PostgreSQL (RDS)
    │  • Matchmaking                 │  └─ Players, Rooms, Match Results
    │  • Room Management             │
    │  • Boss Key Validation         └─ Redis (ElastiCache)
    │  • Match Result Persistence       └─ Minimal Session State
    │
    └──[Gameplay Layer]─────────> Mirror Game Server (Peer-to-Peer / Host)
                                      │
       • HP, Damage, Combat          ├─ SyncVars, NetworkBehaviours
       • Movement, Position          ├─ Commands, RPCs
       • Item Drops, Inventory       ├─ SyncDictionary
       • Revive Mechanics            └─ Server Authority
       • Enemy AI
```

### Responsibility Matrix

| Feature | Twin Blade Backend | Unity Mirror |
|---------|-------------------|--------------|
| **Authentication** | ✅ JWT, AWS Cognito | ❌ |
| **Matchmaking** | ✅ Create/Join/Ready/Start Room | ❌ |
| **Room Session** | ✅ Track room state (Waiting/InGame/Finished) | ❌ |
| **Player HP** | ❌ | ✅ SyncVar |
| **Combat** | ❌ | ✅ Commands/RPCs |
| **Movement** | ❌ | ✅ NetworkTransform |
| **Item Drops** | ❌ | ✅ Server spawns items |
| **Inventory** | ❌ Runtime<br>✅ Persistence at match end | ✅ SyncDictionary<br>❌ |
| **Revive Cards** | ❌ | ✅ SyncVar |
| **Revive Action** | ❌ | ✅ Commands |
| **Boss Keys** | ✅ Validate & consume (persistent) | ✅ Runtime flag |
| **Match Results** | ✅ Save to PostgreSQL | ✅ Send results |

### Key Principles

1. **Backend = Stateless Session Server**
   - No real-time gameplay state
   - Only tracks: RoomId, RoomCode, Status, PlayerIds, DisplayNames
   - Validates persistent resources (boss keys, player progress)

2. **Mirror = Authoritative Game Server**
   - All gameplay happens in Mirror (HP, combat, items, movement)
   - One player acts as host (or dedicated Mirror server)
   - SyncVars automatically replicate state to all clients

3. **Integration Points**
   - Backend validates session (is room InGame?)
   - Backend validates boss key consumption (persistent check)
   - Backend persists match results (inventory merge, gold award)

---

## Authentication

### 1. Sign In

**Endpoint**: `POST /api/auth/sign-in`

**Request**:
```json
{
  "username": "player123",
  "password": "SecurePass123!"
}
```

**Response** (200 OK):
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "idToken": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "eyJhbGciOiJIUzI1NiIs..."
}
```

**Unity Example**:
```csharp
public class AuthManager : MonoBehaviour
{
    private const string API_BASE = "https://your-api-domain.com";
    private string _accessToken;
    private Guid _playerId;

    public async Task<bool> SignIn(string username, string password)
    {
        var request = new { Username = username, Password = password };
        var json = JsonUtility.ToJson(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        using var client = new HttpClient();
        var response = await client.PostAsync($"{API_BASE}/api/auth/sign-in", content);

        if (!response.IsSuccessStatusCode)
            return false;

        var result = await response.Content.ReadAsStringAsync();
        var authResult = JsonUtility.FromJson<AuthResult>(result);

        _accessToken = authResult.AccessToken;
        _playerId = ExtractPlayerIdFromToken(_accessToken);

        PlayerPrefs.SetString("AccessToken", _accessToken);
        return true;
    }

    public string GetAuthHeader() => $"Bearer {_accessToken}";
    public Guid GetPlayerId() => _playerId;

    private Guid ExtractPlayerIdFromToken(string token)
    {
        // Decode JWT and extract "sub" claim
        var payload = token.Split('.')[1];
        var json = Encoding.UTF8.GetString(Convert.FromBase64String(payload));
        var claims = JsonUtility.FromJson<TokenClaims>(json);
        return Guid.Parse(claims.sub);
    }
}

[Serializable]
public class AuthResult
{
    public string AccessToken;
    public string IdToken;
    public string RefreshToken;
}

[Serializable]
public class TokenClaims
{
    public string sub; // Player ID
}
```

---

## Session Management API

### Room Management

#### Create Room
**POST** `/api/room`

**Headers**: `Authorization: Bearer {token}`

**Request**:
```json
{
  "maxPlayers": 4
}
```

**Response** (201 Created):
```json
{
  "id": "room-guid",
  "roomCode": "ABC123",
  "hostPlayerId": "player-guid",
  "status": "Waiting",
  "maxPlayers": 4,
  "players": [],
  "createdAt": "2026-03-23T10:00:00Z"
}
```

#### Join Room
**POST** `/api/room/join`

**Request**:
```json
{
  "roomCode": "ABC123"
}
```

**Response** (200 OK): Room object with current players

#### Set Ready Status
**PUT** `/api/room/{roomId}/ready?isReady=true`

**Response** (200 OK): Room object with updated ready status

#### Start Game (Host Only)
**POST** `/api/room/{roomId}/start`

**Requirements**:
- Caller must be host
- All players must be ready
- Minimum 2 players

**Response** (200 OK): Room object with status = `InGame`

**What happens**:
1. Backend sets room status to `InGame`
2. Backend sends SignalR `GameStarted` event to all room members
3. Clients receive event and connect to Mirror host

---

### Boss Progression

#### Activate Boss Map
**POST** `/api/game/room/{roomId}/boss-map/activate`

**Purpose**: Validate and consume 3 boss keys from persistent storage.

**Requirements**:
- Player must have 3+ boss keys in `PlayerProgress.HasBossCrard` (persistent)
- Boss map not already activated in room

**Response** (200 OK):
```json
{
  "roomId": "room-guid",
  "bossMapActivated": true,
  "remainingBossKeys": 0
}
```

**Note**: This is a **backend validation** because boss keys are persistent resource. Mirror handles runtime `bossMapActivated` flag.

---

### Match Completion

#### Submit Match Result
**POST** `/api/match/submit`

**Purpose**: Persist match results, merge inventories, award gold.

**Request**:
```json
{
  "roomId": "room-guid",
  "players": [
    {
      "playerId": "player-guid",
      "score": 1500,
      "earnedGold": 250,
      "pickedItems": [
        {
          "itemId": "item-guid",
          "quantity": 3
        }
      ]
    }
  ]
}
```

**Response** (200 OK):
```json
{
  "id": "match-result-guid",
  "roomId": "room-guid",
  "finishedAt": "2026-03-23T10:30:00Z",
  "players": [...]
}
```

**What happens**:
1. Awards gold to each player (persistent storage)
2. **Merges runtime inventory** (from Mirror) into `PlayerProgress.Inventory` (PostgreSQL)
   - Stacks duplicate items
   - Creates new entries for new items
3. Creates `MatchResult` record
4. Sets room status to `Finished`
5. Cleans up Redis session state

---

## Mirror Game Server Integration

### NetworkManager Setup

```csharp
using Mirror;
using UnityEngine;

public class TwinBladeNetworkManager : NetworkManager
{
    [Header("Backend Integration")]
    public string backendApiUrl = "https://your-api-domain.com";

    private string _jwtToken;
    private Guid _roomId;
    private Guid _playerId;

    public void StartGameSession(Guid roomId, Guid playerId, string jwtToken, bool isHost)
    {
        _roomId = roomId;
        _playerId = playerId;
        _jwtToken = jwtToken;

        if (isHost)
        {
            StartHost();
            Debug.Log($"Started Mirror host for room {roomId}");
        }
        else
        {
            networkAddress = GetHostAddressFromBackend(); // Or P2P discovery
            StartClient();
            Debug.Log($"Connecting to Mirror host...");
        }
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        var playerObj = Instantiate(playerPrefab);
        var player = playerObj.GetComponent<PlayerController>();

        // Initialize player (could fetch base stats from backend if needed)
        player.Initialize(_playerId);

        NetworkServer.AddPlayerForConnection(conn, playerObj);
    }

    public async void EndGameSession(List<PlayerMatchResult> results)
    {
        // Send results to backend
        await SubmitMatchResultsToBackend(results);

        // Stop Mirror
        if (NetworkServer.active)
            StopHost();
        else
            StopClient();

        // Return to lobby
        SceneManager.LoadScene("LobbyScene");
    }

    private async Task SubmitMatchResultsToBackend(List<PlayerMatchResult> results)
    {
        var request = new SubmitMatchResultRequest
        {
            RoomId = _roomId,
            Players = results.Select(r => new PlayerMatchResultRequest
            {
                PlayerId = r.PlayerId,
                Score = r.Score,
                EarnedGold = r.GoldEarned,
                PickedItems = r.InventoryItems
            }).ToList()
        };

        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_jwtToken}");

        var json = JsonUtility.ToJson(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync($"{backendApiUrl}/api/match/submit", content);

        if (response.IsSuccessStatusCode)
            Debug.Log("Match results saved to backend!");
        else
            Debug.LogError($"Failed to save results: {response.StatusCode}");
    }
}
```

### Player Controller (Mirror)

```csharp
using Mirror;
using UnityEngine;
using System.Collections.Generic;

public class PlayerController : NetworkBehaviour
{
    [Header("Identity")]
    [SyncVar] private Guid playerId;
    [SyncVar] private string displayName;

    [Header("Stats (Mirror Authority)")]
    [SyncVar(hook = nameof(OnHpChanged))]
    private int currentHp = 100;

    [SyncVar] private int maxHp = 100;
    [SyncVar] private int attackPower = 15;
    [SyncVar] private int defense = 5;

    [SyncVar(hook = nameof(OnAliveChanged))]
    private bool isAlive = true;

    [SyncVar(hook = nameof(OnReviveCountChanged))]
    private int reviveCardsCount = 0;

    [Header("Inventory (Mirror Authority)")]
    private readonly SyncDictionary<string, int> inventory = new(); // itemId -> quantity

    public void Initialize(Guid id)
    {
        playerId = id;
        displayName = $"Player_{id.ToString().Substring(0, 8)}";
    }

    // ==================== COMBAT ====================

    [Command]
    public void CmdAttackEnemy(NetworkIdentity targetEnemy)
    {
        if (!isServer || !isAlive) return;

        var enemy = targetEnemy.GetComponent<EnemyController>();
        if (enemy == null || !enemy.IsAlive) return;

        // SERVER CALCULATES DAMAGE
        int baseDamage = Mathf.Max(1, attackPower - enemy.Defense);
        bool isCritical = Random.Range(0f, 1f) < 0.15f;
        int finalDamage = isCritical ? baseDamage * 2 : baseDamage;

        // Apply damage
        enemy.TakeDamage(finalDamage);

        // Show VFX to all clients
        RpcShowDamageEffect(targetEnemy.gameObject, finalDamage, isCritical);
    }

    [ClientRpc]
    private void RpcShowDamageEffect(GameObject target, int damage, bool isCritical)
    {
        DamagePopup.Show(target.transform.position, damage, isCritical);
    }

    [Server]
    public void TakeDamage(int damage)
    {
        if (!isAlive) return;

        currentHp = Mathf.Max(0, currentHp - damage);

        if (currentHp == 0)
        {
            isAlive = false;
            RpcPlayDeathAnimation();
        }
    }

    [ClientRpc]
    private void RpcPlayDeathAnimation()
    {
        // Play death animation
        GetComponent<Animator>().SetTrigger("Death");
    }

    // ==================== ITEMS ====================

    [Command]
    public void CmdPickupItem(NetworkIdentity itemObj)
    {
        if (!isServer || !isAlive) return;

        var item = itemObj.GetComponent<ItemPickup>();
        if (item == null) return;

        string itemId = item.ItemId;

        // Check inventory capacity (50 unique items)
        if (!inventory.ContainsKey(itemId) && inventory.Count >= 50)
        {
            TargetShowError(connectionToClient, "Inventory is full!");
            return;
        }

        // Add to inventory (SyncDictionary auto-syncs)
        if (inventory.ContainsKey(itemId))
            inventory[itemId]++;
        else
            inventory.Add(itemId, 1);

        // Destroy item in world
        NetworkServer.Destroy(itemObj.gameObject);

        // Show notification
        RpcShowItemPickup(itemId, inventory[itemId]);
    }

    [TargetRpc]
    private void TargetShowError(NetworkConnection conn, string message)
    {
        UIManager.Instance.ShowToast(message, Color.red);
    }

    [ClientRpc]
    private void RpcShowItemPickup(string itemId, int newQuantity)
    {
        ItemDatabase.TryGetItem(itemId, out var itemData);
        UIManager.Instance.ShowToast($"Picked up {itemData.Name} x{newQuantity}", Color.green);
        InventoryUI.Instance.RefreshInventory();
    }

    // ==================== REVIVE ====================

    [Command]
    public void CmdPickupReviveCard()
    {
        if (!isServer || !isAlive) return;
        reviveCardsCount++;
    }

    [Command]
    public void CmdRevivePlayer(NetworkIdentity targetPlayerIdentity)
    {
        if (!isServer || !isAlive || reviveCardsCount <= 0) return;

        var targetPlayer = targetPlayerIdentity.GetComponent<PlayerController>();
        if (targetPlayer == null || targetPlayer.isAlive) return;

        // Consume revive card
        reviveCardsCount--;

        // Revive target
        int restoredHp = targetPlayer.maxHp / 2;
        targetPlayer.Revive(restoredHp);

        // Show notification
        RpcShowReviveNotification(playerId, targetPlayer.playerId);
    }

    [Server]
    private void Revive(int hpToRestore)
    {
        currentHp = hpToRestore;
        isAlive = true;
        RpcPlayReviveAnimation();
    }

    [ClientRpc]
    private void RpcPlayReviveAnimation()
    {
        GetComponent<Animator>().SetTrigger("Revive");
        UIManager.Instance.ShowToast("You have been revived!", Color.cyan);
    }

    [ClientRpc]
    private void RpcShowReviveNotification(Guid reviverId, Guid revivedId)
    {
        string message = reviverId == playerId
            ? $"You revived {revivedId}!"
            : $"{reviverId} revived {revivedId}";

        UIManager.Instance.ShowToast(message, Color.yellow);
    }

    // ==================== HOOKS ====================

    private void OnHpChanged(int oldValue, int newValue)
    {
        if (!hasAuthority) return;
        HealthBarUI.Instance.UpdateHealth(newValue, maxHp);
    }

    private void OnAliveChanged(bool oldValue, bool newValue)
    {
        if (!hasAuthority) return;

        if (!newValue)
            DeathScreenUI.Instance.Show();
        else
            DeathScreenUI.Instance.Hide();
    }

    private void OnReviveCountChanged(int oldValue, int newValue)
    {
        if (!hasAuthority) return;
        ReviveCardsUI.Instance.SetCount(newValue);
    }

    // ==================== MATCH END ====================

    public PlayerMatchResult GetMatchResult()
    {
        return new PlayerMatchResult
        {
            PlayerId = playerId,
            Score = CalculateScore(),
            GoldEarned = CalculateGoldEarned(),
            InventoryItems = GetInventoryItems()
        };
    }

    private List<RuntimePlayerItemDto> GetInventoryItems()
    {
        var items = new List<RuntimePlayerItemDto>();
        foreach (var kvp in inventory)
        {
            items.Add(new RuntimePlayerItemDto
            {
                ItemId = kvp.Key,
                Quantity = kvp.Value
            });
        }
        return items;
    }

    private int CalculateScore()
    {
        // Your scoring logic (kills, survival time, etc.)
        return 1000;
    }

    private int CalculateGoldEarned()
    {
        // Your gold calculation
        return 250;
    }
}
```

### Enemy Controller (Mirror)

```csharp
using Mirror;
using UnityEngine;

public class EnemyController : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnHpChanged))]
    private int currentHp = 50;

    [SerializeField] private int maxHp = 50;
    [SerializeField] private int defense = 3;

    [Header("Loot Table")]
    [SerializeField] private LootTableSO lootTable;
    [SerializeField] private GameObject itemPickupPrefab;

    public int Defense => defense;
    public bool IsAlive => currentHp > 0;

    [Server]
    public void TakeDamage(int damage)
    {
        if (currentHp <= 0) return;

        currentHp = Mathf.Max(0, currentHp - damage);

        if (currentHp == 0)
            OnDeath();
    }

    [Server]
    private void OnDeath()
    {
        // SERVER ROLLS LOOT (Mirror authority)
        var lootRoll = Random.Range(0f, 100f);
        var droppedItem = lootTable.RollLoot(lootRoll);

        if (droppedItem != null)
        {
            // Spawn item in world
            var itemObj = Instantiate(itemPickupPrefab, transform.position, Quaternion.identity);
            var itemPickup = itemObj.GetComponent<ItemPickup>();
            itemPickup.ItemId = droppedItem.Id;

            NetworkServer.Spawn(itemObj);
        }

        // Destroy enemy
        NetworkServer.Destroy(gameObject);
    }

    private void OnHpChanged(int oldValue, int newValue)
    {
        // Update health bar UI
        transform.Find("HealthBar").GetComponent<EnemyHealthBar>().SetHealth(newValue, maxHp);
    }
}
```

### Item Pickup (Mirror)

```csharp
using Mirror;
using UnityEngine;

public class ItemPickup : NetworkBehaviour
{
    [SyncVar] public string ItemId;

    private void OnTriggerEnter(Collider other)
    {
        if (!isServer) return;

        var player = other.GetComponent<PlayerController>();
        if (player != null && player.isLocalPlayer)
        {
            player.CmdPickupItem(netIdentity);
        }
    }
}
```

---

## Backend Integration Points

### 1. Lobby SignalR (Backend)

```csharp
using Microsoft.AspNetCore.SignalR.Client;

public class LobbyManager : MonoBehaviour
{
    private HubConnection _connection;
    private TwinBladeNetworkManager _networkManager;

    private async void Start()
    {
        _networkManager = FindObjectOfType<TwinBladeNetworkManager>();

        // Connect to backend SignalR
        _connection = new HubConnectionBuilder()
            .WithUrl("wss://your-api.com/hubs/game", options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(GetJwtToken());
            })
            .WithAutomaticReconnect()
            .Build();

        RegisterEvents();
        await _connection.StartAsync();
    }

    private void RegisterEvents()
    {
        // Backend notifies when game starts
        _connection.On<GameStartedPayload>("GameStarted", payload =>
        {
            Debug.Log($"Game started! Room: {payload.RoomId}");

            // Determine if this player is host
            bool isHost = DetermineIfHost(payload.RoomId);

            // Start Mirror session
            _networkManager.StartGameSession(
                Guid.Parse(payload.RoomId),
                GetPlayerId(),
                GetJwtToken(),
                isHost
            );

            // Load game scene
            SceneManager.LoadScene("GameplayScene");
        });

        _connection.On<PlayerJoinedPayload>("PlayerJoined", payload =>
        {
            Debug.Log($"Player joined: {payload.DisplayName}");
            LobbyUI.Instance.AddPlayer(payload.PlayerId, payload.DisplayName);
        });

        _connection.On<PlayerLeftPayload>("PlayerLeft", payload =>
        {
            Debug.Log($"Player left: {payload.PlayerId}");
            LobbyUI.Instance.RemovePlayer(payload.PlayerId);
        });
    }

    public async Task JoinRoomGroupAsync(string roomId)
    {
        await _connection.InvokeAsync("JoinRoom", roomId);
    }

    private bool DetermineIfHost(string roomId)
    {
        // Logic to determine if this player should be Mirror host
        // Option 1: First player (host in backend room) becomes Mirror host
        // Option 2: Dedicated server
        // Option 3: Use room.HostPlayerId from backend
        return GetPlayerId() == GetRoomHostId(roomId);
    }
}

[Serializable]
public class GameStartedPayload { public string RoomId; }

[Serializable]
public class PlayerJoinedPayload { public string PlayerId; public string DisplayName; }

[Serializable]
public class PlayerLeftPayload { public string PlayerId; }
```

### 2. Boss Map Activation (Backend Validation)

```csharp
public class BossMapController : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnBossMapActivated))]
    private bool bossMapActivated = false;

    [Command(requiresAuthority = false)]
    public async void CmdActivateBossMap(NetworkConnectionToClient sender = null)
    {
        if (!isServer || bossMapActivated) return;

        var player = sender.identity.GetComponent<PlayerController>();

        // Call backend to validate and consume boss keys
        bool success = await ValidateBossKeysWithBackend(player.PlayerId);

        if (success)
        {
            bossMapActivated = true;
            RpcShowBossMapActivated();
        }
        else
        {
            TargetShowError(sender, "You don't have enough boss keys!");
        }
    }

    private async Task<bool> ValidateBossKeysWithBackend(Guid playerId)
    {
        try
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", GetJwtToken());

            var response = await client.PostAsync(
                $"{API_BASE}/api/game/room/{GetRoomId()}/boss-map/activate",
                null
            );

            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    [ClientRpc]
    private void RpcShowBossMapActivated()
    {
        UIManager.Instance.ShowToast("Boss Map Activated!", Color.red);
        // Spawn boss, change music, etc.
    }

    [TargetRpc]
    private void TargetShowError(NetworkConnection conn, string message)
    {
        UIManager.Instance.ShowToast(message, Color.red);
    }

    private void OnBossMapActivated(bool oldValue, bool newValue)
    {
        if (newValue)
        {
            // Visual changes for boss map
            BossMapPortal.SetActive(true);
        }
    }
}
```

---

## Data Models

### Backend DTOs

```csharp
[Serializable]
public class SubmitMatchResultRequest
{
    public string RoomId;
    public List<PlayerMatchResultRequest> Players;
}

[Serializable]
public class PlayerMatchResultRequest
{
    public string PlayerId;
    public int Score;
    public int EarnedGold;
    public List<RuntimePlayerItemDto> PickedItems;
}

[Serializable]
public class RuntimePlayerItemDto
{
    public string ItemId;
    public int Quantity;
}
```

### Room Status Enum

```csharp
public enum RoomStatus
{
    Waiting = 0,    // Lobby, waiting for players
    InGame = 1,     // Game in progress (Mirror active)
    Finished = 2    // Game completed
}
```

---

## Complete Game Flow

```
┌──────────────────────────────────────────────────────────────────┐
│                        COMPLETE GAME FLOW                         │
└──────────────────────────────────────────────────────────────────┘

=== PHASE 1: LOBBY (Backend Session) ===

Client A (Host)                     Backend                     Client B & C
     |                                 |                              |
     |-- POST /api/auth/sign-in ------>|                              |
     |<-- 200 OK (JWT) ----------------|                              |
     |                                 |                              |
     |-- POST /api/room (create) ----->|                              |
     |<-- 201 (RoomCode: ABC123) ------|                              |
     |                                 |                              |
     |-- Connect SignalR ------------->|                              |
     |-- InvokeAsync("JoinRoom") ----->|                              |
     |                                 |                              |
     |                                 |      <-- POST /api/room/join|
     |                                 |      --> 200 OK (Room) ---->|
     |                                 |                              |
     |<-- PlayerJoined (SignalR) ------|<-- PlayerJoined (SignalR) --|
     |                                 |                              |
     |-- PUT /room/{id}/ready -------->|                              |
     |                                 |      <-- PUT /ready ---------|
     |                                 |                              |
     |-- POST /room/{id}/start ------->|                              |
     |                                 |-- Set Status = InGame        |
     |<-- GameStarted (SignalR) -------|<-- GameStarted (SignalR) ---|
     |                                 |                              |

=== PHASE 2: GAMEPLAY (Mirror Authority) ===

Host (Mirror Server)                                           Clients
     |                                                              |
     |-- StartHost() (Mirror)                                      |
     |                                                              |
     |                               <-- StartClient() (Mirror) ---|
     |                                                              |
     |-- Spawn Players, Enemies, Items (NetworkServer.Spawn) ----->|
     |                                                              |
     [Player attacks enemy]                                        |
     |<-- CmdAttackEnemy(enemy) ------------------------------------|
     |-- Calculate damage (Mirror Server)                          |
     |-- enemy.TakeDamage(35)                                      |
     |-- RpcShowDamageEffect() ------------------------------------>|
     |                                                              |
     [Enemy dies, drops item]                                      |
     |-- lootTable.RollLoot() (Mirror Server)                      |
     |-- NetworkServer.Spawn(itemPickup) ------------------------->|
     |                                                              |
     [Player picks up item]                                        |
     |<-- CmdPickupItem(itemObj) -----------------------------------|
     |-- inventory[itemId]++ (SyncDictionary auto-syncs)           |
     |-- NetworkServer.Destroy(itemObj) --------------------------->|
     |-- RpcShowItemPickup() -------------------------------------->|
     |                                                              |
     [Player dies]                                                 |
     |-- TakeDamage(100) (HP = 0)                                  |
     |-- isAlive = false (SyncVar) -------------------------------->|
     |-- RpcPlayDeathAnimation() ---------------------------------->|
     |                                                              |
     [Teammate revives]                                            |
     |<-- CmdRevivePlayer(deadPlayer) ------------------------------|
     |-- reviveCardsCount-- (SyncVar)                              |
     |-- deadPlayer.Revive(50) ------------------------------------->|
     |-- RpcShowReviveNotification() ----------------------------->|
     |                                                              |
     [Boss key activation]                                         |
     |<-- CmdActivateBossMap() -------------------------------------|
     |-- ValidateBossKeysWithBackend() -------> Backend            |
     |                                           |-- Check PostgreSQL
     |                                           |-- Consume 3 keys
     |<-- 200 OK --------------------------------|                  |
     |-- bossMapActivated = true (SyncVar) ----------------------->|
     |-- RpcShowBossMapActivated() ------------------------------>|
     |                                                              |
     [Boss defeated, match end]                                    |
     |-- GameManager.OnGameComplete()                              |
     |-- Collect all player results (Mirror local)                 |
     |                                                              |

=== PHASE 3: MATCH END (Backend Persistence) ===

Host (Mirror Server)                     Backend                 Clients
     |                                      |                        |
     |-- POST /api/match/submit ----------->|                        |
     |    {                                 |                        |
     |      roomId,                         |                        |
     |      players: [{                     |                        |
     |        playerId, score, gold,        |                        |
     |        pickedItems: [...]            |                        |
     |      }]                              |                        |
     |    }                                 |                        |
     |                                      |-- Merge inventory      |
     |                                      |   (PostgreSQL)         |
     |                                      |-- Award gold           |
     |                                      |-- Create MatchResult   |
     |                                      |-- Status = Finished    |
     |                                      |-- Clean Redis          |
     |<-- 200 OK (MatchResult) -------------|                        |
     |                                      |                        |
     |-- RpcReturnToLobby() ---------------------------------------->|
     |-- StopHost() / StopClient()                                  |
     |                                                              |
     [All clients return to lobby scene]                            |
```

---

## Best Practices

### 1. Separation of Concerns

**Backend handles**:
- ✅ Authentication tokens
- ✅ Room session lifecycle
- ✅ Persistent resource validation (boss keys)
- ✅ Match result persistence
- ✅ Player progression (gold, inventory merge)

**Mirror handles**:
- ✅ ALL real-time gameplay (HP, damage, combat)
- ✅ Movement and position
- ✅ Item drops and inventory (runtime)
- ✅ Revive mechanics (runtime)
- ✅ Enemy AI and spawning

**Never**:
- ❌ Don't send HP updates to backend during gameplay
- ❌ Don't validate every item pickup with backend
- ❌ Don't sync Mirror state to backend Redis in real-time

### 2. Mirror Authority

```csharp
// ✅ GOOD: Server calculates damage
[Command]
public void CmdAttack(NetworkIdentity target)
{
    if (!isServer) return; // Server authority

    int damage = CalculateDamage(); // SERVER calculates
    target.GetComponent<IDamageable>().TakeDamage(damage);
}

// ❌ BAD: Client calculates damage
[Command]
public void CmdAttack(NetworkIdentity target, int damage)
{
    // Client could send fake damage value!
    target.GetComponent<IDamageable>().TakeDamage(damage);
}
```

### 3. Inventory Management

```csharp
// Runtime inventory in Mirror
private readonly SyncDictionary<string, int> _inventory = new();

// Only send to backend at match end
public List<RuntimePlayerItemDto> GetInventoryForBackend()
{
    return _inventory.Select(kvp => new RuntimePlayerItemDto
    {
        ItemId = kvp.Key,
        Quantity = kvp.Value
    }).ToList();
}

// Backend merges into persistent storage
await SubmitMatchResult(new SubmitMatchResultRequest
{
    Players = players.Select(p => new PlayerMatchResultRequest
    {
        PickedItems = p.GetInventoryForBackend() // Send once at end
    }).ToList()
});
```

### 4. Error Handling

```csharp
// Backend API calls
try
{
    var response = await client.PostAsync(backendUrl, content);

    if (!response.IsSuccessStatusCode)
    {
        var error = await response.Content.ReadAsStringAsync();
        Debug.LogError($"Backend error: {error}");
        ShowErrorToUser("Failed to save results. Please try again.");
    }
}
catch (HttpRequestException ex)
{
    Debug.LogError($"Network error: {ex.Message}");
    ShowErrorToUser("Connection to backend lost.");
}

// Mirror Commands
[Command]
public void CmdAttack(NetworkIdentity target)
{
    if (!isServer || !isAlive) return; // Validation

    var enemy = target.GetComponent<EnemyController>();
    if (enemy == null || !enemy.IsAlive) return; // Validation

    // Safe to proceed
    DealDamage(enemy);
}
```

### 5. Security

**Backend validates**:
- ✅ JWT tokens on all API calls
- ✅ Player owns the room they're trying to start
- ✅ Player has enough boss keys (persistent check)
- ✅ Room is in correct state (InGame before match submit)

**Mirror validates**:
- ✅ `[Command]` methods check `isServer`
- ✅ Range checks (is target in attack range?)
- ✅ State checks (is player alive? is target alive?)
- ✅ Server calculates all gameplay values (damage, loot drops)

### 6. Performance

**Minimize backend calls during gameplay**:
```csharp
// ❌ BAD: Call backend on every item pickup
[Command]
public async void CmdPickupItem(NetworkIdentity item)
{
    await CallBackendToValidateItem(item.ItemId); // SLOW!
    AddToInventory(item.ItemId);
}

// ✅ GOOD: Mirror handles immediately, backend persists at end
[Command]
public void CmdPickupItem(NetworkIdentity item)
{
    if (!isServer) return;

    // Instant - no network roundtrip
    inventory[item.ItemId]++; // SyncDictionary auto-syncs
    NetworkServer.Destroy(item.gameObject);
}

// Backend only called once at match end
await SubmitMatchResults(); // Single call with all items
```

### 7. State Synchronization

```csharp
// Mirror automatically syncs with SyncVars
[SyncVar(hook = nameof(OnHpChanged))]
private int currentHp = 100;

// Hook updates UI on all clients automatically
private void OnHpChanged(int oldValue, int newValue)
{
    if (!hasAuthority) return; // Only update local player's UI
    HealthBarUI.Instance.SetHealth(newValue, maxHp);
}

// No need for manual SignalR events during gameplay!
```

---

## Troubleshooting

### Common Issues

**1. "401 Unauthorized" on backend API calls**
- Check JWT token is valid and not expired
- Verify `Authorization: Bearer {token}` header format
- Check token scopes/claims match required permissions

**2. Mirror clients can't connect to host**
- Verify networkAddress is correct
- Check firewall/NAT settings (use STUN/TURN for P2P)
- Ensure host called `StartHost()` before clients `StartClient()`

**3. "Room not found" when submitting match results**
- Verify room status is `InGame` (not `Finished` already)
- Check roomId matches the one from backend
- Ensure backend session hasn't expired

**4. Inventory items not saved to backend**
- Check `PickedItems` array in match submit request
- Verify ItemIds match items in backend database
- Check backend logs for merge errors

**5. Boss map activation fails**
- Verify player has 3+ boss keys in persistent storage
- Check backend endpoint returns 200 OK
- Ensure `bossMapActivated` SyncVar updates correctly

**6. Mirror SyncVars not updating**
- Ensure only server modifies SyncVars (not clients)
- Check hook methods are registered correctly
- Verify NetworkIdentity is on GameObject

---

## Support & Resources

**Backend**:
- API Base URL: `https://your-api-domain.com`
- Swagger Docs: `/swagger`

**Mirror**:
- Documentation: https://mirror-networking.com/
- Discord: https://discord.gg/N9QVxbM

**Architecture**:
- Backend = Session server (matchmaking, persistence)
- Mirror = Game server (real-time gameplay)
- Integration = Match start/end only

For additional help, contact the backend team or check API documentation.

---

**Last Updated**: 2026-03-23
**API Version**: 1.0
**Architecture**: Backend Session Server + Mirror Game Server
**Compatible Unity**: 2021.3 LTS or higher
**Compatible Mirror**: 80.0.0 or higher
