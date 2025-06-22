-- ===================================
-- トレーニングメニューとタグの初期データ投入（修正版）
-- APIモデルに合わせてテーブル名とカラム名を修正
-- ===================================

USE TrecPlansRDB;
GO

-- ===================================
-- タグマスターデータの挿入
-- ===================================
INSERT INTO TagMaster (TagId, JPName, ENName) VALUES
-- 部位タグ
('chest', N'胸', N'Chest'),
('back', N'背中', N'Back'),
('shoulders', N'肩', N'Shoulders'),
('arms', N'腕', N'Arms'),
('legs', N'脚', N'Legs'),
('core', N'体幹', N'Core'),
('glutes', N'臀部', N'Glutes'),

-- トレーニングタイプタグ
('strength', N'筋力', N'Strength'),
('cardio', N'有酸素', N'Cardio'),
('flexibility', N'柔軟性', N'Flexibility'),
('balance', N'バランス', N'Balance'),
('endurance', N'持久力', N'Endurance'),

-- 難易度タグ
('beginner', N'初級', N'Beginner'),
('intermediate', N'中級', N'Intermediate'),
('advanced', N'上級', N'Advanced'),

-- 器具タグ
('barbell', N'バーベル', N'Barbell'),
('dumbbell', N'ダンベル', N'Dumbbell'),
('machine', N'マシン', N'Machine'),
('bodyweight', N'自重', N'Bodyweight'),
('band', N'バンド', N'Resistance Band'),
('cable', N'ケーブル', N'Cable');
GO

-- ===================================
-- トレーニングメニューの挿入
-- ===================================
INSERT INTO TrainingMenus (MenuId, JPName, ENName, Description) VALUES
-- 胸のトレーニング
('bench_press', N'ベンチプレス', N'Bench Press', N'大胸筋を鍛える基本的なバーベル種目'),
('incline_bench_press', N'インクラインベンチプレス', N'Incline Bench Press', N'大胸筋上部を重点的に鍛えるバーベル種目'),
('dumbbell_fly', N'ダンベルフライ', N'Dumbbell Fly', N'大胸筋を広げる動作で鍛えるダンベル種目'),
('push_up', N'プッシュアップ', N'Push Up', N'自重で大胸筋を鍛える基本種目'),

-- 背中のトレーニング
('deadlift', N'デッドリフト', N'Deadlift', N'背中全体と下半身を鍛える複合種目'),
('pull_up', N'懸垂', N'Pull Up', N'広背筋を鍛える自重トレーニング'),
('bent_over_row', N'ベントオーバーロウ', N'Bent Over Row', N'背中の厚みを作るバーベル種目'),
('lat_pulldown', N'ラットプルダウン', N'Lat Pulldown', N'広背筋を鍛えるケーブル種目'),

-- 肩のトレーニング
('shoulder_press', N'ショルダープレス', N'Shoulder Press', N'三角筋を鍛える基本的なプレス種目'),
('lateral_raise', N'サイドレイズ', N'Lateral Raise', N'三角筋中部を鍛えるダンベル種目'),
('rear_delt_fly', N'リアデルトフライ', N'Rear Delt Fly', N'三角筋後部を鍛える種目'),

-- 腕のトレーニング
('bicep_curl', N'バイセップカール', N'Bicep Curl', N'上腕二頭筋を鍛える基本種目'),
('hammer_curl', N'ハンマーカール', N'Hammer Curl', N'上腕二頭筋と前腕を鍛える種目'),
('tricep_extension', N'トライセップエクステンション', N'Tricep Extension', N'上腕三頭筋を鍛える種目'),
('dips', N'ディップス', N'Dips', N'上腕三頭筋と大胸筋下部を鍛える自重種目'),

-- 脚のトレーニング
('squat', N'スクワット', N'Squat', N'下半身全体を鍛える基本種目'),
('leg_press', N'レッグプレス', N'Leg Press', N'大腿四頭筋を中心に鍛えるマシン種目'),
('romanian_deadlift', N'ルーマニアンデッドリフト', N'Romanian Deadlift', N'ハムストリングと臀部を鍛える種目'),
('calf_raise', N'カーフレイズ', N'Calf Raise', N'ふくらはぎを鍛える種目'),
('lunge', N'ランジ', N'Lunge', N'下半身全体をバランスよく鍛える種目'),

-- 体幹のトレーニング
('plank', N'プランク', N'Plank', N'体幹全体を鍛える等尺性運動'),
('crunch', N'クランチ', N'Crunch', N'腹直筋を鍛える基本種目'),
('russian_twist', N'ロシアンツイスト', N'Russian Twist', N'腹斜筋を鍛える回旋種目'),
('hanging_leg_raise', N'ハンギングレッグレイズ', N'Hanging Leg Raise', N'腹直筋下部を鍛える種目'),

-- 有酸素運動
('running', N'ランニング', N'Running', N'基本的な有酸素運動'),
('cycling', N'サイクリング', N'Cycling', N'膝に優しい有酸素運動'),
('rowing', N'ローイング', N'Rowing', N'全身を使う有酸素運動'),
('jump_rope', N'縄跳び', N'Jump Rope', N'高強度の有酸素運動');
GO

-- ===================================
-- トレーニングメニューとタグの関連付け
-- ===================================
INSERT INTO TrainingTag (MenuId, TagId, JPName, ENName) VALUES
-- ベンチプレス
('bench_press', N'chest', N'胸', N'Chest'),
('bench_press', N'strength', N'筋力', N'Strength'),
('bench_press', N'barbell', N'バーベル', N'Barbell'),
('bench_press', N'intermediate', N'中級', N'Intermediate'),

-- インクラインベンチプレス
('incline_bench_press', N'chest', N'胸', N'Chest'),
('incline_bench_press', N'strength', N'筋力', N'Strength'),
('incline_bench_press', N'barbell', N'バーベル', N'Barbell'),
('incline_bench_press', N'intermediate', N'中級', N'Intermediate'),

-- ダンベルフライ
('dumbbell_fly', N'chest', N'胸', N'Chest'),
('dumbbell_fly', N'strength', N'筋力', N'Strength'),
('dumbbell_fly', N'dumbbell', N'ダンベル', N'Dumbbell'),
('dumbbell_fly', N'beginner', N'初級', N'Beginner'),

-- プッシュアップ
('push_up', N'chest', N'胸', N'Chest'),
('push_up', N'strength', N'筋力', N'Strength'),
('push_up', N'bodyweight', N'自重', N'Bodyweight'),
('push_up', N'beginner', N'初級', N'Beginner'),

-- デッドリフト
('deadlift', N'back', N'背中', N'Back'),
('deadlift', N'legs', N'脚', N'Legs'),
('deadlift', N'strength', N'筋力', N'Strength'),
('deadlift', N'barbell', N'バーベル', N'Barbell'),
('deadlift', N'advanced', N'上級', N'Advanced'),

-- 懸垂
('pull_up', N'back', N'背中', N'Back'),
('pull_up', N'arms', N'腕', N'Arms'),
('pull_up', N'strength', N'筋力', N'Strength'),
('pull_up', N'bodyweight', N'自重', N'Bodyweight'),
('pull_up', N'intermediate', N'中級', N'Intermediate'),

-- ベントオーバーロウ
('bent_over_row', N'back', N'背中', N'Back'),
('bent_over_row', N'strength', N'筋力', N'Strength'),
('bent_over_row', N'barbell', N'バーベル', N'Barbell'),
('bent_over_row', N'intermediate', N'中級', N'Intermediate'),

-- ラットプルダウン
('lat_pulldown', N'back', N'背中', N'Back'),
('lat_pulldown', N'strength', N'筋力', N'Strength'),
('lat_pulldown', N'cable', N'ケーブル', N'Cable'),
('lat_pulldown', N'beginner', N'初級', N'Beginner'),

-- ショルダープレス
('shoulder_press', N'shoulders', N'肩', N'Shoulders'),
('shoulder_press', N'strength', N'筋力', N'Strength'),
('shoulder_press', N'dumbbell', N'ダンベル', N'Dumbbell'),
('shoulder_press', N'intermediate', N'中級', N'Intermediate'),

-- サイドレイズ
('lateral_raise', N'shoulders', N'肩', N'Shoulders'),
('lateral_raise', N'strength', N'筋力', N'Strength'),
('lateral_raise', N'dumbbell', N'ダンベル', N'Dumbbell'),
('lateral_raise', N'beginner', N'初級', N'Beginner'),

-- リアデルトフライ
('rear_delt_fly', N'shoulders', N'肩', N'Shoulders'),
('rear_delt_fly', N'strength', N'筋力', N'Strength'),
('rear_delt_fly', N'dumbbell', N'ダンベル', N'Dumbbell'),
('rear_delt_fly', N'intermediate', N'中級', N'Intermediate'),

-- バイセップカール
('bicep_curl', N'arms', N'腕', N'Arms'),
('bicep_curl', N'strength', N'筋力', N'Strength'),
('bicep_curl', N'dumbbell', N'ダンベル', N'Dumbbell'),
('bicep_curl', N'beginner', N'初級', N'Beginner'),

-- ハンマーカール
('hammer_curl', N'arms', N'腕', N'Arms'),
('hammer_curl', N'strength', N'筋力', N'Strength'),
('hammer_curl', N'dumbbell', N'ダンベル', N'Dumbbell'),
('hammer_curl', N'beginner', N'初級', N'Beginner'),

-- トライセップエクステンション
('tricep_extension', N'arms', N'腕', N'Arms'),
('tricep_extension', N'strength', N'筋力', N'Strength'),
('tricep_extension', N'dumbbell', N'ダンベル', N'Dumbbell'),
('tricep_extension', N'beginner', N'初級', N'Beginner'),

-- ディップス
('dips', N'arms', N'腕', N'Arms'),
('dips', N'chest', N'胸', N'Chest'),
('dips', N'strength', N'筋力', N'Strength'),
('dips', N'bodyweight', N'自重', N'Bodyweight'),
('dips', N'intermediate', N'中級', N'Intermediate'),

-- スクワット
('squat', N'legs', N'脚', N'Legs'),
('squat', N'glutes', N'臀部', N'Glutes'),
('squat', N'strength', N'筋力', N'Strength'),
('squat', N'barbell', N'バーベル', N'Barbell'),
('squat', N'intermediate', N'中級', N'Intermediate'),

-- レッグプレス
('leg_press', N'legs', N'脚', N'Legs'),
('leg_press', N'strength', N'筋力', N'Strength'),
('leg_press', N'machine', N'マシン', N'Machine'),
('leg_press', N'beginner', N'初級', N'Beginner'),

-- ルーマニアンデッドリフト
('romanian_deadlift', N'legs', N'脚', N'Legs'),
('romanian_deadlift', N'glutes', N'臀部', N'Glutes'),
('romanian_deadlift', N'strength', N'筋力', N'Strength'),
('romanian_deadlift', N'barbell', N'バーベル', N'Barbell'),
('romanian_deadlift', N'intermediate', N'中級', N'Intermediate'),

-- カーフレイズ
('calf_raise', N'legs', N'脚', N'Legs'),
('calf_raise', N'strength', N'筋力', N'Strength'),
('calf_raise', N'bodyweight', N'自重', N'Bodyweight'),
('calf_raise', N'beginner', N'初級', N'Beginner'),

-- ランジ
('lunge', N'legs', N'脚', N'Legs'),
('lunge', N'glutes', N'臀部', N'Glutes'),
('lunge', N'strength', N'筋力', N'Strength'),
('lunge', N'bodyweight', N'自重', N'Bodyweight'),
('lunge', N'beginner', N'初級', N'Beginner'),

-- プランク
('plank', N'core', N'体幹', N'Core'),
('plank', N'strength', N'筋力', N'Strength'),
('plank', N'bodyweight', N'自重', N'Bodyweight'),
('plank', N'beginner', N'初級', N'Beginner'),

-- クランチ
('crunch', N'core', N'体幹', N'Core'),
('crunch', N'strength', N'筋力', N'Strength'),
('crunch', N'bodyweight', N'自重', N'Bodyweight'),
('crunch', N'beginner', N'初級', N'Beginner'),

-- ロシアンツイスト
('russian_twist', N'core', N'体幹', N'Core'),
('russian_twist', N'strength', N'筋力', N'Strength'),
('russian_twist', N'bodyweight', N'自重', N'Bodyweight'),
('russian_twist', N'intermediate', N'中級', N'Intermediate'),

-- ハンギングレッグレイズ
('hanging_leg_raise', N'core', N'体幹', N'Core'),
('hanging_leg_raise', N'strength', N'筋力', N'Strength'),
('hanging_leg_raise', N'bodyweight', N'自重', N'Bodyweight'),
('hanging_leg_raise', N'advanced', N'上級', N'Advanced'),

-- ランニング
('running', N'cardio', N'有酸素', N'Cardio'),
('running', N'endurance', N'持久力', N'Endurance'),
('running', N'bodyweight', N'自重', N'Bodyweight'),
('running', N'beginner', N'初級', N'Beginner'),

-- サイクリング
('cycling', N'cardio', N'有酸素', N'Cardio'),
('cycling', N'endurance', N'持久力', N'Endurance'),
('cycling', N'machine', N'マシン', N'Machine'),
('cycling', N'beginner', N'初級', N'Beginner'),

-- ローイング
('rowing', N'cardio', N'有酸素', N'Cardio'),
('rowing', N'endurance', N'持久力', N'Endurance'),
('rowing', N'machine', N'マシン', N'Machine'),
('rowing', N'intermediate', N'中級', N'Intermediate'),

-- 縄跳び
('jump_rope', N'cardio', N'有酸素', N'Cardio'),
('jump_rope', N'endurance', N'持久力', N'Endurance'),
('jump_rope', N'bodyweight', N'自重', N'Bodyweight'),
('jump_rope', N'intermediate', N'中級', N'Intermediate');
GO

PRINT '===================================';
PRINT 'メニューとタグデータの投入が完了しました';
PRINT '===================================';
PRINT '';
PRINT '投入されたデータ:';
PRINT '- トレーニングメニュー: 29種目';
PRINT '- タグマスター: 21種類';
PRINT '- メニュー-タグ関連: 95件';
PRINT '';
PRINT '次のステップ: テストユーザーとトレーニングデータの投入';
GO