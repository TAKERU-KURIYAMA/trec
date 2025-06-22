-- ===================================
-- TrecPlansRDBデータベース削除スクリプト（修正版）
-- 依存関係を考慮した順序で削除
-- ===================================

USE master;
GO

-- アクティブな接続を切断してデータベース削除
IF EXISTS (SELECT name FROM sys.databases WHERE name = 'TrecPlansRDB')
BEGIN
    PRINT 'TrecPlansRDBデータベースを削除中...';
    ALTER DATABASE TrecPlansRDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE TrecPlansRDB;
    PRINT 'TrecPlansRDBデータベース削除完了';
END
ELSE
BEGIN
    PRINT 'TrecPlansRDBデータベースは存在しません';
END
GO

PRINT '==================================='
PRINT 'データベース削除スクリプト完了'
PRINT '==================================='