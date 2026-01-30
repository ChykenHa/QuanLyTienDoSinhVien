using System;
using System.Collections.Generic;

namespace QuanLyTienDoSinhVien.Models;

public partial class Enrollment
{
    public int Id { get; set; }

    public int StudentId { get; set; }

    public int SubjectId { get; set; }

    public int SemesterId { get; set; }

    public string? Status { get; set; }

    public virtual Semester Semester { get; set; } = null!;

    public virtual Student Student { get; set; } = null!;

    public virtual ICollection<StudyProgress> StudyProgresses { get; set; } = new List<StudyProgress>();

    public virtual Subject Subject { get; set; } = null!;
}
