# Protect Broker

A production-ready middleware broker between UniFi Protect and farm automation systems.

## Architecture

```
ProtectBroker/
├── src/
│   ├── ProtectBroker.Api           # ASP.NET Core Web API
│   ├── ProtectBroker.Core          # Domain models & business logic
│   ├── ProtectBroker.Infrastructure # Database & external services
│   ├── ProtectBroker.Worker        # Background services
│   └── ProtectBroker.Web           # React frontend
├── tests/                          # Unit & integration tests
└── docs/                           # Documentation
```

## Project Status

**Phase 1: Foundation & Authentication** (In Progress)

- [ ] Project structure & Git setup
- [ ] Database schema
- [ ] ASP.NET Core API & Identity
- [ ] Login UI
- [ ] Dashboard layout
- [ ] Settings pages
- [ ] Testing framework

## Quick Start

### Prerequisites

- Docker & Docker Compose
- VS Code or JetBrains Rider (optional)

### Development

```bash
# Start all services
docker-compose up -d

# API runs on http://localhost:5000
# Web runs on http://localhost:5173
# PostgreSQL on localhost:5432

# Stop all services
docker-compose down
```

### Database Migrations

```bash
# Inside API container
docker-compose exec api dotnet ef migrations add InitialCreate
docker-compose exec api dotnet ef database update
```

## Stack

- **Backend**: ASP.NET Core 9, SignalR, Entity Framework Core
- **Frontend**: React 18, TypeScript, Material UI, Vite
- **Database**: PostgreSQL 16
- **Authentication**: ASP.NET Identity, JWT
- **Deployment**: Docker, Docker Compose

## Documentation

- [Architecture](./docs/ARCHITECTURE.md)
- [API Documentation](./docs/API.md)
- [Deployment Guide](./docs/DEPLOYMENT.md)
- [Developer Guide](./docs/DEVELOPER.md)

## License

MIT
