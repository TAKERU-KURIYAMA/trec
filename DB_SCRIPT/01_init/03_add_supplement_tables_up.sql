-- サプリメント管理機能のテーブル作成

-- サプリメントマスタテーブル
CREATE TABLE supplement_master (
    supplement_id INT IDENTITY(1,1) PRIMARY KEY,
    user_id INT NOT NULL,
    supplement_name NVARCHAR(100) NOT NULL,
    unit NVARCHAR(20) NOT NULL, -- 錠、ml、g など
    description NVARCHAR(500),
    is_active BIT NOT NULL DEFAULT 1,
    created_at DATETIME2 DEFAULT GETDATE(),
    updated_at DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT FK_supplement_user FOREIGN KEY (user_id) REFERENCES users(user_id)
);

-- サプリメント摂取記録テーブル
CREATE TABLE supplement_intake_records (
    record_id INT IDENTITY(1,1) PRIMARY KEY,
    user_id INT NOT NULL,
    supplement_id INT NOT NULL,
    intake_date DATE NOT NULL,
    intake_time TIME NOT NULL,
    amount DECIMAL(10,2) NOT NULL,
    timing_type NVARCHAR(20), -- 朝、昼、夜、トレーニング前、トレーニング後など
    memo NVARCHAR(500),
    created_at DATETIME2 DEFAULT GETDATE(),
    updated_at DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT FK_intake_user FOREIGN KEY (user_id) REFERENCES users(user_id),
    CONSTRAINT FK_intake_supplement FOREIGN KEY (supplement_id) REFERENCES supplement_master(supplement_id)
);

-- サプリメント摂取スケジュールテーブル
CREATE TABLE supplement_schedules (
    schedule_id INT IDENTITY(1,1) PRIMARY KEY,
    user_id INT NOT NULL,
    supplement_id INT NOT NULL,
    schedule_time TIME NOT NULL,
    amount DECIMAL(10,2) NOT NULL,
    timing_type NVARCHAR(20), -- 朝、昼、夜、トレーニング前、トレーニング後など
    days_of_week NVARCHAR(20), -- 1,2,3,4,5,6,7 (月〜日) または ALL
    is_active BIT NOT NULL DEFAULT 1,
    memo NVARCHAR(500),
    created_at DATETIME2 DEFAULT GETDATE(),
    updated_at DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT FK_schedule_user FOREIGN KEY (user_id) REFERENCES users(user_id),
    CONSTRAINT FK_schedule_supplement FOREIGN KEY (supplement_id) REFERENCES supplement_master(supplement_id)
);

-- インデックスの作成
CREATE INDEX IX_supplement_master_user_id ON supplement_master(user_id);
CREATE INDEX IX_intake_records_user_date ON supplement_intake_records(user_id, intake_date);
CREATE INDEX IX_intake_records_supplement_id ON supplement_intake_records(supplement_id);
CREATE INDEX IX_schedules_user_id ON supplement_schedules(user_id);
CREATE INDEX IX_schedules_supplement_id ON supplement_schedules(supplement_id);