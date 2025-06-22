-- サプリメント管理機能のテーブル削除

-- インデックスの削除
DROP INDEX IF EXISTS IX_schedules_supplement_id ON supplement_schedules;
DROP INDEX IF EXISTS IX_schedules_user_id ON supplement_schedules;
DROP INDEX IF EXISTS IX_intake_records_supplement_id ON supplement_intake_records;
DROP INDEX IF EXISTS IX_intake_records_user_date ON supplement_intake_records;
DROP INDEX IF EXISTS IX_supplement_master_user_id ON supplement_master;

-- テーブルの削除
DROP TABLE IF EXISTS supplement_schedules;
DROP TABLE IF EXISTS supplement_intake_records;
DROP TABLE IF EXISTS supplement_master;