using System;
using System.Collections.Generic;

namespace QuanLyTienDoSinhVien.Models;

public partial class Major
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Class> Classes { get; set; } = new List<Class>();
}
