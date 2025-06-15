-- ===================================
-- サンプルトレーニングデータの投入
-- 過去30日分のトレーニング記録を生成
-- ===================================

-- 変数宣言
DECLARE @today DATE = GETDATE();
DECLARE @user_id NVARCHAR(50);
DECLARE @menu_id NVARCHAR(50);
DECLARE @training_date DATE;
DECLARE @set_number INT;
DECLARE @weight DECIMAL(5,2);
DECLARE @reps INT;

-- ===================================
-- ユーザー2（中級者）のトレーニングデータ
-- ===================================
SET @user_id = 'user_002';

-- 30日前からのベンチプレス記録
SET @training_date = DATEADD(DAY, -30, @today);
WHILE @training_date <= @today
BEGIN
    -- 週3回トレーニング（月・水・金）
    IF DATEPART(WEEKDAY, @training_date) IN (2, 4, 6)
    BEGIN
        -- ベンチプレス（徐々に重量アップ）
        SET @weight = 60 + DATEDIFF(DAY, DATEADD(DAY, -30, @today), @training_date) * 0.5;
        
        -- 5セット記録
        SET @set_number = 1;
        WHILE @set_number <= 5
        BEGIN
            SET @reps = CASE 
                WHEN @set_number <= 3 THEN 10
                WHEN @set_number = 4 THEN 8
                ELSE 6
            END;
            
            INSERT INTO TrainingRecordSets (UserCommonId, MenuId, TrainingDate, SetNumber, Reps, Weight, CreatedAt)
            VALUES (@user_id, 'bench_press', @training_date, @set_number, @reps, @weight, GETDATE());
            
            SET @set_number = @set_number + 1;
        END;
        
        -- スクワット（徐々に重量アップ）
        SET @weight = 80 + DATEDIFF(DAY, DATEADD(DAY, -30, @today), @training_date) * 0.8;
        
        SET @set_number = 1;
        WHILE @set_number <= 4
        BEGIN
            SET @reps = CASE 
                WHEN @set_number <= 2 THEN 12
                WHEN @set_number = 3 THEN 10
                ELSE 8
            END;
            
            INSERT INTO TrainingRecordSets (UserCommonId, MenuId, TrainingDate, SetNumber, Reps, Weight, CreatedAt)
            VALUES (@user_id, 'squat', @training_date, @set_number, @reps, @weight, GETDATE());
            
            SET @set_number = @set_number + 1;
        END;
    END
    
    -- 週2回の軽いトレーニング（火・木）
    IF DATEPART(WEEKDAY, @training_date) IN (3, 5)
    BEGIN
        -- プッシュアップ（自重）
        SET @set_number = 1;
        WHILE @set_number <= 3
        BEGIN
            SET @reps = 15 + @set_number * 5;
            
            INSERT INTO TrainingRecordSets (UserCommonId, MenuId, TrainingDate, SetNumber, Reps, Weight, CreatedAt)
            VALUES (@user_id, 'push_up', @training_date, @set_number, @reps, NULL, GETDATE());
            
            SET @set_number = @set_number + 1;
        END;
        
        -- プランク（秒数をrepsとして記録）
        INSERT INTO TrainingRecordSets (UserCommonId, MenuId, TrainingDate, SetNumber, Reps, Weight, CreatedAt)
        VALUES 
        (@user_id, 'plank', @training_date, 1, 60, NULL, GETDATE()),
        (@user_id, 'plank', @training_date, 2, 45, NULL, GETDATE()),
        (@user_id, 'plank', @training_date, 3, 30, NULL, GETDATE());
    END
    
    SET @training_date = DATEADD(DAY, 1, @training_date);
END;

-- ===================================
-- ユーザー3（上級者）のトレーニングデータ
-- ===================================
SET @user_id = 'user_003';

-- 30日前からの高強度トレーニング記録
SET @training_date = DATEADD(DAY, -30, @today);
WHILE @training_date <= @today
BEGIN
    -- 週5回トレーニング（月～金）
    IF DATEPART(WEEKDAY, @training_date) BETWEEN 2 AND 6
    BEGIN
        -- 月曜：胸の日
        IF DATEPART(WEEKDAY, @training_date) = 2
        BEGIN
            -- ベンチプレス（高重量）
            INSERT INTO TrainingRecordSets (UserCommonId, MenuId, TrainingDate, SetNumber, Reps, Weight, CreatedAt)
            VALUES 
            (@user_id, 'bench_press', @training_date, 1, 12, 80, GETDATE()),
            (@user_id, 'bench_press', @training_date, 2, 10, 90, GETDATE()),
            (@user_id, 'bench_press', @training_date, 3, 8, 100, GETDATE()),
            (@user_id, 'bench_press', @training_date, 4, 6, 110, GETDATE()),
            (@user_id, 'bench_press', @training_date, 5, 4, 120, GETDATE());
            
            -- インクラインベンチプレス
            INSERT INTO TrainingRecordSets (UserCommonId, MenuId, TrainingDate, SetNumber, Reps, Weight, CreatedAt)
            VALUES 
            (@user_id, 'incline_bench_press', @training_date, 1, 12, 60, GETDATE()),
            (@user_id, 'incline_bench_press', @training_date, 2, 10, 70, GETDATE()),
            (@user_id, 'incline_bench_press', @training_date, 3, 8, 80, GETDATE()),
            (@user_id, 'incline_bench_press', @training_date, 4, 8, 80, GETDATE());
        END
        
        -- 火曜：背中の日
        IF DATEPART(WEEKDAY, @training_date) = 3
        BEGIN
            -- デッドリフト（高重量）
            INSERT INTO TrainingRecordSets (UserCommonId, MenuId, TrainingDate, SetNumber, Reps, Weight, CreatedAt)
            VALUES 
            (@user_id, 'deadlift', @training_date, 1, 8, 120, GETDATE()),
            (@user_id, 'deadlift', @training_date, 2, 6, 140, GETDATE()),
            (@user_id, 'deadlift', @training_date, 3, 4, 160, GETDATE()),
            (@user_id, 'deadlift', @training_date, 4, 3, 180, GETDATE()),
            (@user_id, 'deadlift', @training_date, 5, 2, 200, GETDATE());
            
            -- 懸垂（加重）
            INSERT INTO TrainingRecordSets (UserCommonId, MenuId, TrainingDate, SetNumber, Reps, Weight, CreatedAt)
            VALUES 
            (@user_id, 'pull_up', @training_date, 1, 12, 10, GETDATE()),
            (@user_id, 'pull_up', @training_date, 2, 10, 15, GETDATE()),
            (@user_id, 'pull_up', @training_date, 3, 8, 20, GETDATE()),
            (@user_id, 'pull_up', @training_date, 4, 6, 25, GETDATE());
        END
        
        -- 水曜：脚の日
        IF DATEPART(WEEKDAY, @training_date) = 4
        BEGIN
            -- スクワット（高重量）
            INSERT INTO TrainingRecordSets (UserCommonId, MenuId, TrainingDate, SetNumber, Reps, Weight, CreatedAt)
            VALUES 
            (@user_id, 'squat', @training_date, 1, 12, 100, GETDATE()),
            (@user_id, 'squat', @training_date, 2, 10, 120, GETDATE()),
            (@user_id, 'squat', @training_date, 3, 8, 140, GETDATE()),
            (@user_id, 'squat', @training_date, 4, 6, 160, GETDATE()),
            (@user_id, 'squat', @training_date, 5, 4, 180, GETDATE());
            
            -- ルーマニアンデッドリフト
            INSERT INTO TrainingRecordSets (UserCommonId, MenuId, TrainingDate, SetNumber, Reps, Weight, CreatedAt)
            VALUES 
            (@user_id, 'romanian_deadlift', @training_date, 1, 12, 80, GETDATE()),
            (@user_id, 'romanian_deadlift', @training_date, 2, 10, 90, GETDATE()),
            (@user_id, 'romanian_deadlift', @training_date, 3, 10, 100, GETDATE()),
            (@user_id, 'romanian_deadlift', @training_date, 4, 8, 110, GETDATE());
        END
        
        -- 木曜：肩と腕の日
        IF DATEPART(WEEKDAY, @training_date) = 5
        BEGIN
            -- ショルダープレス
            INSERT INTO training_record_set (user_common_id, menu_id, training_date, set_number, reps, weight, created_at)
            VALUES 
            (@user_id, 'shoulder_press', @training_date, 1, 12, 30, GETDATE()),
            (@user_id, 'shoulder_press', @training_date, 2, 10, 35, GETDATE()),
            (@user_id, 'shoulder_press', @training_date, 3, 8, 40, GETDATE()),
            (@user_id, 'shoulder_press', @training_date, 4, 6, 45, GETDATE());
            
            -- バイセップカール
            INSERT INTO training_record_set (user_common_id, menu_id, training_date, set_number, reps, weight, created_at)
            VALUES 
            (@user_id, 'bicep_curl', @training_date, 1, 15, 15, GETDATE()),
            (@user_id, 'bicep_curl', @training_date, 2, 12, 17.5, GETDATE()),
            (@user_id, 'bicep_curl', @training_date, 3, 10, 20, GETDATE()),
            (@user_id, 'bicep_curl', @training_date, 4, 8, 22.5, GETDATE());
        END
        
        -- 金曜：体幹と有酸素の日
        IF DATEPART(WEEKDAY, @training_date) = 6
        BEGIN
            -- ハンギングレッグレイズ
            INSERT INTO training_record_set (user_common_id, menu_id, training_date, set_number, reps, weight, created_at)
            VALUES 
            (@user_id, 'hanging_leg_raise', @training_date, 1, 15, NULL, GETDATE()),
            (@user_id, 'hanging_leg_raise', @training_date, 2, 12, NULL, GETDATE()),
            (@user_id, 'hanging_leg_raise', @training_date, 3, 10, NULL, GETDATE()),
            (@user_id, 'hanging_leg_raise', @training_date, 4, 8, NULL, GETDATE());
            
            -- ランニング（分数をrepsとして記録）
            INSERT INTO training_record_set (user_common_id, menu_id, training_date, set_number, reps, weight, created_at)
            VALUES 
            (@user_id, 'running', @training_date, 1, 30, NULL, GETDATE());
        END
    END
    
    SET @training_date = DATEADD(DAY, 1, @training_date);
END;

-- ===================================
-- ユーザー1（初心者）のトレーニングデータ
-- ===================================
SET @user_id = 'user_001';

-- 最近2週間の軽いトレーニング記録
SET @training_date = DATEADD(DAY, -14, @today);
WHILE @training_date <= @today
BEGIN
    -- 週2回トレーニング（火・金）
    IF DATEPART(WEEKDAY, @training_date) IN (3, 6)
    BEGIN
        -- レッグプレス（マシン）
        INSERT INTO TrainingRecordSets (UserCommonId, MenuId, TrainingDate, SetNumber, Reps, Weight, CreatedAt)
        VALUES 
        (@user_id, 'leg_press', @training_date, 1, 15, 40, GETDATE()),
        (@user_id, 'leg_press', @training_date, 2, 12, 50, GETDATE()),
        (@user_id, 'leg_press', @training_date, 3, 10, 60, GETDATE());
        
        -- ラットプルダウン
        INSERT INTO TrainingRecordSets (UserCommonId, MenuId, TrainingDate, SetNumber, Reps, Weight, CreatedAt)
        VALUES 
        (@user_id, 'lat_pulldown', @training_date, 1, 15, 30, GETDATE()),
        (@user_id, 'lat_pulldown', @training_date, 2, 12, 35, GETDATE()),
        (@user_id, 'lat_pulldown', @training_date, 3, 10, 40, GETDATE());
        
        -- クランチ
        INSERT INTO TrainingRecordSets (UserCommonId, MenuId, TrainingDate, SetNumber, Reps, Weight, CreatedAt)
        VALUES 
        (@user_id, 'crunch', @training_date, 1, 20, NULL, GETDATE()),
        (@user_id, 'crunch', @training_date, 2, 15, NULL, GETDATE()),
        (@user_id, 'crunch', @training_date, 3, 10, NULL, GETDATE());
    END
    
    SET @training_date = DATEADD(DAY, 1, @training_date);
END;

-- ===================================
-- デイリートレーニングレコードの集計
-- トリガーがない場合は手動で集計
-- ===================================

-- 各ユーザーの日次集計を作成
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