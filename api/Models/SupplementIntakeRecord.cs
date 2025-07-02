using System;

namespace Api.Models
{
    public partial class SupplementIntakeRecord
    {
        public int RecordId { get; set; }
        public string UserCommonId { get; set; } = null!;
        public int SupplementId { get; set; }
        public DateTime IntakeDate { get; set; }
        public TimeSpan IntakeTime { get; set; }
        public decimal Amount { get; set; }
        public string? TimingType { get; set; }
        public string? Memo { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public virtual User User { get; set; } = null!;
        public virtual SupplementMaster Supplement { get; set; } = null!;
    }
}