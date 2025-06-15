#!/bin/bash

# Weekly maintenance script
set -e

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

LOG_FILE="/var/log/trecplans/maintenance-$(date +%Y%m%d_%H%M%S).log"

# Function to log
log() {
    echo -e "$1" | tee -a "$LOG_FILE"
}

log "${BLUE}=== Starting TrecPlans Maintenance - $(date) ===${NC}"

# Create log directory if it doesn't exist
mkdir -p "$(dirname "$LOG_FILE")"

# 1. Backup database
log "${YELLOW}1. Creating database backup...${NC}"
/home/trecadmin/trecplans/scripts/backup-database.sh
log "${GREEN}✓ Database backup completed${NC}"

# 2. Clean Docker system
log "${YELLOW}2. Cleaning Docker system...${NC}"
docker system prune -af --volumes
docker network prune -f
log "${GREEN}✓ Docker cleanup completed${NC}"

# 3. Optimize database
log "${YELLOW}3. Optimizing database...${NC}"
/home/trecadmin/trecplans/scripts/optimize-database.sh
log "${GREEN}✓ Database optimization completed${NC}"

# 4. Clean old logs
log "${YELLOW}4. Cleaning old logs...${NC}"
find /var/log/trecplans -name "*.log" -mtime +30 -delete
find /home/trecadmin/backups -name "*.gz" -mtime +30 -delete
log "${GREEN}✓ Log cleanup completed${NC}"

# 5. Update system packages
log "${YELLOW}5. Updating system packages...${NC}"
sudo apt update
sudo apt upgrade -y
sudo apt autoremove -y
sudo apt autoclean
log "${GREEN}✓ System updates completed${NC}"

# 6. Check and rotate Docker logs
log "${YELLOW}6. Rotating Docker logs...${NC}"
for container in $(docker ps -q); do
    log_file=$(docker inspect --format='{{.LogPath}}' $container)
    if [ -f "$log_file" ]; then
        size=$(du -sh "$log_file" | cut -f1)
        log "Container $(docker inspect --format='{{.Name}}' $container): $size"
    fi
done
log "${GREEN}✓ Log rotation check completed${NC}"

# 7. Security scan
log "${YELLOW}7. Running security checks...${NC}"
# Check for failed login attempts
failed_logins=$(grep "Failed password" /var/log/auth.log | wc -l)
log "Failed login attempts: $failed_logins"

# Check disk space
log "${YELLOW}8. Checking disk space...${NC}"
df -h | tee -a "$LOG_FILE"

# Check memory usage
log "${YELLOW}9. Checking memory usage...${NC}"
free -h | tee -a "$LOG_FILE"

# 10. Generate health report
log "${YELLOW}10. Generating health report...${NC}"
/home/trecadmin/trecplans/scripts/health-check.sh >> "$LOG_FILE"

# Send maintenance report (optional)
# cat "$LOG_FILE" | mail -s "TrecPlans Maintenance Report - $(date)" admin@your-domain.com

log "${BLUE}=== Maintenance Completed - $(date) ===${NC}"
log "Log saved to: $LOG_FILE"