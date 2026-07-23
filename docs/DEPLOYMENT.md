# Protect Broker - Deployment Guide

This guide covers deploying Protect Broker in production environments.

## Table of Contents

1. [Quick Start](#quick-start)
2. [Docker Deployment](#docker-deployment)
3. [Proxmox LXC Deployment](#proxmox-lxc-deployment)
4. [Configuration](#configuration)
5. [Verification](#verification)
6. [Troubleshooting](#troubleshooting)
7. [Monitoring](#monitoring)
8. [Backups](#backups)

## Quick Start

### Prerequisites

- Docker & Docker Compose (for containerized deployment)
- OR Debian 12 LXC container (for Proxmox)
- 4GB RAM minimum (8GB recommended)
- 20GB disk space minimum

### Docker Quick Start

```bash
# Clone repository
git clone https://github.com/jhiebner/unifi-protect-broker.git
cd unifi-protect-broker

# Copy and configure environment
cp .env.example .env
nano .env

# Start services
docker-compose up -d

# Verify health
curl http://localhost:5000/health

# Access application
# API: http://localhost:5000
# Web: http://localhost:5173
```

## Docker Deployment

### Single Machine Deployment

```bash
# Create application directory
mkdir -p /opt/protect-broker
cd /opt/protect-broker

# Clone repository
git clone https://github.com/jhiebner/unifi-protect-broker.git .

# Configure environment
cp .env.example .env
# Edit .env with your settings
```

### Environment Configuration

Create `.env` file with these settings:

```env
# PostgreSQL
POSTGRES_PASSWORD=your_secure_password
ConnectionStrings__DefaultConnection=Host=postgres;Port=5432;Database=protect_broker;Username=broker_user;Password=your_secure_password

# API Configuration
ASPNETCORE_ENVIRONMENT=Production
Jwt__Key=your-secret-key-min-32-chars-change-this
Jwt__Issuer=ProtectBroker
Jwt__Audience=ProtectBroker

# UniFi Protect
UnifiProtect__Host=192.168.1.100
UnifiProtect__Username=admin
UnifiProtect__Password=your_protect_password

# SMTP (Email)
Smtp__Host=smtp.gmail.com
Smtp__Port=587
Smtp__Username=your@email.com
Smtp__Password=app_password
```

### Start Services

```bash
# Start in background
docker-compose up -d

# View logs
docker-compose logs -f

# Check status
docker-compose ps

# Stop services
docker-compose down
```

### Verify Deployment

```bash
# Health check
curl http://localhost:5000/health

# API documentation
curl http://localhost:5000/swagger

# Check services
docker-compose ps
```

## Proxmox LXC Deployment

For detailed Proxmox LXC deployment instructions, see [PROXMOX_SETUP.md](./PROXMOX_SETUP.md).

### Quick Overview

1. Create Debian 12 LXC container
2. Enable nesting: `features: nesting=1,keyctl=1`
3. Install Docker
4. Deploy with docker-compose
5. Configure firewall and reverse proxy (optional)

## Configuration

### Environment Variables

All configuration uses environment variables. See `.env.example` for complete list.

### Database

Migrations run automatically on startup. No manual steps needed.

### SSL/TLS (Production)

For HTTPS, use a reverse proxy:

```nginx
server {
    listen 443 ssl http2;
    server_name your-domain.com;
    
    ssl_certificate /etc/letsencrypt/live/your-domain.com/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/your-domain.com/privkey.pem;
    
    location / {
        proxy_pass http://localhost:5173;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
    }
    
    location /api/ {
        proxy_pass http://localhost:5000;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
    }
}
```

## Verification

### Health Checks

```bash
# Basic health
curl http://localhost:5000/health

# Detailed health
curl http://localhost:5000/health/detailed

# Database check
docker-compose exec postgres psql -U broker_user -d protect_broker -c "SELECT 1"
```

### Service Status

```bash
# List running containers
docker-compose ps

# View API logs
docker-compose logs api

# View database logs
docker-compose logs postgres

# View web logs
docker-compose logs web
```

## Troubleshooting

### Container Won't Start

```bash
# Check logs
docker-compose logs api

# Verify configuration
cat .env

# Rebuild containers
docker-compose build --no-cache
docker-compose up -d
```

### Database Connection Failed

```bash
# Check PostgreSQL
docker-compose logs postgres

# Verify credentials in .env
grep POSTGRES .env

# Test connection
docker-compose exec postgres psql -U broker_user -d protect_broker -c "SELECT 1"
```

### API Not Responding

```bash
# Check API logs
docker-compose logs api

# Verify port
netstat -tlnp | grep 5000

# Restart API
docker-compose restart api
```

### WebSocket Issues

```bash
# Check nginx logs
docker-compose logs web

# Verify SignalR connection
curl -I http://localhost:5000/signalr/negotiate
```

## Monitoring

### View Logs

```bash
# All services
docker-compose logs -f

# Specific service
docker-compose logs -f api
docker-compose logs -f postgres
docker-compose logs -f web

# Last N lines
docker-compose logs --tail=100 api
```

### System Metrics

```bash
# Container resource usage
docker stats

# Disk space
df -h

# Memory usage
free -h

# Process CPU
top
```

### Application Metrics

Visit API health endpoints:
- `/health` - Basic health check
- `/health/detailed` - Detailed component status

## Backups

### Database Backup

```bash
# Backup PostgreSQL
docker-compose exec postgres pg_dump -U broker_user protect_broker > backup.sql

# Backup with gzip
docker-compose exec postgres pg_dump -U broker_user protect_broker | gzip > backup.sql.gz

# Restore
docker-compose exec -T postgres psql -U broker_user protect_broker < backup.sql
```

### Volume Backup

```bash
# Backup PostgreSQL volume
docker run --rm -v protect_broker_postgres_data:/data -v $(pwd):/backup \
  alpine tar czf /backup/postgres_backup.tar.gz -C /data .

# Restore volume
docker run --rm -v protect_broker_postgres_data:/data -v $(pwd):/backup \
  alpine tar xzf /backup/postgres_backup.tar.gz -C /data
```

### Automated Backups

Create `/opt/protect-broker/backup.sh`:

```bash
#!/bin/bash
BACKUP_DIR="/opt/protect-broker/backups"
mkdir -p $BACKUP_DIR
TIMESTAMP=$(date +%Y%m%d_%H%M%S)

cd /opt/protect-broker

# Backup database
docker-compose exec -T postgres pg_dump -U broker_user protect_broker | \
  gzip > $BACKUP_DIR/db_backup_$TIMESTAMP.sql.gz

# Keep only last 30 days
find $BACKUP_DIR -name "db_backup_*.sql.gz" -mtime +30 -delete

echo "Backup completed: $BACKUP_DIR/db_backup_$TIMESTAMP.sql.gz"
```

Add to crontab:

```bash
# Daily backup at 2 AM
0 2 * * * /opt/protect-broker/backup.sh >> /opt/protect-broker/backup.log 2>&1
```

## Production Checklist

Before deploying to production:

- [ ] Change all default passwords in `.env`
- [ ] Set secure JWT key (min 32 characters)
- [ ] Configure SSL/TLS certificates
- [ ] Setup SMTP for email notifications
- [ ] Configure UniFi Protect connection
- [ ] Setup automated backups
- [ ] Configure firewall rules
- [ ] Setup monitoring/alerting
- [ ] Document disaster recovery plan
- [ ] Test backup restore procedure

## Performance Optimization

### Database

```env
# Add to docker-compose.yml PostgreSQL service
POSTGRES_INITDB_ARGS=-c max_connections=200 -c shared_buffers=256MB -c effective_cache_size=1GB
```

### Application

```env
# Increase connection pool
ConnectionStrings__DefaultConnection=...;Maximum Pool Size=100;
```

## Support

For issues:
1. Check logs: `docker-compose logs -f`
2. Verify configuration: `cat .env`
3. See [PROXMOX_SETUP.md](./PROXMOX_SETUP.md) for LXC-specific help
4. Review [ARCHITECTURE.md](./ARCHITECTURE.md) for system design

## Additional Resources

- [README.md](../README.md) - Project overview
- [ARCHITECTURE.md](./ARCHITECTURE.md) - System design
- [PROXMOX_SETUP.md](./PROXMOX_SETUP.md) - Proxmox LXC deployment
- [GitHub Repository](https://github.com/jhiebner/unifi-protect-broker)
