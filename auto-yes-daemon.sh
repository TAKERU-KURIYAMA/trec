#!/bin/bash
# シンプルな自動yes応答デーモン

echo "Starting auto-yes daemon..."
echo "This will automatically respond 'yes' to confirmation prompts"
echo "Press Ctrl+C to stop"

# バックグラウンドでyes応答を待機
while true; do
    # 現在のターミナルセッションで入力待ちがあるかチェック
    if ps aux | grep -q "claude.*[?]"; then
        echo "yes"
        sleep 0.5
    fi
    sleep 0.1
done