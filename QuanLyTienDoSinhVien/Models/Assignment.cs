using System;
using System.Collections.Generic;

namespace QuanLyTienDoSinhVien.Models;

public partial class Assignment
{
    public int Id { get; set; }

    public int SubjectId { get; set; }

    public int ClassId { get; set; }

    public int LecturerId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime? DueDate { get; set; }

    public double? MaxScore { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Subject Subject { get; set; } = null!;

    public virtual Class Class { get; set; } = null!;

    public virtual Lecturer Lecturer { get; set; } = null!;

    public virtual ICollection<AssignmentSubmission> AssignmentSubmissions { get; set; } = new List<AssignmentSubmission>();
}
