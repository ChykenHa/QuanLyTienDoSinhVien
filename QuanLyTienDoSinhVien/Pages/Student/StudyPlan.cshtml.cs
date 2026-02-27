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
    public class StudyPlanModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public StudyPlanModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public Models.Student? CurrentStudent { get; set; }
        public List<StudyPlan> StudyPlans { get; set; } = new();
        public List<Semester> Semesters { get; set; } = new();
        public List<Subject> Subjects { get; set; } = new();
        public string? StatusFilter { get; set; }
        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(string? statusFilter)
        {
            var student = await GetCurrentStudentAsync();
            if (student == null) return RedirectToPage("/Auth/Login");

            CurrentStudent = student;
            StatusFilter = statusFilter;

            var query = _context.StudyPlans
                .Include(sp => sp.StudyPlanDetails)
                    .ThenInclude(d => d.Subject)
                .Include(sp => sp.StudyPlanDetails)
                    .ThenInclude(d => d.Semester)
                .Include(sp => sp.StudyPlanReviews)
                    .ThenInclude(r => r.Lecturer)
                .Where(sp => sp.StudentId == student.Id);

            if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "All")
            {
                query = query.Where(sp => sp.Status == statusFilter);
            }

            StudyPlans = await query
                .OrderByDescending(sp => sp.CreatedAt)
                .ToListAsync();

            Semesters = await _context.Semesters.OrderByDescending(s => s.StartDate).ToListAsync();
            Subjects = await _context.Subjects.OrderBy(s => s.Code).ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostCreateAsync(int semesterId, int[] subjectIds)
        {
            var student = await GetCurrentStudentAsync();
            if (student == null) return RedirectToPage("/Auth/Login");

            if (subjectIds == null || subjectIds.Length == 0)
            {
                ErrorMessage = "Vui lòng chọn ít nhất một môn học.";
                return await OnGetAsync(null);
            }

            var plan = new StudyPlan
            {
                StudentId = student.Id,
                Status = "Pending",
                CreatedAt = DateTime.Now
            };

            _context.StudyPlans.Add(plan);
            await _context.SaveChangesAsync();

            foreach (var subjectId in subjectIds)
            {
                _context.StudyPlanDetails.Add(new StudyPlanDetail
                {
                    StudyPlanId = plan.Id,
                    SubjectId = subjectId,
                    SemesterId = semesterId
                });
            }

            await _context.SaveChangesAsync();

            SuccessMessage = "Tạo kế hoạch học tập thành công!";
            return RedirectToPage(new { statusFilter = (string?)null });
        }

        public async Task<IActionResult> OnPostEditAsync(int planId, int semesterId, int[] subjectIds)
        {
            var student = await GetCurrentStudentAsync();
            if (student == null) return RedirectToPage("/Auth/Login");

            var plan = await _context.StudyPlans
                .Include(sp => sp.StudyPlanDetails)
                .FirstOrDefaultAsync(sp => sp.Id == planId && sp.StudentId == student.Id);

            if (plan == null || (plan.Status != "Rejected" && plan.Status != "NeedsRevision"))
            {
                ErrorMessage = "Không thể chỉnh sửa kế hoạch này.";
                return await OnGetAsync(null);
            }

            // Remove old details
            _context.StudyPlanDetails.RemoveRange(plan.StudyPlanDetails);

            // Add new details
            foreach (var subjectId in subjectIds)
            {
                _context.StudyPlanDetails.Add(new StudyPlanDetail
                {
                    StudyPlanId = plan.Id,
                    SubjectId = subjectId,
                    SemesterId = semesterId
                });
            }

            plan.Status = "Pending";
            await _context.SaveChangesAsync();

            SuccessMessage = "Cập nhật kế hoạch thành công!";
            return RedirectToPage(new { statusFilter = (string?)null });
        }

        private async Task<Models.Student?> GetCurrentStudentAsync()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return null;

            var userId = int.Parse(userIdClaim);
            return await _context.Students
                .FirstOrDefaultAsync(s => s.UserId == userId);
        }
    }
}
