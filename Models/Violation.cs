using System;
using System.Collections.Generic;

namespace QuanLyTienDoSinhVien.Models;

public partial class Violation
{
    public int Id { get; set; }

    public int StudentId { get; set; }

    public string? Description { get; set; }

    public DateOnly? ViolationDate { get; set; }

    public virtual Student Student { get; set; } = null!;
}
