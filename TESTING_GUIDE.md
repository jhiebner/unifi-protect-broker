# Protect Broker - Testing Guide

## Quick Start (5 minutes to running)

### 1. Prerequisites Check
```bash
# Verify all tools installed
dotnet --version          # Should be 9.0+
node --version            # Should be 18.0+
npm --version             # Should be 9.0+
psql --version            # Should be 14.0+

# Ping UniFi Protect on your network
ping 192.168.X.X          # Replace with your NVR IP
```

### 2. Environment Setup (2 minutes)
```bash
# In project root directory
cp .env.example .env

# Edit .env and update THESE 4 VALUES:
# - ConnectionStrings__DefaultConnection (your DB password)
# - UnifiProtect__Host (your NVR IP address)
# - UnifiProtect__Username (usually 'admin')
# - UnifiProtect__Password (your NVR password)

# Create frontend env
cd src/ProtectBroker.Web
cp .env.example .env.local
cd ../..
```

### 3. Database (1 minute)
```bash
# Create database
createdb -U postgres protect_broker
```

### 4. Start Backend (Terminal 1)
```bash
# From project root
dotnet restore
dotnet build
dotnet run --project src/ProtectBroker.Api
```

Expected output:
```
Listening on: http://localhost:5000
Database migration completed
Protect Device Sync Service started
```

### 5. Start Frontend (Terminal 2)
```bash
cd src/ProtectBroker.Web
npm install
npm run dev
```

Expected output:
```
➜  Local:   http://localhost:5173/
```

### 6. Test in Browser
- Open: http://localhost:5173
- Login: `admin@example.com` / `Admin123!`
- Should see dashboard with device count
- Go to "Sensors" → should see your UniFi Protect sensors

---

## What You'll See

### 1. Login Page
- Clean, professional interface
- Dark mode toggle in top-right
- Default credentials work

### 2. Dashboard
```
┌─────────────────────────────────────────────────┐
│ Protect Broker              🌙                  │
├─────────────────────────────────────────────────┤
│ Dashboard  Sensors  Relays  Settings            │
│                                                 │
│ ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────┐ │
│ │Total     │ │Active    │ │Relays    │ │Alerts│ │
│ │Devices   │ │Sensors   │ │          │ │      │ │
│ │    15    │ │    14    │ │    3     │ │  0   │ │
│ └──────────┘ └──────────┘ └──────────┘ └──────┘ │
└─────────────────────────────────────────────────┘
```

### 3. Sensors Page
```
┌─────────────────────────────────────────────────┐
│ Sensors                                         │
├─────────────────────────────────────────────────┤
│ Search [           ]  Clear Filter              │
│                                                 │
│ Online(12)  Offline(2)  Warning(1)              │
│                                                 │
│ ┌─ Sensor 1 ─┐ ┌─ Sensor 2 ─┐ ┌─ Sensor 3 ─┐ │
│ │ Motion     │ │ Door       │ │ Temp       │ │
│ │ Online     │ │ Offline    │ │ Online     │ │
│ │ Battery: 87│ │ Battery: 42│ │ Battery: 95│ │
│ │ Signal: OK │ │ Signal: OK │ │ Signal: OK │ │
│ │ 2m ago     │ │ 5h ago     │ │ Now        │ │
│ └────────────┘ └────────────┘ └────────────┘ │
└─────────────────────────────────────────────────┘
```

---

## Real-Time Testing

### Test 1: Verify Backend Connection
```bash
# In another terminal
curl http://localhost:5000/health

# Expected response (200 OK):
# {"status":"Healthy",...}
```

### Test 2: Verify Protect API Connection
```bash
# Get your JWT token first (check backend startup output)
# Then:
curl -H "Authorization: Bearer YOUR_TOKEN" \
  http://localhost:5000/api/debug/protect-status

# Expected response:
# {"apiConnected":true,"webSocketConnected":true,...}
```

### Test 3: Get Device List
```bash
curl -H "Authorization: Bearer YOUR_TOKEN" \
  http://localhost:5000/api/devices

# Expected response:
# [{"id":"...", "name":"Camera 1", "type":"Camera", ...}, ...]
```

### Test 4: Real-Time Updates
1. Open browser DevTools (F12)
2. Go to Console tab
3. Check for messages:
   - ✅ "SignalR connected" - WebSocket working
   - ✅ No red error messages
4. Change a device state in UniFi Protect
5. Check frontend updates in real-time
6. Check console for "SensorUpdated" events

---

## Monitoring

### Backend Console Logs

**Watch for these SUCCESS messages:**
```
[INF] Protect Device Sync Service started
[INF] Device sync completed: 3 cameras, 12 sensors added/updated
[INF] WebSocket connected successfully
[DBG] Broadcasting sensor update: Temp-Sensor = 72.5
```

**Watch for these WARNING/ERROR messages:**
```
[WRN] WebSocket connection failed, will retry
[ERR] Failed to authenticate with UniFi Protect
[ERR] Error syncing devices
```

### Frontend Console (F12 → Console)

**Expected messages:**
```
SignalR connected
SensorUpdated event received
Device status changed: Camera-1 → Online
```

**Errors to investigate:**
```
Network request failed - Check API running
401 Unauthorized - Check JWT token
CORS error - Check CORS settings
```

### Database

```bash
# Check devices synced
psql -d protect_broker -c "SELECT name, type, status FROM \"Devices\" LIMIT 10;"

# Check sensors
psql -d protect_broker -c "SELECT name, \"SensorType\", \"CurrentValue\" FROM \"Sensors\" LIMIT 10;"

# Count records
psql -d protect_broker -c "SELECT COUNT(*) FROM \"Devices\"; SELECT COUNT(*) FROM \"Sensors\";"
```

---

## Expected Data Flow

```
┌─────────────────────────────────────────────────────────────┐
│ UniFi Protect NVR (192.168.X.X:7443)                        │
│ Contains: Cameras, Sensors, Relays                          │
└─────────────────────┬───────────────────────────────────────┘
                      │
                      │ HTTPS API + WebSocket
                      ▼
┌─────────────────────────────────────────────────────────────┐
│ Protect Broker Backend (localhost:5000)                     │
│ ┌───────────────────────────────────────────────────────┐   │
│ │ ProtectApiClient: Polls bootstrap every 5 min        │   │
│ │ ProtectWebSocketClient: Real-time events             │   │
│ │ ProtectDeviceSyncService: Stores in PostgreSQL       │   │
│ │ DeviceHub (SignalR): Broadcasts to frontend          │   │
│ └───────────────────────────────────────────────────────┘   │
└─────────────────────┬───────────────────────────────────────┘
                      │
                      │ WebSocket + HTTP
                      ▼
┌─────────────────────────────────────────────────────────────┐
│ Frontend React (localhost:5173)                             │
│ ┌───────────────────────────────────────────────────────┐   │
│ │ Dashboard: Shows device counts                       │   │
│ │ Sensors Page: Real-time sensor grid with SignalR     │   │
│ │ Device Store: Zustand state management               │   │
│ └───────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
```

---

## Troubleshooting Checklist

### Backend Won't Start
- [ ] PostgreSQL running? `psql -U postgres -c "SELECT 1"`
- [ ] Database created? `psql -l | grep protect_broker`
- [ ] .env file exists? `ls -la .env`
- [ ] All env vars set? `grep UnifiProtect .env`
- [ ] Port 5000 available? `lsof -i:5000`

### Frontend Won't Load
- [ ] Backend running? `curl http://localhost:5000/health`
- [ ] npm packages installed? `ls src/ProtectBroker.Web/node_modules`
- [ ] .env.local exists? `ls src/ProtectBroker.Web/.env.local`
- [ ] Port 5173 available? `lsof -i:5173`

### No Sensors Appearing
- [ ] Backend logs show successful sync? Check console
- [ ] Database has devices? `psql -d protect_broker -c "SELECT COUNT(*) FROM \"Devices\""`
- [ ] Frontend making API call? Check DevTools → Network
- [ ] SignalR connected? Check DevTools → Console
- [ ] Wait 5+ minutes? First sync has delay

### Real-Time Not Working
- [ ] SignalR connected? Check browser console
- [ ] Backend WebSocket started? Check backend logs
- [ ] CORS configured? Check backend .env
- [ ] Firewall blocking? Try localhost first

---

## Performance Baseline

**Expected on MacBook Pro M1:**

| Action | Time |
|--------|------|
| Backend startup | ~3-5 seconds |
| Database migration | ~2-3 seconds |
| Device sync | ~1-2 seconds |
| Frontend load | ~1-2 seconds |
| Sensor data display | Instant (real-time) |
| Login | ~1 second |

**Database size (typical):**
- 10 devices: ~1 MB
- 50 devices: ~2 MB
- 100 devices: ~4 MB

---

## Testing Scenarios

### Scenario 1: New Device Added to Protect
1. Add new sensor in UniFi Protect web interface
2. Should auto-appear in Protect Broker within 5 minutes
3. Check backend logs for sync success
4. Refresh Sensors page if needed

### Scenario 2: Device Loses Connection
1. Disable/disconnect sensor from Protect
2. Backend sync should mark as "Offline" within 5 minutes
3. Frontend should show red/offline indicator
4. Database should update LastSeen timestamp

### Scenario 3: Sensor Reading Changes
1. Trigger sensor (motion, door open, etc.)
2. Should broadcast via WebSocket immediately
3. Check SignalR event in browser console
4. Frontend updates in real-time
5. Database stores new reading

### Scenario 4: Backend Restart
1. Stop backend (Ctrl+C)
2. Restart backend
3. Database migrates (if needed)
4. Device sync runs
5. Frontend should reconnect via SignalR
6. No data loss

---

## Success Checklist

- [ ] Backend starts without errors
- [ ] Frontend loads at http://localhost:5173
- [ ] Login with admin@example.com / Admin123! works
- [ ] Dashboard shows correct device count
- [ ] Sensors page displays all sensors
- [ ] Each sensor shows: name, battery, signal, status
- [ ] Browser console shows "SignalR connected"
- [ ] Backend console shows sync success
- [ ] Database has populated tables
- [ ] Real-time updates work (check console)
- [ ] No HTTP errors (check DevTools Network tab)
- [ ] No CORS errors
- [ ] Can refresh page without losing connection

---

## Next: Phase 3 Testing

Once Phase 2 works perfectly, you'll be able to test:
- [ ] Relay on/off buttons
- [ ] Relay scheduling
- [ ] Confirmation dialogs
- [ ] Audit logging
- [ ] Activity feed

---

**Questions?** Check SETUP_CHECKLIST.md for detailed troubleshooting.
