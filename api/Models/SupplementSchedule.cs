using System;

namespace Api.Models
{
    public partial class SupplementSchedule
    {
        public int ScheduleId { get; set; }
        public string UserCommonId { get; set; } = null!;
        public int SupplementId { get; set; }
        public TimeSpan ScheduleTime { get; set; }
        public decimal Amount { get; set; }
        public string? TimingType { get; set; }
        public string? DaysOfWeek { get; set; }
        public bool IsActive { get; set; }
        public string? Memo { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public virtual User User { get; set; } = null!;
        public virtual SupplementMaster Supplement { get; set; } = null!;
    }
}