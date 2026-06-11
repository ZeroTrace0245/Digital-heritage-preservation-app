# 🚀 Quick Start Guide

## One-Minute Setup

### Step 1: Open Command Prompt or PowerShell
```
cd "E:\Program Files\Microsoft Visual Studio\18\Repo\digital heritage preservation app"
```

### Step 2: Run the Application
```
dotnet run --project digital-heritage-api.csproj
```

### Step 3: Open Browser
Go to: **http://localhost:5000**

---

## What You'll See

### 📊 Dashboard
- Real-time stats showing total stories, artifacts, languages, and events
- Quick action buttons to jump to any section

### 📚 Community Storytelling
Fill out the form to record stories:
- **Title**: Name of the story
- **Description**: What it's about
- **Culture/Community**: Which culture it belongs to
- **Language**: Language spoken
- **Location**: Where the story originates
- **Transcription**: Text version (optional)

Your stories will appear as cards below with view count!

### 🏛️ Artifact Archive
Upload cultural items:
- **Name**: Artifact name
- **Category**: Type (clothing, tools, art, etc.)
- **Culture**: Which culture it's from
- **Estimated Age**: How old is it?
- **Materials**: What's it made from?
- **Historical Context**: Its significance

### 🗣️ Language Preservation
Build language dictionaries:
- **Language**: Language name
- **Word**: The word
- **Translation**: What it means
- **IPA**: Pronunciation guide
- **Example**: Sentence using the word
- **Dialect**: Regional variation

### 📅 Events
Create cultural calendar entries:
- **Title**: Event name
- **Description**: What happens
- **Type**: Festival, ceremony, ritual, etc.
- **Date/Time**: When it happens
- **Recurring**: Does it repeat annually?
- **Location**: Where it takes place
- **Attendees**: Expected count

---

## Sample Test Data

When you start the app, it automatically creates a database with:
✅ 1 Sample Story: "The Legend of the Sacred River"
✅ 1 Sample Artifact: "Traditional Weaving Loom"
✅ 1 Sample Language Entry: "Mitakuye Oyasin" (Lakota)
✅ 1 Sample Event: "Spring Harvest Festival"

---

## Troubleshooting

**"dotnet: command not found"**
- Install .NET 8.0 from https://dotnet.microsoft.com/download

**"Port 5000 is already in use"**
- Run: `dotnet run --project digital-heritage-api.csproj --urls "http://localhost:5001"`

**"database is locked"**
- Close any other instances of the app
- Delete `heritage.db` to start fresh

**CORS errors in browser console**
- The API is configured to accept requests from any origin (development mode)
- In production, configure specific allowed origins

---

## Admin Credentials

Currently, all data is added by "Admin User" (ID: 1)

To add a new contributor, modify the `HeritageDbContext.cs` seed data.

---

## API Testing

### Using Swagger UI
The API includes Swagger documentation. After starting the app:
- Go to: **http://localhost:5000/swagger**

### Using curl
```bash
# Get all stories
curl http://localhost:5000/api/stories

# Create a story
curl -X POST http://localhost:5000/api/stories \
  -H "Content-Type: application/json" \
  -d '{"title":"My Story","description":"Story desc","contributorId":1}'

# Get upcoming events
curl http://localhost:5000/api/events/upcoming
```

---

## Database Location

SQLite database stored at:
```
E:\Program Files\Microsoft Visual Studio\18\Repo\digital heritage preservation app\heritage.db
```

It contains:
- Users table
- Stories table
- Artifacts table
- LanguageEntries table
- CulturalEvents table

---

## Next Steps After Setup

1. **Explore the Dashboard** - Click around all sections
2. **Add Sample Data** - Record a story, upload an artifact, add a language entry
3. **Check the API** - Open Swagger at `/swagger`
4. **Customize** - Edit colors in `wwwroot/css/style.css`
5. **Deploy** - Ready for Docker, Azure, AWS, or any .NET hosting

---

## Project Files to Know

| File | Purpose |
|------|---------|
| `Program.cs` | Main app configuration |
| `digital-heritage-api.csproj` | Project file with dependencies |
| `appsettings.json` | Configuration settings |
| `Data/HeritageDbContext.cs` | Database schema |
| `Models/*.cs` | Data models |
| `Controllers/*.cs` | API endpoints |
| `wwwroot/index.html` | Frontend UI |
| `wwwroot/js/app.js` | Frontend logic |
| `wwwroot/css/style.css` | Styling |

---

## Support

For issues:
1. Check the browser console (F12)
2. Check the terminal output where you ran `dotnet run`
3. Verify .NET 8.0 is installed: `dotnet --version`
4. Ensure port 5000 is not in use: `netstat -ano | findstr :5000`

---

Happy preserving! 🌍✨
