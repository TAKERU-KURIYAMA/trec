-- ===================================
-- 既存テストデータのクリア
-- ===================================

USE TrecPlansRDB;
GO

PRINT '既存テストデータをクリア中...';

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
GO