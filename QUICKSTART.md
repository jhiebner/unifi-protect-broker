# Protect Broker - Quick Start Guide

## Prerequisites

- .NET 9 SDK
- Node.js 18+
- PostgreSQL 14+ (running)
- UniFi Protect NVR on your network

## 1. Database Setup

```bash
# Create PostgreSQL database
createdb protect_broker

# Or using psql:
psql -U postgres -c "CREATE DATABASE protect_broker;"
```

## 2. Environment Configuration

Copy and update `.env.example` to `.env`:

```bash
cp .env.example .env
```

Edit `.env` with your settings:

```
# PostgreSQL
DB_HOST=localhost
DB_PORT=5432
DB_NAME=protect_broker
DB_USER=postgres
DB_PASSWORD=your_password

# UniFi Protect Connection
UNIFI_PROTECT_HOST=192.168.1.X  # Your UniFi Protect IP
UNIFI_PROTECT_PORT=7443
UNIFI_PROTECT_ACCESS_KEY=your_access_key
UNIFI_PROTECT_SECRET_KEY=your_secret_key

# JWT
JWT_KEY=GenerateWithAtLeast32CharactersLongerKey123456
JWT_ISSUER=protect-broker
JWT_AUDIENCE=protect-broker-app
JWT_EXPIRY_MINUTES=60

# API
API_PORT=5000
API_CORS_ORIGINS=http://localhost:5173,http://localhost:3000

# Logging
LOG_LEVEL=Information
```

### Getting UniFi Protect Credentials

1. Log into your UniFi Protect NVR web interface
2. Go to Settings → Users
3. Create an API token or use bootstrap endpoint to get credentials
4. Note the access key and secret key

## 3. Build & Run Backend

```bash
# Restore packages
dotnet restore

# Build solution
dotnet build

# Run API (will migrate database automatically)
dotnet run --project src/ProtectBroker.Api

# API will be available at: http://localhost:5000
# Swagger UI at: http://localhost:5000/swagger
```

## 4. Build & Run Frontend

```bash
cd src/ProtectBroker.Web

# Install dependencies
npm install

# Create .env.local for development
cp .env.example .env.local

# Update .env.local:
# VITE_API_URL=http://localhost:5000
# VITE_SIGNALR_URL=http://localhost:5000/signalr/devices

# Start dev server
npm run dev

# Frontend will be available at: http://localhost:5173
```

## 5. Test the Application

1. **Open Frontend**: http://localhost:5173
2. **Login** with default credentials:
   - Email: `admin@example.com`
   - Password: `Admin123!`
3. **Check Dashboard**: Should show device counts
4. **Go to Sensors**: Should display sensors from your UniFi Protect
5. **Watch Logs**:
   - Backend: Check console output for sync status
   - Frontend: Open browser DevTools (F12) → Console

## 6. Verify Connections

### Check API Health
```bash
curl -H "Authorization: Bearer YOUR_TOKEN" http://localhost:5000/health
```

### Check SignalR Connection
- Open http://localhost:5173 in browser
- Open DevTools (F12) → Console
- Should see: "SignalR connected"

### Check Database
```bash
psql -U postgres -d protect_broker -c "\dt"  # List tables
```

## Troubleshooting

### Database Connection Error
```
Error: Unable to connect to database
```
**Solution:**
- Check PostgreSQL is running: `psql -U postgres -c "SELECT version();"`
- Verify DB_HOST, DB_USER, DB_PASSWORD in .env
- Ensure database exists: `createdb protect_broker`

### UniFi Protect Connection Error
```
Error: Failed to authenticate with UniFi Protect
```
**Solution:**
- Verify UNIFI_PROTECT_HOST is correct IP/hostname
- Verify UNIFI_PROTECT_PORT (usually 7443)
- Verify access key and secret key are correct
- Check firewall allows HTTPS to NVR

### No Sensors Appearing
```
Sensors list is empty
```
**Solution:**
- Check backend logs for sync errors
- Verify UniFi Protect connection succeeded
- Check that sensors are enabled in UniFi Protect
- Wait 5 minutes for first sync to complete

### Frontend Can't Connect to API
```
Error: Network request failed
```
**Solution:**
- Verify API is running on http://localhost:5000
- Check CORS settings in .env
- Check browser DevTools → Network tab
- Verify VITE_API_URL in frontend .env.local

## Development Workflow

### Make Backend Changes
1. Edit C# files
2. Backend auto-reloads (if using `dotnet watch`)
3. Or restart: `dotnet run --project src/ProtectBroker.Api`

### Make Frontend Changes
1. Edit React/TypeScript files
2. Hot reload happens automatically
3. Check browser console for errors

### Check Logs
- **Backend**: Console output with Serilog structured logs
- **Frontend**: Browser DevTools console
- **Database**: Check `logs/` directory for file logs

## Next Steps

After confirming everything works:
1. Review sensor data in UI
2. Check database for synced devices
3. Monitor logs for any warnings
4. Test with real sensor data
5. Proceed to Phase 3: Relay Control

## Production Deployment

See `docs/DEPLOYMENT.md` and `docs/PROXMOX_SETUP.md` for Docker/Proxmox setup.
