using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyTienDoSinhVien.Data;
using QuanLyTienDoSinhVien.Models;

namespace QuanLyTienDoSinhVien.Pages.Admin.Classes;

public class EditModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public EditModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Class Class { get; set; } = default!;

    public SelectList Majors { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var cls = await _context.Classes.FindAsync(id);
        if (cls == null)
        {
            return NotFound();
        }
        Class = cls;
        Majors = new SelectList(await _context.Majors.ToListAsync(), "Id", "Name");
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            Majors = new SelectList(await _context.Majors.ToListAsync(), "Id", "Name");
            return Page();
        }

        _context.Attach(Class).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Đã cập nhật lớp '{Class.Name}' thành công!";
        return RedirectToPage("Index");
    }
}
