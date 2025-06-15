-- ===================================
-- トレーニングメニューとタグの初期データ投入（修正版）
-- APIモデルに合わせてテーブル名とカラム名を修正
-- ===================================

USE MessageRDB;
GO

-- ===================================
-- タグマスターデータの挿入
-- ===================================
INSERT INTO TagMaster (TagId, JPName, ENName) VALUES
-- 部位タグ
('chest', '胸', 'Chest'),
('back', '背中', 'Back'),
('shoulders', '肩', 'Shoulders'),
('arms', '腕', 'Arms'),
('legs', '脚', 'Legs'),
('core', '体幹', 'Core'),
('glutes', '臀部', 'Glutes'),

-- トレーニングタイプタグ
('strength', '筋力', 'Strength'),
('cardio', '有酸素', 'Cardio'),
('flexibility', '柔軟性', 'Flexibility'),
('balance', 'バランス', 'Balance'),
('endurance', '持久力', 'Endurance'),

-- 難易度タグ
('beginner', '初級', 'Beginner'),
('intermediate', '中級', 'Intermediate'),
('advanced', '上級', 'Advanced'),

-- 器具タグ
('barbell', 'バーベル', 'Barbell'),
('dumbbell', 'ダンベル', 'Dumbbell'),
('machine', 'マシン', 'Machine'),
('bodyweight', '自重', 'Bodyweight'),
('band', 'バンド', 'Resistance Band'),
('cable', 'ケーブル', 'Cable');
GO

-- ===================================
-- トレーニングメニューの挿入
-- ===================================
INSERT INTO TrainingMenus (MenuId, JPName, ENName, Description) VALUES
-- 胸のトレーニング
('bench_press', 'ベンチプレス', 'Bench Press', '大胸筋を鍛える基本的なバーベル種目'),
('incline_bench_press', 'インクラインベンチプレス', 'Incline Bench Press', '大胸筋上部を重点的に鍛えるバーベル種目'),
('dumbbell_fly', 'ダンベルフライ', 'Dumbbell Fly', '大胸筋を広げる動作で鍛えるダンベル種目'),
('push_up', 'プッシュアップ', 'Push Up', '自重で大胸筋を鍛える基本種目'),

-- 背中のトレーニング
('deadlift', 'デッドリフト', 'Deadlift', '背中全体と下半身を鍛える複合種目'),
('pull_up', '懸垂', 'Pull Up', '広背筋を鍛える自重トレーニング'),
('bent_over_row', 'ベントオーバーロウ', 'Bent Over Row', '背中の厚みを作るバーベル種目'),
('lat_pulldown', 'ラットプルダウン', 'Lat Pulldown', '広背筋を鍛えるケーブル種目'),

-- 肩のトレーニング
('shoulder_press', 'ショルダープレス', 'Shoulder Press', '三角筋を鍛える基本的なプレス種目'),
('lateral_raise', 'サイドレイズ', 'Lateral Raise', '三角筋中部を鍛えるダンベル種目'),
('rear_delt_fly', 'リアデルトフライ', 'Rear Delt Fly', '三角筋後部を鍛える種目'),

-- 腕のトレーニング
('bicep_curl', 'バイセップカール', 'Bicep Curl', '上腕二頭筋を鍛える基本種目'),
('hammer_curl', 'ハンマーカール', 'Hammer Curl', '上腕二頭筋と前腕を鍛える種目'),
('tricep_extension', 'トライセップエクステンション', 'Tricep Extension', '上腕三頭筋を鍛える種目'),
('dips', 'ディップス', 'Dips', '上腕三頭筋と大胸筋下部を鍛える自重種目'),

-- 脚のトレーニング
('squat', 'スクワット', 'Squat', '下半身全体を鍛える基本種目'),
('leg_press', 'レッグプレス', 'Leg Press', '大腿四頭筋を中心に鍛えるマシン種目'),
('romanian_deadlift', 'ルーマニアンデッドリフト', 'Romanian Deadlift', 'ハムストリングと臀部を鍛える種目'),
('calf_raise', 'カーフレイズ', 'Calf Raise', 'ふくらはぎを鍛える種目'),
('lunge', 'ランジ', 'Lunge', '下半身全体をバランスよく鍛える種目'),

-- 体幹のトレーニング
('plank', 'プランク', 'Plank', '体幹全体を鍛える等尺性運動'),
('crunch', 'クランチ', 'Crunch', '腹直筋を鍛える基本種目'),
('russian_twist', 'ロシアンツイスト', 'Russian Twist', '腹斜筋を鍛える回旋種目'),
('hanging_leg_raise', 'ハンギングレッグレイズ', 'Hanging Leg Raise', '腹直筋下部を鍛える種目'),

-- 有酸素運動
('running', 'ランニング', 'Running', '基本的な有酸素運動'),
('cycling', 'サイクリング', 'Cycling', '膝に優しい有酸素運動'),
('rowing', 'ローイング', 'Rowing', '全身を使う有酸素運動'),
('jump_rope', '縄跳び', 'Jump Rope', '高強度の有酸素運動');
GO

-- ===================================
-- トレーニングメニューとタグの関連付け
-- ===================================
INSERT INTO TrainingTag (MenuId, TagId, JPName, ENName) VALUES
-- ベンチプレス
('bench_press', 'chest', '胸', 'Chest'),
('bench_press', 'strength', '筋力', 'Strength'),
('bench_press', 'barbell', 'バーベル', 'Barbell'),
('bench_press', 'intermediate', '中級', 'Intermediate'),

-- インクラインベンチプレス
('incline_bench_press', 'chest', '胸', 'Chest'),
('incline_bench_press', 'strength', '筋力', 'Strength'),
('incline_bench_press', 'barbell', 'バーベル', 'Barbell'),
('incline_bench_press', 'intermediate', '中級', 'Intermediate'),

-- ダンベルフライ
('dumbbell_fly', 'chest', '胸', 'Chest'),
('dumbbell_fly', 'strength', '筋力', 'Strength'),
('dumbbell_fly', 'dumbbell', 'ダンベル', 'Dumbbell'),
('dumbbell_fly', 'beginner', '初級', 'Beginner'),

-- プッシュアップ
('push_up', 'chest', '胸', 'Chest'),
('push_up', 'strength', '筋力', 'Strength'),
('push_up', 'bodyweight', '自重', 'Bodyweight'),
('push_up', 'beginner', '初級', 'Beginner'),

-- デッドリフト
('deadlift', 'back', '背中', 'Back'),
('deadlift', 'legs', '脚', 'Legs'),
('deadlift', 'strength', '筋力', 'Strength'),
('deadlift', 'barbell', 'バーベル', 'Barbell'),
('deadlift', 'advanced', '上級', 'Advanced'),

-- 懸垂
('pull_up', 'back', '背中', 'Back'),
('pull_up', 'arms', '腕', 'Arms'),
('pull_up', 'strength', '筋力', 'Strength'),
('pull_up', 'bodyweight', '自重', 'Bodyweight'),
('pull_up', 'intermediate', '中級', 'Intermediate'),

-- ベントオーバーロウ
('bent_over_row', 'back', '背中', 'Back'),
('bent_over_row', 'strength', '筋力', 'Strength'),
('bent_over_row', 'barbell', 'バーベル', 'Barbell'),
('bent_over_row', 'intermediate', '中級', 'Intermediate'),

-- ラットプルダウン
('lat_pulldown', 'back', '背中', 'Back'),
('lat_pulldown', 'strength', '筋力', 'Strength'),
('lat_pulldown', 'cable', 'ケーブル', 'Cable'),
('lat_pulldown', 'beginner', '初級', 'Beginner'),

-- ショルダープレス
('shoulder_press', 'shoulders', '肩', 'Shoulders'),
('shoulder_press', 'strength', '筋力', 'Strength'),
('shoulder_press', 'dumbbell', 'ダンベル', 'Dumbbell'),
('shoulder_press', 'intermediate', '中級', 'Intermediate'),

-- サイドレイズ
('lateral_raise', 'shoulders', '肩', 'Shoulders'),
('lateral_raise', 'strength', '筋力', 'Strength'),
('lateral_raise', 'dumbbell', 'ダンベル', 'Dumbbell'),
('lateral_raise', 'beginner', '初級', 'Beginner'),

-- リアデルトフライ
('rear_delt_fly', 'shoulders', '肩', 'Shoulders'),
('rear_delt_fly', 'strength', '筋力', 'Strength'),
('rear_delt_fly', 'dumbbell', 'ダンベル', 'Dumbbell'),
('rear_delt_fly', 'intermediate', '中級', 'Intermediate'),

-- バイセップカール
('bicep_curl', 'arms', '腕', 'Arms'),
('bicep_curl', 'strength', '筋力', 'Strength'),
('bicep_curl', 'dumbbell', 'ダンベル', 'Dumbbell'),
('bicep_curl', 'beginner', '初級', 'Beginner'),

-- ハンマーカール
('hammer_curl', 'arms', '腕', 'Arms'),
('hammer_curl', 'strength', '筋力', 'Strength'),
('hammer_curl', 'dumbbell', 'ダンベル', 'Dumbbell'),
('hammer_curl', 'beginner', '初級', 'Beginner'),

-- トライセップエクステンション
('tricep_extension', 'arms', '腕', 'Arms'),
('tricep_extension', 'strength', '筋力', 'Strength'),
('tricep_extension', 'dumbbell', 'ダンベル', 'Dumbbell'),
('tricep_extension', 'beginner', '初級', 'Beginner'),

-- ディップス
('dips', 'arms', '腕', 'Arms'),
('dips', 'chest', '胸', 'Chest'),
('dips', 'strength', '筋力', 'Strength'),
('dips', 'bodyweight', '自重', 'Bodyweight'),
('dips', 'intermediate', '中級', 'Intermediate'),

-- スクワット
('squat', 'legs', '脚', 'Legs'),
('squat', 'glutes', '臀部', 'Glutes'),
('squat', 'strength', '筋力', 'Strength'),
('squat', 'barbell', 'バーベル', 'Barbell'),
('squat', 'intermediate', '中級', 'Intermediate'),

-- レッグプレス
('leg_press', 'legs', '脚', 'Legs'),
('leg_press', 'strength', '筋力', 'Strength'),
('leg_press', 'machine', 'マシン', 'Machine'),
('leg_press', 'beginner', '初級', 'Beginner'),

-- ルーマニアンデッドリフト
('romanian_deadlift', 'legs', '脚', 'Legs'),
('romanian_deadlift', 'glutes', '臀部', 'Glutes'),
('romanian_deadlift', 'strength', '筋力', 'Strength'),
('romanian_deadlift', 'barbell', 'バーベル', 'Barbell'),
('romanian_deadlift', 'intermediate', '中級', 'Intermediate'),

-- カーフレイズ
('calf_raise', 'legs', '脚', 'Legs'),
('calf_raise', 'strength', '筋力', 'Strength'),
('calf_raise', 'bodyweight', '自重', 'Bodyweight'),
('calf_raise', 'beginner', '初級', 'Beginner'),

-- ランジ
('lunge', 'legs', '脚', 'Legs'),
('lunge', 'glutes', '臀部', 'Glutes'),
('lunge', 'strength', '筋力', 'Strength'),
('lunge', 'bodyweight', '自重', 'Bodyweight'),
('lunge', 'beginner', '初級', 'Beginner'),

-- プランク
('plank', 'core', '体幹', 'Core'),
('plank', 'strength', '筋力', 'Strength'),
('plank', 'bodyweight', '自重', 'Bodyweight'),
('plank', 'beginner', '初級', 'Beginner'),

-- クランチ
('crunch', 'core', '体幹', 'Core'),
('crunch', 'strength', '筋力', 'Strength'),
('crunch', 'bodyweight', '自重', 'Bodyweight'),
('crunch', 'beginner', '初級', 'Beginner'),

-- ロシアンツイスト
('russian_twist', 'core', '体幹', 'Core'),
('russian_twist', 'strength', '筋力', 'Strength'),
('russian_twist', 'bodyweight', '自重', 'Bodyweight'),
('russian_twist', 'intermediate', '中級', 'Intermediate'),

-- ハンギングレッグレイズ
('hanging_leg_raise', 'core', '体幹', 'Core'),
('hanging_leg_raise', 'strength', '筋力', 'Strength'),
('hanging_leg_raise', 'bodyweight', '自重', 'Bodyweight'),
('hanging_leg_raise', 'advanced', '上級', 'Advanced'),

-- ランニング
('running', 'cardio', '有酸素', 'Cardio'),
('running', 'endurance', '持久力', 'Endurance'),
('running', 'bodyweight', '自重', 'Bodyweight'),
('running', 'beginner', '初級', 'Beginner'),

-- サイクリング
('cycling', 'cardio', '有酸素', 'Cardio'),
('cycling', 'endurance', '持久力', 'Endurance'),
('cycling', 'machine', 'マシン', 'Machine'),
('cycling', 'beginner', '初級', 'Beginner'),

-- ローイング
('rowing', 'cardio', '有酸素', 'Cardio'),
('rowing', 'endurance', '持久力', 'Endurance'),
('rowing', 'machine', 'マシン', 'Machine'),
('rowing', 'intermediate', '中級', 'Intermediate'),

-- 縄跳び
('jump_rope', 'cardio', '有酸素', 'Cardio'),
('jump_rope', 'endurance', '持久力', 'Endurance'),
('jump_rope', 'bodyweight', '自重', 'Bodyweight'),
('jump_rope', 'intermediate', '中級', 'Intermediate');
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