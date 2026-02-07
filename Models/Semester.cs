using System;
using System.Collections.Generic;

namespace QuanLyTienDoSinhVien.Models;

public partial class Semester
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

    public virtual ICollection<StudyPlanDetail> StudyPlanDetails { get; set; } = new List<StudyPlanDetail>();
}
