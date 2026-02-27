using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using QuanLyTienDoSinhVien.Data;
using QuanLyTienDoSinhVien.Models;
using System.Security.Claims;

namespace QuanLyTienDoSinhVien.Pages.Teacher
{
    [Authorize(Roles = "Teacher")]
    public class EvaluateModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public EvaluateModel(ApplicationDbContext context) { _context = context; }

        public List<StudentEvalInfo> Students { get; set; } = new();
        public List<EvalHistory> EvalHistories { get; set; } = new();
        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var lecturer = await GetCurrentLecturerAsync();
            if (lecturer == null) return RedirectToPage("/Auth/Login");
            await LoadDataAsync(lecturer);
            return Page();
        }

        // Add violation
        public async Task<IActionResult> OnPostViolationAsync(int studentId, string description)
        {
            var lecturer = await GetCurrentLecturerAsync();
            if (lecturer == null) return RedirectToPage("/Auth/Login");

            if (string.IsNullOrWhiteSpace(description))
            {
                ErrorMessage = "Vui lòng nhập mô tả vi phạm.";
                await LoadDataAsync(lecturer);
                return Page();
            }

            _context.Violations.Add(new Violation
            {
                StudentId = studentId,
                Description = description,
                ViolationDate = DateOnly.FromDateTime(DateTime.Now)
            });
            await _context.SaveChangesAsync();

            SuccessMessage = "Đã ghi nhận vi phạm.";
            await LoadDataAsync(lecturer);
            return Page();
        }

        // Add study plan review/comment
        public async Task<IActionResult> OnPostCommentAsync(int studentId, string comment)
        {
            var lecturer = await GetCurrentLecturerAsync();
            if (lecturer == null) return RedirectToPage("/Auth/Login");

            if (string.IsNullOrWhiteSpace(comment))
            {
                ErrorMessage = "Vui lòng nhập nhận xét.";
                await LoadDataAsync(lecturer);
                return Page();
            }

            // Find latest study plan of the student
            var plan = await _context.StudyPlans
                .Where(sp => sp.StudentId == studentId)
                .OrderByDescending(sp => sp.CreatedAt)
                .FirstOrDefaultAsync();

            if (plan != null)
            {
                _context.StudyPlanReviews.Add(new StudyPlanReview
                {
                    StudyPlanId = plan.Id,
                    LecturerId = lecturer.Id,
                    Comment = comment,
                    ReviewedAt = DateTime.Now
                });
                await _context.SaveChangesAsync();
                SuccessMessage = "Đã gửi nhận xét.";
            }
            else
            {
                ErrorMessage = "Sinh viên chưa có kế hoạch học tập nào.";
            }

            await LoadDataAsync(lecturer);
            return Page();
        }

        private async Task LoadDataAsync(Lecturer lecturer)
        {
            var assignedClassIds = await _context.LecturerAssignments
                .Where(la => la.LecturerId == lecturer.Id)
                .Select(la => la.ClassId).Distinct().ToListAsync();

            Students = await _context.Students
                .Include(s => s.Class)
                .Where(s => s.ClassId != null && assignedClassIds.Contains(s.ClassId.Value))
                .Select(s => new StudentEvalInfo
                {
                    Id = s.Id,
                    StudentCode = s.StudentCode,
                    FullName = s.FullName ?? "N/A",
                    ClassName = s.Class!.Name,
                    ViolationCount = s.Violations.Count
                }).ToListAsync();

            // Recent evaluations
            var studentIds = Students.Select(s => s.Id).ToList();
            var recentViolations = await _context.Violations
                .Include(v => v.Student)
                .Where(v => studentIds.Contains(v.StudentId))
                .OrderByDescending(v => v.ViolationDate)
                .Take(10).ToListAsync();

            var recentReviews = await _context.StudyPlanReviews
                .Include(r => r.StudyPlan).ThenInclude(sp => sp.Student)
                .Where(r => r.LecturerId == lecturer.Id)
                .OrderByDescending(r => r.ReviewedAt)
                .Take(10).ToListAsync();

            EvalHistories = recentViolations.Select(v => new EvalHistory
            {
                Type = "Violation",
                StudentName = v.Student?.FullName ?? "N/A",
                StudentCode = v.Student?.StudentCode ?? "",
                Description = v.Description ?? "",
                Date = v.ViolationDate.HasValue ? v.ViolationDate.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null
            }).Concat(recentReviews.Select(r => new EvalHistory
            {
                Type = "Review",
                StudentName = r.StudyPlan?.Student?.FullName ?? "N/A",
                StudentCode = r.StudyPlan?.Student?.StudentCode ?? "",
                Description = r.Comment ?? "",
                Date = r.ReviewedAt
            })).OrderByDescending(e => e.Date).Take(15).ToList();
        }

        private async Task<Lecturer?> GetCurrentLecturerAsync()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return null;
            return await _context.Lecturers.FirstOrDefaultAsync(l => l.UserId == int.Parse(userIdClaim));
        }

        public class StudentEvalInfo
        {
            public int Id { get; set; }
            public string StudentCode { get; set; } = "";
            public string FullName { get; set; } = "";
            public string ClassName { get; set; } = "";
            public int ViolationCount { get; set; }
        }

        public class EvalHistory
        {
            public string Type { get; set; } = "";
            public string StudentName { get; set; } = "";
            public string StudentCode { get; set; } = "";
            public string Description { get; set; } = "";
            public DateTime? Date { get; set; }
        }
    }
}
