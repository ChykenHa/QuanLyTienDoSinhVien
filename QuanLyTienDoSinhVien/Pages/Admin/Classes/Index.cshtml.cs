using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using QuanLyTienDoSinhVien.Data;
using QuanLyTienDoSinhVien.Models;

namespace QuanLyTienDoSinhVien.Pages.Admin.Classes;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public IndexModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public List<Class> Classes { get; set; } = new();

    public async Task OnGetAsync()
    {
        Classes = await _context.Classes
            .Include(c => c.Major)
            .Include(c => c.Students)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }
}
