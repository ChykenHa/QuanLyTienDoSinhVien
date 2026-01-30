using System;
using System.Collections.Generic;

namespace QuanLyTienDoSinhVien.Models;

public partial class StudyPlan
{
    public int Id { get; set; }

    public int StudentId { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Student Student { get; set; } = null!;

    public virtual ICollection<StudyPlanDetail> StudyPlanDetails { get; set; } = new List<StudyPlanDetail>();

    public virtual ICollection<StudyPlanReview> StudyPlanReviews { get; set; } = new List<StudyPlanReview>();
}
