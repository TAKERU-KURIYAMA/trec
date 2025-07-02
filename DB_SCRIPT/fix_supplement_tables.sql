-- サプリメント関連テーブルの修正スクリプト
-- user_id (INT) を user_common_id (VARCHAR(16)) に変更

-- 既存のテーブルを削除（存在する場合）
IF OBJECT_ID('supplement_schedules', 'U') IS NOT NULL
    DROP TABLE supplement_schedules;
GO

IF OBJECT_ID('supplement_intake_records', 'U') IS NOT NULL
    DROP TABLE supplement_intake_records;
GO

IF OBJECT_ID('supplement_master', 'U') IS NOT NULL
    DROP TABLE supplement_master;
GO

-- サプリメントマスタテーブル（修正版）
CREATE TABLE supplement_master (
    supplement_id INT IDENTITY(1,1) PRIMARY KEY,
    user_common_id VARCHAR(16) NOT NULL,
    supplement_name NVARCHAR(100) NOT NULL,
    unit NVARCHAR(20) NOT NULL, -- 錠、ml、g など
    description NVARCHAR(500),
    is_active BIT NOT NULL DEFAULT 1,
    created_at DATETIME2 DEFAULT GETDATE(),
    updated_at DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT FK_supplement_user FOREIGN KEY (user_common_id) REFERENCES Users(UserCommonId)
);

-- サプリメント摂取記録テーブル（修正版）
CREATE TABLE supplement_intake_records (
    record_id INT IDENTITY(1,1) PRIMARY KEY,
    user_common_id VARCHAR(16) NOT NULL,
    supplement_id INT NOT NULL,
    intake_date DATE NOT NULL,
    intake_time TIME NOT NULL,
    amount DECIMAL(10,2) NOT NULL,
    timing_type NVARCHAR(20), -- 朝、昼、夜、トレーニング前、トレーニング後など
    memo NVARCHAR(500),
    created_at DATETIME2 DEFAULT GETDATE(),
    updated_at DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT FK_intake_user FOREIGN KEY (user_common_id) REFERENCES Users(UserCommonId),
    CONSTRAINT FK_intake_supplement FOREIGN KEY (supplement_id) REFERENCES supplement_master(supplement_id)
);

-- サプリメント摂取スケジュールテーブル（修正版）
CREATE TABLE supplement_schedules (
    schedule_id INT IDENTITY(1,1) PRIMARY KEY,
    user_common_id VARCHAR(16) NOT NULL,
    supplement_id INT NOT NULL,
    schedule_time TIME NOT NULL,
    amount DECIMAL(10,2) NOT NULL,
    timing_type NVARCHAR(20), -- 朝、昼、夜、トレーニング前、トレーニング後など
    days_of_week NVARCHAR(20), -- 1,2,3,4,5,6,7 (月〜日) または ALL
    is_active BIT NOT NULL DEFAULT 1,
    memo NVARCHAR(500),
    created_at DATETIME2 DEFAULT GETDATE(),
    updated_at DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT FK_schedule_user FOREIGN KEY (user_common_id) REFERENCES Users(UserCommonId),
    CONSTRAINT FK_schedule_supplement FOREIGN KEY (supplement_id) REFERENCES supplement_master(supplement_id)
);

-- インデックスの作成
CREATE INDEX IX_supplement_master_user_common_id ON supplement_master(user_common_id);
CREATE INDEX IX_intake_records_user_date ON supplement_intake_records(user_common_id, intake_date);
CREATE INDEX IX_intake_records_supplement_id ON supplement_intake_records(supplement_id);
CREATE INDEX IX_schedules_user_common_id ON supplement_schedules(user_common_id);
CREATE INDEX IX_schedules_supplement_id ON supplement_schedules(supplement_id);

PRINT 'サプリメント関連テーブルの修正が完了しました。';
GO