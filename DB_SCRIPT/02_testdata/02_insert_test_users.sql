-- ===================================
-- テストユーザーデータの投入（修正版）
-- APIモデルに合わせてテーブル名とカラム名を修正
-- ===================================

USE TrecPlansRDB;
GO

-- テストユーザーの作成
-- 注意: パスワードハッシュは実際のAuthServiceで生成されたものではありません
-- 実際の使用には、アプリケーション起動後にAPI経由でユーザーを作成してください
-- create_admin_via_api.sh スクリプトを使用することを推奨

-- 開発用のダミーユーザー（ログイン不可）
INSERT INTO Users (UserCommonId, LoginId, PasswordHash, PasswordSalt, DisplayName, CreatedAt, UpdatedAt) VALUES
('user_001', N'beginner_user', N'dummy_hash_1', N'dummy_salt_1', N'初心者太郎', GETDATE(), GETDATE()),
('user_002', N'intermediate_user', N'dummy_hash_2', N'dummy_salt_2', N'トレーニング花子', GETDATE(), GETDATE()),
('user_003', N'advanced_user', N'dummy_hash_3', N'dummy_salt_3', N'マッスル次郎', GETDATE(), GETDATE()),
('user_demo', N'demo', N'dummy_hash_demo', N'dummy_salt_demo', N'デモユーザー', GETDATE(), GETDATE());

-- ユーザーデータ（Key-Value形式）の投入
INSERT INTO UserData (UserCommonId, ClientId, DataKey, DataValue, CreatedAt, UpdatedAt) VALUES
-- ユーザー1のデータ
('user_001', N'default_web', N'display_name', N'初心者太郎', GETDATE(), GETDATE()),
('user_001', N'default_web', N'preferred_units', N'metric', GETDATE(), GETDATE()),
('user_001', N'default_web', N'theme', N'light', GETDATE(), GETDATE()),

-- ユーザー2のデータ
('user_002', N'default_web', N'display_name', N'トレーニング花子', GETDATE(), GETDATE()),
('user_002', N'default_web', N'preferred_units', N'metric', GETDATE(), GETDATE()),
('user_002', N'default_web', N'theme', N'dark', GETDATE(), GETDATE()),

-- ユーザー3のデータ
('user_003', N'default_web', N'display_name', N'マッスル次郎', GETDATE(), GETDATE()),
('user_003', N'default_web', N'preferred_units', N'imperial', GETDATE(), GETDATE()),
('user_003', N'default_web', N'theme', N'dark', GETDATE(), GETDATE()),

-- デモユーザーのデータ
('user_demo', N'default_web', N'display_name', N'デモユーザー', GETDATE(), GETDATE()),
('user_demo', N'default_web', N'preferred_units', N'metric', GETDATE(), GETDATE()),
('user_demo', N'default_web', N'theme', N'light', GETDATE(), GETDATE()),

-- 管理者のデータ
('user_admin', N'default_web', N'display_name', N'管理者', GETDATE(), GETDATE()),
('user_admin', N'default_web', N'preferred_units', N'metric', GETDATE(), GETDATE()),
('user_admin', N'default_web', N'theme', N'light', GETDATE(), GETDATE());
GO

PRINT '===================================';
PRINT 'テストユーザーデータ投入完了';
PRINT '===================================';
PRINT '';
PRINT '作成されたユーザー:';
PRINT '- user_001 (beginner_user): 初心者太郎';
PRINT '- user_002 (intermediate_user): トレーニング花子';
PRINT '- user_003 (advanced_user): マッスル次郎';
PRINT '- user_demo (demo): デモユーザー';
PRINT '- user_admin (admin): 管理者';
PRINT '';
PRINT 'パスワード: Test123! (全ユーザー共通)';
PRINT '次のステップ: サンプルトレーニングデータの投入';
GO