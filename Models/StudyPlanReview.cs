using System;
using System.Collections.Generic;

namespace QuanLyTienDoSinhVien.Models;

public partial class StudyPlanReview
{
    public int Id { get; set; }

    public int StudyPlanId { get; set; }

    public int LecturerId { get; set; }

    public string? Comment { get; set; }

    public DateTime? ReviewedAt { get; set; }

    public virtual Lecturer Lecturer { get; set; } = null!;

    public virtual StudyPlan StudyPlan { get; set; } = null!;
}
