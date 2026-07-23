# Protect Broker - Setup Checklist

## ✅ Pre-Flight Checks

### Required Software
- [ ] .NET 9 SDK installed: `dotnet --version` (should be 9.0.0+)
- [ ] Node.js 18+ installed: `node --version`
- [ ] npm installed: `npm --version`
- [ ] PostgreSQL 14+ running: `psql --version`
- [ ] Git installed: `git --version`

### Network
- [ ] UniFi Protect NVR accessible on network
- [ ] Note UniFi Protect IP address: _______________
- [ ] Ping test successful: `ping <ip>`
- [ ] Can reach web interface: `https://<ip>:7443` (or your port)

### UniFi Protect Setup
- [ ] UniFi Protect login credentials available
- [ ] API access enabled in settings
- [ ] Access key/secret obtained from bootstrap or settings
- [ ] At least one sensor or camera added to Protect

---

## 🗂️ Project Setup (One Time)

### 1. Clone Repository
```bash
cd ~/projects  # or your preferred directory
git clone https://github.com/jhiebner/unifi-protect-broker.git
cd unifi-protect-broker
```

### 2. Copy Environment Files
```bash
# In project root
cp .env.example .env

# In frontend directory
cd src/ProtectBroker.Web
cp .env.example .env.local
cd ../..
```

### 3. Update Configuration

**Edit `.env` file:**
```bash
nano .env  # or your favorite editor
```

Update these values:
```
# Database - CRITICAL
ConnectionStrings__DefaultConnection=Host=localhost;Port=5432;Database=protect_broker;Username=postgres;Password=YOUR_PASSWORD_HERE

# UniFi Protect - CRITICAL
UnifiProtect__Host=192.168.X.X          # Your NVR IP
UnifiProtect__Port=7443                 # Or your custom port
UnifiProtect__Username=admin            # Or your API user
UnifiProtect__Password=YOUR_PASSWORD    # Your password

# JWT (generate a long random string)
Jwt__Key=ChangeMe123456789ChangeMe123456789ChangeMe  # Min 32 chars

# Local Development (no changes needed)
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://+:5000
```

**Edit `src/ProtectBroker.Web/.env.local`:**
```bash
VITE_API_URL=http://localhost:5000
VITE_SIGNALR_URL=http://localhost:5000/signalr/devices
```

---

## 🗄️ Database Setup

### Create Database
```bash
# Option 1: Using createdb
createdb -U postgres protect_broker

# Option 2: Using psql
psql -U postgres
> CREATE DATABASE protect_broker;
> \q
```

### Verify Database Created
```bash
psql -U postgres -d protect_broker -c "\dt"
```
Should show: `Did not find any relations.` (tables will be created by migrations)

---

## 🔧 Backend Setup & Run

### First Time Build
```bash
cd ~/unfit-protect-broker  # project root

# Restore NuGet packages
dotnet restore

# Build solution
dotnet build

# Should complete with: "Build succeeded."
```

### Run Backend (Development)
```bash
# From project root
dotnet run --project src/ProtectBroker.Api

# Expected output:
# Listening on: http://localhost:5000
# Database migration completed
# Protect Device Sync Service started
```

### Verify Backend Running
Open new terminal:
```bash
# Check API health
curl http://localhost:5000/health

# Should return JSON with healthy status
```

### Access Swagger API Docs
- Open: http://localhost:5000/swagger
- Should see all API endpoints

---

## 🎨 Frontend Setup & Run

### First Time Setup
```bash
cd src/ProtectBroker.Web

# Install npm packages (may take 2-3 minutes)
npm install

# Should complete with no errors
```

### Run Frontend (Development)
```bash
# From src/ProtectBroker.Web directory
npm run dev

# Expected output:
# ➜  Local:   http://localhost:5173/
# ➜  press h to show help
```

### Verify Frontend Running
- Open browser: http://localhost:5173
- Should see login page
- Check browser console (F12 → Console) for errors

---

## 🧪 Test the Integration

### Step 1: Login
- URL: http://localhost:5173
- Email: `admin@example.com`
- Password: `Admin123!`
- Click Sign In

Expected: Dashboard with "Total Devices: X"

### Step 2: Check Device Sync
- Wait 5-10 seconds
- Check backend terminal for sync logs
- Should see: `Device sync completed: X cameras, Y sensors added/updated`

### Step 3: View Sensors
- Click "Sensors" in left menu
- Should see list of your UniFi Protect sensors
- Each card shows: name, status, battery, signal, last update

### Step 4: Verify Real-Time Connection
- Open browser DevTools (F12)
- Go to Console tab
- Should see: `SignalR connected`
- No red errors

### Step 5: Debug UniFi Connection (if no sensors appear)
- URL: http://localhost:5000/swagger
- Scroll to "Debug" section
- Click "GET /api/debug/protect-status"
- Try it out

Expected response:
```json
{
  "apiConnected": true,
  "webSocketConnected": true,
  "timestamp": "2024-07-23T..."
}
```

If `false`, check your `.env` UniFi Protect settings

---

## 📊 Monitor Logs

### Backend Logs (Console)
Watch for:
```
[INF] Device sync completed
[INF] WebSocket connected
[DBG] Broadcasting sensor update
```

Warnings to investigate:
```
[WRN] WebSocket connection failed
[ERR] Failed to authenticate with UniFi Protect
```

### Frontend Logs (Browser Console - F12)
Watch for:
```
SignalR connected
SensorUpdated event received
```

Errors to investigate:
```
Network request failed
401 Unauthorized
CORS error
```

### Database Logs
```bash
ls -la logs/
tail -f logs/protect-broker-YYYY-MM-DD.txt
```

---

## 🚨 Troubleshooting

### "Database connection failed"
```bash
# Check PostgreSQL running
psql -U postgres -c "SELECT version();"

# If error, start PostgreSQL
# On Mac: brew services start postgresql
# On Linux: sudo systemctl start postgresql
# On Windows: Services → PostgreSQL
```

### "Could not connect to UniFi Protect"
1. Verify IP address: `ping 192.168.X.X`
2. Check credentials in `.env`
3. Verify port (usually 7443)
4. Try HTTPS directly: `curl -k https://192.168.X.X:7443/api/bootstrap`

### "SignalR: Failed to connect"
1. Verify backend running: `curl http://localhost:5000/health`
2. Check VITE_SIGNALR_URL in frontend `.env.local`
3. Check browser console for specific error
4. Verify CORS settings in backend

### "No sensors appearing"
1. Check backend sync logs for errors
2. Verify sensors exist in UniFi Protect
3. Wait 5 minutes (first sync delay)
4. Check: `curl http://localhost:5000/swagger` → `/api/debug/protect-bootstrap`

### "Port already in use"
Backend:
```bash
# Kill process on port 5000
lsof -ti:5000 | xargs kill -9

# Or use different port
export ASPNETCORE_URLS=http://+:5001
```

Frontend:
```bash
# Vite will auto-use 5174 if 5173 taken
# Or specify port
npm run dev -- --port 3000
```

---

## 📝 Development Workflow

### Making Changes to Backend
1. Edit C# files in `src/ProtectBroker.Api` or other projects
2. Backend doesn't auto-reload (Press Ctrl+C and restart)
3. Or run with: `dotnet watch run --project src/ProtectBroker.Api`

### Making Changes to Frontend
1. Edit React/TypeScript files in `src/ProtectBroker.Web/src`
2. Changes auto-reload in browser
3. Check browser console for errors

### Committing Changes
```bash
git add .
git commit -m "Your descriptive message"
git push origin main  # or your branch
```

---

## ✨ Success Indicators

- [ ] Backend running without errors
- [ ] Frontend loads without errors
- [ ] Login works with admin credentials
- [ ] Dashboard shows "Total Devices: > 0"
- [ ] Sensors page displays sensor cards
- [ ] Browser console shows "SignalR connected"
- [ ] Backend console shows sync success logs
- [ ] No HTTP 401/CORS errors
- [ ] Database contains device records

---

## 🎯 Next Steps

Once everything works:

1. **Explore the Data**
   - Check database: `psql -d protect_broker -c "SELECT * FROM \"Devices\""`
   - Verify sensor readings stored
   - Test filtering on Sensors page

2. **Test Relay Control** (Phase 3)
   - Will add relay on/off buttons
   - Will add relay scheduling

3. **Test Rules Engine** (Phase 4)
   - Will add automation rules
   - Will add notifications

---

## 📞 Support

If issues persist:

1. Check all checklist items are complete
2. Verify all environment variables are set correctly
3. Check logs for specific error messages
4. Verify UniFi Protect is accessible from your machine
5. Try stopping/restarting all services

---

**Happy Testing!** 🎉
