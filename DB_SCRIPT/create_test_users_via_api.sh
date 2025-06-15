#!/bin/bash

# Create test users via API with proper two-stage password hashing
# Password: Test123!
# SHA256("Test123!") = c775e7b757ede630cd0aa1113bd102661ab38829ca52a6422ab782862f268646

API_URL="http://localhost:5001/api/auth/register"
PASSWORD_HASH="c775e7b757ede630cd0aa1113bd102661ab38829ca52a6422ab782862f268646"

echo "Creating test users with password: Test123!"
echo "Client-side SHA256 hash: $PASSWORD_HASH"
echo ""

# Define test users
declare -a users=(
    "beginner_user:初心者太郎"
    "intermediate_user:トレーニング花子"
    "advanced_user:マッスル次郎"
    "demo:デモユーザー"
    "admin:管理者"
)

# Create each user
for user_info in "${users[@]}"; do
    IFS=':' read -r login_id display_name <<< "$user_info"
    
    echo "Creating user: $login_id ($display_name)..."
    
    response=$(curl -s -X POST "$API_URL" \
        -H "Content-Type: application/json" \
        -d "{\"loginId\": \"$login_id\", \"password\": \"$PASSWORD_HASH\", \"displayName\": \"$display_name\"}")
    
    # Check if successful
    if echo "$response" | grep -q "token"; then
        echo "✓ Successfully created user: $login_id"
    else
        echo "✗ Failed to create user: $login_id"
        echo "  Response: $response"
    fi
    echo ""
done

echo "Test user creation complete!"
echo "You can now login with any of these users using password: Test123!"