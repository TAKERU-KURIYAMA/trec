@echo off
REM ===================================
REM テストデータ自動セットアップスクリプト (Windows)
REM ===================================
REM このバッチファイルはWindowsでテストデータを自動投入します
REM 
REM 使用前に以下を設定してください:
REM - SQL Server接続情報
REM - データベース名

setlocal enabledelayedexpansion

echo ===================================
echo テストデータ自動セットアップ
echo ===================================
echo.

REM 設定値（必要に応じて変更してください）
set SERVER=localhost,1433
set DATABASE=TrecPlansRDB
set USERNAME=sa

REM パスワードの入力
echo SQL Server SAユーザーのパスワードを入力してください:
set /p PASSWORD=Password: 

echo.
echo 接続情報:
echo Server: %SERVER%
echo Database: %DATABASE%
echo Username: %USERNAME%
echo.

REM 接続テスト
echo データベース接続をテスト中...
sqlcmd -S %SERVER% -U %USERNAME% -P %PASSWORD% -d %DATABASE% -Q "SELECT 1 as connection_test;" >nul 2>&1

if %ERRORLEVEL% NEQ 0 (
    echo エラー: データベースに接続できません。
    echo 以下を確認してください:
    echo - SQL Serverが起動しているか
    echo - 接続情報が正しいか（サーバー名、ポート、ユーザー名、パスワード）
    echo - データベース %DATABASE% が存在するか
    pause
    exit /b 1
)

echo データベース接続成功！
echo.

REM 実行確認
echo 以下のテストデータが投入されます:
echo - トレーニングメニュー（29種目）
echo - タグマスター（21種類）
echo - テストユーザー（5名、パスワード: Test123!）
echo - サンプルトレーニング記録（30日分）
echo - 追加機能データ（目標、アチーブメント等）
echo.
echo 既存のデータは上書きされる可能性があります。
set /p CONFIRM=続行しますか？ (Y/N): 

if /i "%CONFIRM%" NEQ "Y" (
    echo セットアップを中止しました。
    pause
    exit /b 0
)

echo.
echo ===================================
echo テストデータ投入開始
echo ===================================

REM 各SQLスクリプトを順番に実行
echo 1. メニューとタグを投入中...
sqlcmd -S %SERVER% -U %USERNAME% -P %PASSWORD% -d %DATABASE% -i "01_insert_menus_and_tags.sql"
if %ERRORLEVEL% NEQ 0 (
    echo エラー: メニューとタグの投入に失敗しました。
    pause
    exit /b 1
)
echo    完了
echo.

echo 2. テストユーザーを作成中...
sqlcmd -S %SERVER% -U %USERNAME% -P %PASSWORD% -d %DATABASE% -i "02_insert_test_users.sql"
if %ERRORLEVEL% NEQ 0 (
    echo エラー: テストユーザーの作成に失敗しました。
    pause
    exit /b 1
)
echo    完了
echo.

echo 3. サンプルトレーニングデータを投入中...
sqlcmd -S %SERVER% -U %USERNAME% -P %PASSWORD% -d %DATABASE% -i "03_insert_sample_training_data.sql"
if %ERRORLEVEL% NEQ 0 (
    echo エラー: サンプルトレーニングデータの投入に失敗しました。
    pause
    exit /b 1
)
echo    完了
echo.

echo 4. 追加機能データを投入中...
sqlcmd -S %SERVER% -U %USERNAME% -P %PASSWORD% -d %DATABASE% -i "04_insert_additional_features.sql"
if %ERRORLEVEL% NEQ 0 (
    echo エラー: 追加機能データの投入に失敗しました。
    pause
    exit /b 1
)
echo    完了
echo.

REM 結果確認
echo ===================================
echo 投入結果の確認
echo ===================================
sqlcmd -S %SERVER% -U %USERNAME% -P %PASSWORD% -d %DATABASE% -Q "SELECT '投入されたメニュー数' as 項目, COUNT(*) as 件数 FROM training_menu UNION ALL SELECT '投入されたタグ数', COUNT(*) FROM tag_master UNION ALL SELECT '投入されたユーザー数', COUNT(*) FROM [user] UNION ALL SELECT '投入されたトレーニング記録数', COUNT(*) FROM training_record_set;"

echo.
echo ===================================
echo テストデータ投入完了！
echo ===================================
echo.
echo テストユーザー情報:
echo - beginner_user     / Test123!  (初心者太郎)
echo - intermediate_user / Test123!  (トレーニング花子)
echo - advanced_user     / Test123!  (マッスル次郎)
echo - demo              / Test123!  (デモユーザー)
echo - admin             / Test123!  (管理者)
echo.
echo アプリケーションで上記のユーザーでログインしてテストできます。
echo.
pause