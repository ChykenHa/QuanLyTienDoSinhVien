using System;
using System.Collections.Generic;

namespace QuanLyTienDoSinhVien.Models;

public partial class StudyPlanDetail
{
    public int Id { get; set; }

    public int StudyPlanId { get; set; }

    public int SubjectId { get; set; }

    public int SemesterId { get; set; }

    public virtual Semester Semester { get; set; } = null!;

    public virtual StudyPlan StudyPlan { get; set; } = null!;

    public virtual Subject Subject { get; set; } = null!;
}
