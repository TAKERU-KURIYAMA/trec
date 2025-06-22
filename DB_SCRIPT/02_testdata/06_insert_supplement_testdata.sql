-- サプリメントマスタのテストデータ
INSERT INTO supplement_master (user_id, supplement_name, unit, description, is_active)
VALUES 
-- test_user_001のサプリメント
(1, N'プロテイン', N'g', N'ホエイプロテイン（バニラ味）', 1),
(1, N'BCAA', N'g', N'運動前後に摂取', 1),
(1, N'マルチビタミン', N'錠', N'1日1錠', 1),
(1, N'クレアチン', N'g', N'トレーニング前に摂取', 1),
(1, N'グルタミン', N'g', N'回復促進', 1),

-- test_user_002のサプリメント
(2, N'プロテイン', N'g', N'ソイプロテイン', 1),
(2, N'ビタミンC', N'mg', N'免疫力向上', 1),
(2, N'亜鉛', N'mg', N'1日15mg', 1),
(2, N'オメガ3', N'カプセル', N'DHA/EPA', 1),

-- adminのサプリメント
(3, N'プロテイン', N'g', N'カゼインプロテイン', 1),
(3, N'ビタミンD', N'IU', N'日光不足を補う', 1),
(3, N'マグネシウム', N'mg', N'筋肉の疲労回復', 1);

-- サプリメント摂取記録のテストデータ（過去7日分）
DECLARE @today DATE = GETDATE();
DECLARE @i INT = 0;

WHILE @i < 7
BEGIN
    DECLARE @date DATE = DATEADD(DAY, -@i, @today);
    
    -- test_user_001の記録
    INSERT INTO supplement_intake_records (user_id, supplement_id, intake_date, intake_time, amount, timing_type, memo)
    VALUES 
    (1, 1, @date, '07:00:00', 30, N'朝', N'朝食後'),
    (1, 1, @date, '15:00:00', 30, N'トレーニング後', N'トレーニング後30分以内'),
    (1, 3, @date, '08:00:00', 1, N'朝', N'朝食と一緒に'),
    (1, 4, @date, '14:30:00', 5, N'トレーニング前', NULL);
    
    -- test_user_002の記録（1日おき）
    IF @i % 2 = 0
    BEGIN
        INSERT INTO supplement_intake_records (user_id, supplement_id, intake_date, intake_time, amount, timing_type, memo)
        VALUES 
        (2, 6, @date, '08:30:00', 20, N'朝', NULL),
        (2, 7, @date, '08:30:00', 1000, N'朝', N'朝食後'),
        (2, 9, @date, '21:00:00', 2, N'夜', N'就寝前');
    END
    
    SET @i = @i + 1;
END

-- サプリメント摂取スケジュールのテストデータ
INSERT INTO supplement_schedules (user_id, supplement_id, schedule_time, amount, timing_type, days_of_week, is_active, memo)
VALUES 
-- test_user_001のスケジュール
(1, 1, '07:00:00', 30, N'朝', 'ALL', 1, N'朝食後に摂取'),
(1, 1, '15:00:00', 30, N'トレーニング後', '1,3,5', 1, N'トレーニング日のみ'),
(1, 3, '08:00:00', 1, N'朝', 'ALL', 1, N'毎日'),
(1, 4, '14:30:00', 5, N'トレーニング前', '1,3,5', 1, N'トレーニング日のみ'),
(1, 5, '22:00:00', 5, N'夜', 'ALL', 1, N'就寝前'),

-- test_user_002のスケジュール
(2, 6, '08:30:00', 20, N'朝', 'ALL', 1, N'朝食と一緒に'),
(2, 7, '08:30:00', 1000, N'朝', 'ALL', 1, NULL),
(2, 8, '12:00:00', 15, N'昼', 'ALL', 1, N'昼食後'),
(2, 9, '21:00:00', 2, N'夜', 'ALL', 1, N'就寝1時間前'),

-- adminのスケジュール
(3, 10, '22:00:00', 30, N'夜', 'ALL', 1, N'就寝前のカゼイン'),
(3, 11, '09:00:00', 2000, N'朝', 'ALL', 1, N'朝食後'),
(3, 12, '21:00:00', 400, N'夜', 'ALL', 1, N'就寝前');