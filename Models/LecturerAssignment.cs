using System;
using System.Collections.Generic;

namespace QuanLyTienDoSinhVien.Models;

public partial class LecturerAssignment
{
    public int Id { get; set; }

    public int LecturerId { get; set; }

    public int SubjectId { get; set; }

    public int ClassId { get; set; }

    public virtual Class Class { get; set; } = null!;

    public virtual Lecturer Lecturer { get; set; } = null!;

    public virtual Subject Subject { get; set; } = null!;
}
