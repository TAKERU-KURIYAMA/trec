#!/usr/bin/env python3
"""
ターミナル全体を監視して確認プロンプトに自動応答するプログラム
"""

import subprocess
import time
import os
import signal
import sys
import threading
import psutil
from datetime import datetime

class TerminalMonitor:
    def __init__(self):
        self.running = False
        self.monitored_processes = set()
        
    def find_claude_processes(self):
        """Claude プロセスを検索"""
        claude_processes = []
        for proc in psutil.process_iter(['pid', 'name', 'cmdline']):
            try:
                if proc.info['name'] == 'claude':
                    claude_processes.append(proc)
            except (psutil.NoSuchProcess, psutil.AccessDenied):
                continue
        return claude_processes
    
    def inject_yes_response(self, pid):
        """プロセスに yes を送信"""
        try:
            # プロセスの標準入力に yes を送信
            os.system(f'echo "yes" | kill -PIPE {pid}')
        except Exception as e:
            print(f"Failed to inject response to PID {pid}: {e}")
    
    def monitor_process_output(self, pid):
        """プロセスの出力を監視"""
        try:
            # プロセスの出力を監視するためのスクリプト
            monitor_script = f"""
#!/bin/bash
strace -p {pid} -e trace=write -o /tmp/claude_monitor_{pid}.log 2>&1 &
STRACE_PID=$!
sleep 1
tail -f /tmp/claude_monitor_{pid}.log | while read line; do
    if echo "$line" | grep -qE "(\\?.*\\[?[yY]/[nN]\\]?|\\?.*\\(y/n\\)|Do you want|Are you sure|Continue|Proceed)"; then
        echo "yes" > /proc/{pid}/fd/0 2>/dev/null || true
        echo "[AUTO-CONFIRM] Sent 'yes' to PID {pid} at $(date)"
        sleep 0.5
    fi
done
kill $STRACE_PID 2>/dev/null || true
"""
            with open(f'/tmp/monitor_{pid}.sh', 'w') as f:
                f.write(monitor_script)
            os.chmod(f'/tmp/monitor_{pid}.sh', 0o755)
            
            # バックグラウンドで実行
            subprocess.Popen(['/bin/bash', f'/tmp/monitor_{pid}.sh'])
            
        except Exception as e:
            print(f"Error monitoring PID {pid}: {e}")
    
    def start_monitoring(self):
        """監視開始"""
        print("Starting terminal monitor for Claude processes...")
        print("Press Ctrl+C to stop monitoring")
        
        self.running = True
        
        while self.running:
            try:
                claude_processes = self.find_claude_processes()
                
                for proc in claude_processes:
                    pid = proc.pid
                    if pid not in self.monitored_processes:
                        print(f"[{datetime.now()}] Found new Claude process: PID {pid}")
                        self.monitor_process_output(pid)
                        self.monitored_processes.add(pid)
                
                # 終了したプロセスを除去
                active_pids = {proc.pid for proc in claude_processes}
                self.monitored_processes &= active_pids
                
                time.sleep(2)  # 2秒ごとにチェック
                
            except KeyboardInterrupt:
                break
            except Exception as e:
                print(f"Error in monitoring loop: {e}")
                time.sleep(5)
    
    def stop(self):
        """監視停止"""
        self.running = False
        print("\nStopping terminal monitor...")
        
        # 一時ファイルを削除
        for pid in self.monitored_processes:
            try:
                os.remove(f'/tmp/monitor_{pid}.sh')
                os.remove(f'/tmp/claude_monitor_{pid}.log')
            except:
                pass

def signal_handler(signum, frame):
    """シグナルハンドラ"""
    print("\nReceived interrupt signal")
    sys.exit(0)

def main():
    signal.signal(signal.SIGINT, signal_handler)
    signal.signal(signal.SIGTERM, signal_handler)
    
    monitor = TerminalMonitor()
    try:
        monitor.start_monitoring()
    except KeyboardInterrupt:
        pass
    finally:
        monitor.stop()

if __name__ == "__main__":
    main()