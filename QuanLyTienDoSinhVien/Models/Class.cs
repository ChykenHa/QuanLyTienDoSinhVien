using System;
using System.Collections.Generic;

namespace QuanLyTienDoSinhVien.Models;

public partial class Class
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int MajorId { get; set; }

    public virtual ICollection<LecturerAssignment> LecturerAssignments { get; set; } = new List<LecturerAssignment>();

    public virtual Major Major { get; set; } = null!;

    public virtual ICollection<Student> Students { get; set; } = new List<Student>();
}
