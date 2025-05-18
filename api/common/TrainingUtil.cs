
using Api.Models;
using System.Security.Cryptography;

namespace Common
{
    /// <summary>
    /// トレーニング関連処理クラス
    /// </summary>
    public static class TrainingUtil
    {
        public static TrainingMenu ValidateMenuId(MessageRDBContext context, string MenuId)
        { 
            TrainingMenu? menu = context.TrainingMenus.Where(x => x.MenuId == MenuId).SingleOrDefault();
            if (menu == null)
            {
                throw new AppException(FoundationCode.Errors.CLIENT_PARAMETER_ERROR, "トレーニングメニューが存在しません");
            }
            return menu;
        }

        public static DailyTrainingRecord AggregateDailyTrainingRecord(MessageRDBContext context
            , DateOnly date, User user, TrainingMenu menu)
        {
            List<TrainingRecordSet> records= context.TrainingRecordSets.Where(
                x => x.UserCommonId == user.UserCommonId
                && x.MenuId == menu.MenuId
                && x.TrainingDate == date
                ).ToList();

            int setCount = records.Count;

            TrainingRecordSet? maxRepsRecord = records.OrderByDescending(r => r.Reps).First();
            TrainingRecordSet? maxWeightRecord = records
                .Where(r => r.Weight.HasValue)
                .OrderByDescending(r => r.Weight!.Value)
                .FirstOrDefault();

            decimal totalLoad = records
                .Where(r => r.Weight.HasValue)
                .Sum(r => r.Reps * r.Weight!.Value);

            int totalReps = records.Sum(r => r.Reps);

            DailyTrainingRecord? dailyTrainingRecord = context.DailyTrainingRecords.Where(
                x => x.UserCommonId == user.UserCommonId
                && x.MenuId == menu.MenuId
                && x.TrainingDate == date).FirstOrDefault();


            if (dailyTrainingRecord == null)
            {
                dailyTrainingRecord = new DailyTrainingRecord
                {
                    UserCommonId = user.UserCommonId,
                    MenuId = menu.MenuId!,
                    TrainingDate = date,
                    CreatedAt = Common.Utility.GetJstNow()
                };
                context.DailyTrainingRecords.Add(dailyTrainingRecord);
            }

            // 集計値の更新
            dailyTrainingRecord.SetCount = setCount;
            dailyTrainingRecord.MaxReps = maxRepsRecord.Reps;
            dailyTrainingRecord.MaxRepsWeight = maxRepsRecord.Weight;
            dailyTrainingRecord.MaxWeight = maxWeightRecord?.Weight;
            dailyTrainingRecord.MaxWeightReps = maxWeightRecord?.Reps;
            dailyTrainingRecord.TotalLoadAmount = totalLoad;
            dailyTrainingRecord.TotalReps = totalReps;
            dailyTrainingRecord.UpdatedAt = Common.Utility.GetJstNow();
            return dailyTrainingRecord;
        }
    }
}