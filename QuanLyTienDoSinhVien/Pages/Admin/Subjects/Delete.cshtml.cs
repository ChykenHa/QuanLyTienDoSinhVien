using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QuanLyTienDoSinhVien.Data;
using QuanLyTienDoSinhVien.Models;

namespace QuanLyTienDoSinhVien.Pages.Admin.Subjects;

public class DeleteModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public DeleteModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Subject Subject { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var subject = await _context.Subjects.FindAsync(id);
        if (subject == null)
        {
            return NotFound();
        }
        Subject = subject;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var subject = await _context.Subjects.FindAsync(Subject.Id);
        if (subject != null)
        {
            _context.Subjects.Remove(subject);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Đã xóa môn học '{subject.Name}' thành công!";
        }
        return RedirectToPage("Index");
    }
}
