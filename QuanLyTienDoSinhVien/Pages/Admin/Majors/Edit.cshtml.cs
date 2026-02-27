using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyTienDoSinhVien.Data;
using QuanLyTienDoSinhVien.Models;

namespace QuanLyTienDoSinhVien.Pages.Admin.Majors;

public class EditModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public EditModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Major Major { get; set; } = default!;



    public async Task<IActionResult> OnGetAsync(int id)
    {
        var major = await _context.Majors.FindAsync(id);
        if (major == null)
        {
            return NotFound();
        }
        Major = major;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {

            return Page();
        }

        _context.Attach(Major).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Đã cập nhật ngành '{Major.Name}' thành công!";
        return RedirectToPage("Index");
    }
}
