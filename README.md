# 🌍 Digital Heritage Preservation App - MVP Prototype

A full-stack ASP.NET Core application for preserving cultural heritage through community storytelling, artifact archiving, language preservation, and cultural event mapping.

## 📋 Features Implemented

### 1. **Community Storytelling** 📚
- Record oral histories, folk tales, and personal experiences
- Store audio/video metadata and transcriptions
- Tag stories by culture, language, and location
- View count tracking

### 2. **Interactive Artifact Archive** 🏛️
- Upload photos and metadata for artifacts
- Categorize artifacts (clothing, tools, art, etc.)
- Store historical context and materials information
- Geotagging support

### 3. **Language Preservation** 🗣️
- Dictionary entries with translations
- Pronunciation guides (IPA notation)
- Example sentences and usage context
- Dialect tracking
- Audio pronunciation support

### 4. **Cultural Event Mapping** 📅
- Create and track cultural festivals, ceremonies, and rituals
- Calendar with date/time management
- Recurring event support
- Geotagging for events
- Attendance tracking

## 🏗️ Architecture

### Backend: ASP.NET Core REST API
- **Database**: SQLite (lightweight, file-based)
- **ORM**: Entity Framework Core 8
- **Framework**: .NET 8

### Frontend: Vanilla HTML/CSS/JavaScript
- Responsive design (mobile-friendly)
- Real-time data fetching
- Interactive UI with sections for each feature

### Models:
- `User` - Community contributors
- `Story` - Oral histories and narratives
- `Artifact` - Physical cultural items
- `LanguageEntry` - Language dictionary entries
- `CulturalEvent` - Festivals, ceremonies, events

## 📂 Project Structure

```
digital heritage preservation app/
├── Models/
│   ├── User.cs
│   ├── Story.cs
│   ├── Artifact.cs
│   ├── LanguageEntry.cs
│   └── CulturalEvent.cs
├── Controllers/
│   ├── StoriesController.cs
│   ├── ArtifactsController.cs
│   ├── LanguagesController.cs
│   └── EventsController.cs
├── Data/
│   └── HeritageDbContext.cs
├── wwwroot/
│   ├── index.html
│   ├── css/
│   │   └── style.css
│   └── js/
│       └── app.js
├── Program.cs
├── appsettings.json
├── digital-heritage-api.csproj
└── heritage.db (created on first run)
```

## 🚀 Getting Started

### Prerequisites
- .NET 8.0 SDK or later
- Windows, macOS, or Linux
- A web browser

### Installation & Running

1. **Navigate to project directory:**
   ```bash
   cd "E:\Program Files\Microsoft Visual Studio\18\Repo\digital heritage preservation app"
   ```

2. **Restore dependencies:**
   ```bash
   dotnet restore digital-heritage-api.csproj
   ```

3. **Build the project:**
   ```bash
   dotnet build digital-heritage-api.csproj
   ```

4. **Run the application:**
   ```bash
   dotnet run --project digital-heritage-api.csproj
   ```

   The app will start on `http://localhost:5000` (or `https://localhost:5001`)

5. **Open in browser:**
   - Navigate to `http://localhost:5000`
   - You'll see the dashboard with all features

## 📡 API Endpoints

### Stories
- `GET /api/stories` - List all stories
- `GET /api/stories/{id}` - Get specific story
- `POST /api/stories` - Create new story
- `PUT /api/stories/{id}` - Update story
- `DELETE /api/stories/{id}` - Delete story

### Artifacts
- `GET /api/artifacts` - List all artifacts
- `GET /api/artifacts/{id}` - Get specific artifact
- `POST /api/artifacts` - Create new artifact
- `PUT /api/artifacts/{id}` - Update artifact
- `DELETE /api/artifacts/{id}` - Delete artifact

### Languages
- `GET /api/languages` - List all language entries
- `GET /api/languages/{id}` - Get specific entry
- `GET /api/languages/languages` - List unique languages
- `POST /api/languages` - Create new entry
- `PUT /api/languages/{id}` - Update entry
- `DELETE /api/languages/{id}` - Delete entry

### Cultural Events
- `GET /api/events` - List all events
- `GET /api/events/{id}` - Get specific event
- `GET /api/events/upcoming` - Get upcoming 10 events
- `POST /api/events` - Create new event
- `PUT /api/events/{id}` - Update event
- `DELETE /api/events/{id}` - Delete event

## 🎨 Frontend Features

- **Dashboard** - Overview with stats and quick actions
- **Stories Section** - Record and browse community stories
- **Artifacts Section** - Upload and archive cultural items
- **Languages Section** - Build language dictionaries
- **Events Section** - Manage cultural calendar
- **Responsive Design** - Works on desktop, tablet, and mobile
- **Real-time Updates** - Instant stat updates and data refresh

## 💾 Database

SQLite database (`heritage.db`) is created automatically on first run with:
- 5 tables (Users, Stories, Artifacts, LanguageEntries, CulturalEvents)
- Sample seed data for demonstration
- Full relationship mappings

## 🔧 Customization

### Add more fields to models:
Edit the model classes in `Models/` folder and add properties.

### Modify styling:
Edit `wwwroot/css/style.css` - uses CSS custom properties for easy theming.

### Add more API endpoints:
Create new controller classes in `Controllers/` folder following the same pattern.

## 📦 Dependencies

- `Microsoft.EntityFrameworkCore.Sqlite` - SQLite database
- `Microsoft.EntityFrameworkCore.Design` - EF Core tools
- `Microsoft.AspNetCore.OpenApi` - API documentation
- `Swashbuckle.AspNetCore` - Swagger/OpenAPI UI

## 🔮 Future Enhancements

- [ ] File upload support (audio, video, images)
- [ ] User authentication & authorization
- [ ] Advanced search and filtering
- [ ] Data export (CSV, PDF, JSON)
- [ ] Mobile app (React Native/Flutter)
- [ ] AI-powered transcription
- [ ] Blockchain for artifact authenticity
- [ ] Multi-language UI support
- [ ] Community moderation system
- [ ] Offline-first sync

## 📄 License

MIT License - Open source for community preservation efforts

## 💬 Contributing

Community contributions are welcome! This is a project by and for heritage communities.

---

Built with ❤️ for preserving traditions, stories, and artifacts 🌍
