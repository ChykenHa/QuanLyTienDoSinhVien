using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using QuanLyTienDoSinhVien.Data;
using QuanLyTienDoSinhVien.Models;

namespace QuanLyTienDoSinhVien.Pages.Admin.Classes;

public class DeleteModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public DeleteModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Class Class { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var cls = await _context.Classes
            .Include(c => c.Major)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (cls == null)
        {
            return NotFound();
        }
        Class = cls;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var cls = await _context.Classes.FindAsync(Class.Id);
        if (cls != null)
        {
            _context.Classes.Remove(cls);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Đã xóa lớp '{cls.Name}' thành công!";
        }
        return RedirectToPage("Index");
    }
}
