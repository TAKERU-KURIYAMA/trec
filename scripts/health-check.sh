#!/bin/bash

# Health check script for TrecPlans
set -e

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

# Configuration
FRONTEND_URL="http://localhost/health"
API_URL="http://localhost/api/health"
TIMEOUT=10

# Function to check service
check_service() {
    local name=$1
    local url=$2
    
    echo -n "Checking $name... "
    
    if curl -f -s --connect-timeout $TIMEOUT "$url" > /dev/null; then
        echo -e "${GREEN}✓ OK${NC}"
        return 0
    else
        echo -e "${RED}✗ FAILED${NC}"
        return 1
    fi
}

# Function to check container
check_container() {
    local name=$1
    
    echo -n "Checking container $name... "
    
    if docker ps | grep -q "$name"; then
        local status=$(docker inspect -f '{{.State.Health.Status}}' "$name" 2>/dev/null || echo "no health check")
        if [ "$status" == "healthy" ] || [ "$status" == "no health check" ]; then
            echo -e "${GREEN}✓ Running${NC}"
            return 0
        else
            echo -e "${YELLOW}⚠ Unhealthy ($status)${NC}"
            return 1
        fi
    else
        echo -e "${RED}✗ Not running${NC}"
        return 1
    fi
}

echo -e "${BLUE}=== TrecPlans Health Check ===${NC}"
echo ""

# Check containers
echo -e "${YELLOW}Container Status:${NC}"
check_container "trecplans-frontend-1"
check_container "trecplans-api-1"
check_container "trecplans-nginx-1"
check_container "trecplans-db-1"
check_container "trecplans-redis-1"
echo ""

# Check services
echo -e "${YELLOW}Service Status:${NC}"
check_service "Frontend" "$FRONTEND_URL"
check_service "API" "$API_URL"
echo ""

# Check database connectivity
echo -e "${YELLOW}Database Status:${NC}"
echo -n "Checking database connection... "
if docker exec trecplans-db-1 /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "${SA_PASSWORD}" -Q "SELECT 1" > /dev/null 2>&1; then
    echo -e "${GREEN}✓ Connected${NC}"
else
    echo -e "${RED}✗ Connection failed${NC}"
fi
echo ""

# Check Redis
echo -e "${YELLOW}Redis Status:${NC}"
echo -n "Checking Redis connection... "
if docker exec trecplans-redis-1 redis-cli ping > /dev/null 2>&1; then
    echo -e "${GREEN}✓ Connected${NC}"
else
    echo -e "${RED}✗ Connection failed${NC}"
fi
echo ""

# Check disk space
echo -e "${YELLOW}Disk Space:${NC}"
df -h | grep -E "^/dev|Filesystem"
echo ""

# Check memory usage
echo -e "${YELLOW}Memory Usage:${NC}"
free -h
echo ""

# Docker stats
echo -e "${YELLOW}Container Resource Usage:${NC}"
docker stats --no-stream --format "table {{.Container}}\t{{.CPUPerc}}\t{{.MemUsage}}\t{{.NetIO}}" | grep trecplans || true

echo ""
echo -e "${BLUE}=== Health Check Complete ===${NC}"