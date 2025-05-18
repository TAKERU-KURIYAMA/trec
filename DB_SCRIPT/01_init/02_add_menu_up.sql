INSERT INTO TrainingMenus (MenuId, JPName, ENName, Description, CreatedAt)
VALUES
('bench_press', N'ベンチプレス', 'Bench Press', N'胸を鍛える代表的な種目', GETDATE()),
('squat', N'スクワット', 'Squat', N'下半身を鍛える基本種目', GETDATE()),
('deadlift', N'デッドリフト', 'Deadlift', N'全身の筋力を使う高負荷種目', GETDATE()),
('lat_pull', N'ラットプルダウン', 'Lat Pulldown', N'広背筋を鍛える種目', GETDATE());


INSERT INTO TagMaster (TagId, JPName, ENName, CreatedAt)
VALUES
('chest', N'胸', 'Chest', GETDATE()),
('leg', N'脚', 'Leg', GETDATE()),
('back', N'背中', 'Back', GETDATE()),
('big3', 'BIG3', 'BIG3', GETDATE());


-- ベンチプレス → 胸, BIG3
INSERT INTO TrainingTag (TagId, MenuId, JPName, ENName, CreatedAt)
VALUES
('chest', 'bench_press', N'胸', 'Chest', GETDATE()),
('big3', 'bench_press', 'BIG3', 'BIG3', GETDATE());

-- スクワット → 脚, BIG3
INSERT INTO TrainingTag (TagId, MenuId, JPName, ENName, CreatedAt)
VALUES
('leg', 'squat', N'脚', 'Leg', GETDATE()),
('big3', 'squat', 'BIG3', 'BIG3', GETDATE());

-- デッドリフト → 背中, BIG3
INSERT INTO TrainingTag (TagId, MenuId, JPName, ENName, CreatedAt)
VALUES
('back', 'deadlift', N'背中', 'Back', GETDATE()),
('big3', 'deadlift', 'BIG3', 'BIG3', GETDATE());

-- ラットプルダウン → 背中
INSERT INTO TrainingTag (TagId, MenuId, JPName, ENName, CreatedAt)
VALUES
('back', 'lat_pull', N'背中', 'Back', GETDATE());
