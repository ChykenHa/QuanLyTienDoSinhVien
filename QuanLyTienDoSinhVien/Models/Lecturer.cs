using System;
using System.Collections.Generic;

namespace QuanLyTienDoSinhVien.Models;

public partial class Lecturer
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string? FullName { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public virtual ICollection<LecturerAssignment> LecturerAssignments { get; set; } = new List<LecturerAssignment>();

    public virtual ICollection<StudyPlanReview> StudyPlanReviews { get; set; } = new List<StudyPlanReview>();

    public virtual User User { get; set; } = null!;
}
