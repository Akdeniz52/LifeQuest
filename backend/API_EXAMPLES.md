# API Test Examples

Base URL: `http://localhost:5240` or `https://localhost:7240`

## 1. Register a New User

```http
POST /api/auth/register
Content-Type: application/json

{
  "email": "hunter@example.com",
  "password": "Test123!",
  "characterName": "Shadow Monarch"
}
```

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "email": "hunter@example.com",
  "characterId": "guid-here",
  "characterName": "Shadow Monarch",
  "level": 1,
  "currentXP": 0
}
```

## 2. Login

```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "hunter@example.com",
  "password": "Test123!"
}
```

## 3. Get Character Profile

```http
GET /api/character
Authorization: Bearer YOUR_TOKEN
```

**Response:**
```json
{
  "id": "guid",
  "name": "Shadow Monarch",
  "level": 1,
  "currentXP": 0,
  "totalXP": 0,
  "xpForNextLevel": 141,
  "stats": [
    {
      "id": "guid",
      "statName": "Strength",
      "category": "Physical",
      "currentValue": 0,
      "maxValue": 100,
      "lastUsedAt": null,
      "isLocked": false
    }
  ],
  "unreadMessages": 1
}
```

## 4. Get Detailed Stats

```http
GET /api/character/stats
Authorization: Bearer YOUR_TOKEN
```

## 5. Create a Quest

```http
POST /api/quests/definitions
Authorization: Bearer YOUR_TOKEN
Content-Type: application/json

{
  "title": "30 Minutes Coding",
  "description": "Write code for at least 30 minutes",
  "questType": "Daily",
  "isMandatory": true,
  "baseXP": 50,
  "difficultyMultiplier": 1.0,
  "statEffects": [
    {
      "statDefinitionId": "FOCUS_STAT_ID_FROM_GET_STATS",
      "effectMultiplier": 1.5
    },
    {
      "statDefinitionId": "DISCIPLINE_STAT_ID",
      "effectMultiplier": 1.0
    }
  ]
}
```

**Response:**
```json
{
  "id": "quest-guid",
  "title": "30 Minutes Coding",
  "description": "Write code for at least 30 minutes",
  "questType": "Daily",
  "isMandatory": true,
  "baseXP": 50,
  "difficultyMultiplier": 1.0,
  "isActive": true,
  "createdAt": "2026-01-18T13:00:00Z",
  "statEffects": [...]
}
```

## 6. Get All Quest Definitions

```http
GET /api/quests/definitions
Authorization: Bearer YOUR_TOKEN
```

## 7. Update a Quest

```http
PUT /api/quests/definitions/{questId}
Authorization: Bearer YOUR_TOKEN
Content-Type: application/json

{
  "title": "45 Minutes Coding",
  "baseXP": 75
}
```

## 8. Get Today's Quests

```http
GET /api/quests/today
Authorization: Bearer YOUR_TOKEN
```

## 9. Get All Active Quests

```http
GET /api/quests/active
Authorization: Bearer YOUR_TOKEN
```

## 10. Complete a Quest

```http
POST /api/quests/{questInstanceId}/complete
Authorization: Bearer YOUR_TOKEN
```

**Response:**
```json
{
  "success": true,
  "xpGained": 50,
  "leveledUp": false,
  "newLevel": null,
  "statChanges": [
    {
      "statName": "Focus",
      "oldValue": 0,
      "newValue": 7.5,
      "change": 7.5
    },
    {
      "statName": "Discipline",
      "oldValue": 0,
      "newValue": 5.0,
      "change": 5.0
    }
  ],
  "systemMessage": "Quest '30 Minutes Coding' completed!\n+50 XP"
}
```

## 11. Fail a Quest

```http
POST /api/quests/{questInstanceId}/fail
Authorization: Bearer YOUR_TOKEN
```

**Response:**
```json
{
  "success": true,
  "xpGained": 0,
  "leveledUp": false,
  "newLevel": null,
  "statChanges": [
    {
      "statName": "Discipline",
      "oldValue": 5.0,
      "newValue": 3.0,
      "change": -2.0
    }
  ],
  "systemMessage": "Quest '30 Minutes Coding' failed.\nDiscipline -2\n⚠️ Penalty quest will be assigned."
}
```

## 12. Soft Delete a Quest

```http
DELETE /api/quests/definitions/{questId}
Authorization: Bearer YOUR_TOKEN
```

## Notes

- All endpoints except `/api/auth/*` require JWT token in `Authorization: Bearer TOKEN` header
- Quest instances are created manually for now (Sprint 2 will add auto-assignment)
- To test quest completion, you need to manually create a QuestInstance in the database
- Stat IDs can be obtained from `GET /api/character/stats` endpoint

## Testing Workflow

1. Register → Get token
2. Get character profile → See default stats
3. Create quest definitions
4. Manually create quest instances (or wait for Sprint 2)
5. Complete/fail quests → See XP gain and stat changes
6. Check profile again → See updated level/XP/stats
