-- ===================================
-- 修正版データベース完全セットアップスクリプト
-- APIモデルとの整合性を確保した統合スクリプト
-- ===================================

PRINT '===================================';
PRINT 'MessageRDB 修正版セットアップ開始';
PRINT '===================================';
PRINT '';

-- ===================================
-- 1. 既存データベースの削除（存在する場合）
-- ===================================
USE master;
GO

IF EXISTS (SELECT name FROM sys.databases WHERE name = 'MessageRDB')
BEGIN
    PRINT '既存のMessageRDBを削除中...';
    ALTER DATABASE MessageRDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE MessageRDB;
    PRINT '既存データベース削除完了';
END
GO

-- ===================================
-- 2. データベース作成
-- ===================================
PRINT '新しいMessageRDBを作成中...';
CREATE DATABASE MessageRDB;
GO
USE MessageRDB;
GO

-- ===================================
-- 3. テーブル作成
-- ===================================
PRINT 'テーブル作成中...';

-- ユーザー基本情報テーブル
CREATE TABLE Users (
    UserCommonId VARCHAR(16) PRIMARY KEY,
    LoginId NVARCHAR(256) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(512) NOT NULL,
    PasswordSalt NVARCHAR(512) NOT NULL,
    DisplayName NVARCHAR(100),
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2
);

-- クライアント情報テーブル
CREATE TABLE Clients (
    ClientId VARCHAR(64) PRIMARY KEY,
    ClientName NVARCHAR(100) NOT NULL,
    ClientSecret NVARCHAR(256) NOT NULL,
    Description NVARCHAR(255),
    CreatedAt DATETIME2 DEFAULT GETDATE()
);

-- クライアントデータキー定義テーブル
CREATE TABLE ClientDataKeys (
    ClientId VARCHAR(64) NOT NULL,
    DataKey NVARCHAR(100) NOT NULL,
    Description NVARCHAR(255),
    IsRequired BIT DEFAULT 0,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    PRIMARY KEY (ClientId, DataKey),
    FOREIGN KEY (ClientId) REFERENCES Clients(ClientId) ON DELETE CASCADE
);

-- ユーザーデータテーブル（Key-Value形式）
CREATE TABLE UserData (
    UserCommonId VARCHAR(16) NOT NULL,
    ClientId VARCHAR(64) NOT NULL,
    DataKey NVARCHAR(100) NOT NULL,
    DataValue NVARCHAR(MAX),
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2,
    PRIMARY KEY (UserCommonId, ClientId, DataKey),
    FOREIGN KEY (UserCommonId) REFERENCES Users(UserCommonId) ON DELETE CASCADE,
    FOREIGN KEY (ClientId) REFERENCES Clients(ClientId) ON DELETE CASCADE
);

-- トレーニングメニューマスター
CREATE TABLE TrainingMenus (
    MenuId VARCHAR(64) PRIMARY KEY,
    JPName NVARCHAR(100) NOT NULL,
    ENName VARCHAR(100) NOT NULL,
    Description NVARCHAR(255),
    CreatedAt DATETIME2 DEFAULT GETDATE()
);

-- タグマスター
CREATE TABLE TagMaster (
    TagId VARCHAR(64) PRIMARY KEY,
    JPName NVARCHAR(100) NOT NULL,
    ENName VARCHAR(100) NOT NULL,
    CreatedAt DATETIME2 DEFAULT GETDATE()
);

-- トレーニングタグ関連テーブル（中間テーブル）
CREATE TABLE TrainingTag (
    TagId VARCHAR(64) NOT NULL,
    MenuId VARCHAR(64) NOT NULL,
    JPName NVARCHAR(100) NOT NULL,
    ENName VARCHAR(100) NOT NULL,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    PRIMARY KEY (TagId, MenuId),
    FOREIGN KEY (MenuId) REFERENCES TrainingMenus(MenuId) ON DELETE CASCADE,
    FOREIGN KEY (TagId) REFERENCES TagMaster(TagId) ON DELETE CASCADE
);

-- トレーニング記録セットテーブル
CREATE TABLE TrainingRecordSets (
    UserCommonId VARCHAR(16) NOT NULL,
    MenuId VARCHAR(64) NOT NULL,
    TrainingDate DATE NOT NULL,
    SetNumber INT NOT NULL,
    Reps INT NOT NULL,
    Weight DECIMAL(5, 2),
    Note NVARCHAR(500),
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2 DEFAULT GETDATE(),
    PRIMARY KEY (UserCommonId, MenuId, TrainingDate, SetNumber),
    FOREIGN KEY (MenuId) REFERENCES TrainingMenus(MenuId) ON DELETE CASCADE,
    FOREIGN KEY (UserCommonId) REFERENCES Users(UserCommonId) ON DELETE CASCADE
);

-- 日次トレーニング記録サマリーテーブル
CREATE TABLE DailyTrainingRecord (
    UserCommonId VARCHAR(16) NOT NULL,
    MenuId VARCHAR(64) NOT NULL,
    TrainingDate DATE NOT NULL,
    SetCount INT NOT NULL,
    MaxReps INT NOT NULL,
    MaxRepsWeight DECIMAL(5, 2),
    MaxWeightReps INT,
    MaxWeight DECIMAL(5, 2),
    TotalLoadAmount DECIMAL(10, 2),
    TotalReps INT NOT NULL,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2 DEFAULT GETDATE(),
    PRIMARY KEY (UserCommonId, MenuId, TrainingDate),
    FOREIGN KEY (MenuId) REFERENCES TrainingMenus(MenuId) ON DELETE CASCADE,
    FOREIGN KEY (UserCommonId) REFERENCES Users(UserCommonId) ON DELETE CASCADE
);

-- ユーザートークンテーブル
CREATE TABLE UserTokens (
    UserCommonId VARCHAR(16) NOT NULL,
    RefreshToken NVARCHAR(512) PRIMARY KEY,
    ExpiresAt DATETIME2 NOT NULL,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    FOREIGN KEY (UserCommonId) REFERENCES Users(UserCommonId) ON DELETE CASCADE
);

PRINT 'テーブル作成完了';

-- ===================================
-- 4. インデックス作成
-- ===================================
PRINT 'インデックス作成中...';

CREATE UNIQUE INDEX IX_Users_LoginId ON Users(LoginId);
CREATE INDEX IX_TrainingRecordSets_UserDate ON TrainingRecordSets(UserCommonId, TrainingDate);
CREATE INDEX IX_TrainingRecordSets_UserMenu ON TrainingRecordSets(UserCommonId, MenuId);
CREATE INDEX IX_DailyTrainingRecord_UserDate ON DailyTrainingRecord(UserCommonId, TrainingDate);
CREATE INDEX IX_DailyTrainingRecord_UserMenu ON DailyTrainingRecord(UserCommonId, MenuId);
CREATE INDEX IX_UserTokens_UserCommonId ON UserTokens(UserCommonId);
CREATE INDEX IX_UserTokens_ExpiresAt ON UserTokens(ExpiresAt);

PRINT 'インデックス作成完了';

-- ===================================
-- 5. 初期データ投入
-- ===================================
PRINT '初期データ投入中...';

-- デフォルトクライアント作成
INSERT INTO Clients (ClientId, ClientName, ClientSecret, Description) VALUES
('default_web', 'Web Application', 'web_secret_key_2024', 'デフォルトWebアプリケーション'),
('default_mobile', 'Mobile Application', 'mobile_secret_key_2024', 'デフォルトモバイルアプリケーション');

-- デフォルトクライアントデータキー定義
INSERT INTO ClientDataKeys (ClientId, DataKey, Description, IsRequired) VALUES
('default_web', 'display_name', '表示名', 1),
('default_web', 'preferred_units', '単位設定（metric/imperial）', 0),
('default_web', 'theme', 'テーマ設定', 0),
('default_mobile', 'display_name', '表示名', 1),
('default_mobile', 'device_token', 'プッシュ通知用デバイストークン', 0),
('default_mobile', 'preferred_units', '単位設定（metric/imperial）', 0);

PRINT '初期データ投入完了';

-- ===================================
-- 6. データベースセットアップ完了
-- ===================================
PRINT '';
PRINT '===================================';
PRINT 'MessageRDB 修正版セットアップ完了';
PRINT '===================================';
PRINT '';
PRINT '作成されたテーブル:';
PRINT '- Users: ユーザー基本情報';
PRINT '- Clients: クライアント情報';  
PRINT '- ClientDataKeys: クライアントデータキー定義';
PRINT '- UserData: ユーザーデータ（Key-Value）';
PRINT '- TrainingMenus: トレーニングメニューマスター';
PRINT '- TagMaster: タグマスター';
PRINT '- TrainingTag: トレーニングタグ関連';
PRINT '- TrainingRecordSets: トレーニング記録セット';
PRINT '- DailyTrainingRecord: 日次トレーニング記録';
PRINT '- UserTokens: ユーザートークン';
PRINT '';
PRINT '次のステップ:';
PRINT '1. 01_insert_menus_and_tags_fixed.sql でメニューとタグを投入';
PRINT '2. 02_insert_test_users_fixed.sql でテストユーザーを作成';
PRINT '3. 03_insert_sample_training_data.sql でサンプルデータを投入';
PRINT '';
PRINT 'データベースはAPIモデルと完全に整合しています！';
GO