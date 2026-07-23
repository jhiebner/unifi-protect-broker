# Protect Broker - Proxmox LXC Deployment Guide

This guide covers deploying Protect Broker on a Debian container in Proxmox VE.

## Prerequisites

- Proxmox VE host with sufficient resources
- LXC container with Debian 12 (Bookworm)
- 4GB RAM minimum (8GB recommended)
- 20GB disk space minimum
- Network access to UniFi Protect system

## Container Creation

### Option 1: Create Container via Proxmox UI

1. **Create Container**
   - Node: Select your Proxmox node
   - Container ID: Auto-select
   - Hostname: `protect-broker`
   - Unprivileged: ✓ (checked)
   - Password: Set secure password
   - Template: Debian 12 Standard

2. **Resources**
   - Cores: 4
   - RAM: 8GB
   - Disk: 50GB

3. **Network**
   - eth0: DHCP or Static IP
   - Ensure DNS is configured

4. **Advanced**
   - Unprivileged Container: Yes (recommended)

### Option 2: Command-line with LXC CLI

```bash
# On Proxmox host
pct create 100 local:vztmpl/debian-12-standard_12.2-1_amd64.tar.zst \
  -hostname protect-broker \
  -cores 4 \
  -memory 8192 \
  -disk 50 \
  -net0 name=eth0,bridge=vmbr0,ip=dhcp \
  -unprivileged 1
```

## Enable Nesting for Docker

For Docker to work properly inside the LXC container, you must enable nesting:

```bash
# On Proxmox host, edit container config
nano /etc/pve/lxc/100.conf

# Add these lines:
lxc.apparmor.profile = lxc-container-default-cgns
features: nesting=1,keyctl=1
```

Alternative (one-liner):
```bash
# On Proxmox host
echo "features: nesting=1,keyctl=1" >> /etc/pve/lxc/100.conf
```

## Setup Inside Container

### 1. Start Container and Enter Shell

```bash
pct start 100
pct shell 100
```

### 2. Update System

```bash
apt-get update
apt-get upgrade -y
apt-get install -y curl wget git sudo
```

### 3. Install Docker

```bash
# Install Docker repository
curl -fsSL https://get.docker.com -o get-docker.sh
sh get-docker.sh

# Verify installation
docker --version
docker run hello-world
```

### 4. Install Docker Compose

```bash
curl -L "https://github.com/docker/compose/releases/latest/download/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
chmod +x /usr/local/bin/docker-compose
docker-compose --version
```

### 5. Install Additional Tools

```bash
apt-get install -y postgresql-client net-tools htop
```

### 6. Create Application Directory

```bash
mkdir -p /opt/protect-broker
cd /opt/protect-broker
```

### 7. Clone Repository

```bash
git clone https://github.com/yourusername/protect-broker.git .
```

Or if using local deployment:

```bash
# Copy files from host
# (On Proxmox host)
pct push 100 /path/to/protect-broker /opt/protect-broker -r
```

## Configuration

### 1. Update `.env` File

```bash
cp .env.example .env
nano .env
```

Update the following:

```env
# PostgreSQL (change default password!)
POSTGRES_PASSWORD=your_secure_password_here

# API Configuration
ASPNETCORE_ENVIRONMENT=Production
JWT_KEY=your_super_secret_key_min_32_chars_long_change_this
JWT_ISSUER=ProtectBroker
JWT_AUDIENCE=ProtectBroker

# UniFi Protect Configuration
UNIFI_PROTECT_HOST=192.168.1.100
UNIFI_PROTECT_USERNAME=admin
UNIFI_PROTECT_PASSWORD=your_protect_password

# SMTP (for email notifications)
SMTP_HOST=smtp.gmail.com
SMTP_PORT=587
SMTP_USERNAME=your@email.com
SMTP_PASSWORD=your_app_password
```

### 2. Update docker-compose.yml

Modify ports and volumes if needed:

```bash
nano docker-compose.yml
```

## Running the Application

### Start Services

```bash
cd /opt/protect-broker
docker-compose up -d
```

### View Logs

```bash
# All services
docker-compose logs -f

# Specific service
docker-compose logs -f api
docker-compose logs -f web
docker-compose logs -f postgres
```

### Check Status

```bash
docker-compose ps
```

### Access Application

- **Web UI**: http://container-ip:5173
- **API**: http://container-ip:5000
- **API Docs**: http://container-ip:5000/swagger

## Persistence

### Database Backup

```bash
# Backup PostgreSQL
docker-compose exec postgres pg_dump -U broker_user protect_broker > backup.sql

# Restore
docker-compose exec -T postgres psql -U broker_user protect_broker < backup.sql
```

### Volume Management

```bash
# List volumes
docker volume ls

# Inspect volume
docker volume inspect protect_broker_postgres_data

# Backup volume
docker run --rm -v protect_broker_postgres_data:/data -v $(pwd):/backup \
  alpine tar czf /backup/postgres_backup.tar.gz -C /data .
```

## Performance Optimization

### Container Settings

```bash
# Adjust limits (on Proxmox host)
nano /etc/pve/lxc/100.conf

# Add/modify:
memory: 8192
swap: 2048
cpulimit: 4
```

### PostgreSQL Tuning

Add to docker-compose.yml environment:

```yaml
environment:
  POSTGRES_INITDB_ARGS: "-c max_connections=200 -c shared_buffers=256MB -c effective_cache_size=1GB"
```

## Troubleshooting

### Docker Not Starting

```bash
# Check Docker daemon
systemctl status docker

# Restart Docker
systemctl restart docker

# Check logs
journalctl -u docker -n 50
```

### Container DNS Issues

```bash
# On Proxmox host, check container resolv.conf
pct push 100 /etc/resolv.conf /etc/resolv.conf

# Or manually set in container
echo "nameserver 8.8.8.8" > /etc/resolv.conf
echo "nameserver 8.8.4.4" >> /etc/resolv.conf
```

### Database Connection Failed

```bash
# Verify PostgreSQL is running
docker-compose ps postgres

# Check logs
docker-compose logs postgres

# Verify connection
docker-compose exec postgres psql -U broker_user -d protect_broker -c "SELECT 1"
```

### WebSocket Connection Issues

Ensure nginx is configured to support WebSocket upgrades:

```bash
# Verify nginx config
docker-compose exec web cat /etc/nginx/conf.d/default.conf | grep -A5 signalr
```

## Monitoring

### System Resources

```bash
# Inside container
htop

# Or from Proxmox host
pct resources 100
```

### Application Monitoring

```bash
# Health checks
curl http://localhost:5000/health
curl http://localhost:5173/
```

## Backups

### Automated Daily Backup

```bash
# Create backup script
cat > /opt/protect-broker/backup.sh << 'BACKUP_EOF'
#!/bin/bash
BACKUP_DIR="/opt/protect-broker/backups"
mkdir -p $BACKUP_DIR
TIMESTAMP=$(date +%Y%m%d_%H%M%S)

cd /opt/protect-broker
docker-compose exec -T postgres pg_dump -U broker_user protect_broker | \
  gzip > $BACKUP_DIR/db_backup_$TIMESTAMP.sql.gz

# Keep only last 7 days
find $BACKUP_DIR -name "db_backup_*.sql.gz" -mtime +7 -delete
BACKUP_EOF

chmod +x /opt/protect-broker/backup.sh

# Add to crontab
(crontab -l 2>/dev/null; echo "0 2 * * * /opt/protect-broker/backup.sh") | crontab -
```

## Security

### Firewall Configuration

```bash
# On Proxmox host
ufw allow from any to any port 5173 proto tcp  # Web UI
ufw allow from any to any port 5000 proto tcp  # API
```

### SSL/TLS Setup (Optional)

For production, use Nginx reverse proxy with Let's Encrypt:

```yaml
# Update docker-compose.yml to expose on 443
ports:
  - "80:80"
  - "443:443"
```

Then configure Certbot for automatic certificate management.

### Default Credentials

**Change immediately in production!**

- Default user: `admin@protectbroker.local`
- Default password: Set during first login
- PostgreSQL user: `broker_user`
- PostgreSQL password: `dev_password_change_in_production`

## Updating Application

```bash
cd /opt/protect-broker

# Pull latest changes
git pull origin main

# Rebuild containers
docker-compose build --no-cache

# Restart services
docker-compose down
docker-compose up -d

# View logs
docker-compose logs -f
```

## Support & Troubleshooting

For issues, collect diagnostic information:

```bash
# System info
uname -a
docker --version
docker-compose --version

# Container status
docker-compose ps

# Recent logs
docker-compose logs --tail=100
```

Save output and include when reporting issues.

## Next Steps

1. Access the web interface at `http://container-ip:5173`
2. Complete initial setup wizard
3. Configure UniFi Protect connection
4. Add users and roles
5. Create automation rules
