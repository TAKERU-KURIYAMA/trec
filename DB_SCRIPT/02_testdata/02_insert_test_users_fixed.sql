-- ===================================
-- テストユーザーデータの投入（修正版）
-- APIモデルに合わせてテーブル名とカラム名を修正
-- ===================================

USE MessageRDB;
GO

-- ===================================
-- テストユーザーの作成
-- ===================================
-- パスワードは全て "Test123!" のPBKDF2ハッシュ値
-- 注意: 実際の本番環境では適切なハッシュ化を行ってください
DECLARE @passwordHash NVARCHAR(512) = 'AQAAAAIAAYagAAAAEOKW4Ye1yMtXFoGR8Z7kH5dGOa+GlYmYnZxGlKrXpH3vg=='; -- Test123!のハッシュ値
DECLARE @passwordSalt NVARCHAR(512) = 'mK9wE4lOlIGhFrZa3vxT5Q=='; -- ソルト値

-- ===================================
-- ユーザー1: 初心者ユーザー
-- ===================================
INSERT INTO Users (UserCommonId, LoginId, PasswordHash, PasswordSalt, DisplayName) VALUES
('user_001', 'beginner_user', @passwordHash, @passwordSalt, '初心者太郎');

-- ユーザーデータ投入
INSERT INTO UserData (UserCommonId, ClientId, DataKey, DataValue) VALUES
('user_001', 'default_web', 'display_name', '初心者太郎'),
('user_001', 'default_web', 'preferred_units', 'metric'),
('user_001', 'default_web', 'theme', 'light');
GO

-- ===================================
-- ユーザー2: 中級者ユーザー
-- ===================================
INSERT INTO Users (UserCommonId, LoginId, PasswordHash, PasswordSalt, DisplayName) VALUES
('user_002', 'intermediate_user', @passwordHash, @passwordSalt, 'トレーニング花子');

-- ユーザーデータ投入
INSERT INTO UserData (UserCommonId, ClientId, DataKey, DataValue) VALUES
('user_002', 'default_web', 'display_name', 'トレーニング花子'),
('user_002', 'default_web', 'preferred_units', 'metric'),
('user_002', 'default_web', 'theme', 'dark'),
('user_002', 'default_mobile', 'display_name', 'トレーニング花子'),
('user_002', 'default_mobile', 'preferred_units', 'metric');
GO

-- ===================================
-- ユーザー3: 上級者ユーザー
-- ===================================
INSERT INTO Users (UserCommonId, LoginId, PasswordHash, PasswordSalt, DisplayName) VALUES
('user_003', 'advanced_user', @passwordHash, @passwordSalt, 'マッスル次郎');

-- ユーザーデータ投入
INSERT INTO UserData (UserCommonId, ClientId, DataKey, DataValue) VALUES
('user_003', 'default_web', 'display_name', 'マッスル次郎'),
('user_003', 'default_web', 'preferred_units', 'imperial'),
('user_003', 'default_web', 'theme', 'dark'),
('user_003', 'default_mobile', 'display_name', 'マッスル次郎'),
('user_003', 'default_mobile', 'preferred_units', 'imperial');
GO

-- ===================================
-- ユーザー4: デモユーザー
-- ===================================
INSERT INTO Users (UserCommonId, LoginId, PasswordHash, PasswordSalt, DisplayName) VALUES
('user_demo', 'demo', @passwordHash, @passwordSalt, 'デモユーザー');

-- ユーザーデータ投入
INSERT INTO UserData (UserCommonId, ClientId, DataKey, DataValue) VALUES
('user_demo', 'default_web', 'display_name', 'デモユーザー'),
('user_demo', 'default_web', 'preferred_units', 'metric'),
('user_demo', 'default_web', 'theme', 'light');
GO

-- ===================================
-- 管理者ユーザー
-- ===================================
INSERT INTO Users (UserCommonId, LoginId, PasswordHash, PasswordSalt, DisplayName) VALUES
('user_admin', 'admin', @passwordHash, @passwordSalt, '管理者');

-- ユーザーデータ投入
INSERT INTO UserData (UserCommonId, ClientId, DataKey, DataValue) VALUES
('user_admin', 'default_web', 'display_name', '管理者'),
('user_admin', 'default_web', 'preferred_units', 'metric'),
('user_admin', 'default_web', 'theme', 'dark');
GO

PRINT '===================================';
PRINT 'テストユーザーの作成が完了しました';
PRINT '===================================';
PRINT '';
PRINT '作成されたユーザー:';
PRINT '- beginner_user (初心者太郎) - パスワード: Test123!';
PRINT '- intermediate_user (トレーニング花子) - パスワード: Test123!';
PRINT '- advanced_user (マッスル次郎) - パスワード: Test123!';
PRINT '- demo (デモユーザー) - パスワード: Test123!';
PRINT '- admin (管理者) - パスワード: Test123!';
PRINT '';
PRINT '次のステップ: サンプルトレーニングデータの投入';
GO