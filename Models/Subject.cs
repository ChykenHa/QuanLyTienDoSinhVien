using System;
using System.Collections.Generic;

namespace QuanLyTienDoSinhVien.Models;

public partial class Subject
{
    public int Id { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public int Credit { get; set; }

    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

    public virtual ICollection<LecturerAssignment> LecturerAssignments { get; set; } = new List<LecturerAssignment>();

    public virtual ICollection<StudyPlanDetail> StudyPlanDetails { get; set; } = new List<StudyPlanDetail>();
}
