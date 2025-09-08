# Waldhari.Core

![.NET for GTA V](https://img.shields.io/badge/Library%20for%20GTA%20V%20Mods-Waldhari.Core-blueviolet?style=for-the-badge&logo=dotnet)
![License: GPL v3](https://img.shields.io/badge/License-GPL%20v3-darkgreen?style=for-the-badge&logo=gnu)
![Release](https://img.shields.io/github/v/release/RomainDel59/Waldhari.Core?style=for-the-badge)

**Waldhari.Core** is the core library for the *Waldhari* suite of mods.  
It provides reusable and extensible services that simplify persistence, logging, and localization across multiple mods, while following clean architecture principles (interfaces, modular implementations, unit testing).

---


## ✨ Features

* **Persistence Service**
    * Interface-based (`IPersistenceService`)
    * XML implementation (`XmlPersistenceService`) using `System.Xml.Serialization`
    * Safe file operations with error handling
    * Easily extendable to support other formats (JSON, database, etc.)

* **Logging Service**
    * Interface-based (`ILogService`)
    * Tab-separated value (TSV) logs (`TsvLogService`)
        * Human-readable in Notepad
        * Easily importable into Excel for filtering and analysis
    * Supports multiple log levels: `Debug`, `Info`, `Warn`, `Error`
    * Thread-safe file writing

* **Localization Service**
    * Interface-based (`ILanguageService`)
    * CSV implementation (`CsvLanguageService`) using semicolon-separated format
    * Automatic language detection based on system culture
    * Feature-based organization (multiple CSV files per language)
    * Fallback to key name if translation not found
    * Support for multiple mods with separate localization directories

* **Unit Testing**
    * Comprehensive NUnit test suite
    * High code coverage, including nominal and edge cases
    * Automatic cleanup after test execution

---


## 🚀 Usage

### Persistence

```csharp
using Waldhari.Core.Persistence;

// Create the persistence service (default = XML)
IPersistenceService persistence = new XmlPersistenceService();

// Save data
var character = new Character { Name = "John", Rank = "Officer" };
persistence.Save("character", character);

// Load data
var loaded = persistence.Load<Character>("character");
```

The save file will be written under:
```
<GTAV Directory>/scripts/Waldhari/<ModName>.xml
```

### Logging

```csharp
using Waldhari.Core.Logging;

// Create a logger (default log file = scripts/Waldhari/<ModName>.log)
ILogService logger = new TsvLogService("MyMod");

// Log messages
logger.Debug("This is a debug message");
logger.Info("Game started");
logger.Warn("Low health detected");
logger.Error("An error occurred", new Exception("Something went wrong"));
```

The log file will be written under:
```
<GTAV Directory>/scripts/Waldhari/<ModName>.log
```

Example TSV line:
```
2025-09-06 14:20:01.123   Info    "Game started"   ""
```

### Localization

```csharp
using Waldhari.Core.Localization;

// Create a language service with automatic language detection
ILanguageService language = new CsvLanguageService("MyMod");

// Or specify language and features explicitly
var features = new[] { "General", "UI", "Messages" };
ILanguageService language = new CsvLanguageService("MyMod", "en-US", features);

// Get localized messages
string welcome = language.GetMessage("WELCOME_MESSAGE");
string currentLang = language.CurrentLanguage;

// Reload with different language
language.Load("MyMod", "fr-FR");
```

**Directory Structure:**
```
<GTAV Directory>/scripts/Waldhari/<ModName>/
├── en-US/
│   ├── General.csv
│   ├── UI.csv
│   └── Messages.csv
└── fr-FR/
    ├── General.csv
    ├── UI.csv
    └── Messages.csv
```

**CSV Format (semicolon-separated):**
```csv
# This is a comment
WELCOME_MESSAGE;Welcome to the game!
HEALTH_LOW;Your health is low
MISSION_COMPLETE;Mission completed successfully
```

---


## 🔮 Roadmap

* ~~Management of localization files~~ ✅ **Completed**
* Configurable log file rotation

---


## 🤝 Contributing

Contributions are welcome!

Please ensure code follows the existing structure, remains simple (KISS principle), and includes unit tests for new features.

---


## 📜 License

This project is licensed under the **GPL-3.0 License**.

You are free to use, modify, and distribute this project, as long as any project that uses it is also open-source under GPL-3.0 and provides attribution to this original project.