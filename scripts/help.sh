#!/bin/bash

# Help script showing all available commands

# Colors
BLUE='\033[0;34m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m'

echo -e "${BLUE}=== TrecPlans Deployment Scripts ===${NC}"
echo ""

echo -e "${YELLOW}Setup & Deployment:${NC}"
echo -e "  ${GREEN}./scripts/setup-ssl.sh${NC} domain email    - Setup SSL certificate with Let's Encrypt"
echo -e "  ${GREEN}./scripts/init-database.sh${NC}             - Initialize database with schema and data"
echo -e "  ${GREEN}./scripts/deploy.sh${NC}                    - Deploy application (full deployment)"
echo ""

echo -e "${YELLOW}Backup & Restore:${NC}"
echo -e "  ${GREEN}./scripts/backup-database.sh${NC}           - Create database backup"
echo -e "  ${GREEN}./scripts/restore-database.sh${NC} file     - Restore database from backup file"
echo ""

echo -e "${YELLOW}Monitoring & Maintenance:${NC}"
echo -e "  ${GREEN}./scripts/status.sh${NC}                    - Quick status overview"
echo -e "  ${GREEN}./scripts/health-check.sh${NC}              - Comprehensive health check"
echo -e "  ${GREEN}./scripts/maintenance.sh${NC}               - Weekly maintenance tasks"
echo -e "  ${GREEN}./scripts/optimize-database.sh${NC}         - Database optimization"
echo ""

echo -e "${YELLOW}Emergency & Recovery:${NC}"
echo -e "  ${GREEN}./scripts/rollback.sh${NC} commit-hash      - Rollback to previous version"
echo ""

echo -e "${YELLOW}Utilities:${NC}"
echo -e "  ${GREEN}./scripts/setup-permissions.sh${NC}         - Set execute permissions for all scripts"
echo -e "  ${GREEN}./scripts/help.sh${NC}                      - Show this help message"
echo ""

echo -e "${YELLOW}Configuration Files:${NC}"
echo -e "  ${GREEN}.env.production.example${NC}               - Production environment template"
echo -e "  ${GREEN}.env.staging.example${NC}                  - Staging environment template"
echo -e "  ${GREEN}docker-compose.prod.yml${NC}               - Production Docker Compose"
echo ""

echo -e "${YELLOW}Documentation:${NC}"
echo -e "  ${GREEN}DEPLOYMENT_GUIDE_CONOHA.md${NC}            - Complete deployment guide"
echo -e "  ${GREEN}DEPLOYMENT_README.md${NC}                  - Quick start instructions"
echo ""

echo -e "${YELLOW}Example Usage:${NC}"
echo -e "  # Initial setup"
echo -e "  ./scripts/setup-ssl.sh example.com admin@example.com"
echo -e "  ./scripts/init-database.sh"
echo -e "  ./scripts/deploy.sh"
echo ""
echo -e "  # Daily operations"
echo -e "  ./scripts/status.sh"
echo -e "  ./scripts/backup-database.sh"
echo ""
echo -e "  # Emergency"
echo -e "  ./scripts/rollback.sh v1.2.3"
echo -e "  ./scripts/restore-database.sh MessageRDB_backup_20240113_120000.bak.gz"