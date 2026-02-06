using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyTienDoSinhVien.Data;
using QuanLyTienDoSinhVien.Models;

namespace QuanLyTienDoSinhVien.Pages.Admin.Classes;

public class CreateModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public CreateModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Class Class { get; set; } = new();

    public SelectList Majors { get; set; } = default!;

    public async Task OnGetAsync()
    {
        Majors = new SelectList(await _context.Majors.ToListAsync(), "Id", "Name");
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            Majors = new SelectList(await _context.Majors.ToListAsync(), "Id", "Name");
            return Page();
        }

        _context.Classes.Add(Class);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Đã thêm lớp '{Class.Name}' thành công!";
        return RedirectToPage("Index");
    }
}
