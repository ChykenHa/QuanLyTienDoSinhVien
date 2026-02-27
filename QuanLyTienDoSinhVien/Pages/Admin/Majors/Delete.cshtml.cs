using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using QuanLyTienDoSinhVien.Data;
using QuanLyTienDoSinhVien.Models;

namespace QuanLyTienDoSinhVien.Pages.Admin.Majors;

public class DeleteModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public DeleteModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Major Major { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var major = await _context.Majors
            .FirstOrDefaultAsync(m => m.Id == id);

        if (major == null)
        {
            return NotFound();
        }
        Major = major;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var major = await _context.Majors.FindAsync(Major.Id);
        if (major != null)
        {
            _context.Majors.Remove(major);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Đã xóa ngành '{major.Name}' thành công!";
        }
        return RedirectToPage("Index");
    }
}
