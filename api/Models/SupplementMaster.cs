using System;
using System.Collections.Generic;

namespace Api.Models
{
    public partial class SupplementMaster
    {
        public SupplementMaster()
        {
            SupplementIntakeRecords = new HashSet<SupplementIntakeRecord>();
            SupplementSchedules = new HashSet<SupplementSchedule>();
        }

        public int SupplementId { get; set; }
        public string UserCommonId { get; set; } = null!;
        public string SupplementName { get; set; } = null!;
        public string Unit { get; set; } = null!;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public virtual User User { get; set; } = null!;
        public virtual ICollection<SupplementIntakeRecord> SupplementIntakeRecords { get; set; }
        public virtual ICollection<SupplementSchedule> SupplementSchedules { get; set; }
    }
}