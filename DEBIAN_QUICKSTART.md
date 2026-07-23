# Protect Broker - Debian Quick Start Reference

**For experienced Linux users.** For detailed walkthrough, see `DEBIAN_INSTALLATION.md`.

---

## 📋 Prerequisites

- Debian 12 container on Proxmox
- `sudo` access
- Already run: `apt update && apt upgrade`
- UniFi Protect accessible on same network

---

## ⚡ Installation (Copy & Paste)

### 1. Install Dependencies

```bash
# .NET 9
wget https://packages.microsoft.com/config/debian/12/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb && rm packages-microsoft-prod.deb
sudo apt update
sudo apt install -y dotnet-sdk-9.0

# Node.js 20 LTS
sudo apt install -y curl
curl -fsSL https://deb.nodesource.com/setup_20.x | sudo -E bash -
sudo apt install -y nodejs

# PostgreSQL 14
sudo apt install -y postgresql postgresql-contrib
sudo systemctl start postgresql
sudo systemctl enable postgresql

# Git
sudo apt install -y git
```

### 2. Setup Database

```bash
sudo -u postgres psql
CREATE DATABASE protect_broker;
\q
```

### 3. Clone Repository

```bash
mkdir -p ~/projects
cd ~/projects
git clone https://github.com/jhiebner/unifi-protect-broker.git
cd unifi-protect-broker
git checkout phase/2-unifi-integration
```

### 4. Configure Environment

```bash
cp .env.example .env
nano .env
```

**Critical values to update:**
- `ConnectionStrings__DefaultConnection` - PostgreSQL connection string
- `UnifiProtect__Host` - Your Protect IP (e.g., 192.168.1.50)
- `UnifiProtect__Port` - Usually 443 or 7443
- `UnifiProtect__Username` & `UnifiProtect__Password` - Protect credentials
- `Jwt__Key` - Generate: `openssl rand -base64 32`
- `Api__Cors__AllowedOrigins` - Set to your container IP

Save: `Ctrl+X` → `Y` → `Enter`

### 5. Build Backend

```bash
dotnet restore
dotnet build
```

### 6. Configure & Build Frontend

```bash
cd src/ProtectBroker.Web
cp .env.example .env.local
nano .env.local
```

Update:
```
VITE_API_URL=http://localhost:5000
VITE_SIGNALR_URL=http://localhost:5000/signalr/devices
```

Save and install:
```bash
npm install
```

---

## 🚀 Run Manually (for Testing)

### Terminal 1 - Backend

```bash
cd ~/projects/unifi-protect-broker
dotnet run --project src/ProtectBroker.Api
```

Should show: "Now listening on: http://localhost:5000"

### Terminal 2 - Frontend

```bash
cd ~/projects/unifi-protect-broker/src/ProtectBroker.Web
npm run dev
```

Should show: "Local: http://localhost:5173"

### Terminal 3 - Access

```bash
curl http://localhost:5000/health    # Backend health
curl http://localhost:5173           # Frontend
```

Then visit: `http://localhost:5173` in browser

---

## 🛠️ Setup Systemd Services (Recommended)

### Backend Service

```bash
sudo nano /etc/systemd/system/protect-broker-api.service
```

Paste (replace USERNAME):
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

### Frontend Service

```bash
sudo nano /etc/systemd/system/protect-broker-web.service
```

Paste (replace USERNAME):
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

### Enable & Start

```bash
sudo systemctl daemon-reload
sudo systemctl enable protect-broker-api.service protect-broker-web.service
sudo systemctl start protect-broker-api.service protect-broker-web.service

# Verify
sudo systemctl status protect-broker-api.service
sudo systemctl status protect-broker-web.service
```

---

## ✅ Verification Checklist

- [ ] `dotnet --version` returns 9.x
- [ ] `node --version` returns 20.x
- [ ] `npm --version` returns 10.x+
- [ ] PostgreSQL running: `sudo systemctl status postgresql`
- [ ] Database exists: `sudo -u postgres psql -l | grep protect_broker`
- [ ] Backend health: `curl http://localhost:5000/health`
- [ ] Frontend loads: `curl http://localhost:5173 | head -10`
- [ ] Access dashboard: `http://localhost:5173` (login: admin@example.com / Admin123!)
- [ ] Sensors appear: Go to "Sensors" page
- [ ] Backend logs show sync: `sudo journalctl -u protect-broker-api.service -f`

---

## 🔧 Common Commands

| Command | Purpose |
|---------|---------|
| `sudo systemctl start protect-broker-api.service` | Start backend |
| `sudo systemctl stop protect-broker-api.service` | Stop backend |
| `sudo systemctl restart protect-broker-api.service` | Restart backend |
| `sudo journalctl -u protect-broker-api.service -f` | Backend logs (live) |
| `sudo journalctl -u protect-broker-api.service -n 50` | Last 50 backend log lines |
| `sudo systemctl start protect-broker-web.service` | Start frontend |
| `sudo systemctl stop protect-broker-web.service` | Stop frontend |
| `curl http://localhost:5000/api/debug/protect-status` | Check Protect connection |
| `hostname -I` | Get container IP |
| `sudo -u postgres psql -d protect_broker` | Access database |

---

## 🚨 Troubleshooting

### Backend won't start
```bash
sudo journalctl -u protect-broker-api.service -n 50
# Check: PostgreSQL running? .env correct? Port 5000 free?
```

### No sensors showing
```bash
curl http://localhost:5000/api/debug/protect-status
# Should show: apiConnected: true, websocketConnected: true
# If false, check Protect IP/credentials in .env
```

### PostgreSQL connection failed
```bash
sudo systemctl status postgresql
# If not running: sudo systemctl start postgresql
sudo -u postgres psql -d protect_broker
# If fails, verify database exists
```

### Port 5000 in use
```bash
# Edit .env:
# ASPNETCORE_URLS=http://+:5001
sudo systemctl restart protect-broker-api.service
```

---

## 📍 Access Application

**URL:** `http://CONTAINER_IP:5173`

**Login:**
- Email: `admin@example.com`
- Password: `Admin123!`

**Get container IP:**
```bash
hostname -I
```

---

## 📚 More Information

- Full guide: `DEBIAN_INSTALLATION.md`
- Testing: `README_TESTING.md`
- Setup checklist: `SETUP_CHECKLIST.md`
- Quick reference: `QUICKSTART.md`

---

**Ready to deploy!** Start with the manual run, then setup systemd services for production. ⚙️
