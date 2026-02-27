using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using QuanLyTienDoSinhVien.Data;
using QuanLyTienDoSinhVien.Models;

namespace QuanLyTienDoSinhVien.Pages.Admin.Configuration
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public IndexModel(ApplicationDbContext context) { _context = context; }

        public List<Semester> Semesters { get; set; } = new();
        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            Semesters = await _context.Semesters.OrderByDescending(s => s.StartDate).ToListAsync();
        }

        public async Task<IActionResult> OnPostAddAsync(string name, DateTime startDate, DateTime endDate)
        {
            if (string.IsNullOrWhiteSpace(name)) { ErrorMessage = "Tên học kỳ không được trống."; Semesters = await _context.Semesters.OrderByDescending(s => s.StartDate).ToListAsync(); return Page(); }
            if (endDate <= startDate) { ErrorMessage = "Ngày kết thúc phải sau ngày bắt đầu."; Semesters = await _context.Semesters.OrderByDescending(s => s.StartDate).ToListAsync(); return Page(); }

            _context.Semesters.Add(new Semester
            {
                Name = name,
                StartDate = DateOnly.FromDateTime(startDate),
                EndDate = DateOnly.FromDateTime(endDate)
            });
            await _context.SaveChangesAsync();
            SuccessMessage = "Đã thêm học kỳ!";
            Semesters = await _context.Semesters.OrderByDescending(s => s.StartDate).ToListAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var sem = await _context.Semesters.FindAsync(id);
            if (sem != null)
            {
                var hasData = await _context.Enrollments.AnyAsync(e => e.SemesterId == id)
                    || await _context.StudyPlanDetails.AnyAsync(d => d.SemesterId == id);
                if (hasData) { ErrorMessage = "Không thể xóa: học kỳ đang có dữ liệu liên quan."; }
                else { _context.Semesters.Remove(sem); await _context.SaveChangesAsync(); SuccessMessage = "Đã xóa học kỳ."; }
            }
            Semesters = await _context.Semesters.OrderByDescending(s => s.StartDate).ToListAsync();
            return Page();
        }
    }
}
