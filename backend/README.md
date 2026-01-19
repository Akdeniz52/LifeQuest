# Leveling System API

Solo Leveling-inspired personal development system with gamification, quests, stats, and progression mechanics.

## 🚀 Quick Start

### Prerequisites
- .NET 9.0 SDK
- PostgreSQL 15+ (or Docker)

### PostgreSQL Setup

#### Option 1: Using Docker (Recommended)
```bash
docker run --name levelingsystem-postgres -e POSTGRES_PASSWORD=postgres -p 5432:5432 -d postgres:15
```

#### Option 2: Local Installation
1. Download and install PostgreSQL from https://www.postgresql.org/download/
2. Create a database named `levelingsystem`
3. Update connection string in `appsettings.json` if needed

### Running the API

1. **Clone and navigate to project**
   ```bash
   cd "C:\Users\akden\OneDrive\Masaüstü\leveling system"
   ```

2. **Apply database migrations**
   ```bash
   dotnet ef database update
   ```

3. **Run the application**
   ```bash
   dotnet run
   ```

4. **Access Swagger UI**
   - Open browser to: `https://localhost:5001` or `http://localhost:5000`

## 📚 API Endpoints

### Authentication
- `POST /api/auth/register` - Register new user and create character
- `POST /api/auth/login` - Login and get JWT token

### Health
- `GET /health` - API health check

## 🎮 System Features

### Sprint 1 (Current)
- ✅ User authentication with JWT
- ✅ Character creation with XP/Level system
- ✅ Stat definitions (Strength, Focus, Discipline, etc.)
- ✅ Database schema with PostgreSQL
- 🚧 Quest management
- 🚧 Quest completion/fail logic

### Sprint 2 (Planned)
- Mandatory quests & penalties
- Stat decay system
- System messages (Solo Leveling-style)
- Streak tracking
- Fatigue & dynamic difficulty

### Sprint 3 (Planned)
- Skill tree
- Quest chains & story arcs
- Analytics & heatmaps
- AI quest recommendations

## 🗄️ Database Schema

Key entities:
- **Users** - Authentication
- **Characters** - Player profile with XP/Level
- **StatDefinitions** - Configurable stat types
- **CharacterStats** - Individual stat values
- **QuestDefinitions** - Quest templates
- **QuestInstances** - Active/completed quests
- **ProgressLogs** - Event history
- **SystemMessages** - In-game notifications

## 🔧 Configuration

Edit `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=levelingsystem;Username=postgres;Password=YOUR_PASSWORD"
  },
  "JwtSettings": {
    "Secret": "your-secret-key-minimum-32-characters",
    "ExpirationMinutes": 1440
  }
}
```

## 📝 Development Notes

- **Data-driven design**: Stats, quests, and rules are stored in database, not hard-coded
- **Soft deletes**: Nothing is permanently deleted, use `IsActive` flags
- **Extensible**: Easy to add new stats, quest types, or game mechanics
- **API-first**: Same backend for web, mobile, and desktop clients

## 🎯 Next Steps

1. Complete quest management endpoints
2. Implement XP calculation engine
3. Add quest completion logic with stat updates
4. Create character profile endpoints
5. Set up background jobs for stat decay

---

**Status**: Sprint 1 in progress 🚧
