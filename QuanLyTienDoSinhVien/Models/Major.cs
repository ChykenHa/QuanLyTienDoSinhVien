using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuanLyTienDoSinhVien.Models;

public partial class Major
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Tên ngành là bắt buộc")]
    [Display(Name = "Tên Ngành")]
    public string Name { get; set; } = null!;





    public virtual ICollection<Class> Classes { get; set; } = new List<Class>();
}
