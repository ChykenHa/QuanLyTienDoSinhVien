using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using QuanLyTienDoSinhVien.Data;
using QuanLyTienDoSinhVien.Models;
using System.Security.Claims;

namespace QuanLyTienDoSinhVien.Pages.Student
{
    [Authorize(Roles = "Student")]
    public class WarningsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public WarningsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public Models.Student? CurrentStudent { get; set; }
        public List<Violation> Violations { get; set; } = new();
        public int TotalViolations { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return RedirectToPage("/Auth/Login");

            var userId = int.Parse(userIdClaim);
            CurrentStudent = await _context.Students.FirstOrDefaultAsync(s => s.UserId == userId);

            if (CurrentStudent == null) return RedirectToPage("/Auth/Login");

            Violations = await _context.Violations
                .Where(v => v.StudentId == CurrentStudent.Id)
                .OrderByDescending(v => v.ViolationDate)
                .ToListAsync();

            TotalViolations = Violations.Count;

            return Page();
        }
    }
}
