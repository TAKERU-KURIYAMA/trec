-- ===================================
-- 追加機能用のテストデータ
-- プログラム、目標、実績など
-- ===================================

-- ===================================
-- トレーニングプログラムの作成
-- ===================================
IF OBJECT_ID('training_program', 'U') IS NOT NULL
BEGIN
    -- 初心者向けプログラム
    INSERT INTO training_program (program_id, name, description, difficulty, duration_weeks, created_by, created_at) VALUES
    ('beginner_strength', '初心者向け筋力アッププログラム', '基本的な種目で全身をバランスよく鍛える8週間プログラム', 'beginner', 8, 'system', GETDATE()),
    ('beginner_cardio', '初心者向け有酸素プログラム', '心肺機能向上を目指す6週間プログラム', 'beginner', 6, 'system', GETDATE());

    -- 中級者向けプログラム
    INSERT INTO training_program (program_id, name, description, difficulty, duration_weeks, created_by, created_at) VALUES
    ('intermediate_hypertrophy', '中級者向け筋肥大プログラム', '筋肥大に特化した12週間プログラム', 'intermediate', 12, 'system', GETDATE()),
    ('intermediate_strength', '中級者向けストレングスプログラム', '最大筋力向上を目指す10週間プログラム', 'intermediate', 10, 'system', GETDATE());

    -- 上級者向けプログラム
    INSERT INTO training_program (program_id, name, description, difficulty, duration_weeks, created_by, created_at) VALUES
    ('advanced_powerlifting', '上級者向けパワーリフティングプログラム', 'BIG3の記録更新を目指す16週間プログラム', 'advanced', 16, 'system', GETDATE()),
    ('advanced_bodybuilding', '上級者向けボディビルディングプログラム', '競技レベルの肉体を目指す20週間プログラム', 'advanced', 20, 'system', GETDATE());
END

-- ===================================
-- ユーザー目標の設定
-- ===================================
IF OBJECT_ID('user_goal', 'U') IS NOT NULL
BEGIN
    -- ユーザー1の目標
    INSERT INTO user_goal (goal_id, user_common_id, goal_type, target_value, current_value, target_date, status, created_at) VALUES
    ('goal_001_bench', 'user_001', 'weight', 60, 0, DATEADD(MONTH, 3, GETDATE()), 'active', GETDATE()),
    ('goal_001_consistency', 'user_001', 'frequency', 24, 8, DATEADD(MONTH, 1, GETDATE()), 'active', GETDATE());

    -- ユーザー2の目標
    INSERT INTO user_goal (goal_id, user_common_id, goal_type, target_value, current_value, target_date, status, created_at) VALUES
    ('goal_002_bench', 'user_002', 'weight', 100, 75, DATEADD(MONTH, 6, GETDATE()), 'active', GETDATE()),
    ('goal_002_squat', 'user_002', 'weight', 140, 110, DATEADD(MONTH, 6, GETDATE()), 'active', GETDATE()),
    ('goal_002_volume', 'user_002', 'volume', 50000, 35000, DATEADD(MONTH, 1, GETDATE()), 'active', GETDATE());

    -- ユーザー3の目標
    INSERT INTO user_goal (goal_id, user_common_id, goal_type, target_value, current_value, target_date, status, created_at) VALUES
    ('goal_003_bench', 'user_003', 'weight', 150, 120, DATEADD(YEAR, 1, GETDATE()), 'active', GETDATE()),
    ('goal_003_deadlift', 'user_003', 'weight', 250, 200, DATEADD(YEAR, 1, GETDATE()), 'active', GETDATE()),
    ('goal_003_squat', 'user_003', 'weight', 200, 180, DATEADD(MONTH, 9, GETDATE()), 'active', GETDATE());
END

-- ===================================
-- 実績・アチーブメント
-- ===================================
IF OBJECT_ID('achievement', 'U') IS NOT NULL
BEGIN
    -- アチーブメント定義
    INSERT INTO achievement (achievement_id, name, description, category, criteria, icon, points, created_at) VALUES
    ('first_workout', '初めてのワークアウト', '初めてトレーニングを記録しました', 'milestone', 'complete_first_workout', '🎯', 10, GETDATE()),
    ('week_streak', '週間戦士', '7日連続でトレーニングを完了', 'consistency', 'workout_streak_7', '🔥', 50, GETDATE()),
    ('month_streak', '月間マスター', '30日連続でトレーニングを完了', 'consistency', 'workout_streak_30', '💪', 200, GETDATE()),
    ('100kg_club', '100kgクラブ', 'いずれかの種目で100kg以上を達成', 'strength', 'lift_100kg', '🏆', 100, GETDATE()),
    ('200kg_club', '200kgクラブ', 'いずれかの種目で200kg以上を達成', 'strength', 'lift_200kg', '👑', 500, GETDATE()),
    ('volume_10k', 'ボリューム1万', '1回のワークアウトで総負荷量1万kg達成', 'volume', 'session_volume_10000', '📊', 75, GETDATE()),
    ('early_bird', '早起き鳥', '朝6時前にワークアウトを完了', 'special', 'workout_before_6am', '🌅', 25, GETDATE()),
    ('night_owl', '夜のフクロウ', '夜10時以降にワークアウトを完了', 'special', 'workout_after_10pm', '🦉', 25, GETDATE());

    -- ユーザー実績の付与
    INSERT INTO user_achievement (user_common_id, achievement_id, unlocked_at, progress) VALUES
    -- ユーザー1
    ('user_001', 'first_workout', DATEADD(DAY, -14, GETDATE()), 100),
    
    -- ユーザー2
    ('user_002', 'first_workout', DATEADD(DAY, -30, GETDATE()), 100),
    ('user_002', 'week_streak', DATEADD(DAY, -10, GETDATE()), 100),
    ('user_002', '100kg_club', DATEADD(DAY, -5, GETDATE()), 100),
    
    -- ユーザー3
    ('user_003', 'first_workout', DATEADD(DAY, -30, GETDATE()), 100),
    ('user_003', 'week_streak', DATEADD(DAY, -20, GETDATE()), 100),
    ('user_003', 'month_streak', DATEADD(DAY, -1, GETDATE()), 100),
    ('user_003', '100kg_club', DATEADD(DAY, -25, GETDATE()), 100),
    ('user_003', '200kg_club', DATEADD(DAY, -1, GETDATE()), 100),
    ('user_003', 'volume_10k', DATEADD(DAY, -15, GETDATE()), 100);
END

-- ===================================
-- お気に入りメニューの設定
-- ===================================
IF OBJECT_ID('user_favorite_menu', 'U') IS NOT NULL
BEGIN
    INSERT INTO user_favorite_menu (user_common_id, menu_id, created_at) VALUES
    -- ユーザー1のお気に入り
    ('user_001', 'leg_press', GETDATE()),
    ('user_001', 'lat_pulldown', GETDATE()),
    
    -- ユーザー2のお気に入り
    ('user_002', 'bench_press', GETDATE()),
    ('user_002', 'squat', GETDATE()),
    ('user_002', 'deadlift', GETDATE()),
    
    -- ユーザー3のお気に入り
    ('user_003', 'bench_press', GETDATE()),
    ('user_003', 'squat', GETDATE()),
    ('user_003', 'deadlift', GETDATE()),
    ('user_003', 'pull_up', GETDATE()),
    ('user_003', 'shoulder_press', GETDATE());
END

-- ===================================
-- 体重・体脂肪率の記録
-- ===================================
IF OBJECT_ID('body_measurement', 'U') IS NOT NULL
BEGIN
    -- ユーザー2の体重記録（30日分）
    DECLARE @measurement_date DATE = DATEADD(DAY, -30, GETDATE());
    DECLARE @weight DECIMAL(5,2) = 75.0;
    DECLARE @body_fat DECIMAL(4,2) = 18.0;
    
    WHILE @measurement_date <= GETDATE()
    BEGIN
        -- 週1回記録（日曜日）
        IF DATEPART(WEEKDAY, @measurement_date) = 1
        BEGIN
            INSERT INTO body_measurement (user_common_id, measurement_date, weight, body_fat_percentage, created_at)
            VALUES ('user_002', @measurement_date, @weight, @body_fat, GETDATE());
            
            -- 徐々に体重減少、体脂肪率低下
            SET @weight = @weight - 0.2;
            SET @body_fat = @body_fat - 0.3;
        END
        
        SET @measurement_date = DATEADD(DAY, 1, @measurement_date);
    END
    
    -- ユーザー3の体重記録（増量期）
    SET @measurement_date = DATEADD(DAY, -30, GETDATE());
    SET @weight = 85.0;
    SET @body_fat = 12.0;
    
    WHILE @measurement_date <= GETDATE()
    BEGIN
        -- 週2回記録（水・日）
        IF DATEPART(WEEKDAY, @measurement_date) IN (1, 4)
        BEGIN
            INSERT INTO body_measurement (user_common_id, measurement_date, weight, body_fat_percentage, muscle_mass, created_at)
            VALUES ('user_003', @measurement_date, @weight, @body_fat, @weight * (1 - @body_fat/100) * 0.5, GETDATE());
            
            -- 徐々に体重増加（筋量増加）
            SET @weight = @weight + 0.1;
            SET @body_fat = @body_fat + 0.05;
        END
        
        SET @measurement_date = DATEADD(DAY, 1, @measurement_date);
    END
END

-- ===================================
-- コメント・メモの追加
-- ===================================
UPDATE TrainingRecordSets
SET Note = '調子良かった！次回は重量アップ予定'
WHERE UserCommonId = 'user_002' 
  AND MenuId = 'bench_press' 
  AND TrainingDate = CAST(GETDATE() AS DATE)
  AND SetNumber = 5;

UPDATE TrainingRecordSets
SET Note = '新記録達成！フォームも完璧だった'
WHERE UserCommonId = 'user_003' 
  AND MenuId = 'deadlift' 
  AND TrainingDate = CAST(GETDATE() AS DATE)
  AND Weight = 200;

-- ===================================
-- 統計サマリーの確認用ビュー作成
-- ===================================
IF OBJECT_ID('v_user_training_summary', 'V') IS NOT NULL
    DROP VIEW v_user_training_summary;
GO

CREATE VIEW v_user_training_summary AS
SELECT 
    u.LoginId,
    u.DisplayName,
    COUNT(DISTINCT dtr.TrainingDate) as total_training_days,
    COUNT(DISTINCT dtr.MenuId) as unique_exercises,
    SUM(dtr.TotalLoadAmount) as total_volume,
    MAX(dtr.TrainingDate) as last_workout_date,
    DATEDIFF(DAY, MIN(dtr.TrainingDate), MAX(dtr.TrainingDate)) + 1 as training_period_days
FROM Users u
LEFT JOIN DailyTrainingRecord dtr ON u.UserCommonId = dtr.UserCommonId
GROUP BY u.LoginId, u.DisplayName;
GO

PRINT 'テストデータの投入が完了しました。';
PRINT '';
PRINT '=== テストユーザー情報 ===';
PRINT 'ログインID: beginner_user, intermediate_user, advanced_user, demo, admin';
PRINT 'パスワード: Test123! (全ユーザー共通)';
PRINT '';
PRINT '=== 投入されたデータ ===';
PRINT '- トレーニングメニュー: 29種目';
PRINT '- タグ: 21種類';
PRINT '- ユーザー: 5名';
PRINT '- トレーニング記録: 過去30日分';
PRINT '- 目標・実績・お気に入りなどの追加データ';
PRINT '';
PRINT 'v_user_training_summary ビューでユーザーごとの統計を確認できます。';