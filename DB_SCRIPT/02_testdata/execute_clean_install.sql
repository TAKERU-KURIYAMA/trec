-- ===================================
-- クリーンインストール用スクリプト
-- 既存データを完全削除して、新規にテストデータを投入
-- ===================================

USE MessageRDB;
GO

-- SQLCMDモードを有効化（PRINTメッセージ表示用）
:setvar SQLCMDMAXVARTYPEWIDTH 0
:setvar SQLCMDMAXFIXEDTYPEWIDTH 0

PRINT '';
PRINT '===================================';
PRINT 'クリーンインストール開始';
PRINT '===================================';
PRINT '';

-- ===================================
-- STEP 1: 既存データの完全削除
-- ===================================
PRINT 'STEP 1: 既存データを削除中...';
PRINT '';

-- 外部キー制約を一時的に無効化
EXEC sp_MSforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT all';

-- 全テーブルのデータを削除
PRINT '- DailyTrainingRecord テーブルをクリア';
DELETE FROM DailyTrainingRecord;

PRINT '- TrainingRecordSets テーブルをクリア';
DELETE FROM TrainingRecordSets;

PRINT '- TrainingTag テーブルをクリア';
DELETE FROM TrainingTag;

PRINT '- TrainingMenus テーブルをクリア';
DELETE FROM TrainingMenus;

PRINT '- TagMaster テーブルをクリア';
DELETE FROM TagMaster;

PRINT '- UserTokens テーブルをクリア';
DELETE FROM UserTokens;

PRINT '- UserData テーブルをクリア';
DELETE FROM UserData;

PRINT '- ClientDataKeys テーブルをクリア';
DELETE FROM ClientDataKeys;

PRINT '- Clients テーブルをクリア';
DELETE FROM Clients;

PRINT '- Users テーブルをクリア';
DELETE FROM Users;

-- 外部キー制約を再度有効化
EXEC sp_MSforeachtable 'ALTER TABLE ? WITH CHECK CHECK CONSTRAINT all';

PRINT '';
PRINT '既存データの削除完了！';
PRINT '';

-- ===================================
-- STEP 2: 基本マスターデータの投入
-- ===================================
PRINT 'STEP 2: 基本マスターデータを投入中...';
PRINT '';

-- クライアント作成
PRINT '- デフォルトクライアントを作成';
INSERT INTO Clients (ClientId, ClientName, ClientSecret, Description) VALUES
('default_web', N'Web Application', 'web_secret_key_2024', N'デフォルトWebアプリケーション'),
('default_mobile', N'Mobile Application', 'mobile_secret_key_2024', N'デフォルトモバイルアプリケーション');

-- クライアントデータキー定義
PRINT '- クライアントデータキーを定義';
INSERT INTO ClientDataKeys (ClientId, DataKey, Description, IsRequired) VALUES
('default_web', 'display_name', N'表示名', 1),
('default_web', 'preferred_units', N'単位設定（metric/imperial）', 0),
('default_web', 'theme', N'テーマ設定', 0),
('default_mobile', 'display_name', N'表示名', 1),
('default_mobile', 'device_token', N'プッシュ通知用デバイストークン', 0),
('default_mobile', 'preferred_units', N'単位設定（metric/imperial）', 0);

PRINT '';

-- ===================================
-- STEP 3: タグマスターの投入
-- ===================================
PRINT 'STEP 3: タグマスターを投入中...';
PRINT '';

INSERT INTO TagMaster (TagId, JPName, ENName) VALUES
-- 部位タグ
('chest', N'胸', 'Chest'),
('back', N'背中', 'Back'),
('shoulders', N'肩', 'Shoulders'),
('arms', N'腕', 'Arms'),
('legs', N'脚', 'Legs'),
('core', N'体幹', 'Core'),
('glutes', N'臀部', 'Glutes'),

-- トレーニングタイプタグ
('strength', N'筋力', 'Strength'),
('cardio', N'有酸素', 'Cardio'),
('flexibility', N'柔軟性', 'Flexibility'),
('balance', N'バランス', 'Balance'),
('endurance', N'持久力', 'Endurance'),

-- 難易度タグ
('beginner', N'初級', 'Beginner'),
('intermediate', N'中級', 'Intermediate'),
('advanced', N'上級', 'Advanced'),

-- 器具タグ
('barbell', N'バーベル', 'Barbell'),
('dumbbell', N'ダンベル', 'Dumbbell'),
('machine', N'マシン', 'Machine'),
('bodyweight', N'自重', 'Bodyweight'),
('band', N'バンド', 'Resistance Band'),
('cable', N'ケーブル', 'Cable');

PRINT '- タグマスター: 21種類を投入完了';
PRINT '';

-- ===================================
-- STEP 4: トレーニングメニューの投入
-- ===================================
PRINT 'STEP 4: トレーニングメニューを投入中...';
PRINT '';

INSERT INTO TrainingMenus (MenuId, JPName, ENName, Description) VALUES
-- 胸のトレーニング
('bench_press', N'ベンチプレス', 'Bench Press', N'大胸筋を鍛える基本的なバーベル種目'),
('incline_bench_press', N'インクラインベンチプレス', 'Incline Bench Press', N'大胸筋上部を重点的に鍛えるバーベル種目'),
('dumbbell_fly', N'ダンベルフライ', 'Dumbbell Fly', N'大胸筋を広げる動作で鍛えるダンベル種目'),
('push_up', N'プッシュアップ', 'Push Up', N'自重で大胸筋を鍛える基本種目'),

-- 背中のトレーニング
('deadlift', N'デッドリフト', 'Deadlift', N'背中全体と下半身を鍛える複合種目'),
('pull_up', N'懸垂', 'Pull Up', N'広背筋を鍛える自重トレーニング'),
('bent_over_row', N'ベントオーバーロウ', 'Bent Over Row', N'背中の厚みを作るバーベル種目'),
('lat_pulldown', N'ラットプルダウン', 'Lat Pulldown', N'広背筋を鍛えるケーブル種目'),

-- 肩のトレーニング
('shoulder_press', N'ショルダープレス', 'Shoulder Press', N'三角筋を鍛える基本的なプレス種目'),
('lateral_raise', N'サイドレイズ', 'Lateral Raise', N'三角筋中部を鍛えるダンベル種目'),
('rear_delt_fly', N'リアデルトフライ', 'Rear Delt Fly', N'三角筋後部を鍛える種目'),

-- 腕のトレーニング
('bicep_curl', N'バイセップカール', 'Bicep Curl', N'上腕二頭筋を鍛える基本種目'),
('hammer_curl', N'ハンマーカール', 'Hammer Curl', N'上腕二頭筋と前腕を鍛える種目'),
('tricep_extension', N'トライセップエクステンション', 'Tricep Extension', N'上腕三頭筋を鍛える種目'),
('dips', N'ディップス', 'Dips', N'上腕三頭筋と大胸筋下部を鍛える自重種目'),

-- 脚のトレーニング
('squat', N'スクワット', 'Squat', N'下半身全体を鍛える基本種目'),
('leg_press', N'レッグプレス', 'Leg Press', N'大腿四頭筋を中心に鍛えるマシン種目'),
('romanian_deadlift', N'ルーマニアンデッドリフト', 'Romanian Deadlift', N'ハムストリングと臀部を鍛える種目'),
('calf_raise', N'カーフレイズ', 'Calf Raise', N'ふくらはぎを鍛える種目'),
('lunge', N'ランジ', 'Lunge', N'下半身全体をバランスよく鍛える種目'),

-- 体幹のトレーニング
('plank', N'プランク', 'Plank', N'体幹全体を鍛える等尺性運動'),
('crunch', N'クランチ', 'Crunch', N'腹直筋を鍛える基本種目'),
('russian_twist', N'ロシアンツイスト', 'Russian Twist', N'腹斜筋を鍛える回旋種目'),
('hanging_leg_raise', N'ハンギングレッグレイズ', 'Hanging Leg Raise', N'腹直筋下部を鍛える種目'),

-- 有酸素運動
('running', N'ランニング', 'Running', N'基本的な有酸素運動'),
('cycling', N'サイクリング', 'Cycling', N'膝に優しい有酸素運動'),
('rowing', N'ローイング', 'Rowing', N'全身を使う有酸素運動'),
('jump_rope', N'縄跳び', 'Jump Rope', N'高強度の有酸素運動');

PRINT '- トレーニングメニュー: 28種目を投入完了';
PRINT '';

-- ===================================
-- STEP 5: メニューとタグの関連付け（一部のみ）
-- ===================================
PRINT 'STEP 5: メニューとタグを関連付け中...';
PRINT '';

INSERT INTO TrainingTag (MenuId, TagId, JPName, ENName) VALUES
-- ベンチプレス
('bench_press', 'chest', N'胸', 'Chest'),
('bench_press', 'strength', N'筋力', 'Strength'),
('bench_press', 'barbell', N'バーベル', 'Barbell'),
('bench_press', 'intermediate', N'中級', 'Intermediate'),

-- スクワット
('squat', 'legs', N'脚', 'Legs'),
('squat', 'glutes', N'臀部', 'Glutes'),
('squat', 'strength', N'筋力', 'Strength'),
('squat', 'barbell', N'バーベル', 'Barbell'),
('squat', 'intermediate', N'中級', 'Intermediate'),

-- デッドリフト
('deadlift', 'back', N'背中', 'Back'),
('deadlift', 'legs', N'脚', 'Legs'),
('deadlift', 'strength', N'筋力', 'Strength'),
('deadlift', 'barbell', N'バーベル', 'Barbell'),
('deadlift', 'advanced', N'上級', 'Advanced'),

-- プッシュアップ
('push_up', 'chest', N'胸', 'Chest'),
('push_up', 'strength', N'筋力', 'Strength'),
('push_up', 'bodyweight', N'自重', 'Bodyweight'),
('push_up', 'beginner', N'初級', 'Beginner');

PRINT '- メニュー・タグ関連付け: 完了';
PRINT '';

-- ===================================
-- STEP 6: テストユーザーの作成
-- ===================================
PRINT 'STEP 6: テストユーザーを作成中...';
PRINT '';

-- ユーザー作成
INSERT INTO Users (UserCommonId, LoginId, PasswordHash, PasswordSalt, DisplayName, CreatedAt, UpdatedAt) VALUES
('user_001', 'beginner_user', 'AQAAAAIAAYagAAAAEDJhK8VqxPzV5N+LKqP7sL3TGqL+GlYmYnZxGlKrXpH3vg==', 'salt_001_random_string_here', N'初心者太郎', GETDATE(), GETDATE()),
('user_002', 'intermediate_user', 'AQAAAAIAAYagAAAAEDJhK8VqxPzV5N+LKqP7sL3TGqL+GlYmYnZxGlKrXpH3vg==', 'salt_002_random_string_here', N'トレーニング花子', GETDATE(), GETDATE()),
('user_003', 'advanced_user', 'AQAAAAIAAYagAAAAEDJhK8VqxPzV5N+LKqP7sL3TGqL+GlYmYnZxGlKrXpH3vg==', 'salt_003_random_string_here', N'マッスル次郎', GETDATE(), GETDATE()),
('user_demo', 'demo', 'AQAAAAIAAYagAAAAEDJhK8VqxPzV5N+LKqP7sL3TGqL+GlYmYnZxGlKrXpH3vg==', 'salt_demo_random_string_here', N'デモユーザー', GETDATE(), GETDATE()),
('user_admin', 'admin', 'AQAAAAIAAYagAAAAEDJhK8VqxPzV5N+LKqP7sL3TGqL+GlYmYnZxGlKrXpH3vg==', 'salt_admin_random_string_here', N'管理者', GETDATE(), GETDATE());

PRINT '- 5名のテストユーザーを作成完了';

-- ユーザーデータ投入
INSERT INTO UserData (UserCommonId, ClientId, DataKey, DataValue, CreatedAt, UpdatedAt) VALUES
('user_001', 'default_web', 'display_name', N'初心者太郎', GETDATE(), GETDATE()),
('user_002', 'default_web', 'display_name', N'トレーニング花子', GETDATE(), GETDATE()),
('user_003', 'default_web', 'display_name', N'マッスル次郎', GETDATE(), GETDATE()),
('user_demo', 'default_web', 'display_name', N'デモユーザー', GETDATE(), GETDATE()),
('user_admin', 'default_web', 'display_name', N'管理者', GETDATE(), GETDATE());

PRINT '- ユーザーデータを設定完了';
PRINT '';

-- ===================================
-- STEP 7: サンプルトレーニングデータ
-- ===================================
PRINT 'STEP 7: サンプルトレーニングデータを投入中...';
PRINT '';

-- user_002の直近のトレーニング記録
DECLARE @today DATE = GETDATE();

-- 昨日のトレーニング
INSERT INTO TrainingRecordSets (UserCommonId, MenuId, TrainingDate, SetNumber, Reps, Weight, CreatedAt) VALUES
('user_002', 'bench_press', DATEADD(DAY, -1, @today), 1, 10, 60, GETDATE()),
('user_002', 'bench_press', DATEADD(DAY, -1, @today), 2, 8, 65, GETDATE()),
('user_002', 'bench_press', DATEADD(DAY, -1, @today), 3, 6, 70, GETDATE()),
('user_002', 'squat', DATEADD(DAY, -1, @today), 1, 12, 80, GETDATE()),
('user_002', 'squat', DATEADD(DAY, -1, @today), 2, 10, 90, GETDATE()),
('user_002', 'squat', DATEADD(DAY, -1, @today), 3, 8, 100, GETDATE());

PRINT '- サンプルトレーニングデータを投入完了';
PRINT '';

-- ===================================
-- 完了レポート
-- ===================================
PRINT '';
PRINT '===================================';
PRINT 'クリーンインストール完了！';
PRINT '===================================';
PRINT '';
PRINT '投入されたデータ:';
PRINT '- クライアント: 2個';
PRINT '- タグマスター: 21種類';
PRINT '- トレーニングメニュー: 28種目';
PRINT '- テストユーザー: 5名';
PRINT '- サンプルトレーニング記録: 6件';
PRINT '';
PRINT 'テストユーザー情報:';
PRINT '===================================';
PRINT 'ログインID         パスワード';
PRINT '-----------------------------------';
PRINT 'beginner_user      Test123!';
PRINT 'intermediate_user  Test123!';
PRINT 'advanced_user      Test123!';
PRINT 'demo               Test123!';
PRINT 'admin              Test123!';
PRINT '===================================';
PRINT '';
PRINT 'APIテストが可能になりました！';
GO

-- 投入結果の確認
PRINT '';
PRINT '投入結果の確認:';
PRINT '===================================';

SELECT 'Users' as テーブル名, COUNT(*) as 件数 FROM Users
UNION ALL
SELECT 'TrainingMenus', COUNT(*) FROM TrainingMenus
UNION ALL
SELECT 'TagMaster', COUNT(*) FROM TagMaster
UNION ALL
SELECT 'TrainingTag', COUNT(*) FROM TrainingTag
UNION ALL
SELECT 'TrainingRecordSets', COUNT(*) FROM TrainingRecordSets;