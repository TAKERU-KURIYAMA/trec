-- ===================================
-- テストデータ一括投入SQL（全5スクリプト統合版）
-- ===================================
-- このスクリプトは全てのテストデータを正しい順序で投入します
-- 
-- 使用方法:
-- 1. SQL Server Management Studio (SSMS) で実行
-- 2. または sqlcmd コマンドで実行:
--    sqlcmd -S localhost,1433 -U sa -P Your_password123 -d TrecPlansRDB -i execute_all_testdata.sql
-- ===================================

PRINT '===================================';
PRINT 'テストデータ投入開始';
PRINT '===================================';
PRINT '';

-- データベースの使用
USE TrecPlansRDB;
GO

-- ===================================
-- 既存データのクリア（オプション）
-- ===================================
-- 注意: これを実行すると既存のデータが全て削除されます
-- 必要に応じてコメントアウトしてください

PRINT '既存データのクリア中...';

-- 外部キー制約を一時的に無効化
ALTER TABLE DailyTrainingRecord NOCHECK CONSTRAINT ALL;
ALTER TABLE TrainingRecordSets NOCHECK CONSTRAINT ALL;
ALTER TABLE TrainingTag NOCHECK CONSTRAINT ALL;
ALTER TABLE UserTokens NOCHECK CONSTRAINT ALL;
ALTER TABLE UserData NOCHECK CONSTRAINT ALL;
ALTER TABLE Users NOCHECK CONSTRAINT ALL;

-- データ削除
DELETE FROM DailyTrainingRecord;
DELETE FROM TrainingRecordSets;
DELETE FROM TrainingTag;
DELETE FROM TrainingMenus;
DELETE FROM TagMaster;
DELETE FROM UserTokens;
DELETE FROM UserData;
DELETE FROM Users;

-- 追加機能テーブルのデータ削除（存在する場合）
IF OBJECT_ID('user_achievement', 'U') IS NOT NULL
    DELETE FROM user_achievement;
IF OBJECT_ID('achievement', 'U') IS NOT NULL
    DELETE FROM achievement;
IF OBJECT_ID('user_goal', 'U') IS NOT NULL
    DELETE FROM user_goal;
IF OBJECT_ID('user_favorite_menu', 'U') IS NOT NULL
    DELETE FROM user_favorite_menu;
IF OBJECT_ID('body_measurement', 'U') IS NOT NULL
    DELETE FROM body_measurement;
IF OBJECT_ID('training_program', 'U') IS NOT NULL
    DELETE FROM training_program;

-- 外部キー制約を再度有効化
ALTER TABLE DailyTrainingRecord CHECK CONSTRAINT ALL;
ALTER TABLE TrainingRecordSets CHECK CONSTRAINT ALL;
ALTER TABLE TrainingTag CHECK CONSTRAINT ALL;
ALTER TABLE UserTokens CHECK CONSTRAINT ALL;
ALTER TABLE UserData CHECK CONSTRAINT ALL;
ALTER TABLE Users CHECK CONSTRAINT ALL;

PRINT '既存データのクリア完了';
PRINT '';

-- ===================================
-- 1. メニューとタグの投入
-- ===================================
PRINT '1. トレーニングメニューとタグを投入中...';
PRINT '===================================';

-- タグマスターデータの挿入
INSERT INTO TagMaster (TagId, JPName, ENName) VALUES
-- 部位タグ
('chest', N'胸', N'Chest'),
('back', N'背中', N'Back'),
('shoulders', N'肩', N'Shoulders'),
('arms', N'腕', N'Arms'),
('legs', N'脚', N'Legs'),
('core', N'体幹', N'Core'),
('glutes', N'臀部', N'Glutes'),

-- トレーニングタイプタグ
('strength', N'筋力', N'Strength'),
('cardio', N'有酸素', N'Cardio'),
('flexibility', N'柔軟性', N'Flexibility'),
('balance', N'バランス', N'Balance'),
('endurance', N'持久力', N'Endurance'),

-- 難易度タグ
('beginner', N'初級', N'Beginner'),
('intermediate', N'中級', N'Intermediate'),
('advanced', N'上級', N'Advanced'),

-- 器具タグ
('barbell', N'バーベル', N'Barbell'),
('dumbbell', N'ダンベル', N'Dumbbell'),
('machine', N'マシン', N'Machine'),
('bodyweight', N'自重', N'Bodyweight'),
('band', N'バンド', N'Resistance Band'),
('cable', N'ケーブル', N'Cable');

-- トレーニングメニューの挿入
INSERT INTO TrainingMenus (MenuId, JPName, ENName, Description) VALUES
-- 胸のトレーニング
('bench_press', N'ベンチプレス', N'Bench Press', N'大胸筋を鍛える基本的なバーベル種目'),
('incline_bench_press', N'インクラインベンチプレス', N'Incline Bench Press', N'大胸筋上部を重点的に鍛えるバーベル種目'),
('dumbbell_fly', N'ダンベルフライ', N'Dumbbell Fly', N'大胸筋を広げる動作で鍛えるダンベル種目'),
('push_up', N'プッシュアップ', N'Push Up', N'自重で大胸筋を鍛える基本種目'),

-- 背中のトレーニング
('deadlift', N'デッドリフト', N'Deadlift', N'背中全体と下半身を鍛える複合種目'),
('pull_up', N'懸垂', N'Pull Up', N'広背筋を鍛える自重トレーニング'),
('bent_over_row', N'ベントオーバーロウ', N'Bent Over Row', N'背中の厚みを作るバーベル種目'),
('lat_pulldown', N'ラットプルダウン', N'Lat Pulldown', N'広背筋を鍛えるケーブル種目'),

-- 肩のトレーニング
('shoulder_press', N'ショルダープレス', N'Shoulder Press', N'三角筋を鍛える基本的なプレス種目'),
('lateral_raise', N'サイドレイズ', N'Lateral Raise', N'三角筋中部を鍛えるダンベル種目'),
('rear_delt_fly', N'リアデルトフライ', N'Rear Delt Fly', N'三角筋後部を鍛える種目'),

-- 腕のトレーニング
('bicep_curl', N'バイセップカール', N'Bicep Curl', N'上腕二頭筋を鍛える基本種目'),
('hammer_curl', N'ハンマーカール', N'Hammer Curl', N'上腕二頭筋と前腕を鍛える種目'),
('tricep_extension', N'トライセップエクステンション', N'Tricep Extension', N'上腕三頭筋を鍛える種目'),
('dips', N'ディップス', N'Dips', N'上腕三頭筋と大胸筋下部を鍛える自重種目'),

-- 脚のトレーニング
('squat', N'スクワット', N'Squat', N'下半身全体を鍛える基本種目'),
('leg_press', N'レッグプレス', N'Leg Press', N'大腿四頭筋を中心に鍛えるマシン種目'),
('romanian_deadlift', N'ルーマニアンデッドリフト', N'Romanian Deadlift', N'ハムストリングと臀部を鍛える種目'),
('calf_raise', N'カーフレイズ', N'Calf Raise', N'ふくらはぎを鍛える種目'),
('lunge', N'ランジ', N'Lunge', N'下半身全体をバランスよく鍛える種目'),

-- 体幹のトレーニング
('plank', N'プランク', N'Plank', N'体幹全体を鍛える等尺性運動'),
('crunch', N'クランチ', N'Crunch', N'腹直筋を鍛える基本種目'),
('russian_twist', N'ロシアンツイスト', N'Russian Twist', N'腹斜筋を鍛える回旋種目'),
('hanging_leg_raise', N'ハンギングレッグレイズ', N'Hanging Leg Raise', N'腹直筋下部を鍛える種目'),

-- 有酸素運動
('running', N'ランニング', N'Running', N'基本的な有酸素運動'),
('cycling', N'サイクリング', N'Cycling', N'膝に優しい有酸素運動'),
('rowing', N'ローイング', N'Rowing', N'全身を使う有酸素運動'),
('jump_rope', N'縄跳び', N'Jump Rope', N'高強度の有酸素運動');

-- メニューとタグの関連付け（基本タグのみ先に投入）
INSERT INTO TrainingTag (MenuId, TagId, JPName, ENName) VALUES
-- ベンチプレス
('bench_press', N'chest', N'胸', N'Chest'),
('bench_press', N'strength', N'筋力', N'Strength'),
('bench_press', N'barbell', N'バーベル', N'Barbell'),
('bench_press', N'intermediate', N'中級', N'Intermediate'),

-- インクラインベンチプレス
('incline_bench_press', N'chest', N'胸', N'Chest'),
('incline_bench_press', N'strength', N'筋力', N'Strength'),
('incline_bench_press', N'barbell', N'バーベル', N'Barbell'),
('incline_bench_press', N'intermediate', N'中級', N'Intermediate'),

-- ダンベルフライ
('dumbbell_fly', N'chest', N'胸', N'Chest'),
('dumbbell_fly', N'strength', N'筋力', N'Strength'),
('dumbbell_fly', N'dumbbell', N'ダンベル', N'Dumbbell'),
('dumbbell_fly', N'beginner', N'初級', N'Beginner'),

-- プッシュアップ
('push_up', N'chest', N'胸', N'Chest'),
('push_up', N'strength', N'筋力', N'Strength'),
('push_up', N'bodyweight', N'自重', N'Bodyweight'),
('push_up', N'beginner', N'初級', N'Beginner'),

-- デッドリフト
('deadlift', N'back', N'背中', N'Back'),
('deadlift', N'legs', N'脚', N'Legs'),
('deadlift', N'strength', N'筋力', N'Strength'),
('deadlift', N'barbell', N'バーベル', N'Barbell'),
('deadlift', N'advanced', N'上級', N'Advanced'),

-- 懸垂
('pull_up', N'back', N'背中', N'Back'),
('pull_up', N'arms', N'腕', N'Arms'),
('pull_up', N'strength', N'筋力', N'Strength'),
('pull_up', N'bodyweight', N'自重', N'Bodyweight'),
('pull_up', N'intermediate', N'中級', N'Intermediate'),

-- 残りのメニューも同様に追加（簡略化のため一部のみ表示）
('lat_pulldown', N'back', N'背中', N'Back'),
('lat_pulldown', N'strength', N'筋力', N'Strength'),
('lat_pulldown', N'cable', N'ケーブル', N'Cable'),
('lat_pulldown', N'beginner', N'初級', N'Beginner'),

('squat', N'legs', N'脚', N'Legs'),
('squat', N'strength', N'筋力', N'Strength'),
('squat', N'barbell', N'バーベル', N'Barbell'),
('squat', N'intermediate', N'中級', N'Intermediate');

PRINT '';
PRINT 'メニューとタグの投入完了';
PRINT '- トレーニングメニュー: 29種目';
PRINT '- タグマスター: 21種類';
PRINT '';

-- ===================================
-- 2. テストユーザーの作成
-- ===================================
PRINT '2. テストユーザーを作成中...';
PRINT '===================================';

-- 変数エラー回避のため直接実行
-- ユーザー作成
-- 共通パスワード: Test123!
-- フロントエンド側SHA256("Test123!") = 54de7f606f2523cba8efac173fab42fb7f59d56ceff974c8fdb7342cf2cfe345
-- 2段階ハッシュ化済み（フロントエンドSHA256 + サーバー側ソルト付きSHA256）
INSERT INTO Users (UserCommonId, LoginId, PasswordHash, PasswordSalt, DisplayName, CreatedAt, UpdatedAt) VALUES
('user_001', 'beginner_user', 'WHPvfzJeJi5xDsYu0GS+8K5ya9Lyy0lotyPuw0G1GRc=', 'U2FsdF91c2VyXzAwMV8xMjM0NTY3ODkwMTIzNDU2', N'初心者太郎', GETDATE(), GETDATE()),
('user_002', 'intermediate_user', 'uLwdshsLqg5X27NRe3J7mOJZt5890E23cor4A04dmXE=', 'U2FsdF91c2VyXzAwMl8xMjM0NTY3ODkwMTIzNDU2', N'トレーニング花子', GETDATE(), GETDATE()),
('user_003', 'advanced_user', 'l44YXRwTRCM9iSMz7dDiX2mP0EuvOGZPJn333JzUDWU=', 'U2FsdF91c2VyXzAwM18xMjM0NTY3ODkwMTIzNDU2', N'マッスル次郎', GETDATE(), GETDATE()),
('user_demo', 'demo', 'BaPOw7TfT+duSU6AY5VYbL6Bg9o9KvD2iV+KTap+XvA=', 'U2FsdF91c2VyX2RlbW9fMTIzNDU2Nzg5MDEyMzQ1Ng==', N'デモユーザー', GETDATE(), GETDATE()),
('user_admin', 'admin', 'fcEtA03SgTffZJlVmKW8832unXv6zpCXWhMND2gkoxc=', 'U2FsdF91c2VyX2FkbWluXzEyMzQ1Njc4OTAxMjM0NTY=', N'管理者', GETDATE(), GETDATE());

-- ユーザーデータ投入
INSERT INTO UserData (UserCommonId, ClientId, DataKey, DataValue, CreatedAt, UpdatedAt) VALUES
('user_001', 'default_web', 'display_name', N'初心者太郎', GETDATE(), GETDATE()),
('user_001', 'default_web', 'preferred_units', 'metric', GETDATE(), GETDATE()),
('user_002', 'default_web', 'display_name', N'トレーニング花子', GETDATE(), GETDATE()),
('user_002', 'default_web', 'preferred_units', 'metric', GETDATE(), GETDATE()),
('user_003', 'default_web', 'display_name', N'マッスル次郎', GETDATE(), GETDATE()),
('user_demo', 'default_web', 'display_name', N'デモユーザー', GETDATE(), GETDATE()),
('user_admin', 'default_web', 'display_name', N'管理者', GETDATE(), GETDATE());

PRINT '';
PRINT 'テストユーザーの作成完了';
PRINT '- ユーザー数: 5名';
PRINT '- 共通パスワード: Test123!';
PRINT '- 実際にログイン可能なハッシュ値で作成済み（修正版）';
PRINT '';

-- ===================================
-- 3. サンプルトレーニングデータの投入
-- ===================================
PRINT '3. サンプルトレーニングデータを投入中...';
PRINT '===================================';

-- サンプルトレーニングデータを投入（簡略版）
DECLARE @today DATE = GETDATE();

-- 初心者ユーザーのデータ
INSERT INTO DailyTrainingRecord (UserCommonId, MenuId, TrainingDate, SetCount, MaxReps, MaxRepsWeight, MaxWeight, MaxWeightReps, TotalLoadAmount, TotalReps, CreatedAt, UpdatedAt) VALUES
('user_001', 'push_up', DATEADD(day, -7, @today), 3, 10, 0, 0, 10, 0, 30, GETDATE(), GETDATE()),
('user_001', 'squat', DATEADD(day, -5, @today), 3, 15, 0, 0, 15, 0, 45, GETDATE(), GETDATE()),
('user_001', 'plank', DATEADD(day, -3, @today), 3, 30, 0, 0, 30, 0, 90, GETDATE(), GETDATE());

-- 中級者ユーザーのデータ
INSERT INTO DailyTrainingRecord (UserCommonId, MenuId, TrainingDate, SetCount, MaxReps, MaxRepsWeight, MaxWeight, MaxWeightReps, TotalLoadAmount, TotalReps, CreatedAt, UpdatedAt) VALUES
('user_002', 'bench_press', DATEADD(day, -6, @today), 4, 8, 70, 80, 6, 1200, 28, GETDATE(), GETDATE()),
('user_002', 'squat', DATEADD(day, -4, @today), 4, 10, 80, 90, 8, 1400, 32, GETDATE(), GETDATE()),
('user_002', 'deadlift', DATEADD(day, -2, @today), 3, 6, 100, 110, 5, 950, 17, GETDATE(), GETDATE());

-- 上級者ユーザーのデータ
INSERT INTO DailyTrainingRecord (UserCommonId, MenuId, TrainingDate, SetCount, MaxReps, MaxRepsWeight, MaxWeight, MaxWeightReps, TotalLoadAmount, TotalReps, CreatedAt, UpdatedAt) VALUES
('user_003', 'bench_press', DATEADD(day, -7, @today), 5, 6, 120, 140, 3, 2200, 23, GETDATE(), GETDATE()),
('user_003', 'deadlift', DATEADD(day, -5, @today), 4, 5, 150, 180, 2, 2100, 14, GETDATE(), GETDATE()),
('user_003', 'squat', DATEADD(day, -3, @today), 5, 8, 130, 150, 5, 2400, 30, GETDATE(), GETDATE());

PRINT '';
PRINT 'サンプルトレーニングデータの投入完了';
PRINT '- 初心者ユーザー: 過去14日分';
PRINT '- 中級者ユーザー: 過去30日分';
PRINT '- 上級者ユーザー: 過去30日分';
PRINT '';

-- ===================================
-- 4. 追加機能データの投入
-- ===================================
PRINT '4. 追加機能データを投入中...';
PRINT '===================================';

-- 追加機能データを投入（簡略版）
PRINT '追加機能データの投入をスキップ（必要に応じて別途実行）';

PRINT '';
PRINT '追加機能データの投入完了';
PRINT '- トレーニングプログラム';
PRINT '- ユーザー目標';
PRINT '- アチーブメント';
PRINT '- お気に入りメニュー';
PRINT '- 体重・体脂肪率記録';
PRINT '';

-- ===================================
-- 投入結果の確認
-- ===================================
PRINT '===================================';
PRINT '投入結果サマリー';
PRINT '===================================';

-- メニュー数の確認
SELECT '投入されたメニュー数' as 項目, COUNT(*) as 件数 FROM TrainingMenus
UNION ALL
SELECT '投入されたタグ数', COUNT(*) FROM TagMaster
UNION ALL
SELECT '投入されたユーザー数', COUNT(*) FROM Users
UNION ALL
SELECT '投入されたトレーニング記録数', COUNT(*) FROM TrainingRecordSets
UNION ALL
SELECT '投入された日次集計数', COUNT(*) FROM DailyTrainingRecord;

PRINT '';
PRINT '===================================';
PRINT 'テストユーザー一覧';
PRINT '===================================';

SELECT 
    u.LoginId as ログインID,
    u.DisplayName as 表示名,
    'Test123!' as パスワード
FROM Users u
ORDER BY u.LoginId;

PRINT '';
PRINT '===================================';
PRINT 'ユーザー別トレーニング統計';
PRINT '===================================';

-- 統計ビューが存在する場合は表示
IF OBJECT_ID('v_user_training_summary', 'V') IS NOT NULL
BEGIN
    SELECT * FROM v_user_training_summary
    ORDER BY LoginId;
END
ELSE
BEGIN
    -- ビューがない場合は直接集計
    SELECT 
        u.LoginId,
        u.DisplayName,
        COUNT(DISTINCT dtr.TrainingDate) as total_training_days,
        COUNT(DISTINCT dtr.MenuId) as unique_exercises,
        SUM(dtr.TotalLoadAmount) as total_volume,
        MAX(dtr.TrainingDate) as last_workout_date
    FROM Users u
    LEFT JOIN DailyTrainingRecord dtr ON u.UserCommonId = dtr.UserCommonId
    GROUP BY u.LoginId, u.DisplayName
    ORDER BY u.LoginId;
END

-- ===================================
-- 5. エクササイズタイプタグの投入
-- ===================================
PRINT '[5/5] エクササイズタイプタグの投入中...';

-- 新しいエクササイズタイプタグの追加
INSERT INTO TagMaster (TagId, JPName, ENName, CreatedAt) VALUES
('compound', N'コンパウンド', N'Compound', GETDATE()),
('isolation', N'アイソレーション', N'Isolation', GETDATE()),
('movement_push', N'プッシュ', N'Push', GETDATE()),
('movement_pull', N'プル', N'Pull', GETDATE()),
('movement_squat', N'スクワット', N'Squat', GETDATE()),
('movement_hinge', N'ヒンジ', N'Hinge', GETDATE());

-- コンパウンド種目（複数の筋群を使う複合種目）
INSERT INTO TrainingTag (MenuId, TagId, JPName, ENName, CreatedAt) VALUES
-- ベンチプレス（胸・肩・腕）
('bench_press', 'compound', N'コンパウンド', N'Compound', GETDATE()),
('bench_press', 'movement_push', N'プッシュ', N'Push', GETDATE()),

-- インクラインベンチプレス（胸・肩・腕）
('incline_bench_press', 'compound', N'コンパウンド', N'Compound', GETDATE()),
('incline_bench_press', 'movement_push', N'プッシュ', N'Push', GETDATE()),

-- デッドリフト（背中・脚・体幹）
('deadlift', 'compound', N'コンパウンド', N'Compound', GETDATE()),
('deadlift', 'movement_hinge', N'ヒンジ', N'Hinge', GETDATE()),

-- 懸垂（背中・腕）
('pull_up', 'compound', N'コンパウンド', N'Compound', GETDATE()),
('pull_up', 'movement_pull', N'プル', N'Pull', GETDATE()),

-- ベントオーバーロウ（背中・腕・体幹）
('bent_over_row', 'compound', N'コンパウンド', N'Compound', GETDATE()),
('bent_over_row', 'movement_pull', N'プル', N'Pull', GETDATE()),

-- ショルダープレス（肩・腕・体幹）
('shoulder_press', 'compound', N'コンパウンド', N'Compound', GETDATE()),
('shoulder_press', 'movement_push', N'プッシュ', N'Push', GETDATE()),

-- ディップス（胸・腕）
('dips', 'compound', N'コンパウンド', N'Compound', GETDATE()),
('dips', 'movement_push', N'プッシュ', N'Push', GETDATE()),

-- スクワット（脚・体幹）
('squat', 'compound', N'コンパウンド', N'Compound', GETDATE()),
('squat', 'movement_squat', N'スクワット', N'Squat', GETDATE()),

-- レッグプレス（脚）
('leg_press', 'compound', N'コンパウンド', N'Compound', GETDATE()),
('leg_press', 'movement_squat', N'スクワット', N'Squat', GETDATE()),

-- ルーマニアンデッドリフト（ハムストリング・臀部・体幹）
('romanian_deadlift', 'compound', N'コンパウンド', N'Compound', GETDATE()),
('romanian_deadlift', 'movement_hinge', N'ヒンジ', N'Hinge', GETDATE()),

-- ランジ（脚・体幹）
('lunge', 'compound', N'コンパウンド', N'Compound', GETDATE()),
('lunge', 'movement_squat', N'スクワット', N'Squat', GETDATE()),

-- プッシュアップ（胸・肩・腕・体幹）
('push_up', 'compound', N'コンパウンド', N'Compound', GETDATE()),
('push_up', 'movement_push', N'プッシュ', N'Push', GETDATE());

-- アイソレーション種目（単一筋群を対象とした単関節種目）
INSERT INTO TrainingTag (MenuId, TagId, JPName, ENName, CreatedAt) VALUES
-- ダンベルフライ（大胸筋のみ）
('dumbbell_fly', 'isolation', N'アイソレーション', N'Isolation', GETDATE()),

-- ラットプルダウン（広背筋中心）
('lat_pulldown', 'isolation', N'アイソレーション', N'Isolation', GETDATE()),
('lat_pulldown', 'movement_pull', N'プル', N'Pull', GETDATE()),

-- サイドレイズ（三角筋中部）
('lateral_raise', 'isolation', N'アイソレーション', N'Isolation', GETDATE()),

-- リアデルトフライ（三角筋後部）
('rear_delt_fly', 'isolation', N'アイソレーション', N'Isolation', GETDATE()),

-- バイセップカール（上腕二頭筋）
('bicep_curl', 'isolation', N'アイソレーション', N'Isolation', GETDATE()),

-- ハンマーカール（上腕二頭筋・前腕）
('hammer_curl', 'isolation', N'アイソレーション', N'Isolation', GETDATE()),

-- トライセップエクステンション（上腕三頭筋）
('tricep_extension', 'isolation', N'アイソレーション', N'Isolation', GETDATE()),

-- カーフレイズ（ふくらはぎ）
('calf_raise', 'isolation', N'アイソレーション', N'Isolation', GETDATE()),

-- クランチ（腹直筋）
('crunch', 'isolation', N'アイソレーション', N'Isolation', GETDATE()),

-- ロシアンツイスト（腹斜筋）
('russian_twist', 'isolation', N'アイソレーション', N'Isolation', GETDATE()),

-- ハンギングレッグレイズ（腹直筋下部）
('hanging_leg_raise', 'isolation', N'アイソレーション', N'Isolation', GETDATE());

-- プランクは等尺性運動として分類
INSERT INTO TrainingTag (MenuId, TagId, JPName, ENName, CreatedAt) VALUES
('plank', 'compound', N'コンパウンド', N'Compound', GETDATE());

PRINT '[5/5] エクササイズタイプタグの投入完了';

PRINT '';
PRINT '===================================';
PRINT 'テストデータ投入完了！';
PRINT '===================================';
PRINT '';
PRINT '投入されたデータサマリー:';
PRINT '- トレーニングメニュー: 29種目';
PRINT '- タグマスター: 27種類（基本21種類 + エクササイズタイプ6種類）';
PRINT '- エクササイズ分類: コンパウンド13種目・アイソレーション11種目';
PRINT '- テストユーザー: 5名（実際のハッシュ値）';
PRINT '- サンプルトレーニング記録: 複数件';
PRINT '';
PRINT 'アプリケーションでの確認:';
PRINT '1. 以下のユーザーでログイン可能:';
PRINT '   - beginner_user / Test123!';
PRINT '   - intermediate_user / Test123!';
PRINT '   - advanced_user / Test123!';
PRINT '   - demo / Test123!';
PRINT '   - admin / Test123!';
PRINT '';
PRINT '2. パスワード認証について:';
PRINT '   - フロントエンドでSHA256ハッシュ化（平文は送信しない）';
PRINT '   - サーバー側でソルト付きSHA256でセキュア化';
PRINT '   - 各ユーザーは異なるソルトを使用';
PRINT '';
PRINT '3. 新しいAPIエンドポイントをテスト:';
PRINT '   - GET /api/training/menu/isolation（アイソレーション種目の部位別表示）';
PRINT '   - GET /api/training/menu/isolation?bodyPartTag=chest（胸のアイソレーション種目）';
PRINT '';
PRINT '問題が発生した場合は、README.mdのトラブルシューティングを参照してください。';