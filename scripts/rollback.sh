#!/bin/bash

# Rollback script
set -e

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

# Check if commit hash is provided
if [ $# -eq 0 ]; then
    echo "Usage: $0 <commit-hash|tag>"
    echo "Example: $0 v1.2.3"
    echo ""
    echo "Recent tags:"
    git tag -l | tail -10
    echo ""
    echo "Recent commits:"
    git log --oneline -10
    exit 1
fi

TARGET=$1
APP_DIR="/home/trecadmin/trecplans"

log() {
    echo -e "$1"
}

# Start rollback
log "${BLUE}Starting rollback to ${TARGET}...${NC}"

cd "$APP_DIR"

# Create backup before rollback
log "${YELLOW}Creating backup before rollback...${NC}"
./scripts/backup-database.sh

# Save current commit hash
CURRENT_COMMIT=$(git rev-parse HEAD)
echo "$CURRENT_COMMIT" > .last_deployment

# Checkout target version
log "${YELLOW}Checking out ${TARGET}...${NC}"
git fetch --all
git checkout "$TARGET"

if [ $? -ne 0 ]; then
    log "${RED}Failed to checkout ${TARGET}${NC}"
    exit 1
fi

# Rebuild and restart
log "${YELLOW}Rebuilding application...${NC}"
docker-compose -f docker-compose.prod.yml build

log "${YELLOW}Restarting services...${NC}"
docker-compose -f docker-compose.prod.yml down
docker-compose -f docker-compose.prod.yml up -d

# Wait for services
log "${YELLOW}Waiting for services to start...${NC}"
sleep 30

# Health check
log "${YELLOW}Running health checks...${NC}"
if ! curl -f http://localhost/health; then
    log "${RED}Frontend health check failed!${NC}"
    exit 1
fi

if ! curl -f http://localhost/api/health; then
    log "${RED}API health check failed!${NC}"
    exit 1
fi

log "${GREEN}Rollback completed successfully!${NC}"
log "${BLUE}Application is now running version: $(git describe --always)${NC}"