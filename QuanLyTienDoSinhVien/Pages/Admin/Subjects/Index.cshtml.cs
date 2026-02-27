using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using QuanLyTienDoSinhVien.Data;
using QuanLyTienDoSinhVien.Models;

namespace QuanLyTienDoSinhVien.Pages.Admin.Subjects;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public IndexModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public List<Subject> Subjects { get; set; } = new();

    public async Task OnGetAsync()
    {
        Subjects = await _context.Subjects
            .OrderBy(s => s.Code)
            .ToListAsync();
    }
}
