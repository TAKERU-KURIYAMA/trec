#!/usr/bin/env python3
"""
ターミナル監視プログラム - 確認プロンプトに自動でyesを送信
"""

import subprocess
import sys
import time
import re
import threading
import signal
import os
from queue import Queue, Empty

class AutoConfirm:
    def __init__(self):
        self.running = False
        self.process = None
        self.patterns = [
            r'.*\?\s*\[?[yY]/[nN]\]?\s*:?\s*$',  # y/n形式
            r'.*\?\s*\(y/n\)\s*:?\s*$',           # (y/n)形式
            r'.*\?\s*\[Y/n\]\s*:?\s*$',           # [Y/n]形式
            r'.*\?\s*\[y/N\]\s*:?\s*$',           # [y/N]形式
            r'.*Do you want to.*\?\s*$',          # Do you want to...?
            r'.*Are you sure.*\?\s*$',            # Are you sure...?
            r'.*Continue.*\?\s*$',                # Continue...?
            r'.*Proceed.*\?\s*$',                 # Proceed...?
        ]
        
    def is_confirmation_prompt(self, text):
        """確認プロンプトかどうかを判定"""
        for pattern in self.patterns:
            if re.search(pattern, text, re.IGNORECASE):
                return True
        return False
    
    def read_output(self, pipe, queue):
        """プロセスの出力を非同期で読み取り"""
        try:
            while self.running:
                line = pipe.readline()
                if line:
                    queue.put(line.decode('utf-8', errors='ignore'))
                else:
                    break
        except Exception as e:
            print(f"Error reading output: {e}")
    
    def monitor_process(self, command):
        """プロセスを監視して確認プロンプトに自動応答"""
        print(f"Starting command: {' '.join(command)}")
        print("Monitoring for confirmation prompts...")
        
        try:
            self.process = subprocess.Popen(
                command,
                stdin=subprocess.PIPE,
                stdout=subprocess.PIPE,
                stderr=subprocess.STDOUT,
                bufsize=0,
                universal_newlines=False
            )
            
            self.running = True
            output_queue = Queue()
            
            # 出力読み取り用スレッド
            output_thread = threading.Thread(
                target=self.read_output,
                args=(self.process.stdout, output_queue)
            )
            output_thread.daemon = True
            output_thread.start()
            
            current_line = ""
            
            while self.running and self.process.poll() is None:
                try:
                    # 非ブロッキングで出力を取得
                    char = output_queue.get(timeout=0.1)
                    sys.stdout.write(char)
                    sys.stdout.flush()
                    
                    # 行バッファを更新
                    if char == '\n':
                        if self.is_confirmation_prompt(current_line.strip()):
                            print("\n[AUTO-CONFIRM] Detected confirmation prompt, sending 'yes'")
                            self.process.stdin.write(b'yes\n')
                            self.process.stdin.flush()
                        current_line = ""
                    else:
                        current_line += char
                        
                        # 行末の確認プロンプトもチェック
                        if current_line.strip() and self.is_confirmation_prompt(current_line.strip()):
                            print("\n[AUTO-CONFIRM] Detected confirmation prompt, sending 'yes'")
                            self.process.stdin.write(b'yes\n')
                            self.process.stdin.flush()
                            current_line = ""
                    
                except Empty:
                    continue
                except Exception as e:
                    print(f"Error in monitoring loop: {e}")
                    break
            
            # プロセス終了まで待機
            if self.process:
                return_code = self.process.wait()
                print(f"\n[AUTO-CONFIRM] Process finished with return code: {return_code}")
                return return_code
                
        except KeyboardInterrupt:
            print("\n[AUTO-CONFIRM] Interrupted by user")
            self.stop()
            return 130
        except Exception as e:
            print(f"Error running command: {e}")
            return 1
    
    def stop(self):
        """プロセスを停止"""
        self.running = False
        if self.process and self.process.poll() is None:
            try:
                self.process.terminate()
                time.sleep(1)
                if self.process.poll() is None:
                    self.process.kill()
            except:
                pass

def signal_handler(signum, frame):
    """シグナルハンドラ"""
    print("\n[AUTO-CONFIRM] Received signal, stopping...")
    sys.exit(0)

def main():
    if len(sys.argv) < 2:
        print("Usage: python3 auto-confirm.py <command> [args...]")
        print("Example: python3 auto-confirm.py claude")
        print("Example: python3 auto-confirm.py git push origin main")
        sys.exit(1)
    
    # シグナルハンドラを設定
    signal.signal(signal.SIGINT, signal_handler)
    signal.signal(signal.SIGTERM, signal_handler)
    
    command = sys.argv[1:]
    auto_confirm = AutoConfirm()
    
    try:
        return_code = auto_confirm.monitor_process(command)
        sys.exit(return_code)
    except Exception as e:
        print(f"Fatal error: {e}")
        sys.exit(1)

if __name__ == "__main__":
    main()