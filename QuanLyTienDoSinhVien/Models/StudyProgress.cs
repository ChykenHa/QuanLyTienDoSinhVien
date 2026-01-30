using System;
using System.Collections.Generic;

namespace QuanLyTienDoSinhVien.Models;

public partial class StudyProgress
{
    public int Id { get; set; }

    public int EnrollmentId { get; set; }

    public double? Score { get; set; }

    public int? CompletionPercent { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Enrollment Enrollment { get; set; } = null!;
}
