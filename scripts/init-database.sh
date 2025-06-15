#!/bin/bash

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Load environment variables
if [ -f .env.production ]; then
    export $(cat .env.production | grep -v '^#' | xargs)
else
    echo -e "${RED}Error: .env.production file not found${NC}"
    exit 1
fi

echo -e "${YELLOW}Initializing database...${NC}"

# Wait for SQL Server to be ready
echo "Waiting for SQL Server to be ready..."
until docker exec trecplans-db-1 /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "$SA_PASSWORD" -Q "SELECT 1" > /dev/null 2>&1
do
    echo "Waiting for SQL Server..."
    sleep 5
done

echo -e "${GREEN}SQL Server is ready!${NC}"

# Run initialization scripts
echo "Running database initialization scripts..."

# Copy init scripts to container
docker cp ./DB_SCRIPT/01_init/01_init_up.sql trecplans-db-1:/tmp/
docker cp ./DB_SCRIPT/01_init/02_add_menu_up.sql trecplans-db-1:/tmp/

# Execute init scripts
docker exec trecplans-db-1 /opt/mssql-tools/bin/sqlcmd \
    -S localhost -U sa -P "$SA_PASSWORD" \
    -i /tmp/01_init_up.sql

if [ $? -eq 0 ]; then
    echo -e "${GREEN}Database schema created successfully${NC}"
else
    echo -e "${RED}Failed to create database schema${NC}"
    exit 1
fi

docker exec trecplans-db-1 /opt/mssql-tools/bin/sqlcmd \
    -S localhost -U sa -P "$SA_PASSWORD" \
    -i /tmp/02_add_menu_up.sql

if [ $? -eq 0 ]; then
    echo -e "${GREEN}Menu data added successfully${NC}"
else
    echo -e "${RED}Failed to add menu data${NC}"
    exit 1
fi

# Create backup directory in container
docker exec trecplans-db-1 mkdir -p /backup

echo -e "${GREEN}Database initialization completed!${NC}"