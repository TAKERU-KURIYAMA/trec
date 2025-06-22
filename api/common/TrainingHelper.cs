using Api.Models;
using Api.Common;
using Microsoft.EntityFrameworkCore;

namespace Api.Common
{
    /// <summary>
    /// トレーニング関連のヘルパークラス
    /// トレーニングメニューやレコードの操作、集計処理を提供
    /// </summary>
    public static class TrainingHelper
    {
        /// <summary>
        /// メニューIDの検証とトレーニングメニュー取得
        /// 指定されたメニューIDが存在するかを検証し、メニュー情報を返す
        /// </summary>
        /// <param name="context">データベースコンテキスト</param>
        /// <param name="menuId">検証するメニューID</param>
        /// <returns>トレーニングメニュー情報</returns>
        /// <exception cref="AppException">メニューが存在しない場合</exception>
        public static TrainingMenu ValidateMenuId(TrecPlansRDBContext context, string menuId)
        {
            if (context == null) 
                throw new ArgumentNullException(nameof(context));
            
            if (string.IsNullOrWhiteSpace(menuId)) 
                throw new AppException(ApplicationConstants.ErrorCodes.ParameterError, "メニューIDが指定されていません");

            TrainingMenu? menu = context.TrainingMenus
                .Where(x => x.MenuId == menuId)
                .SingleOrDefault();

            if (menu == null)
            {
                throw new AppException(ApplicationConstants.ErrorCodes.ParameterError, "トレーニングメニューが存在しません");
            }
            
            return menu;
        }

        /// <summary>
        /// メニューIDの検証とトレーニングメニュー取得（非同期版）
        /// パフォーマンス向上のための非同期版
        /// </summary>
        /// <param name="context">データベースコンテキスト</param>
        /// <param name="menuId">検証するメニューID</param>
        /// <returns>トレーニングメニュー情報</returns>
        /// <exception cref="AppException">メニューが存在しない場合</exception>
        public static async Task<TrainingMenu> ValidateMenuIdAsync(TrecPlansRDBContext context, string menuId)
        {
            if (context == null) 
                throw new ArgumentNullException(nameof(context));
            
            if (string.IsNullOrWhiteSpace(menuId)) 
                throw new AppException(ApplicationConstants.ErrorCodes.ParameterError, "メニューIDが指定されていません");

            TrainingMenu? menu = await context.TrainingMenus
                .Where(x => x.MenuId == menuId)
                .SingleOrDefaultAsync();

            if (menu == null)
            {
                throw new AppException(ApplicationConstants.ErrorCodes.ParameterError, "トレーニングメニューが存在しません");
            }
            
            return menu;
        }

        /// <summary>
        /// 日次トレーニングレコードの集計処理
        /// 指定された日付、ユーザー、メニューの全セットを集計してデイリーレコードを更新
        /// </summary>
        /// <param name="context">データベースコンテキスト</param>
        /// <param name="date">トレーニング日</param>
        /// <param name="user">ユーザー情報</param>
        /// <param name="menu">トレーニングメニュー情報</param>
        /// <returns>集計後のデイリートレーニングレコード</returns>
        public static DailyTrainingRecord AggregateDailyTrainingRecord(
            TrecPlansRDBContext context, 
            DateOnly date, 
            User user, 
            TrainingMenu menu)
        {
            if (context == null) 
                throw new ArgumentNullException(nameof(context));
            if (user == null) 
                throw new ArgumentNullException(nameof(user));
            if (menu == null) 
                throw new ArgumentNullException(nameof(menu));

            // 指定日のトレーニングレコードセットを取得
            List<TrainingRecordSet> records = context.TrainingRecordSets
                .Where(x => x.UserCommonId == user.UserCommonId
                    && x.MenuId == menu.MenuId
                    && x.TrainingDate == date)
                .ToList();

            if (!records.Any())
            {
                throw new AppException(ApplicationConstants.ErrorCodes.DataNotFound, 
                    "指定された日付のトレーニングレコードが見つかりません");
            }

            // 各種集計値を計算
            int setCount = records.Count;

            // 最大回数記録（回数が最も多いセット）
            TrainingRecordSet maxRepsRecord = records
                .OrderByDescending(r => r.Reps)
                .First();

            // 最大重量記録（重量が最も重いセット）
            TrainingRecordSet? maxWeightRecord = records
                .Where(r => r.Weight.HasValue)
                .OrderByDescending(r => r.Weight!.Value)
                .FirstOrDefault();

            // 総負荷量（重量 × 回数の合計）
            decimal totalLoad = records
                .Where(r => r.Weight.HasValue)
                .Sum(r => r.Reps * r.Weight!.Value);

            // 総回数
            int totalReps = records.Sum(r => r.Reps);

            // 既存のデイリーレコードを検索
            DailyTrainingRecord? dailyTrainingRecord = context.DailyTrainingRecords
                .Where(x => x.UserCommonId == user.UserCommonId
                    && x.MenuId == menu.MenuId
                    && x.TrainingDate == date)
                .FirstOrDefault();

            // デイリーレコードが存在しない場合は新規作成
            if (dailyTrainingRecord == null)
            {
                dailyTrainingRecord = new DailyTrainingRecord
                {
                    UserCommonId = user.UserCommonId,
                    MenuId = menu.MenuId!,
                    TrainingDate = date,
                    CreatedAt = DateTimeHelper.GetJstNow()
                };
                context.DailyTrainingRecords.Add(dailyTrainingRecord);
            }

            // 集計値を更新
            dailyTrainingRecord.SetCount = setCount;
            dailyTrainingRecord.MaxReps = maxRepsRecord.Reps;
            dailyTrainingRecord.MaxRepsWeight = maxRepsRecord.Weight;
            dailyTrainingRecord.MaxWeight = maxWeightRecord?.Weight;
            dailyTrainingRecord.MaxWeightReps = maxWeightRecord?.Reps;
            dailyTrainingRecord.TotalLoadAmount = totalLoad;
            dailyTrainingRecord.TotalReps = totalReps;
            dailyTrainingRecord.UpdatedAt = DateTimeHelper.GetJstNow();

            return dailyTrainingRecord;
        }

        /// <summary>
        /// 日次トレーニングレコードの集計処理（非同期版）
        /// パフォーマンス向上のための非同期版
        /// </summary>
        /// <param name="context">データベースコンテキスト</param>
        /// <param name="date">トレーニング日</param>
        /// <param name="user">ユーザー情報</param>
        /// <param name="menu">トレーニングメニュー情報</param>
        /// <returns>集計後のデイリートレーニングレコード</returns>
        public static async Task<DailyTrainingRecord> AggregateDailyTrainingRecordAsync(
            TrecPlansRDBContext context, 
            DateOnly date, 
            User user, 
            TrainingMenu menu)
        {
            if (context == null) 
                throw new ArgumentNullException(nameof(context));
            if (user == null) 
                throw new ArgumentNullException(nameof(user));
            if (menu == null) 
                throw new ArgumentNullException(nameof(menu));

            // 指定日のトレーニングレコードセットを非同期で取得
            List<TrainingRecordSet> records = await context.TrainingRecordSets
                .Where(x => x.UserCommonId == user.UserCommonId
                    && x.MenuId == menu.MenuId
                    && x.TrainingDate == date)
                .ToListAsync();

            if (!records.Any())
            {
                throw new AppException(ApplicationConstants.ErrorCodes.DataNotFound, 
                    "指定された日付のトレーニングレコードが見つかりません");
            }

            // 各種集計値を計算
            int setCount = records.Count;
            TrainingRecordSet maxRepsRecord = records.OrderByDescending(r => r.Reps).First();
            TrainingRecordSet? maxWeightRecord = records
                .Where(r => r.Weight.HasValue)
                .OrderByDescending(r => r.Weight!.Value)
                .FirstOrDefault();

            decimal totalLoad = records
                .Where(r => r.Weight.HasValue)
                .Sum(r => r.Reps * r.Weight!.Value);
            int totalReps = records.Sum(r => r.Reps);

            // 既存のデイリーレコードを非同期で検索
            DailyTrainingRecord? dailyTrainingRecord = await context.DailyTrainingRecords
                .Where(x => x.UserCommonId == user.UserCommonId
                    && x.MenuId == menu.MenuId
                    && x.TrainingDate == date)
                .FirstOrDefaultAsync();

            // デイリーレコードが存在しない場合は新規作成
            if (dailyTrainingRecord == null)
            {
                dailyTrainingRecord = new DailyTrainingRecord
                {
                    UserCommonId = user.UserCommonId,
                    MenuId = menu.MenuId!,
                    TrainingDate = date,
                    CreatedAt = DateTimeHelper.GetJstNow()
                };
                context.DailyTrainingRecords.Add(dailyTrainingRecord);
            }

            // 集計値を更新
            dailyTrainingRecord.SetCount = setCount;
            dailyTrainingRecord.MaxReps = maxRepsRecord.Reps;
            dailyTrainingRecord.MaxRepsWeight = maxRepsRecord.Weight;
            dailyTrainingRecord.MaxWeight = maxWeightRecord?.Weight;
            dailyTrainingRecord.MaxWeightReps = maxWeightRecord?.Reps;
            dailyTrainingRecord.TotalLoadAmount = totalLoad;
            dailyTrainingRecord.TotalReps = totalReps;
            dailyTrainingRecord.UpdatedAt = DateTimeHelper.GetJstNow();

            return dailyTrainingRecord;
        }

        /// <summary>
        /// 指定期間のトレーニング履歴を取得
        /// ダッシュボードやレポート機能で使用
        /// </summary>
        /// <param name="context">データベースコンテキスト</param>
        /// <param name="userCommonId">ユーザー共通ID</param>
        /// <param name="startDate">開始日</param>
        /// <param name="endDate">終了日</param>
        /// <param name="menuId">メニューID（任意、指定時は特定メニューのみ）</param>
        /// <returns>期間内のデイリートレーニングレコードリスト</returns>
        public static async Task<List<DailyTrainingRecord>> GetTrainingHistoryAsync(
            TrecPlansRDBContext context,
            string userCommonId,
            DateOnly startDate,
            DateOnly endDate,
            string? menuId = null)
        {
            if (context == null) 
                throw new ArgumentNullException(nameof(context));
            
            if (string.IsNullOrWhiteSpace(userCommonId)) 
                throw new AppException(ApplicationConstants.ErrorCodes.ParameterError, "ユーザー共通IDが指定されていません");

            var query = context.DailyTrainingRecords
                .Where(x => x.UserCommonId == userCommonId
                    && x.TrainingDate >= startDate
                    && x.TrainingDate <= endDate);

            if (!string.IsNullOrWhiteSpace(menuId))
            {
                query = query.Where(x => x.MenuId == menuId);
            }

            return await query
                .OrderByDescending(x => x.TrainingDate)
                .ThenBy(x => x.MenuId)
                .ToListAsync();
        }

        /// <summary>
        /// トレーニングメニューの使用統計を取得
        /// 人気メニューの分析などに使用
        /// </summary>
        /// <param name="context">データベースコンテキスト</param>
        /// <param name="userCommonId">ユーザー共通ID</param>
        /// <param name="days">過去何日分の統計を取得するか（デフォルト: 30日）</param>
        /// <returns>メニューごとの使用回数</returns>
        public static async Task<Dictionary<string, int>> GetMenuUsageStatsAsync(
            TrecPlansRDBContext context,
            string userCommonId,
            int days = 30)
        {
            if (context == null) 
                throw new ArgumentNullException(nameof(context));
            
            if (string.IsNullOrWhiteSpace(userCommonId)) 
                throw new AppException(ApplicationConstants.ErrorCodes.ParameterError, "ユーザー共通IDが指定されていません");

            var startDate = DateTimeHelper.GetJstDateOnlyType().AddDays(-days);

            var stats = await context.DailyTrainingRecords
                .Where(x => x.UserCommonId == userCommonId && x.TrainingDate >= startDate)
                .GroupBy(x => x.MenuId)
                .Select(g => new { MenuId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.MenuId, x => x.Count);

            return stats;
        }
    }
}