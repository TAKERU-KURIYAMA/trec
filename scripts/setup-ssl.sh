#!/bin/bash

# SSL Certificate setup script
set -e

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

# Check if domain and email are provided
if [ $# -lt 2 ]; then
    echo "Usage: $0 <domain> <email>"
    echo "Example: $0 trecplans.com admin@trecplans.com"
    exit 1
fi

DOMAIN=$1
EMAIL=$2
NGINX_CONF="/home/trecadmin/trecplans/nginx/default-ssl.conf"

echo -e "${BLUE}Setting up SSL certificate for ${DOMAIN}...${NC}"

# Update nginx configuration with actual domain
echo -e "${YELLOW}Updating nginx configuration...${NC}"
sed -i "s/your-domain.com/${DOMAIN}/g" "$NGINX_CONF"

# Stop nginx to free port 80
echo -e "${YELLOW}Stopping nginx...${NC}"
docker-compose -f docker-compose.prod.yml stop nginx

# Create webroot directory for certbot
sudo mkdir -p /var/www/certbot

# Get certificate
echo -e "${YELLOW}Obtaining SSL certificate...${NC}"
sudo certbot certonly --standalone \
    -d ${DOMAIN} \
    -d www.${DOMAIN} \
    --non-interactive \
    --agree-tos \
    --email ${EMAIL} \
    --expand

if [ $? -eq 0 ]; then
    echo -e "${GREEN}SSL certificate obtained successfully!${NC}"
else
    echo -e "${RED}Failed to obtain SSL certificate${NC}"
    exit 1
fi

# Start nginx
echo -e "${YELLOW}Starting nginx with SSL...${NC}"
docker-compose -f docker-compose.prod.yml start nginx

# Setup auto-renewal
echo -e "${YELLOW}Setting up automatic renewal...${NC}"
(crontab -l 2>/dev/null || true; echo "0 2 * * * /usr/bin/certbot renew --quiet --pre-hook 'docker-compose -f /home/trecadmin/trecplans/docker-compose.prod.yml stop nginx' --post-hook 'docker-compose -f /home/trecadmin/trecplans/docker-compose.prod.yml start nginx'") | crontab -

# Test renewal
echo -e "${YELLOW}Testing renewal process...${NC}"
sudo certbot renew --dry-run

if [ $? -eq 0 ]; then
    echo -e "${GREEN}SSL certificate setup completed successfully!${NC}"
    echo -e "${BLUE}Your site is now accessible at: https://${DOMAIN}${NC}"
else
    echo -e "${RED}Renewal test failed. Please check the configuration.${NC}"
fi