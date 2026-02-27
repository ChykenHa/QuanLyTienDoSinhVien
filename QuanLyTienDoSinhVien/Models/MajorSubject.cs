using System;
using System.Collections.Generic;

namespace QuanLyTienDoSinhVien.Models;

public partial class MajorSubject
{
    public int Id { get; set; }

    public int MajorId { get; set; }

    public int SubjectId { get; set; }

    public int? SemesterOrder { get; set; }

    public bool? IsRequired { get; set; }

    public virtual Major Major { get; set; } = null!;

    public virtual Subject Subject { get; set; } = null!;
}
