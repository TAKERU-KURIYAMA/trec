-- データベース作成
CREATE DATABASE MessageRDB;
GO
USE MessageRDB;
GO

-- ユーザー基本情報
CREATE TABLE Users (
    UserCommonId VARCHAR(16) PRIMARY KEY,
    LoginId NVARCHAR(256) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(512) NOT NULL,
    PasswordSalt NVARCHAR(512) NOT NULL,
    DisplayName NVARCHAR(100),
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2
);
GO

-- クライアント情報（アプリなど）
CREATE TABLE Clients (
    ClientId VARCHAR(64) PRIMARY KEY,
    ClientName NVARCHAR(100) NOT NULL,
    ClientSecret NVARCHAR(256) NOT NULL,
    Description NVARCHAR(255),
    CreatedAt DATETIME2 DEFAULT GETDATE()
);
GO

-- クライアントごとのユーザーデータ（Key-Value形式）
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
GO

-- クライアントが許可するデータキーの定義
CREATE TABLE ClientDataKeys (
    ClientId VARCHAR(64) NOT NULL,
    DataKey NVARCHAR(100) NOT NULL,
    Description NVARCHAR(255),
    IsRequired BIT DEFAULT 0,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    PRIMARY KEY (ClientId, DataKey),
    FOREIGN KEY (ClientId) REFERENCES Clients(ClientId) ON DELETE CASCADE
);
GO

-- トレーニング種目マスタ
CREATE TABLE TrainingMenus (
    MenuId VARCHAR(64) PRIMARY KEY,  -- ユーザーが指定するID
    JPName NVARCHAR(100) NOT NULL,
	ENName VARCHAR(100) NOT NULL,
    Description NVARCHAR(255),
    CreatedAt DATETIME2 DEFAULT GETDATE()
);
GO

-- トレーニングタグマスタ
CREATE TABLE TagMaster (
    TagId VARCHAR(64) PRIMARY KEY,  -- ユーザーが指定するID
    JPName NVARCHAR(100) NOT NULL,
	ENName VARCHAR(100) NOT NULL,
    CreatedAt DATETIME2 DEFAULT GETDATE()
);
GO

-- トレーニングタグリレーション
CREATE TABLE TrainingTag (
    TagId VARCHAR(64) NOT NULL,  -- ユーザーが指定するID
    MenuId VARCHAR(64) NOT NULL,
    JPName NVARCHAR(100) NOT NULL,
	ENName VARCHAR(100) NOT NULL,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
	PRIMARY KEY (TagId, MenuId),
    FOREIGN KEY (MenuId) REFERENCES TrainingMenus(MenuId),
    FOREIGN KEY (TagId) REFERENCES TagMaster(TagId)
);
GO

-- トレーニング記録（セット単位）
-- 子テーブル（トレーニング記録）
CREATE TABLE TrainingRecordSets (
    UserCommonId VARCHAR(16) NOT NULL,
    MenuId VARCHAR(64) NOT NULL,
    TrainingDate DATE NOT NULL,
    SetNumber INT NOT NULL,
    Reps INT NOT NULL,
    Weight DECIMAL(5, 2),
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2 DEFAULT GETDATE(),
    PRIMARY KEY (UserCommonId, MenuId, TrainingDate, SetNumber),
    FOREIGN KEY (MenuId) REFERENCES TrainingMenus(MenuId),
    FOREIGN KEY (UserCommonId) REFERENCES Users(UserCommonId)
);
GO

CREATE TABLE DailyTrainingRecord (
    UserCommonId VARCHAR(16) NOT NULL,
    MenuId VARCHAR(64) NOT NULL,
    TrainingDate DATE NOT NULL,
    SetCount INT NOT NULL, --セット数
	MaxReps INT NOT NULL, --rep数が最大のsetのrep数
    MaxRepsWeight DECIMAL(5, 2), --rep数が最大のsetの負荷
    MaxWeightReps INT,　--負荷が最大のsetのrep数
    MaxWeight DECIMAL(5, 2),　--負荷が最大のsetの負荷
	TotalLoadAmount DECIMAL(10, 2), --総負荷量(Reps*Weightの総計)
	TotalReps INT NOT NULL, --総Rep数
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2 DEFAULT GETDATE(),
    PRIMARY KEY (UserCommonId, MenuId, TrainingDate),
    FOREIGN KEY (MenuId) REFERENCES TrainingMenus(MenuId),
    FOREIGN KEY (UserCommonId) REFERENCES Users(UserCommonId)
);
GO

-- リフレッシュトークンなどセッション管理（必要に応じて）
CREATE TABLE UserTokens (
    UserCommonId VARCHAR(16) NOT NULL,
    RefreshToken NVARCHAR(512) NOT NULL,
    ExpiresAt DATETIME2 NOT NULL,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    PRIMARY KEY (RefreshToken),
    FOREIGN KEY (UserCommonId) REFERENCES Users(UserCommonId) ON DELETE CASCADE,
);
GO
