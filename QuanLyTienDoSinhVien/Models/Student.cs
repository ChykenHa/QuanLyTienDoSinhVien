using System;
using System.Collections.Generic;

namespace QuanLyTienDoSinhVien.Models;

public partial class Student
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string StudentCode { get; set; } = null!;

    public string? FullName { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string? Address { get; set; }

    public int? ClassId { get; set; }

    public bool? IsPrivate { get; set; }

    public virtual Class? Class { get; set; }

    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

    public virtual ICollection<StudyPlan> StudyPlans { get; set; } = new List<StudyPlan>();

    public virtual User User { get; set; } = null!;

    public virtual ICollection<Violation> Violations { get; set; } = new List<Violation>();
}
