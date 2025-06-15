-- ===================================
-- テストデータ完全実行スクリプト（修正版）
-- 既存データクリア → データ投入の順序で実行
-- ===================================

USE MessageRDB;
GO

PRINT '===================================';
PRINT 'テストデータ投入開始（修正版）';
PRINT '===================================';
PRINT '';

-- ===================================
-- 1. 既存データのクリア
-- ===================================
PRINT '1. 既存データをクリア中...';

-- 外部キー制約を一時的に無効化
ALTER TABLE DailyTrainingRecord NOCHECK CONSTRAINT ALL;
ALTER TABLE TrainingRecordSets NOCHECK CONSTRAINT ALL;
ALTER TABLE TrainingTag NOCHECK CONSTRAINT ALL;
ALTER TABLE UserTokens NOCHECK CONSTRAINT ALL;
ALTER TABLE UserData NOCHECK CONSTRAINT ALL;

-- データ削除（依存関係の順序で削除）
DELETE FROM DailyTrainingRecord;
DELETE FROM TrainingRecordSets;
DELETE FROM UserTokens;
DELETE FROM UserData;
DELETE FROM TrainingTag;
DELETE FROM TrainingMenus;
DELETE FROM TagMaster;
DELETE FROM Users;

-- 外部キー制約を再度有効化
ALTER TABLE DailyTrainingRecord CHECK CONSTRAINT ALL;
ALTER TABLE TrainingRecordSets CHECK CONSTRAINT ALL;
ALTER TABLE TrainingTag CHECK CONSTRAINT ALL;
ALTER TABLE UserTokens CHECK CONSTRAINT ALL;
ALTER TABLE UserData CHECK CONSTRAINT ALL;

PRINT '既存データクリア完了';
PRINT '';

-- ===================================
-- 2. トレーニングメニューとタグの投入
-- ===================================
PRINT '2. トレーニングメニューとタグを投入中...';

-- タグマスターデータの挿入
INSERT INTO TagMaster (TagId, JPName, ENName) VALUES
-- 部位タグ
('chest', '胸', 'Chest'),
('back', '背中', 'Back'),
('shoulders', '肩', 'Shoulders'),
('arms', '腕', 'Arms'),
('legs', '脚', 'Legs'),
('core', '体幹', 'Core'),
('glutes', '臀部', 'Glutes'),

-- トレーニングタイプタグ
('strength', '筋力', 'Strength'),
('cardio', '有酸素', 'Cardio'),
('flexibility', '柔軟性', 'Flexibility'),
('balance', 'バランス', 'Balance'),
('endurance', '持久力', 'Endurance'),

-- 難易度タグ
('beginner', '初級', 'Beginner'),
('intermediate', '中級', 'Intermediate'),
('advanced', '上級', 'Advanced'),

-- 器具タグ
('barbell', 'バーベル', 'Barbell'),
('dumbbell', 'ダンベル', 'Dumbbell'),
('machine', 'マシン', 'Machine'),
('bodyweight', '自重', 'Bodyweight'),
('band', 'バンド', 'Resistance Band'),
('cable', 'ケーブル', 'Cable');

-- トレーニングメニューの挿入
INSERT INTO TrainingMenus (MenuId, JPName, ENName, Description) VALUES
-- 胸のトレーニング
('bench_press', 'ベンチプレス', 'Bench Press', '大胸筋を鍛える基本的なバーベル種目'),
('incline_bench_press', 'インクラインベンチプレス', 'Incline Bench Press', '大胸筋上部を重点的に鍛えるバーベル種目'),
('push_up', 'プッシュアップ', 'Push Up', '自重で大胸筋を鍛える基本種目'),

-- 背中のトレーニング
('deadlift', 'デッドリフト', 'Deadlift', '背中全体と下半身を鍛える複合種目'),
('pull_up', '懸垂', 'Pull Up', '広背筋を鍛える自重トレーニング'),
('lat_pulldown', 'ラットプルダウン', 'Lat Pulldown', '広背筋を鍛えるケーブル種目'),

-- 肩のトレーニング
('shoulder_press', 'ショルダープレス', 'Shoulder Press', '三角筋を鍛える基本的なプレス種目'),
('bicep_curl', 'バイセップカール', 'Bicep Curl', '上腕二頭筋を鍛える基本種目'),

-- 脚のトレーニング
('squat', 'スクワット', 'Squat', '下半身全体を鍛える基本種目'),
('leg_press', 'レッグプレス', 'Leg Press', '大腿四頭筋を中心に鍛えるマシン種目'),
('romanian_deadlift', 'ルーマニアンデッドリフト', 'Romanian Deadlift', 'ハムストリングと臀部を鍛える種目'),

-- 体幹のトレーニング
('plank', 'プランク', 'Plank', '体幹全体を鍛える等尺性運動'),
('crunch', 'クランチ', 'Crunch', '腹直筋を鍛える基本種目'),
('hanging_leg_raise', 'ハンギングレッグレイズ', 'Hanging Leg Raise', '腹直筋下部を鍛える種目'),

-- 有酸素運動
('running', 'ランニング', 'Running', '基本的な有酸素運動');

-- トレーニングメニューとタグの関連付け（主要なもののみ）
INSERT INTO TrainingTag (MenuId, TagId, JPName, ENName) VALUES
-- ベンチプレス
('bench_press', 'chest', '胸', 'Chest'),
('bench_press', 'strength', '筋力', 'Strength'),
('bench_press', 'barbell', 'バーベル', 'Barbell'),
('bench_press', 'intermediate', '中級', 'Intermediate'),

-- プッシュアップ
('push_up', 'chest', '胸', 'Chest'),
('push_up', 'bodyweight', '自重', 'Bodyweight'),
('push_up', 'beginner', '初級', 'Beginner'),

-- スクワット
('squat', 'legs', '脚', 'Legs'),
('squat', 'strength', '筋力', 'Strength'),
('squat', 'intermediate', '中級', 'Intermediate'),

-- デッドリフト
('deadlift', 'back', '背中', 'Back'),
('deadlift', 'legs', '脚', 'Legs'),
('deadlift', 'strength', '筋力', 'Strength'),
('deadlift', 'advanced', '上級', 'Advanced'),

-- 懸垂
('pull_up', 'back', '背中', 'Back'),
('pull_up', 'bodyweight', '自重', 'Bodyweight'),
('pull_up', 'intermediate', '中級', 'Intermediate');

PRINT 'メニューとタグ投入完了';
PRINT '';

-- ===================================
-- 3. テストユーザーの作成
-- ===================================
PRINT '3. テストユーザーを作成中...';

-- ユーザー作成
INSERT INTO Users (UserCommonId, LoginId, PasswordHash, PasswordSalt, DisplayName, CreatedAt, UpdatedAt) VALUES
('user_001', 'beginner_user', 'AQAAAAIAAYagAAAAEDJhK8VqxPzV5N+LKqP7sL3TGqL+GlYmYnZxGlKrXpH3vg==', 'salt_001_random_string_here', '初心者太郎', GETDATE(), GETDATE()),
('user_002', 'intermediate_user', 'AQAAAAIAAYagAAAAEDJhK8VqxPzV5N+LKqP7sL3TGqL+GlYmYnZxGlKrXpH3vg==', 'salt_002_random_string_here', 'トレーニング花子', GETDATE(), GETDATE()),
('user_003', 'advanced_user', 'AQAAAAIAAYagAAAAEDJhK8VqxPzV5N+LKqP7sL3TGqL+GlYmYnZxGlKrXpH3vg==', 'salt_003_random_string_here', 'マッスル次郎', GETDATE(), GETDATE()),
('user_demo', 'demo', 'AQAAAAIAAYagAAAAEDJhK8VqxPzV5N+LKqP7sL3TGqL+GlYmYnZxGlKrXpH3vg==', 'salt_demo_random_string_here', 'デモユーザー', GETDATE(), GETDATE()),
('user_admin', 'admin', 'AQAAAAIAAYagAAAAEDJhK8VqxPzV5N+LKqP7sL3TGqL+GlYmYnZxGlKrXpH3vg==', 'salt_admin_random_string_here', '管理者', GETDATE(), GETDATE());

-- ユーザーデータ（Key-Value形式）の投入
INSERT INTO UserData (UserCommonId, ClientId, DataKey, DataValue, CreatedAt, UpdatedAt) VALUES
-- ユーザー1のデータ
('user_001', 'default_web', 'display_name', '初心者太郎', GETDATE(), GETDATE()),
('user_001', 'default_web', 'preferred_units', 'metric', GETDATE(), GETDATE()),
('user_001', 'default_web', 'theme', 'light', GETDATE(), GETDATE()),

-- ユーザー2のデータ
('user_002', 'default_web', 'display_name', 'トレーニング花子', GETDATE(), GETDATE()),
('user_002', 'default_web', 'preferred_units', 'metric', GETDATE(), GETDATE()),
('user_002', 'default_web', 'theme', 'dark', GETDATE(), GETDATE()),

-- ユーザー3のデータ
('user_003', 'default_web', 'display_name', 'マッスル次郎', GETDATE(), GETDATE()),
('user_003', 'default_web', 'preferred_units', 'imperial', GETDATE(), GETDATE()),
('user_003', 'default_web', 'theme', 'dark', GETDATE(), GETDATE()),

-- デモユーザーのデータ
('user_demo', 'default_web', 'display_name', 'デモユーザー', GETDATE(), GETDATE()),
('user_demo', 'default_web', 'preferred_units', 'metric', GETDATE(), GETDATE()),
('user_demo', 'default_web', 'theme', 'light', GETDATE(), GETDATE()),

-- 管理者のデータ
('user_admin', 'default_web', 'display_name', '管理者', GETDATE(), GETDATE()),
('user_admin', 'default_web', 'preferred_units', 'metric', GETDATE(), GETDATE()),
('user_admin', 'default_web', 'theme', 'light', GETDATE(), GETDATE());

PRINT 'テストユーザー作成完了';
PRINT '';

-- ===================================
-- 4. サンプルトレーニングデータ投入
-- ===================================
PRINT '4. サンプルトレーニングデータを投入中...';

-- ユーザー2（中級者）の簡単なトレーニングデータ
DECLARE @training_date DATE = DATEADD(DAY, -7, GETDATE());
DECLARE @user_id NVARCHAR(50) = 'user_002';

-- 過去1週間分の基本的なトレーニング記録
WHILE @training_date <= GETDATE()
BEGIN
    -- 週3回トレーニング（月・水・金）
    IF DATEPART(WEEKDAY, @training_date) IN (2, 4, 6)
    BEGIN
        -- ベンチプレス
        INSERT INTO TrainingRecordSets (UserCommonId, MenuId, TrainingDate, SetNumber, Reps, Weight, CreatedAt) VALUES
        (@user_id, 'bench_press', @training_date, 1, 10, 60, GETDATE()),
        (@user_id, 'bench_press', @training_date, 2, 8, 65, GETDATE()),
        (@user_id, 'bench_press', @training_date, 3, 6, 70, GETDATE());
        
        -- スクワット
        INSERT INTO TrainingRecordSets (UserCommonId, MenuId, TrainingDate, SetNumber, Reps, Weight, CreatedAt) VALUES
        (@user_id, 'squat', @training_date, 1, 12, 80, GETDATE()),
        (@user_id, 'squat', @training_date, 2, 10, 90, GETDATE()),
        (@user_id, 'squat', @training_date, 3, 8, 100, GETDATE());
    END
    
    SET @training_date = DATEADD(DAY, 1, @training_date);
END;

-- 日次集計データの作成
INSERT INTO DailyTrainingRecord (
    UserCommonId, 
    MenuId, 
    TrainingDate, 
    SetCount,
    MaxReps,
    MaxRepsWeight,
    MaxWeight,
    MaxWeightReps,
    TotalLoadAmount,
    TotalReps,
    CreatedAt,
    UpdatedAt
)
SELECT 
    t.UserCommonId,
    t.MenuId,
    t.TrainingDate,
    COUNT(*) as SetCount,
    MAX(t.Reps) as MaxReps,
    (SELECT TOP 1 Weight FROM TrainingRecordSets t2 
     WHERE t2.UserCommonId = t.UserCommonId 
     AND t2.MenuId = t.MenuId 
     AND t2.TrainingDate = t.TrainingDate 
     AND t2.Reps = MAX(t.Reps)
     ORDER BY Weight DESC) as MaxRepsWeight,
    MAX(t.Weight) as MaxWeight,
    (SELECT TOP 1 Reps FROM TrainingRecordSets t3 
     WHERE t3.UserCommonId = t.UserCommonId 
     AND t3.MenuId = t.MenuId 
     AND t3.TrainingDate = t.TrainingDate 
     AND t3.Weight = MAX(t.Weight)) as MaxWeightReps,
    SUM(ISNULL(t.Weight, 0) * t.Reps) as TotalLoadAmount,
    SUM(t.Reps) as TotalReps,
    GETDATE(),
    GETDATE()
FROM TrainingRecordSets t
GROUP BY t.UserCommonId, t.MenuId, t.TrainingDate;

PRINT 'サンプルトレーニングデータ投入完了';
PRINT '';

-- ===================================
-- 5. 結果確認
-- ===================================
PRINT '===================================';
PRINT 'テストデータ投入完了！';
PRINT '===================================';
PRINT '';

-- データ件数確認
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
PRINT 'テストユーザー一覧:';
SELECT 
    LoginId as ログインID,
    DisplayName as 表示名,
    'Test123!' as パスワード
FROM Users
ORDER BY LoginId;

PRINT '';
PRINT 'アプリケーションでテスト可能です！';
GO