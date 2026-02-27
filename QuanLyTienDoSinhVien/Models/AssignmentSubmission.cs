using System;
using System.Collections.Generic;

namespace QuanLyTienDoSinhVien.Models;

public partial class AssignmentSubmission
{
    public int Id { get; set; }

    public int AssignmentId { get; set; }

    public int StudentId { get; set; }

    public double? Score { get; set; }

    public string? Comment { get; set; }

    public DateTime? SubmittedAt { get; set; }

    public DateTime? GradedAt { get; set; }

    public virtual Assignment Assignment { get; set; } = null!;

    public virtual Student Student { get; set; } = null!;
}
