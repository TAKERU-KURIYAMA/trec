-- ===================================
-- エクササイズタイプタグの追加（コンパウンド/アイソレーション）
-- ===================================

USE MessageRDB;
GO

-- ===================================
-- 新しいエクササイズタイプタグの追加
-- ===================================
INSERT INTO TagMaster (TagId, JPName, ENName, CreatedAt) VALUES
('compound', N'コンパウンド', N'Compound', GETDATE()),
('isolation', N'アイソレーション', N'Isolation', GETDATE()),
('movement_push', N'プッシュ', N'Push', GETDATE()),
('movement_pull', N'プル', N'Pull', GETDATE()),
('movement_squat', N'スクワット', N'Squat', GETDATE()),
('movement_hinge', N'ヒンジ', N'Hinge', GETDATE());
GO

-- ===================================
-- 既存メニューへのコンパウンド/アイソレーションタグ付与
-- ===================================

-- コンパウンド種目（複数の筋群を使う複合種目）
INSERT INTO TrainingTag (MenuId, TagId, JPName, ENName, CreatedAt) VALUES
-- ベンチプレス（胸・肩・腕）
('bench_press', 'compound', N'コンパウンド', N'Compound', GETDATE()),
('bench_press', 'movement_push', N'プッシュ', N'Push', GETDATE()),

-- インクラインベンチプレス（胸・肩・腕）
('incline_bench_press', 'compound', N'コンパウンド', N'Compound', GETDATE()),
('incline_bench_press', 'movement_push', N'プッシュ', N'Push', GETDATE()),

-- デッドリフト（背中・脚・体幹）
('deadlift', 'compound', N'コンパウンド', N'Compound', GETDATE()),
('deadlift', 'movement_hinge', N'ヒンジ', N'Hinge', GETDATE()),

-- 懸垂（背中・腕）
('pull_up', 'compound', N'コンパウンド', N'Compound', GETDATE()),
('pull_up', 'movement_pull', N'プル', N'Pull', GETDATE()),

-- ベントオーバーロウ（背中・腕・体幹）
('bent_over_row', 'compound', N'コンパウンド', N'Compound', GETDATE()),
('bent_over_row', 'movement_pull', N'プル', N'Pull', GETDATE()),

-- ショルダープレス（肩・腕・体幹）
('shoulder_press', 'compound', N'コンパウンド', N'Compound', GETDATE()),
('shoulder_press', 'movement_push', N'プッシュ', N'Push', GETDATE()),

-- ディップス（胸・腕）
('dips', 'compound', N'コンパウンド', N'Compound', GETDATE()),
('dips', 'movement_push', N'プッシュ', N'Push', GETDATE()),

-- スクワット（脚・体幹）
('squat', 'compound', N'コンパウンド', N'Compound', GETDATE()),
('squat', 'movement_squat', N'スクワット', N'Squat', GETDATE()),

-- レッグプレス（脚）
('leg_press', 'compound', N'コンパウンド', N'Compound', GETDATE()),
('leg_press', 'movement_squat', N'スクワット', N'Squat', GETDATE()),

-- ルーマニアンデッドリフト（ハムストリング・臀部・体幹）
('romanian_deadlift', 'compound', N'コンパウンド', N'Compound', GETDATE()),
('romanian_deadlift', 'movement_hinge', N'ヒンジ', N'Hinge', GETDATE()),

-- ランジ（脚・体幹）
('lunge', 'compound', N'コンパウンド', N'Compound', GETDATE()),
('lunge', 'movement_squat', N'スクワット', N'Squat', GETDATE()),

-- プッシュアップ（胸・肩・腕・体幹）
('push_up', 'compound', N'コンパウンド', N'Compound', GETDATE()),
('push_up', 'movement_push', N'プッシュ', N'Push', GETDATE());

-- アイソレーション種目（単一筋群を対象とした単関節種目）
INSERT INTO TrainingTag (MenuId, TagId, JPName, ENName, CreatedAt) VALUES
-- ダンベルフライ（大胸筋のみ）
('dumbbell_fly', 'isolation', N'アイソレーション', N'Isolation', GETDATE()),

-- ラットプルダウン（広背筋中心）
('lat_pulldown', 'isolation', N'アイソレーション', N'Isolation', GETDATE()),
('lat_pulldown', 'movement_pull', N'プル', N'Pull', GETDATE()),

-- サイドレイズ（三角筋中部）
('lateral_raise', 'isolation', N'アイソレーション', N'Isolation', GETDATE()),

-- リアデルトフライ（三角筋後部）
('rear_delt_fly', 'isolation', N'アイソレーション', N'Isolation', GETDATE()),

-- バイセップカール（上腕二頭筋）
('bicep_curl', 'isolation', N'アイソレーション', N'Isolation', GETDATE()),

-- ハンマーカール（上腕二頭筋・前腕）
('hammer_curl', 'isolation', N'アイソレーション', N'Isolation', GETDATE()),

-- トライセップエクステンション（上腕三頭筋）
('tricep_extension', 'isolation', N'アイソレーション', N'Isolation', GETDATE()),

-- カーフレイズ（ふくらはぎ）
('calf_raise', 'isolation', N'アイソレーション', N'Isolation', GETDATE()),

-- クランチ（腹直筋）
('crunch', 'isolation', N'アイソレーション', N'Isolation', GETDATE()),

-- ロシアンツイスト（腹斜筋）
('russian_twist', 'isolation', N'アイソレーション', N'Isolation', GETDATE()),

-- ハンギングレッグレイズ（腹直筋下部）
('hanging_leg_raise', 'isolation', N'アイソレーション', N'Isolation', GETDATE());

-- プランクは等尺性運動として分類
INSERT INTO TrainingTag (MenuId, TagId, JPName, ENName, CreatedAt) VALUES
('plank', 'compound', N'コンパウンド', N'Compound', GETDATE());

GO

PRINT '===================================';
PRINT 'エクササイズタイプタグの追加が完了しました';
PRINT '===================================';
PRINT '';
PRINT '追加されたタグ:';
PRINT '- compound (コンパウンド): 複合種目';
PRINT '- isolation (アイソレーション): 単関節種目';
PRINT '- movement_push (プッシュ): 押す動作';
PRINT '- movement_pull (プル): 引く動作';
PRINT '- movement_squat (スクワット): スクワット系';
PRINT '- movement_hinge (ヒンジ): ヒンジ系';
PRINT '';
PRINT 'コンパウンド種目: 13種目';
PRINT 'アイソレーション種目: 11種目';
GO