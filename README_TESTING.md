# 🚀 Ready to Test Protect Broker

The application is **ready for testing** with your real UniFi Protect instance!

## 📚 Documentation Files

Choose your path:

### **🏃 I want to start RIGHT NOW** 
→ Read: **TESTING_GUIDE.md** (5 minutes to running)
- Quick prerequisites check
- Step-by-step startup
- Visual examples of what you'll see
- Quick troubleshooting

### **📋 I want a detailed checklist**
→ Read: **SETUP_CHECKLIST.md** (detailed walkthrough)
- Pre-flight verification
- Database setup
- Backend & frontend build
- Integration testing steps
- Comprehensive troubleshooting

### **⚡ I want just the essentials**
→ Read: **QUICKSTART.md** (concise reference)
- Prerequisites
- Configuration
- Build & run commands
- Key endpoints
- Common issues

---

## 🎯 What's Ready

✅ **Backend API**
- RESTful endpoints for devices and sensors
- SignalR for real-time updates
- Swagger API documentation at `/swagger`
- Debug endpoints for development

✅ **Frontend Dashboard**
- Material UI professional interface
- Dark mode support
- Real-time sensor monitoring
- Responsive design (mobile/tablet/desktop)

✅ **Database**
- PostgreSQL schema with 12 entities
- Auto-migration on startup
- Device/sensor/relay relationships

✅ **UniFi Protect Integration**
- HTTP API client
- WebSocket client for real-time events
- Automatic background sync
- Error handling and retry logic

✅ **Development Tools**
- Swagger API explorer
- Debug endpoints for testing
- Comprehensive logging
- Browser DevTools compatible

---

## ⚙️ One-Time Setup (10 minutes total)

```bash
# 1. Clone/navigate to project
cd ~/path/to/unifi-protect-broker

# 2. Create database
createdb -U postgres protect_broker

# 3. Configure environment
cp .env.example .env
# Edit: DB password, UniFi IP, credentials

cd src/ProtectBroker.Web
cp .env.example .env.local
cd ../..

# 4. Build backend
dotnet restore
dotnet build

# 5. Install frontend
cd src/ProtectBroker.Web
npm install
cd ../..
```

---

## 🏃 Quick Start (< 1 minute each)

### Terminal 1: Start Backend
```bash
dotnet run --project src/ProtectBroker.Api
```

### Terminal 2: Start Frontend
```bash
cd src/ProtectBroker.Web
npm run dev
```

### Browser: Open & Test
```
http://localhost:5173
Email: admin@example.com
Password: Admin123!
```

---

## ✨ What You'll See

### Login Page
```
Professional login interface with dark mode toggle
```

### Dashboard
```
Total Devices: 15
Active Sensors: 14
Relays: 3
Alerts: 0
```

### Sensors Page
```
🏠 Motion Detector          ✅ Online    🔋 87%  📶 OK
🚪 Door Contact             ✅ Online    🔋 92%  📶 OK
🌡️  Temperature Sensor       ✅ Online    🔋 75%  📶 OK
[Real-time updates as sensors change...]
```

---

## 🧪 Testing Real-Time

**Verify connections are working:**

```bash
# Check API
curl http://localhost:5000/health

# Check Protect connection
curl -H "Authorization: Bearer TOKEN" \
  http://localhost:5000/api/debug/protect-status

# Get device list
curl -H "Authorization: Bearer TOKEN" \
  http://localhost:5000/api/devices
```

**Monitor logs:**
- **Backend**: Console shows sync status
- **Frontend**: Browser DevTools (F12 → Console) shows "SignalR connected"
- **Database**: `psql -d protect_broker -c "SELECT COUNT(*) FROM \"Devices\""`

---

## 📊 Architecture

```
┌──────────────────────────────────┐
│   UniFi Protect NVR              │
│   (Your network device)          │
└──────────────┬───────────────────┘
               │ HTTPS API
               │ WebSocket
               ▼
┌──────────────────────────────────┐
│   Protect Broker API             │
│   (localhost:5000)               │
│   ├─ ProtectApiClient            │
│   ├─ ProtectDeviceSyncService    │
│   ├─ DeviceHub (SignalR)         │
│   └─ Controllers (REST)          │
└──────────────┬───────────────────┘
               │ HTTP
               │ WebSocket
               ▼
┌──────────────────────────────────┐
│   React Dashboard                │
│   (localhost:5173)               │
│   ├─ Device Store (Zustand)      │
│   ├─ Real-time Hooks             │
│   └─ Responsive UI (Material UI) │
└──────────────────────────────────┘
               │
               ▼
┌──────────────────────────────────┐
│   PostgreSQL Database            │
│   (Devices, Sensors, Relays)     │
└──────────────────────────────────┘
```

---

## 🔍 Verification Steps

After startup, verify these in order:

1. **Backend Running** (Terminal 1)
   - See: `Listening on: http://localhost:5000`
   - See: `Database migration completed`
   - See: `Protect Device Sync Service started`

2. **Frontend Running** (Terminal 2)
   - See: `Local: http://localhost:5173/`

3. **Login Works**
   - Open: http://localhost:5173
   - Email: `admin@example.com`
   - Password: `Admin123!`
   - See: Dashboard with device count

4. **Devices Synced**
   - Backend logs show: `Device sync completed: X cameras, Y sensors`
   - Dashboard shows: `Total Devices: > 0`

5. **Sensors Display**
   - Click "Sensors" in menu
   - See sensor cards with data
   - Each shows: name, battery, signal, status

6. **Real-Time Works**
   - Open browser DevTools (F12)
   - Console shows: `SignalR connected`
   - Change sensor in Protect → frontend updates instantly

---

## 🐛 Troubleshooting Quick Links

**Backend won't start:**
- See "Database connection failed" in SETUP_CHECKLIST.md

**No sensors appearing:**
- See "No sensors appearing" in SETUP_CHECKLIST.md

**Real-time not working:**
- See "SignalR: Failed to connect" in SETUP_CHECKLIST.md

**UniFi Protect connection error:**
- See "Could not connect to UniFi Protect" in SETUP_CHECKLIST.md

---

## 📞 Support

| Issue | Documentation |
|-------|---|
| Setup questions | SETUP_CHECKLIST.md |
| Testing procedures | TESTING_GUIDE.md |
| Quick reference | QUICKSTART.md |
| API documentation | http://localhost:5000/swagger |
| Architecture details | docs/ARCHITECTURE.md |
| Production deployment | docs/DEPLOYMENT.md |

---

## 🎯 Success Criteria

When all of these are ✅, Phase 2 is working:

- [ ] Backend starts without errors
- [ ] Frontend loads
- [ ] Login works
- [ ] Dashboard shows device count > 0
- [ ] Sensors page displays data
- [ ] Browser console: "SignalR connected"
- [ ] Backend console: successful sync
- [ ] Database populated
- [ ] Real-time updates working
- [ ] No errors in console

---

## 🚀 What's Next

Once testing succeeds:

**Phase 3: Relay Control & Activity Logging**
- Relay on/off/toggle buttons
- Relay scheduling
- Activity feed
- Audit logging

**Phase 4: Rules Engine & Automation**
- Automation rules
- Notifications (Email, SMS, Discord, Teams, Slack)
- Farm zones
- Webhooks

**Phase 5: Production & Testing**
- Unit/integration tests
- Performance optimization
- Security audit
- Docker deployment

---

## 📝 Files Created in Phase 2

**Backend (~2,000 LOC)**
- ProtectApiClient.cs - API communication
- ProtectWebSocketClient.cs - Real-time events
- ProtectDeviceSyncService.cs - Background sync
- DeviceHub.cs - SignalR broadcasts
- DevicesController.cs - REST API
- SensorsController.cs - REST API

**Frontend (~1,100 LOC)**
- App.tsx - Main app
- SensorsPage.tsx - Sensor display
- DashboardPage.tsx - Overview
- Layout.tsx - Navigation
- SensorCard.tsx - Sensor component
- useSignalRConnection.ts - Real-time hook
- deviceStore.ts - State management
- api.ts - HTTP client

**Documentation**
- QUICKSTART.md - Quick reference
- SETUP_CHECKLIST.md - Detailed guide
- TESTING_GUIDE.md - Testing procedures
- DebugController.cs - Development tools

---

## 🎉 You're Ready!

Everything is set up and ready to test. Choose your documentation path above and get started!

**Questions?** Check the appropriate documentation file or review the logs for specific errors.

---

**Status: Phase 2 Ready for Testing** ✅
