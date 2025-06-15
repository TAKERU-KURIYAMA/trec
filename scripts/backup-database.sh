#!/bin/bash

# Configuration
BACKUP_DIR="/backup"
HOST_BACKUP_DIR="/home/trecadmin/backups"
RETENTION_DAYS=7
TIMESTAMP=$(date +%Y%m%d_%H%M%S)
BACKUP_NAME="MessageRDB_backup_${TIMESTAMP}.bak"

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m'

# Load environment variables
if [ -f .env.production ]; then
    export $(cat .env.production | grep -v '^#' | xargs)
fi

echo -e "${YELLOW}Starting database backup...${NC}"

# Create backup directory if it doesn't exist
mkdir -p "$HOST_BACKUP_DIR"

# Create backup
docker exec trecplans-db-1 /opt/mssql-tools/bin/sqlcmd \
    -S localhost -U sa -P "$SA_PASSWORD" \
    -Q "BACKUP DATABASE [MessageRDB] TO DISK = N'${BACKUP_DIR}/${BACKUP_NAME}' WITH NOFORMAT, NOINIT, NAME = 'MessageRDB-full', SKIP, NOREWIND, NOUNLOAD, STATS = 10"

if [ $? -eq 0 ]; then
    echo -e "${GREEN}Backup completed successfully: ${BACKUP_NAME}${NC}"
    
    # Compress backup
    docker exec trecplans-db-1 gzip "${BACKUP_DIR}/${BACKUP_NAME}"
    echo -e "${GREEN}Backup compressed: ${BACKUP_NAME}.gz${NC}"
    
    # Copy to host
    docker cp "trecplans-db-1:${BACKUP_DIR}/${BACKUP_NAME}.gz" "$HOST_BACKUP_DIR/"
    
    # Upload to object storage (optional)
    # Example for AWS S3
    # aws s3 cp "${HOST_BACKUP_DIR}/${BACKUP_NAME}.gz" "s3://your-backup-bucket/database/"
    
    # Example for ConoHa Object Storage
    # swift upload trecplans-backups "${HOST_BACKUP_DIR}/${BACKUP_NAME}.gz"
    
    # Clean old backups
    echo "Cleaning old backups..."
    find "$HOST_BACKUP_DIR" -name "MessageRDB_backup_*.gz" -mtime +${RETENTION_DAYS} -delete
    
    # Also clean inside container
    docker exec trecplans-db-1 find "${BACKUP_DIR}" -name "MessageRDB_backup_*.gz" -mtime +${RETENTION_DAYS} -delete
    
    echo -e "${GREEN}Backup process completed!${NC}"
else
    echo -e "${RED}Backup failed!${NC}"
    exit 1
fi