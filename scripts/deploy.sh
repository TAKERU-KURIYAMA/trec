#!/bin/bash

# Deployment script for TrecPlans
set -e

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

# Configuration
APP_DIR="/home/trecadmin/trecplans"
BACKUP_DIR="/home/trecadmin/backups"
LOG_FILE="/var/log/trecplans/deployment-$(date +%Y%m%d_%H%M%S).log"

# Function to log messages
log() {
    echo -e "$1" | tee -a "$LOG_FILE"
}

# Function to check if command was successful
check_status() {
    if [ $? -eq 0 ]; then
        log "${GREEN}✓ $1${NC}"
    else
        log "${RED}✗ $1 failed${NC}"
        exit 1
    fi
}

# Start deployment
log "${BLUE}Starting TrecPlans deployment - $(date)${NC}"

# Create necessary directories
mkdir -p "$BACKUP_DIR"
mkdir -p "$(dirname "$LOG_FILE")"

# Pull latest code
log "${YELLOW}Pulling latest code from repository...${NC}"
cd "$APP_DIR"
git pull origin main
check_status "Git pull"

# Load environment variables
if [ -f .env.production ]; then
    export $(cat .env.production | grep -v '^#' | xargs)
else
    log "${RED}Error: .env.production file not found${NC}"
    exit 1
fi

# Backup current deployment
log "${YELLOW}Creating backup of current deployment...${NC}"
./scripts/backup-database.sh
check_status "Database backup"

# Build new images
log "${YELLOW}Building Docker images...${NC}"
docker-compose -f docker-compose.prod.yml build --no-cache
check_status "Docker build"

# Stop current containers
log "${YELLOW}Stopping current containers...${NC}"
docker-compose -f docker-compose.prod.yml down
check_status "Container shutdown"

# Start new containers
log "${YELLOW}Starting new containers...${NC}"
docker-compose -f docker-compose.prod.yml up -d
check_status "Container startup"

# Wait for services to be healthy
log "${YELLOW}Waiting for services to be healthy...${NC}"
sleep 30

# Health check
log "${YELLOW}Running health checks...${NC}"
curl -f http://localhost/health || exit 1
check_status "Frontend health check"

curl -f http://localhost/api/health || exit 1
check_status "API health check"

# Run database migrations if needed
log "${YELLOW}Checking for database migrations...${NC}"
# Uncomment if using EF Core migrations
# docker exec trecplans-api-1 dotnet ef database update
# check_status "Database migrations"

# Clear cache
log "${YELLOW}Clearing application cache...${NC}"
docker exec trecplans-redis-1 redis-cli -a "$REDIS_PASSWORD" FLUSHALL
check_status "Cache clear"

# Cleanup old images
log "${YELLOW}Cleaning up old Docker images...${NC}"
docker image prune -f
check_status "Image cleanup"

log "${GREEN}Deployment completed successfully!${NC}"
log "${BLUE}Deployment finished - $(date)${NC}"

# Send notification (optional)
# curl -X POST https://hooks.slack.com/services/YOUR/WEBHOOK/URL \
#     -H 'Content-type: application/json' \
#     -d '{"text":"TrecPlans deployment completed successfully!"}'