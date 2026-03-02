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

    public int TotalStudents { get; set; }
    public int NormalCount { get; set; }
    public int WarningCount { get; set; }
    public int DangerCount { get; set; }
    }
