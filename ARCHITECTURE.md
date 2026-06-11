# Architecture Overview

## System Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                        USER BROWSER                             │
│  (Chrome, Firefox, Safari, Edge on Desktop/Mobile)             │
└────────────────────────┬────────────────────────────────────────┘
						 │
						 │ HTTP/HTTPS
						 │
┌────────────────────────▼────────────────────────────────────────┐
│                   FRONTEND LAYER                                │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │              wwwroot/index.html                          │  │
│  │  ┌─────────────┬──────────────┬─────────────────────┐   │  │
│  │  │ Dashboard   │ Stories      │ Artifacts           │   │  │
│  │  ├─────────────┼──────────────┼─────────────────────┤   │  │
│  │  │ Languages   │ Events       │ Navigation          │   │  │
│  │  └─────────────┴──────────────┴─────────────────────┘   │  │
│  │                                                          │  │
│  │  wwwroot/css/style.css  wwwroot/js/app.js            │  │
│  │  (Responsive Design)     (API Calls & DOM Updates)   │  │
│  └──────────────────────────────────────────────────────────┘  │
└────────────────┬─────────────────────────────────────────────────┘
				 │
				 │ REST API (JSON)
				 │ http://localhost:5000/api/*
				 │
┌────────────────▼─────────────────────────────────────────────────┐
│              ASP.NET CORE 8.0 API LAYER                         │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │                    Program.cs                            │  │
│  │  • Configuration & Dependency Injection                 │  │
│  │  • CORS, Authentication, Static Files                  │  │
│  │  • Database Context Setup                              │  │
│  └──────────────────────────────────────────────────────────┘  │
│                                                                  │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │              CONTROLLER LAYER                           │  │
│  │  ┌──────────────┬──────────────┬─────────────────────┐  │  │
│  │  │ StoriesCtrl  │ ArtifactsCtrl│ LanguagesCtrl       │  │  │
│  │  ├──────────────┼──────────────┼─────────────────────┤  │  │
│  │  │ EventsCtrl   │              │                     │  │  │
│  │  └──────────────┴──────────────┴─────────────────────┘  │  │
│  │                                                          │  │
│  │  • Validate inputs                                      │  │
│  │  • Query database via DbContext                        │  │
│  │  • Return JSON responses                               │  │
│  └──────────────────────────────────────────────────────────┘  │
│                                                                  │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │              DATA MODEL LAYER                           │  │
│  │  ┌──────────────┬──────────────┬─────────────────────┐  │  │
│  │  │ User         │ Story        │ Artifact            │  │  │
│  │  ├──────────────┼──────────────┼─────────────────────┤  │  │
│  │  │ LanguageEntry│ CulturalEvent│                     │  │  │
│  │  └──────────────┴──────────────┴─────────────────────┘  │  │
│  │                                                          │  │
│  │  • Entity properties                                    │  │
│  │  • Navigation properties (relationships)               │  │
│  │  • Validation attributes                               │  │
│  └──────────────────────────────────────────────────────────┘  │
│                                                                  │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │         ENTITY FRAMEWORK CORE 8.0                       │  │
│  │                                                          │  │
│  │  • LINQ to Entities                                     │  │
│  │  • Lazy loading & eager loading                        │  │
│  │  • Change tracking                                      │  │
│  │  • Automatic SQL generation                            │  │
│  └──────────────────────────────────────────────────────────┘  │
└────────────────┬─────────────────────────────────────────────────┘
				 │
				 │ SQL Queries
				 │
┌────────────────▼─────────────────────────────────────────────────┐
│                    SQLITE DATABASE                              │
│                    (heritage.db)                                │
│  ┌──────────────┬──────────────┬─────────────────────────────┐ │
│  │ Users Table  │ Stories Table │ Artifacts Table            │ │
│  ├──────────────┼──────────────┼─────────────────────────────┤ │
│  │ Languages    │ Events Table │                             │ │
│  │ Table        │              │                             │ │
│  └──────────────┴──────────────┴─────────────────────────────┘ │
│                                                                  │
│  • Relationships with Foreign Keys                              │
│  • Automatic timestamps (CreatedAt, UpdatedAt)                │
│  • Seed data for demo                                          │
│  • Lightweight file-based storage                              │
└──────────────────────────────────────────────────────────────────┘
```

## Data Flow Example: Adding a Story

```
USER INPUT
	│
	▼
┌─────────────────────────────────┐
│ User fills story form            │
│ (title, description, culture...)│
└────────────┬────────────────────┘
			 │
			 ▼
┌─────────────────────────────────┐
│ JavaScript validates input      │
│ (checks required fields)         │
└────────────┬────────────────────┘
			 │
			 ▼
┌─────────────────────────────────┐
│ app.js sends POST to API        │
│ /api/stories                    │
│ Content-Type: application/json  │
└────────────┬────────────────────┘
			 │
			 ▼
┌─────────────────────────────────┐
│ StoriesController.CreateStory()  │
│ • Validates Story object        │
│ • Assigns timestamps            │
└────────────┬────────────────────┘
			 │
			 ▼
┌─────────────────────────────────┐
│ _context.Stories.Add(story)     │
│ _context.SaveChangesAsync()     │
└────────────┬────────────────────┘
			 │
			 ▼
┌─────────────────────────────────┐
│ EF Core generates SQL INSERT    │
│ Sends to SQLite                 │
└────────────┬────────────────────┘
			 │
			 ▼
┌─────────────────────────────────┐
│ SQLite inserts record           │
│ Returns success status          │
└────────────┬────────────────────┘
			 │
			 ▼
┌─────────────────────────────────┐
│ Controller returns 201 Created  │
│ With story JSON in response     │
└────────────┬────────────────────┘
			 │
			 ▼
┌─────────────────────────────────┐
│ app.js receives response        │
│ Updates DOM with new card       │
│ Refreshes stats                 │
│ Shows success alert             │
└────────────┬────────────────────┘
			 │
			 ▼
		STORY SAVED!
```

## Component Relationships

```
┌──────────────────────┐
│     User (Admin)     │
│   - Id: 1            │
│   - Name: Admin User │
└──────┬──┬──┬────┬───┘
	   │  │  │    │
	   │  │  │    └──── CulturalEvent 1
	   │  │  │
	   │  │  └──────── LanguageEntry 1
	   │  │
	   │  └──────────── Artifact 1
	   │
	   └──────────────── Story 1
						(Legend of Sacred River)
						- Title
						- Description
						- Culture: Indigenous
						- Language: English
						- Location: River Valley
						- TranscriptionText
						- ViewCount: 0
						- CreatedAt
						- UpdatedAt
```

## Technology Stack Visual

```
┌─────────────────────────────────────────────────────────────┐
│                      PRESENTATION                          │
│  ┌──────────────────────────────────────────────────────┐  │
│  │ HTML5  │  CSS3  │  JavaScript (Vanilla)            │  │
│  └──────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
						  ▲
						  │
┌─────────────────────────────────────────────────────────────┐
│                   APPLICATION LAYER                        │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  ASP.NET Core 8.0 REST API                         │  │
│  │  • Controllers (4)                                  │  │
│  │  • Models (5)                                       │  │
│  │  • Dependency Injection                             │  │
│  │  • CORS Middleware                                  │  │
│  │  • Static File Serving                              │  │
│  └──────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
						  ▲
						  │
┌─────────────────────────────────────────────────────────────┐
│                   DATA ACCESS LAYER                        │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  Entity Framework Core 8.0                          │  │
│  │  • DbContext (HeritageDbContext)                    │  │
│  │  • LINQ-to-SQL Queries                              │  │
│  │  • Automatic migrations                             │  │
│  │  • Relationship management                          │  │
│  └──────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
						  ▲
						  │
┌─────────────────────────────────────────────────────────────┐
│                    DATABASE LAYER                          │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  SQLite 3                                            │  │
│  │  • File: heritage.db                                 │  │
│  │  • 5 Tables                                          │  │
│  │  • Relationships & FK Constraints                   │  │
│  │  • Automatic timestamps                             │  │
│  └──────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
```

## Deployment Architecture (Ready for)

```
					┌──────────────────┐
					│   Browser Users  │
					│  (Public Access) │
					└────────┬─────────┘
							 │ HTTPS
							 ▼
					┌──────────────────┐
					│   Reverse Proxy  │
					│   (nginx/IIS)    │
					└────────┬─────────┘
							 │
							 ▼
					┌──────────────────┐
					│  Docker Container│
					│  (or IIS/Azure)  │
					│  ASP.NET Core 8  │
					└────────┬─────────┘
							 │
		 ┌───────────────────┼───────────────────┐
		 │                   │                   │
		 ▼                   ▼                   ▼
	┌─────────┐        ┌──────────┐      ┌──────────┐
	│ SQLite  │        │ Redis    │      │  S3 Blob │
	│(Dev)    │        │(Cache)   │      │(Files)   │
	└─────────┘        └──────────┘      └──────────┘
		 │
		 ▼
	┌──────────────────┐
	│ PostgreSQL/MSSQL │
	│ (Production)     │
	└──────────────────┘
```

---

This architecture is:
- **Scalable**: Ready to grow from prototype to production
- **Maintainable**: Clear separation of concerns
- **Testable**: Each layer can be unit tested
- **Deployable**: Multiple hosting options supported
- **Secure**: Built-in protections, ready for authentication
