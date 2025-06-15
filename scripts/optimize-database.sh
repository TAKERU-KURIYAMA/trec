#!/bin/bash

# Database optimization script

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m'

# Load environment variables
if [ -f .env.production ]; then
    export $(cat .env.production | grep -v '^#' | xargs)
fi

echo -e "${YELLOW}Running database optimization...${NC}"

# Update statistics
echo "Updating database statistics..."
docker exec trecplans-db-1 /opt/mssql-tools/bin/sqlcmd \
    -S localhost -U sa -P "$SA_PASSWORD" \
    -Q "USE MessageRDB; EXEC sp_updatestats;"

# Rebuild indexes
echo "Rebuilding database indexes..."
docker exec trecplans-db-1 /opt/mssql-tools/bin/sqlcmd \
    -S localhost -U sa -P "$SA_PASSWORD" \
    -Q "
    USE MessageRDB;
    DECLARE @TableName VARCHAR(255)
    DECLARE @sql NVARCHAR(500)
    DECLARE TableCursor CURSOR FOR
    SELECT TABLE_SCHEMA + '.' + TABLE_NAME
    FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_TYPE = 'BASE TABLE' AND TABLE_CATALOG = 'MessageRDB'
    
    OPEN TableCursor
    FETCH NEXT FROM TableCursor INTO @TableName
    WHILE @@FETCH_STATUS = 0
    BEGIN
        SET @sql = 'ALTER INDEX ALL ON ' + @TableName + ' REBUILD'
        PRINT 'Rebuilding indexes for ' + @TableName
        EXEC (@sql)
        FETCH NEXT FROM TableCursor INTO @TableName
    END
    CLOSE TableCursor
    DEALLOCATE TableCursor
    "

# Clean up transaction log
echo "Shrinking transaction log..."
docker exec trecplans-db-1 /opt/mssql-tools/bin/sqlcmd \
    -S localhost -U sa -P "$SA_PASSWORD" \
    -Q "
    USE MessageRDB;
    DBCC SHRINKFILE (MessageRDB_log, 10);
    "

# Check database integrity
echo "Checking database integrity..."
docker exec trecplans-db-1 /opt/mssql-tools/bin/sqlcmd \
    -S localhost -U sa -P "$SA_PASSWORD" \
    -Q "
    USE MessageRDB;
    DBCC CHECKDB('MessageRDB') WITH NO_INFOMSGS;
    "

echo -e "${GREEN}Database optimization completed!${NC}"