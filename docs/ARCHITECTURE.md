# Protect Broker - Architecture & Design

## Overview

Protect Broker is a production-grade middleware application that bridges UniFi Protect systems with farm automation infrastructure. The architecture prioritizes reliability, maintainability, scalability, and security.

## System Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                     Web Browsers (HTTP/WebSocket)            │
└──────────────────────┬──────────────────────────────────────┘
                       │
        ┌──────────────┴──────────────┐
        │                             │
┌───────▼────────────┐      ┌────────▼──────────────┐
│   React Frontend   │      │  Nginx Reverse Proxy  │
│  (Port 5173/80)    │      │  (SPA + API Routing)  │
└───────┬────────────┘      └────────┬──────────────┘
        │                             │
        └──────────────┬──────────────┘
                       │ HTTPS + JWT
        ┌──────────────▼──────────────┐
        │                              │
┌───────▼─────────────────────────────▼────────┐
│  ASP.NET Core 9 Web API (Port 5000)           │
│                                               │
│  ┌─────────────────────────────────────────┐ │
│  │ Middleware Layer                        │ │
│  │ - CORS, Authentication, Logging         │ │
│  └─────────────────────────────────────────┘ │
│                                               │
│  ┌─────────────────────────────────────────┐ │
│  │ API Controllers & SignalR Hubs          │ │
│  │ - RESTful endpoints                     │ │
│  │ - Real-time WebSocket events            │ │
│  └─────────────────────────────────────────┘ │
│                                               │
│  ┌─────────────────────────────────────────┐ │
│  │ Application Services                    │ │
│  │ - Device Management                     │ │
│  │ - Sensor Processing                     │ │
│  │ - Relay Control                         │ │
│  │ - Rule Execution                        │ │
│  │ - Notification Dispatch                 │ │
│  └─────────────────────────────────────────┘ │
│                                               │
│  ┌─────────────────────────────────────────┐ │
│  │ UniFi Protect Integration               │ │
│  │ - HTTP REST client                      │ │
│  │ - Persistent WebSocket connection       │ │
│  │ - Event stream processing               │ │
│  │ - Device sync & discovery               │ │
│  └─────────────────────────────────────────┘ │
└───────┬─────────────────────────────────────┘
        │
        │ Entity Framework Core ORM
        │
┌───────▼──────────────────────────────┐
│  PostgreSQL Database                 │
│                                       │
│  ┌───────────────────────────────────┤
│  │ • Users & Authentication           │
│  │ • Devices & Sensors                │
│  │ • Relays & Commands                │
│  │ • Rules & Automation               │
│  │ • Audit Logs                       │
│  │ • Settings & Configuration         │
│  │ • Notifications                    │
│  └───────────────────────────────────┤
└───────────────────────────────────────┘

┌─────────────────────────────────────────┐
│  Background Services (Hosted Services)  │
│                                         │
│  ┌─────────────────────────────────┐   │
│  │ UniFi Protect Connection        │   │
│  │ - Maintain persistent WS        │   │
│  │ - Sync device state             │   │
│  │ - Auto-reconnect on failure     │   │
│  └─────────────────────────────────┘   │
│                                         │
│  ┌─────────────────────────────────┐   │
│  │ Rule Engine                     │   │
│  │ - Evaluate automation rules     │   │
│  │ - Execute actions               │   │
│  │ - Handle side effects           │   │
│  └─────────────────────────────────┘   │
│                                         │
│  ┌─────────────────────────────────┐   │
│  │ Notification Queue              │   │
│  │ - Email, SMS, Webhooks          │   │
│  │ - Retry failed notifications    │   │
│  │ - Rate limiting                 │   │
│  └─────────────────────────────────┘   │
│                                         │
│  ┌─────────────────────────────────┐   │
│  │ Health Monitoring               │   │
│  │ - Check Protect connectivity    │   │
│  │ - Database health               │   │
│  │ - System metrics                │   │
│  └─────────────────────────────────┘   │
└─────────────────────────────────────────┘
```

## Project Structure

### Backend Projects

#### ProtectBroker.Core
Core domain models and business logic. **No external dependencies** except standard library.

**Responsibilities:**
- Domain entities (User, Device, Sensor, Relay, Zone, Rule, etc.)
- Enumerations (DeviceType, RelayState, UserRole, etc.)
- Interfaces for repositories and services
- Business logic validation

**Key Files:**
- `Entities/` - Domain models with validation attributes
- `Constants/` - Application constants and enumerations

#### ProtectBroker.Infrastructure
Data access, external service integration, and cross-cutting concerns.

**Responsibilities:**
- Entity Framework Core migrations and DbContext
- Repository implementations
- UniFi Protect API client
- SMTP, SMS, and webhook integrations
- Caching layer
- Logging configuration

**Key Files:**
- `Data/ApplicationDbContext.cs` - Database schema and mappings
- `Migrations/` - Database version history
- `Services/` - External integrations (Protect, Email, SMS, etc.)

#### ProtectBroker.Api
ASP.NET Core Web API - REST endpoints and real-time signaling.

**Responsibilities:**
- HTTP Controllers (REST endpoints)
- SignalR Hubs (WebSocket real-time updates)
- Dependency injection configuration
- Middleware setup (auth, logging, error handling)
- DTOs for request/response serialization
- API documentation (Swagger)

**Key Files:**
- `Program.cs` - Application startup and configuration
- `Controllers/` - REST endpoints organized by domain
- `Hubs/` - SignalR real-time communication hubs
- `DTOs/` - Data transfer objects for serialization

#### ProtectBroker.Worker
Background services for long-running operations.

**Responsibilities:**
- Hosted services for continuous operation
- UniFi Protect WebSocket connection management
- Rule engine execution
- Notification queue processing
- Health monitoring
- Scheduled maintenance tasks

**Key Files:**
- `Services/` - Hosted service implementations
- `Jobs/` - Scheduled background jobs

### Frontend Project

#### ProtectBroker.Web
React 18 single-page application built with Vite.

**Structure:**
```
src/
├── components/          # Reusable React components
│   ├── Layout/         # Layout components (Header, Sidebar, Footer)
│   ├── Dashboard/      # Dashboard-specific components
│   ├── Sensors/        # Sensor display components
│   ├── Relays/         # Relay control components
│   └── Common/         # Shared UI components
├── pages/              # Page-level components (routed)
│   ├── Login.tsx
│   ├── Dashboard.tsx
│   ├── Sensors.tsx
│   ├── Relays.tsx
│   ├── Zones.tsx
│   ├── Rules.tsx
│   ├── Logs.tsx
│   ├── Users.tsx
│   └── Settings.tsx
├── services/           # API clients and external services
│   ├── api.ts          # Axios instance with interceptors
│   ├── authService.ts  # Authentication service
│   ├── deviceService.ts
│   └── ...
├── stores/             # Global state management (Zustand)
│   ├── authStore.ts
│   ├── deviceStore.ts
│   └── ...
├── hooks/              # Custom React hooks
│   ├── useAuth.ts
│   ├── useSignalR.ts
│   └── ...
├── types/              # TypeScript interfaces and types
│   └── index.ts
├── App.tsx             # Root component and routing
└── main.tsx            # Entry point
```

## Data Flow

### Device State Update Flow

```
UniFi Protect Device → WebSocket Event
                      ↓
        ProtectBroker WebSocket Handler
                      ↓
    Update ApplicationDbContext (EF Core)
                      ↓
    Broadcast via SignalR Hub
                      ↓
    Subscribers receive real-time update
                      ↓
    React component re-renders
```

### Relay Control Flow

```
User clicks relay button
        ↓
React component sends HTTP POST to API
        ↓
Authentication middleware validates JWT
        ↓
API controller validates request & authorization
        ↓
Application service queues RelayCommand
        ↓
Background service sends command to UniFi Protect
        ↓
UniFi Protect acknowledges
        ↓
Database updated with execution status
        ↓
SignalR broadcasts update to all connected clients
        ↓
React components update UI
```

### Rule Execution Flow

```
Trigger condition met
        ↓
Rule Engine service evaluates conditions
        ↓
Actions validated for authorization
        ↓
Action commands queued
        ↓
Each action executed in order
        ↓
Notification triggered if configured
        ↓
RuleExecution record logged
        ↓
SignalR broadcasts rule execution event
```

## Authentication & Authorization

### JWT Token Flow

1. **Login**: User provides email/password
2. **Validation**: API validates credentials against database
3. **Token Generation**: Creates JWT with claims including UserId and Role
4. **Response**: Returns access token (short-lived) and refresh token (long-lived)
5. **Storage**: React stores tokens in localStorage (or sessionStorage)
6. **Requests**: Axios interceptor adds JWT to Authorization header
7. **Validation**: API validates token on each protected endpoint
8. **Refresh**: When access token expires, use refresh token to get new one

### Role-Based Access Control

| Role | Permissions |
|------|-------------|
| Administrator | Full access to all features |
| Manager | Control relays, create/edit rules, manage notifications |
| Operator | View devices/sensors, read logs, execute pre-approved relays |
| Guest | Read-only access to dashboard and logs |

## Database Schema

### Core Tables

- **Users**: Authentication, roles, audit info
- **Zones**: Device grouping by area
- **Devices**: UniFi Protect devices with metadata
- **Sensors**: Device sensors with current state
- **SensorReadings**: Historical sensor data
- **Relays**: Smart lock relays
- **RelayCommands**: Audit trail for relay operations
- **Rules**: Automation rule definitions
- **RuleExecutions**: Execution history
- **AuditLogs**: System-wide audit trail
- **Settings**: Application configuration
- **Notifications**: Queued notifications for delivery

### Key Design Decisions

1. **UUIDs**: All primary keys are UUIDs (GUID) for distributed systems
2. **Timestamps**: All entities have CreatedAt and LastModifiedAt for audit
3. **Soft Deletes**: Considered but not implemented initially (can add IsDeleted flag)
4. **JSON Columns**: Rules and audit logs use jsonb for flexibility
5. **Indexing**: Composite indexes on frequently queried field combinations

## API Design

### REST Conventions

```
GET    /api/devices              - List all devices
GET    /api/devices/{id}         - Get device details
POST   /api/devices              - Create device
PUT    /api/devices/{id}         - Update device
DELETE /api/devices/{id}         - Delete device

GET    /api/relays/{id}/commands - Get relay command history
POST   /api/relays/{id}/commands - Execute relay command
```

### SignalR Hub Events

**Server → Client (Real-time Updates)**
- `DeviceConnected` - Device came online
- `DeviceDisconnected` - Device went offline
- `SensorUpdated` - Sensor reading changed
- `RelayStateChanged` - Relay state changed
- `AlertTriggered` - Alert/alarm activated
- `RuleExecuted` - Rule execution completed
- `NotificationSent` - Notification sent

**Client → Server (User Actions)**
- `ControlRelay` - User requests relay control
- `ExecuteRule` - User manually triggers rule
- `AcknowledgeAlert` - User acknowledges alert

## Deployment Architecture

### Docker Compose Stack

```
Container 1: PostgreSQL
├── Port: 5432 (internal)
├── Volume: postgres_data
└── Database: protect_broker

Container 2: ASP.NET Core API
├── Port: 5000
├── Environment: CONNECTION_STRINGS, JWT_KEY, etc.
├── Depends on: PostgreSQL
└── Health check: /health endpoint

Container 3: React + Nginx
├── Port: 5173 (dev) / 80 (production)
├── SPA routing: /index.html fallback
├── API proxy: /api → localhost:5000
├── WebSocket proxy: /signalr → localhost:5000
└── Depends on: API
```

### Proxmox LXC Deployment

For Debian containers in Proxmox:

1. Create unprivileged LXC container
2. Enable nesting: `features: nesting=1,keyctl=1`
3. Install Docker inside container
4. Deploy via docker-compose
5. Optional: Nginx reverse proxy for SSL/external access
6. Optional: Configure automated backups

## Security Measures

1. **Authentication**: JWT with secure key (min 32 chars)
2. **HTTPS**: Recommended for production (use reverse proxy with SSL)
3. **CORS**: Restricted to known origins
4. **Rate Limiting**: Configurable per endpoint
5. **Audit Logging**: All user actions logged
6. **Password Policy**: Enforced minimum complexity
7. **Role-Based Access**: Fine-grained permission checks
8. **Encrypted Settings**: Sensitive configuration values encrypted at rest
9. **Database Credentials**: Environment-based configuration
10. **Token Expiration**: Short access token lifecycle with refresh tokens

## Scalability Considerations

### Current (Single Instance)

- Single API instance handles all requests
- Single PostgreSQL database
- Suitable for: 100-1000 devices, <50 concurrent users

### Future (Horizontal Scaling)

- Multiple API instances behind load balancer
- PostgreSQL replication for HA
- Distributed caching (Redis)
- Message queue for background jobs (RabbitMQ)
- Microservices for compute-intensive tasks

## Monitoring & Observability

### Logging

- **Serilog**: Structured logging to console and file
- **Request Logging**: Automatically logs HTTP requests/responses
- **Context Enrichment**: Correlation IDs for request tracing

### Health Checks

- **API**: `/health` endpoint returns status
- **Database**: Connectivity check included in health
- **Container**: Docker health checks configured

### Metrics (Future)

- Prometheus exporter for metrics
- Grafana dashboards for visualization
- Alert thresholds for critical conditions

## Testing Strategy

### Unit Tests

- Domain logic, validation, calculations
- Repository interfaces mocked
- Framework: xUnit with Moq

### Integration Tests

- Database interactions with test database
- API endpoints with test client
- Mock external services (Protect API)

### End-to-End Tests

- Full flow from UI through API to database
- Mock UniFi Protect server
- Framework: Playwright or Selenium

## Future Enhancements

1. **Plugin Architecture**: Support for custom device types and integrations
2. **Multi-Site**: Manage multiple UniFi Protect instances
3. **Advanced Analytics**: Trend analysis, predictive alerts
4. **Mobile App**: Native iOS/Android app
5. **Voice Control**: Integration with voice assistants
6. **Advanced Scheduling**: Cron-like rule scheduling
7. **Webhook Support**: Send/receive webhooks for integration
8. **API Keys**: Machine-to-machine authentication

## Conclusion

Protect Broker is designed for production deployment with emphasis on reliability, maintainability, and clarity. The architecture supports growth and can scale from small farms to enterprise deployments.
