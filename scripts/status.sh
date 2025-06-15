#!/bin/bash

# Quick status check script

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

echo -e "${BLUE}=== TrecPlans Status ===${NC}"
echo ""

# Git status
echo -e "${YELLOW}Git Status:${NC}"
echo "Branch: $(git branch --show-current)"
echo "Commit: $(git rev-parse --short HEAD)"
echo "$(git log -1 --pretty=format:'%s (%an, %cr)')"
echo ""

# Docker containers
echo -e "${YELLOW}Docker Containers:${NC}"
docker-compose -f docker-compose.prod.yml ps
echo ""

# Quick health check
echo -e "${YELLOW}Quick Health Check:${NC}"
if curl -f -s http://localhost/health > /dev/null; then
    echo -e "Frontend: ${GREEN}✓ OK${NC}"
else
    echo -e "Frontend: ${RED}✗ FAILED${NC}"
fi

if curl -f -s http://localhost/api/health > /dev/null; then
    echo -e "API: ${GREEN}✓ OK${NC}"
else
    echo -e "API: ${RED}✗ FAILED${NC}"
fi
echo ""

# System resources
echo -e "${YELLOW}System Resources:${NC}"
echo "Disk: $(df -h / | awk 'NR==2{printf "%s/%s (%s)", $3,$2,$5}')"
echo "Memory: $(free -h | awk 'NR==2{printf "%s/%s", $3,$2}')"
echo "Load: $(uptime | awk -F'load average:' '{print $2}')"
echo ""

# Last backup
echo -e "${YELLOW}Last Backup:${NC}"
if [ -d "/home/trecadmin/backups" ]; then
    latest_backup=$(ls -t /home/trecadmin/backups/MessageRDB_backup_*.gz 2>/dev/null | head -1)
    if [ -n "$latest_backup" ]; then
        echo "$(basename "$latest_backup") ($(stat -c %y "$latest_backup" | cut -d' ' -f1))"
    else
        echo "No backups found"
    fi
else
    echo "Backup directory not found"
fi