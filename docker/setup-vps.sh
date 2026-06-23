#!/bin/bash
# VPS Setup Script for IoTPlatform
# Run once with: bash docker/setup-vps.sh
# Then configure GitHub Actions secrets + point DNS A-records to this VPS IP.

set -euo pipefail

# Configuration
REPO_URL="https://github.com/liewxen/IotPlaform.git"
REPO_DIR="/home/iotplatform"
RUNNER_NAME="${RUNNER_NAME:-iotplatform-vps-runner}"
RUNNER_TOKEN="${RUNNER_TOKEN:?Set RUNNER_TOKEN env var from GitHub}"
DOMAIN="phytolink-venture.com"
VPS_IP=$(hostname -I | awk '{print $1}')

echo "=== IoTPlatform VPS Setup ==="
echo "Repo: $REPO_URL"
echo "Runner: $RUNNER_NAME"
echo "Domain: $DOMAIN"
echo "VPS IP: $VPS_IP"
echo ""

# 1. Update system
echo "[1/8] Updating system..."
sudo apt-get update
sudo apt-get upgrade -y

# 2. Install Docker + Docker Compose
echo "[2/8] Installing Docker..."
sudo apt-get install -y curl gnupg lsb-release ca-certificates
curl -fsSL https://download.docker.com/linux/ubuntu/gpg | sudo gpg --dearmor -o /usr/share/keyrings/docker-archive-keyring.gpg
echo "deb [arch=amd64 signed-by=/usr/share/keyrings/docker-archive-keyring.gpg] https://download.docker.com/linux/ubuntu $(lsb_release -cs) stable" | \
  sudo tee /etc/apt/sources.list.d/docker.list > /dev/null
sudo apt-get update
sudo apt-get install -y docker-ce docker-ce-cli containerd.io docker-compose-plugin

# 3. Create iotplatform user + Docker perms
echo "[3/8] Setting up iotplatform user..."
sudo useradd -m -s /bin/bash iotplatform || true
sudo usermod -aG docker iotplatform

# 4. Clone repo
echo "[4/8] Cloning repository..."
sudo mkdir -p "$REPO_DIR"
sudo chown iotplatform:iotplatform "$REPO_DIR"
if [ -d "$REPO_DIR/.git" ]; then
  cd "$REPO_DIR" && git pull origin main
else
  git clone "$REPO_URL" "$REPO_DIR"
  cd "$REPO_DIR"
fi

# 5. Create shared Docker network
echo "[5/8] Creating Docker network..."
docker network create iotplatform-network || true

# 6. Start PostgreSQL + Mosquitto (shared, once)
echo "[6/8] Starting shared infrastructure (PostgreSQL + Mosquitto)..."
cd "$REPO_DIR"
docker compose up -d postgres mosquitto

# 7. Install Nginx + Certbot
echo "[7/8] Installing Nginx + Certbot..."
sudo apt-get install -y nginx certbot python3-certbot-nginx

# Copy Nginx config
sudo cp docker/nginx/nginx.conf /etc/nginx/nginx.conf
sudo mkdir -p /var/www/certbot
sudo nginx -t
sudo systemctl restart nginx

# 8. Issue SSL certificates (requires DNS to be pointing already)
echo "[8/8] Issuing SSL certificates (this may fail if DNS doesn't point to $VPS_IP yet)..."
for domain in iotproductionbackend.$DOMAIN iotproductionfrontend.$DOMAIN iotstagingbackend.$DOMAIN iotstagingfrontend.$DOMAIN; do
  sudo certbot certonly --nginx -d "$domain" --non-interactive --agree-tos --email admin@$DOMAIN --no-eff-email || \
    echo "Note: Failed for $domain (DNS may not be configured yet; retry later with: sudo certbot renew)"
done

sudo systemctl reload nginx

# 9. Register GitHub self-hosted runner
echo ""
echo "=== GitHub Actions Self-Hosted Runner Setup ==="
echo "Registering runner: $RUNNER_NAME"

RUNNER_DIR="/home/iotplatform/actions-runner"
sudo mkdir -p "$RUNNER_DIR"
sudo chown iotplatform:iotplatform "$RUNNER_DIR"
cd "$RUNNER_DIR"

# Download latest runner (adjust version if needed)
RUNNER_VERSION=$(curl -s https://api.github.com/repos/actions/runner/releases/latest | grep tag_name | cut -d '"' -f 4 | sed 's/v//')
RUNNER_URL="https://github.com/actions/runner/releases/download/v${RUNNER_VERSION}/actions-runner-linux-x64-${RUNNER_VERSION}.tar.gz"

sudo -u iotplatform bash << EOF
  cd "$RUNNER_DIR"
  rm -rf * || true
  curl -o runner.tar.gz -L "$RUNNER_URL"
  tar xzf runner.tar.gz
  rm runner.tar.gz

  # Configure (non-interactive)
  ./config.sh \
    --url "https://github.com/liewxen/IotPlaform" \
    --token "$RUNNER_TOKEN" \
    --name "$RUNNER_NAME" \
    --runnergroup "Default" \
    --labels "self-hosted,iotplatform,vps" \
    --work "/home/iotplatform/actions-work" \
    --unattended \
    --replace
EOF

# Install runner as a systemd service
sudo "$RUNNER_DIR"/svc.sh install iotplatform
sudo systemctl enable actions.runner.*
sudo systemctl start actions.runner.*

echo ""
echo "=== Setup Complete ==="
echo ""
echo "✓ Docker + Docker Compose installed"
echo "✓ PostgreSQL + Mosquitto running"
echo "✓ Nginx + Certbot running (TLS)"
echo "✓ GitHub self-hosted runner registered: $RUNNER_NAME"
echo ""
echo "Next steps:"
echo "1. Point your DNS A-records to: $VPS_IP"
echo "   - iotproductionbackend.$DOMAIN"
echo "   - iotproductionfrontend.$DOMAIN"
echo "   - iotstagingbackend.$DOMAIN"
echo "   - iotstagingfrontend.$DOMAIN"
echo ""
echo "2. Set GitHub Actions secrets (Settings > Secrets and variables > Actions):"
echo "   - DB_PASSWORD (PostgreSQL password)"
echo "   - JWT_SIGNING_KEY (long random string, min 32 chars)"
echo ""
echo "3. Verify runner is online:"
echo "   Settings > Actions > Runners"
echo ""
echo "4. Push to 'staging' or 'main' branch to trigger deployment"
echo ""
