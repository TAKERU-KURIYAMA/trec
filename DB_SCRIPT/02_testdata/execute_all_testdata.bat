@echo off
REM ===================================
REM テストデータ一括投入スクリプト (Windows)
REM ===================================

echo ===================================
echo トレーニングアプリ テストデータ投入
echo ===================================
echo.

REM 環境変数設定（必要に応じて変更）
set SERVER=localhost,1433
set USERNAME=sa
set PASSWORD=Your_password123
set DATABASE=TrecPlansRDB

echo データベース: %DATABASE%
echo サーバー: %SERVER%
echo.

REM 各スクリプトを順次実行
echo [1/5] メニューとタグの投入...
sqlcmd -S %SERVER% -U %USERNAME% -P %PASSWORD% -d %DATABASE% -i 01_insert_menus_and_tags.sql
if %ERRORLEVEL% neq 0 (
    echo エラー: メニューとタグの投入に失敗しました
    pause
    exit /b 1
)

echo [2/5] テストユーザーの投入...
sqlcmd -S %SERVER% -U %USERNAME% -P %PASSWORD% -d %DATABASE% -i 02_insert_test_users.sql
if %ERRORLEVEL% neq 0 (
    echo エラー: テストユーザーの投入に失敗しました
    pause
    exit /b 1
)

echo [3/5] サンプルトレーニングデータの投入...
sqlcmd -S %SERVER% -U %USERNAME% -P %PASSWORD% -d %DATABASE% -i 03_insert_sample_training_data.sql
if %ERRORLEVEL% neq 0 (
    echo エラー: サンプルトレーニングデータの投入に失敗しました
    pause
    exit /b 1
)

echo [4/5] 追加機能データの投入...
sqlcmd -S %SERVER% -U %USERNAME% -P %PASSWORD% -d %DATABASE% -i 04_insert_additional_features.sql
if %ERRORLEVEL% neq 0 (
    echo エラー: 追加機能データの投入に失敗しました
    pause
    exit /b 1
)

echo [5/5] エクササイズタイプタグの投入...
sqlcmd -S %SERVER% -U %USERNAME% -P %PASSWORD% -d %DATABASE% -i 05_add_exercise_type_tags.sql
if %ERRORLEVEL% neq 0 (
    echo エラー: エクササイズタイプタグの投入に失敗しました
    pause
    exit /b 1
)

echo.
echo ===================================
echo テストデータ投入完了！
echo ===================================
echo.
echo 投入されたデータ:
echo - トレーニングメニュー: 29種目
echo - タグマスター: 27種類
echo - テストユーザー: 5名
echo - サンプルトレーニング記録: 約90件
echo - 追加機能データ: プログラム・目標・アチーブメント等
echo - エクササイズ分類: コンパウンド13種目・アイソレーション11種目
echo.
echo 次のステップ: 管理者アカウントの作成
echo 実行: create_admin_via_api.sh
echo.
pause