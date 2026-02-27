using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using QuanLyTienDoSinhVien.Data;
using System.Security.Claims;

namespace QuanLyTienDoSinhVien.Pages.Student
{
    [Authorize(Roles = "Student")]
    public class ProfileModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ProfileModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public Models.Student? CurrentStudent { get; set; }
        public string? ClassName { get; set; }
        public string? MajorName { get; set; }

        [BindProperty]
        public string? FullName { get; set; }
        [BindProperty]
        public string? Email { get; set; }
        [BindProperty]
        public string? Phone { get; set; }
        [BindProperty]
        public string? Address { get; set; }

        public string? SuccessMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var student = await GetCurrentStudentAsync();
            if (student == null) return RedirectToPage("/Auth/Login");

            CurrentStudent = student;
            ClassName = student.Class?.Name;
            MajorName = student.Class?.Major?.Name;
            FullName = student.FullName;
            Email = student.Email;
            Phone = student.Phone;
            Address = student.Address;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var student = await GetCurrentStudentAsync();
            if (student == null) return RedirectToPage("/Auth/Login");

            student.FullName = FullName;
            student.Email = Email;
            student.Phone = Phone;
            student.Address = Address;

            await _context.SaveChangesAsync();

            CurrentStudent = student;
            ClassName = student.Class?.Name;
            MajorName = student.Class?.Major?.Name;
            SuccessMessage = "Cập nhật thông tin thành công!";

            return Page();
        }

        private async Task<Models.Student?> GetCurrentStudentAsync()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return null;

            var userId = int.Parse(userIdClaim);
            return await _context.Students
                .Include(s => s.Class)
                .ThenInclude(c => c!.Major)
                .FirstOrDefaultAsync(s => s.UserId == userId);
        }
    }
}
