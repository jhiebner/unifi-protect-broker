# Protect Broker - Debian Container Installation Guide

Complete step-by-step guide for installing Protect Broker on a Debian Proxmox container.

---

## Prerequisites

- [ ] Debian 12 (Bookworm) container on Proxmox with internet access
- [ ] `sudo` privileges (or root access)
- [ ] Already completed: `apt update` and `apt upgrade`
- [ ] UniFi Protect instance accessible on same network
- [ ] UniFi Protect credentials (username, password, IP address)

---

## Step 1: Install .NET 9 SDK

The backend is built with ASP.NET Core 9.

```bash
# Download Microsoft package configuration
wget https://packages.microsoft.com/config/debian/12/packages-microsoft-prod.deb -O packages-microsoft-prod.deb

# Install the package
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb

# Update package index
sudo apt update

# Install .NET 9 SDK (includes runtime)
sudo apt install -y dotnet-sdk-9.0
```

### Verify Installation

```bash
dotnet --version
```

Should output: `9.0.0` or higher

**Troubleshooting:**
- If command not found, add to PATH: `export PATH=$PATH:/usr/bin/dotnet`
- If package conflicts occur, ensure no other .NET versions are installed

---

## Step 2: Install Node.js & npm

The frontend requires Node.js 20 LTS.

```bash
# Install curl first (needed for setup script)
sudo apt install -y curl

# Add NodeSource repository for Node.js 20 LTS
curl -fsSL https://deb.nodesource.com/setup_20.x | sudo -E bash -

# Install Node.js (includes npm)
sudo apt install -y nodejs
```

### Verify Installation

```bash
node --version      # Should be v20.x
npm --version       # Should be 10.x or higher
```

**Troubleshooting:**
- If npm version is outdated: `sudo npm install -g npm@latest`
- Check PATH if commands not found

---

## Step 3: Install PostgreSQL 14

The application uses PostgreSQL for persistent data storage.

```bash
# Install PostgreSQL and optional utilities
sudo apt install -y postgresql postgresql-contrib

# Start the PostgreSQL service
sudo systemctl start postgresql

# Enable PostgreSQL to start on boot
sudo systemctl enable postgresql

# Verify it's running
sudo systemctl status postgresql
```

Should show: `active (running)`

**Troubleshooting:**
- If service fails to start, check: `sudo journalctl -xe | tail -50`
- Ensure port 5432 is not in use: `sudo netstat -tlnp | grep 5432`

---

## Step 4: Create PostgreSQL Database

Create the database that Protect Broker will use.

```bash
# Connect to PostgreSQL as the postgres user
sudo -u postgres psql

# Inside the psql prompt (you'll see `postgres=#`), run:
CREATE DATABASE protect_broker;

# Exit psql
\q
```

### Verify Database Creation

```bash
sudo -u postgres psql -l | grep protect_broker
```

Should show the `protect_broker` database listed.

**Troubleshooting:**
- Database already exists error: Drop it first with `DROP DATABASE protect_broker;`
- Permission denied: Ensure you have sudo privileges

---

## Step 5: Install Git

Required to clone the Protect Broker repository.

```bash
sudo apt install -y git
```

### Verify Installation

```bash
git --version
```

---

## Step 6: Clone and Checkout Repository

Clone Protect Broker from GitHub.

```bash
# Create a projects directory
mkdir -p ~/projects
cd ~/projects

# Clone the repository
git clone https://github.com/jhiebner/unifi-protect-broker.git

# Navigate into the project
cd unifi-protect-broker

# Checkout the Phase 2 branch (latest tested version)
git checkout phase/2-unifi-integration
```

### Verify Checkout

```bash
git branch
# Should show: * phase/2-unifi-integration
```

---

## Step 7: Configure Environment Variables

The application uses `.env` file for configuration.

```bash
# Copy the example environment file
cp .env.example .env

# Open it for editing
nano .env
```

### Critical Configuration Values

Update the following values in `.env`:

#### Database Connection
```
ConnectionStrings__DefaultConnection=Host=localhost;Port=5432;Database=protect_broker;Username=postgres;Password=YOUR_POSTGRES_PASSWORD
```

**Note:** Replace `YOUR_POSTGRES_PASSWORD` with the password you set during PostgreSQL setup (default is empty for local postgres user).

#### UniFi Protect Connection
```
UnifiProtect__Host=192.168.X.X              # IP address of your UniFi Protect system
UnifiProtect__Port=443                      # Usually 443, sometimes 7443
UnifiProtect__Username=admin                # Your Protect admin username
UnifiProtect__Password=YOUR_PROTECT_PASSWORD # Your Protect admin password
UnifiProtect__VerifySsl=false              # Set to false for self-signed certificates
```

#### JWT Configuration
```
Jwt__Key=YOUR_SUPER_SECRET_KEY_32_CHARS_MINIMUM_CHANGE_IN_PRODUCTION
Jwt__Issuer=ProtectBroker
Jwt__Audience=ProtectBroker
Jwt__ExpirationMinutes=60
```

**Note:** Generate a random string for `Jwt__Key` - use: `openssl rand -base64 32`

#### API Settings
```
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:5000

Api__Cors__AllowedOrigins=http://localhost:5173,http://CONTAINER_IP:5173
```

**Note:** Replace `CONTAINER_IP` with your container's actual IP address (e.g., `192.168.100.50`).

### Save and Exit

Press: `Ctrl+X` → `Y` → `Enter`

---

## Step 8: Build Backend

Build the .NET solution.

```bash
cd ~/projects/unifi-protect-broker

# Restore NuGet packages
dotnet restore
# Should complete with: "Restore completed in ..."

# Build the solution
dotnet build
# Should complete with: "Build succeeded"
```

**Troubleshooting:**
- Build failures with version mismatch: Run `dotnet clean` then `dotnet build`
- NuGet restore timeout: `dotnet restore --disable-parallel`
- Out of memory: May need to allocate more container resources

---

## Step 9: Configure Frontend

Configure the React frontend to connect to the backend.

```bash
# Navigate to frontend directory
cd ~/projects/unifi-protect-broker/src/ProtectBroker.Web

# Copy environment template
cp .env.example .env.local

# Edit the configuration
nano .env.local
```

### Frontend Configuration Values

Update:
```
VITE_API_URL=http://localhost:5000
VITE_SIGNALR_URL=http://localhost:5000/signalr/devices
```

**For Remote Access:** If accessing the frontend from another machine:
```
VITE_API_URL=http://CONTAINER_IP:5000
VITE_SIGNALR_URL=http://CONTAINER_IP:5000/signalr/devices
```

Replace `CONTAINER_IP` with your container's IP.

### Save and Exit

Press: `Ctrl+X` → `Y` → `Enter`

---

## Step 10: Install Frontend Dependencies

Install npm packages for the React frontend.

```bash
# Already in: ~/projects/unifi-protect-broker/src/ProtectBroker.Web
npm install

# Should complete with: "added XXX packages"
```

**Troubleshooting:**
- npm ERR! 404: Clear npm cache: `npm cache clean --force` then retry
- Module not found: Delete `node_modules` and package-lock.json, then `npm install` again
- Permissions denied: Usually not an issue on personal containers

---

## Step 11: Setup Systemd Services (Optional but Recommended)

Create systemd service files for automatic startup and restart on failure.

### Create Backend Service

```bash
sudo nano /etc/systemd/system/protect-broker-api.service
```

Paste the following (replace `USERNAME` with your actual username):

```ini
[Unit]
Description=Protect Broker API
After=network.target postgresql.service
Wants=postgresql.service

[Service]
Type=notify
WorkingDirectory=/home/USERNAME/projects/unifi-protect-broker
ExecStart=/usr/bin/dotnet run --project src/ProtectBroker.Api
Restart=on-failure
RestartSec=10
User=USERNAME
Environment="ASPNETCORE_ENVIRONMENT=Production"
EnvironmentFile=/home/USERNAME/projects/unifi-protect-broker/.env

[Install]
WantedBy=multi-user.target
```

**Save and Exit:** `Ctrl+X` → `Y` → `Enter`

### Create Frontend Service

```bash
sudo nano /etc/systemd/system/protect-broker-web.service
```

Paste the following (replace `USERNAME` with your actual username):

```ini
[Unit]
Description=Protect Broker Web Dashboard
After=protect-broker-api.service
Wants=protect-broker-api.service

[Service]
Type=simple
WorkingDirectory=/home/USERNAME/projects/unifi-protect-broker/src/ProtectBroker.Web
ExecStart=/usr/bin/npm run preview
Restart=on-failure
RestartSec=10
User=USERNAME

[Install]
WantedBy=multi-user.target
```

**Save and Exit:** `Ctrl+X` → `Y` → `Enter`

### Enable and Start Services

```bash
# Reload systemd to recognize new services
sudo systemctl daemon-reload

# Enable services to start on boot
sudo systemctl enable protect-broker-api.service
sudo systemctl enable protect-broker-web.service

# Start the services
sudo systemctl start protect-broker-api.service
sudo systemctl start protect-broker-web.service

# Check status
sudo systemctl status protect-broker-api.service
sudo systemctl status protect-broker-web.service
```

Both should show: `active (running)`

---

## Step 12: Verify Installation

### Check Backend API

```bash
curl http://localhost:5000/health
```

Should return JSON with status information.

### Check Frontend

```bash
curl http://localhost:5173 | head -20
```

Should return HTML (first 20 lines of the React app).

### View Logs

If using systemd services:

```bash
# Backend logs (live)
sudo journalctl -u protect-broker-api.service -f

# Frontend logs (live)
sudo journalctl -u protect-broker-web.service -f

# Last 50 lines of backend logs
sudo journalctl -u protect-broker-api.service -n 50
```

Press `Ctrl+C` to exit log view.

---

## Step 13: Access the Application

Open a web browser from any machine that can reach your container.

### URL
```
http://CONTAINER_IP:5173
```

Replace `CONTAINER_IP` with your container's IP address.

### Default Login Credentials

- **Email:** `admin@example.com`
- **Password:** `Admin123!`

**Security Note:** Change these credentials immediately after first login! (Not yet implemented in Phase 2, will be in Phase 3)

---

## Step 14: Verify Device Sync

Confirm that Protect Broker is successfully syncing devices from UniFi Protect.

1. **Log in** to the dashboard with credentials above
2. **Navigate** to "Sensors" page (left sidebar)
3. **Observe** device cards appear with your UniFi Protect devices
4. **Check logs** for sync confirmation:

```bash
sudo journalctl -u protect-broker-api.service -f
```

Look for log messages like:
```
Device sync completed: 3 cameras, 5 sensors
```

---

## Troubleshooting Guide

### Backend Won't Start

**Check logs:**
```bash
sudo journalctl -u protect-broker-api.service -n 50
```

**Common issues:**

| Error | Solution |
|-------|----------|
| Connection refused to database | Ensure PostgreSQL is running: `sudo systemctl status postgresql` |
| Cannot connect to UniFi Protect | Check IP, port, and credentials in `.env` file |
| Port 5000 already in use | Change port in `.env`: `ASPNETCORE_URLS=http://+:5001` |
| JWT key too short | Regenerate: `openssl rand -base64 32` |

### Frontend Won't Load

**Check if running:**
```bash
curl http://localhost:5173
```

**If no response:**
```bash
# Check frontend service status
sudo systemctl status protect-broker-web.service

# View npm errors
sudo journalctl -u protect-broker-web.service -n 50
```

**Rebuild if needed:**
```bash
cd ~/projects/unifi-protect-broker/src/ProtectBroker.Web
npm run build
sudo systemctl restart protect-broker-web.service
```

### No Sensors Appearing

**First, verify backend connectivity to Protect:**
```bash
curl http://localhost:5000/api/debug/protect-status
```

Should return:
```json
{
  "apiConnected": true,
  "websocketConnected": true,
  "lastSync": "2024-01-15T10:30:00Z",
  "deviceCount": 8
}
```

**If `apiConnected` is false:**
- Verify UniFi Protect IP in `.env` is reachable: `ping 192.168.X.X`
- Verify credentials are correct
- Check firewall rules (port 443)
- For self-signed certs, ensure `UnifiProtect__VerifySsl=false`

### PostgreSQL Connection Failed

**Check PostgreSQL is running:**
```bash
sudo systemctl status postgresql
```

**If not running:**
```bash
sudo systemctl start postgresql
sudo systemctl enable postgresql
```

**Test connection:**
```bash
sudo -u postgres psql -d protect_broker
```

Should show: `protect_broker=#`

Type `\q` to exit.

### Port Already in Use

**Find what's using port 5000:**
```bash
sudo lsof -i :5000
```

**Options:**
1. Kill the process: `sudo kill -9 PID` (replace PID)
2. Change port in `.env`: `ASPNETCORE_URLS=http://+:5001`

### CPU or Memory Issues

If the application is consuming excessive resources:

```bash
# Monitor resource usage
top -p $(pgrep -f "dotnet run") # Backend
top -p $(pgrep -f "npm run preview") # Frontend

# Check available memory
free -h

# If memory low, increase container allocation in Proxmox
```

---

## Advanced Configuration

### Enable HTTPS (Self-Signed Certificate)

For production deployment:

```bash
# Generate self-signed certificate
sudo openssl req -x509 -newkey rsa:4096 -keyout /etc/ssl/private/protect-broker.key -out /etc/ssl/certs/protect-broker.crt -days 365 -nodes

# Configure in .env
ASPNETCORE_URLS=https://+:5000
ASPNETCORE_Kestrel__Certificates__Default__Path=/etc/ssl/certs/protect-broker.crt
ASPNETCORE_Kestrel__Certificates__Default__KeyPath=/etc/ssl/private/protect-broker.key

# Update frontend CORS in .env
Api__Cors__AllowedOrigins=https://CONTAINER_IP:5173

# Restart services
sudo systemctl restart protect-broker-api.service
```

Then access via: `https://CONTAINER_IP:5173`

### Nginx Reverse Proxy

For production with proper SSL management:

```bash
# Install Nginx
sudo apt install -y nginx

# Create proxy configuration
sudo nano /etc/nginx/sites-available/protect-broker
```

```nginx
upstream protect_api {
    server localhost:5000;
}

upstream protect_web {
    server localhost:5173;
}

server {
    listen 80;
    server_name CONTAINER_IP;

    location /api {
        proxy_pass http://protect_api;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
    }

    location /signalr {
        proxy_pass http://protect_api;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";
    }

    location / {
        proxy_pass http://protect_web;
        proxy_set_header Host $host;
    }
}
```

```bash
# Enable site
sudo ln -s /etc/nginx/sites-available/protect-broker /etc/nginx/sites-enabled/

# Remove default site
sudo rm /etc/nginx/sites-enabled/default

# Test and reload
sudo nginx -t
sudo systemctl reload nginx
```

Then access via: `http://CONTAINER_IP:80`

---

## Performance Tuning

### Database Connection Pool

Edit `.env`:
```
ConnectionStrings__DefaultConnection=Host=localhost;Port=5432;Database=protect_broker;Username=postgres;Password=YOUR_PASSWORD;Pooling=true;MaxPoolSize=20;MinPoolSize=5;
```

### Device Sync Interval

Edit `.env`:
```
Protect__SyncIntervalSeconds=300  # Default 5 minutes
```

### WebSocket Timeout

Edit `.env`:
```
Protect__WebsocketTimeoutSeconds=30
Protect__WebsocketReconnectMaxWait=120
```

---

## Backup and Restore

### Backup Database

```bash
# Create backup
sudo -u postgres pg_dump protect_broker > ~/protect_broker_backup.sql

# Or compressed
sudo -u postgres pg_dump protect_broker | gzip > ~/protect_broker_backup.sql.gz
```

### Restore Database

```bash
# Restore from backup
sudo -u postgres psql protect_broker < ~/protect_broker_backup.sql

# Or from compressed backup
gunzip < ~/protect_broker_backup.sql.gz | sudo -u postgres psql protect_broker
```

---

## Next Steps

Once installation is complete and working:

1. **Test Phase 2 Features:**
   - View sensors from UniFi Protect in real-time
   - Check SignalR updates in browser console
   - Monitor logs for any errors

2. **Proceed to Phase 3 (Relay Control):**
   - Implementing relay control buttons
   - Activity feed with real-time updates
   - Comprehensive audit logging

3. **Production Hardening:**
   - Generate strong JWT key
   - Setup HTTPS
   - Configure firewall rules
   - Enable database backups
   - Monitor system resources

---

## Quick Reference

| Task | Command |
|------|---------|
| Start backend | `sudo systemctl start protect-broker-api.service` |
| Stop backend | `sudo systemctl stop protect-broker-api.service` |
| Start frontend | `sudo systemctl start protect-broker-web.service` |
| Stop frontend | `sudo systemctl stop protect-broker-web.service` |
| View backend logs | `sudo journalctl -u protect-broker-api.service -f` |
| View frontend logs | `sudo journalctl -u protect-broker-web.service -f` |
| Check database | `sudo -u postgres psql -d protect_broker` |
| Restart PostgreSQL | `sudo systemctl restart postgresql` |
| Get container IP | `hostname -I` |
| Access app | `http://CONTAINER_IP:5173` |

---

## Support

For issues:

1. Check logs: `sudo journalctl -u protect-broker-api.service -f`
2. Verify connectivity to UniFi Protect: `curl https://PROTECT_IP:443`
3. Review `.env` configuration
4. Check database with: `sudo -u postgres psql`

For detailed documentation, see:
- `README.md` - Project overview
- `README_TESTING.md` - Testing guide
- `SETUP_CHECKLIST.md` - Comprehensive setup walkthrough

---

**Installation Complete!** 🎉

Your Protect Broker is now running and ready for testing with your UniFi Protect system.
