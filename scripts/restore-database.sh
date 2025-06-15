#!/bin/bash

# Check if backup file is provided
if [ $# -eq 0 ]; then
    echo "Usage: $0 <backup-file>"
    echo "Example: $0 MessageRDB_backup_20240113_120000.bak.gz"
    echo ""
    echo "Available backups:"
    ls -la /home/trecadmin/backups/MessageRDB_backup_*.gz 2>/dev/null
    exit 1
fi

BACKUP_FILE=$1
BACKUP_DIR="/backup"
HOST_BACKUP_DIR="/home/trecadmin/backups"

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m'

# Load environment variables
if [ -f .env.production ]; then
    export $(cat .env.production | grep -v '^#' | xargs)
fi

# Check if backup file exists
if [ ! -f "${HOST_BACKUP_DIR}/${BACKUP_FILE}" ]; then
    echo -e "${RED}Error: Backup file not found: ${HOST_BACKUP_DIR}/${BACKUP_FILE}${NC}"
    exit 1
fi

echo -e "${YELLOW}Starting database restore from ${BACKUP_FILE}...${NC}"

# Copy backup to container
docker cp "${HOST_BACKUP_DIR}/${BACKUP_FILE}" "trecplans-db-1:${BACKUP_DIR}/"

# Decompress if needed
if [[ $BACKUP_FILE == *.gz ]]; then
    docker exec trecplans-db-1 gunzip "${BACKUP_DIR}/${BACKUP_FILE}"
    BACKUP_FILE=${BACKUP_FILE%.gz}
fi

# Set database to single user mode
echo "Setting database to single user mode..."
docker exec trecplans-db-1 /opt/mssql-tools/bin/sqlcmd \
    -S localhost -U sa -P "$SA_PASSWORD" \
    -Q "ALTER DATABASE [MessageRDB] SET SINGLE_USER WITH ROLLBACK IMMEDIATE"

# Restore database
echo "Restoring database..."
docker exec trecplans-db-1 /opt/mssql-tools/bin/sqlcmd \
    -S localhost -U sa -P "$SA_PASSWORD" \
    -Q "RESTORE DATABASE [MessageRDB] FROM DISK = N'${BACKUP_DIR}/${BACKUP_FILE}' WITH FILE = 1, NOUNLOAD, REPLACE, STATS = 10"

# Set database back to multi user mode
echo "Setting database back to multi user mode..."
docker exec trecplans-db-1 /opt/mssql-tools/bin/sqlcmd \
    -S localhost -U sa -P "$SA_PASSWORD" \
    -Q "ALTER DATABASE [MessageRDB] SET MULTI_USER"

if [ $? -eq 0 ]; then
    echo -e "${GREEN}Database restored successfully!${NC}"
    
    # Restart API to ensure connections are refreshed
    echo "Restarting API service..."
    docker-compose -f docker-compose.prod.yml restart api
    
else
    echo -e "${RED}Database restore failed!${NC}"
    exit 1
fi