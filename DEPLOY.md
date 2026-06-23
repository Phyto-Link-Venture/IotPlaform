# IoTPlatform Deployment Guide

This guide covers deploying to **staging** (via `staging` branch) and **production** (via `main` branch)
to your self-hosted VPS at `72.60.108.40` with **Nginx + Let's Encrypt TLS** and **GitHub Actions self-hosted runner**.

## Architecture Overview

```
GitHub Push to main/staging
    ↓
GitHub Actions (self-hosted runner on VPS)
    ↓
Build API image + Frontend image
    ↓
Run containers on VPS (API:8080, Frontend:3000/3001)
    ↓
Nginx reverse proxy (TLS termination)
    ↓
Public: https://iotproductionbackend/frontend.phytolink-venture.com
        https://iotstagingbackend/frontend.phytolink-venture.com
```

## One-Time VPS Setup

### 1. SSH to VPS
```bash
ssh root@72.60.108.40
# Enter password when prompted
```

### 2. Run Setup Script
```bash
cd /tmp
git clone https://github.com/liewxen/IotPlaform.git
cd IotPlaform

# Get GitHub runner registration token from:
# GitHub > Settings > Actions > Runners > New self-hosted runner > Copy the token
export RUNNER_TOKEN="<paste-token-here>"

bash docker/setup-vps.sh
```

This will:
- Install Docker, Nginx, Certbot
- Register the self-hosted runner
- Issue Let's Encrypt certs for the 4 subdomains
- Start PostgreSQL + Mosquitto

### 3. Point DNS Records
Update your DNS provider (wherever you manage phytolink-venture.com) to point these A-records to `72.60.108.40`:
```
iotproductionbackend.phytolink-venture.com    A    72.60.108.40
iotproductionfrontend.phytolink-venture.com   A    72.60.108.40
iotstagingbackend.phytolink-venture.com       A    72.60.108.40
iotstagingfrontend.phytolink-venture.com      A    72.60.108.40
```

Wait ~5-15 minutes for DNS propagation.

### 4. Set GitHub Actions Secrets
In your GitHub repo:
**Settings > Secrets and variables > Actions > New repository secret**

Add:
- `DB_PASSWORD`: PostgreSQL password (e.g., generate with `openssl rand -base64 32`)
- `JWT_SIGNING_KEY`: Long random string for JWT signing (min 32 chars, e.g., `openssl rand -base64 48`)

These are passed to the containers at deploy time.

### 5. Verify Runner is Online
**Settings > Actions > Runners**

You should see `iotplatform-vps-runner` with status **Idle**.

## Deploying Code

### To Staging
```bash
git push origin staging
```

The self-hosted runner picks up the push, triggers `deploy.yml`:
1. Builds API image + frontend image
2. Stops old containers
3. Runs new containers (API on port 8081 to avoid collision with prod)
4. Tests `/swagger/` endpoint
5. Nginx routes requests to the containers

Staging lives at:
- Backend: https://iotstagingbackend.phytolink-venture.com
- Frontend: https://iotstagingfrontend.phytolink-venture.com

### To Production
```bash
git push origin main
```

Production lives at:
- Backend: https://iotproductionbackend.phytolink-venture.com
- Frontend: https://iotproductionfrontend.phytolink-venture.com

## Monitoring & Troubleshooting

### Check Runner Status
```bash
ssh iotplatform@72.60.108.40
ps aux | grep Runner.Listener
```

### View Logs
```bash
ssh iotplatform@72.60.108.40

# Runner logs
tail -f /home/iotplatform/actions-runner/_diag/Runner_*.log

# Container logs
docker logs iotplatform-api-production
docker logs iotplatform-api-staging
docker logs iotplatform-frontend-production
docker logs iotplatform-frontend-staging

# Nginx
sudo tail -f /var/log/nginx/access.log
sudo tail -f /var/log/nginx/error.log
```

### Renew SSL Certificates
Certbot auto-renews via cron. To manually renew:
```bash
sudo certbot renew
sudo systemctl reload nginx
```

### Stop/Start Containers
```bash
docker stop iotplatform-api-production
docker start iotplatform-api-production
```

### SSH Into Container
```bash
docker exec -it iotplatform-api-production /bin/bash
```

## Database Management

PostgreSQL is shared between staging and production. Each has its own database:
- `iotplatform_production`
- `iotplatform_staging`

Both connect with the same user: `iotplatform`.

### Access Database
```bash
docker exec -it iotplatform-postgres psql -U iotplatform
\l                    # List databases
\c iotplatform_production  # Switch to production DB
\dt                   # List tables
```

### Run Migrations
On first deploy, the EF Core migration runs automatically (see `Program.cs` for design-time factory).
To run manually:

```bash
docker exec iotplatform-api-production dotnet ef database update
```

## Scaling & Optimization

### Next Steps
1. **Session storage**: Refresh tokens should be stored in Redis (not currently implemented).
2. **Static export**: Optimize Next.js frontend to `npm run build && npm run export` for static hosting (no Node server needed).
3. **Database backups**: Set up automated daily backups of PostgreSQL.
4. **Monitoring**: Add Sentry / New Relic / Datadog for error tracking.
5. **CDN**: Consider Cloudflare for DDoS protection + caching.

## Emergency Procedures

### Rollback to Previous Container
```bash
docker images | grep iotplatform-api
docker run ... <old-image-id>  # Run the previous image
```

### Clear Disk Space
```bash
docker system prune -a
docker volume prune
```

### Restart Everything
```bash
docker restart iotplatform-api-production iotplatform-api-staging
docker restart iotplatform-frontend-production iotplatform-frontend-staging
sudo systemctl restart nginx
```

## Security Reminders

⚠️ **Never commit:**
- `.env` files with real secrets
- Private SSH keys
- Database passwords in code

✓ **Always use GitHub Actions secrets** for:
- `DB_PASSWORD`
- `JWT_SIGNING_KEY`
- API keys (AI provider configs are encrypted at rest in the database)

✓ **Keep the VPS password safe:**
- Change it immediately after setup (`passwd`)
- Store it in a password manager (not the chat)
- Use SSH keys instead for regular access

## Support

For questions:
1. Check `CLAUDE.md` for architecture details
2. Review `README.md` for local dev setup
3. Check deploy logs: **GitHub > Actions > Recent runs**
