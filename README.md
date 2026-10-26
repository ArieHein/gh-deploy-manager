# Deploy Manager (Concept)

This is a lightweight ASP.NET Core Razor Pages concept app inspired by the Octopus Deploy dashboard style.

## What it includes

- Three dashboard UI modes:
  - **Classic board** — Octopus-style matrix (projects × environments grid)
  - **Modern release promotion view** — Project-focused rows with icon promote buttons
  - **Kanban deployment board** — Environment lanes with project cards (premium visual design with smooth animations)
- Release/version/build visibility per project
- Deploy action per environment and project
- Simulated deployment activity timeline
- Deployment animations with progress visualization
- Simple cookie authentication (demo users in appsettings)
- In-memory seeded data model (no database cost/setup)

You can choose the dashboard UI mode on the login screen. If none is selected, the classic view is used by default.

### Kanban Board Features

The Kanban UI includes:

- **Environment lanes** for visual deployment flow (Development → Test → Staging → Production)
- **Project cards** showing current deployment status and versions
- **Deployment animations** with spinner and progress bar
- **Status indicators** (Queued/Deployed/Failed) inline with project information, preserved per project/environment as the most recent deployment attempt
- **Duration display** showing how long the last deployment took
- **Responsive design** with smooth hover effects and visual feedback
- **Promote buttons** for quick version deployment to each environment

## Run locally

```bash
dotnet run
```

Then open the URL shown in the console (typically `https://localhost:xxxx`).

## Demo credentials

- `admin` / `DemoPass!2026`
- `release-bot` / `Release!2026`

These can be changed in `appsettings.json` under `DemoAuth:Users`.
