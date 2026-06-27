CyberBot — Cybersecurity Awareness Assistant (POE Part 3)
CyberBot is a WinForms cybersecurity awareness chatbot. It chats about online safety topics, detects the user's sentiment, remembers their name and interests, runs an interactive quiz, manages a MySQL-backed task/reminder list, and keeps a full activity log — all from one cohesive interface.

Features at a glance
Feature	Where it lives
Chat with sentiment detection & memory	Forms/MainForm.cs, Sentiment/, Memory/
NLP intent processor (multi-turn task creation, typo-tolerant topic matching, follow-ups)	Responses/ResponseEngine.cs, Forms/MainForm.cs
Task Assistant with reminders, backed by MySQL	TaskAssistantForm.cs, Data/DatabaseHelper.cs, Models/CyberTask.cs
Background reminder system (tray balloon + chat alert)	Forms/MainForm.cs (CheckReminders)
18-question cybersecurity quiz (10 per round, shuffled)	QuizEngine.cs, QuizForm.cs
Activity log with paging ("Show more")	ActivityLog.cs, Forms/ActivityLogForm.cs
Voice greeting on launch	Helpers/AudioHelpers.cs
1. Prerequisites
Visual Studio 2022 (with the .NET desktop development workload)
.NET 8 SDK
MySQL Server (MySQL Workbench, XAMPP, or any local MySQL instance)
2. Set up the database
Open MySQL Workbench (or the mysql CLI) and run Database/schema.sql. This creates the cyberbot_db database, the tasks table, and two sample rows so the Task Assistant isn't empty on first run.

Copy db.config.example to db.config (same folder as the .csproj) and edit it with your own MySQL username/password:

Server=localhost;Port=3306;Database=cyberbot_db;Uid=root;Pwd=MYC3RISCARISGTI*;SslMode=None;
db.config is in .gitignore, so your password never gets committed. If no db.config is present, CyberBot falls back to Uid=root;Pwd=; (the default for a fresh local install) and will also try to auto-create the table on startup — but running the schema script yourself first is the most reliable option.

3. Open and run the project
Open CyberSecurityPart.csproj (or the .slnx) in Visual Studio 2022.
Visual Studio will restore the MySql.Data NuGet package automatically on first build (or install it manually via Tools → NuGet Package Manager → Manage NuGet Packages for Solution, search "MySql.Data").
Press F5. Tell the bot your name, then try:
A sidebar topic button, or typing "tell me about phishing"
📋 Task Assistant — add a task with a reminder date, mark it complete, delete it, refresh the list
🎯 Take a Quiz — 10 randomised questions out of an 18-question bank
📜 Activity Log — every action taken is timestamped here, 10 at a time with a "Show more" button
Typing "remind me to update my password" — the bot will ask for a description and a reminder date through natural conversation
4. How the reminder system works
Reminders are stored as a DATE in MySQL against each task.
A background Timer in MainForm checks the database every 60 seconds (and once immediately at login) for tasks that are still Pending and whose reminder date has arrived.
Each due task triggers both a chat bubble and a Windows tray balloon notification, exactly once per task per session (tracked in memory so you are not spammed every minute).
5. GitHub — commits, tags & releases
To score well on the GitHub criterion:

Commit in small, meaningful steps with descriptive messages, e.g. git commit -m "Add MySQL-backed DatabaseHelper and CyberTask model", git commit -m "Add reminder timer with tray notifications", git commit -m "Expand quiz bank to 18 questions". Six or more such commits across the project's history is the top tier.

Tag at least three milestones, each with release notes, e.g.:

git tag -a v1.0 -m "Part 1: Console chatbot with topic responses"
git tag -a v2.0 -m "Part 2: WinForms GUI, sentiment detection, memory"
git tag -a v3.0 -m "Part 3: MySQL task assistant, quiz, activity log, reminders"
git push origin --tags
Turn each tag into a GitHub Release (Releases → Draft a new release) with a short description of what changed.

6. Video presentation
Aim to cover, in order: (1) a quick demo of the chat + sentiment + memory, (2) the Task Assistant — add/complete/delete a task and show MySQL Workbench updating live, (3) a reminder firing, (4) the quiz, (5) the Activity Log, and finally (6) a brief look at the code behind one or two of these features (e.g. DatabaseHelper or the NLP intent matching in ResponseEngine) to demonstrate understanding, not just usage.

Project structure
CyberSecurityPart/
├── Program.cs                  Entry point
├── Forms/
│   ├── MainForm.cs             Main chat window, NLP routing, reminder timer
│   └── ActivityLogForm.cs      Paged activity log window
├── TaskAssistantForm.cs        Task Assistant GUI (CRUD + reminders)
├── QuizForm.cs / QuizEngine.cs Quiz GUI + 18-question bank
├── Models/CyberTask.cs         Task model + status enum
├── Data/DatabaseHelper.cs      MySQL CRUD (create/read/update/delete)
├── Database/schema.sql         One-time DB setup script
├── Responses/ResponseEngine.cs Topic responses + typo-tolerant matching
├── Sentiment/SentimentDetector.cs
├── Memory/UserMemory.cs
├── Helpers/AudioHelpers.cs
├── ActivityLog.cs              In-memory, timestamped action log
└── db.config.example           Copy to db.config and fill in your password
