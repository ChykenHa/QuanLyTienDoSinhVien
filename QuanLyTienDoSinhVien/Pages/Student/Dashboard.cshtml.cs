using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using QuanLyTienDoSinhVien.Data;
using QuanLyTienDoSinhVien.Models;
using System.Security.Claims;

namespace QuanLyTienDoSinhVien.Pages.Student
{
    [Authorize(Roles = "Student")]
    public class DashboardModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DashboardModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public decimal GPA { get; set; }
        public int CompletionRate { get; set; }
        public int RetentionRate { get; set; }
        public List<Enrollment> Enrollments { get; set; } = new();
        public List<StudyPlan> StudyPlans { get; set; } = new();
        public Models.Student? CurrentStudent { get; set; }

        public async Task OnGetAsync()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return;

            var userId = int.Parse(userIdClaim);
            
            // Get current student
            CurrentStudent = await _context.Students
                .Include(s => s.Class)
                .ThenInclude(c => c!.Major)
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (CurrentStudent != null)
            {
                // Get enrollments with progress
                Enrollments = await _context.Enrollments
                    .Include(e => e.Subject)
                    .Include(e => e.Semester)
                    .Include(e => e.StudyProgresses)
                    .Where(e => e.StudentId == CurrentStudent.Id)
                    .OrderByDescending(e => e.Semester.StartDate)
                    .Take(10)
                    .ToListAsync();

                // Calculate GPA (simplified - you may need to adjust based on your grading system)
                var completedEnrollments = Enrollments
                    .Where(e => e.StudyProgresses.Any(sp => sp.Score.HasValue))
                    .ToList();

                if (completedEnrollments.Any())
                {
                    GPA = (decimal)completedEnrollments
                        .SelectMany(e => e.StudyProgresses)
                        .Where(sp => sp.Score.HasValue)
                        .Average(sp => sp.Score!.Value);
                }

                // Calculate completion rate
                var totalEnrollments = Enrollments.Count;
                var completedCount = Enrollments.Count(e => e.Status == "Completed");
                CompletionRate = totalEnrollments > 0 ? (completedCount * 100 / totalEnrollments) : 0;

                // Mock retention rate (you may need to calculate this differently)
                RetentionRate = 88;

                // Get study plans
                StudyPlans = await _context.StudyPlans
                    .Include(sp => sp.StudyPlanReviews)
                    .Where(sp => sp.StudentId == CurrentStudent.Id)
                    .OrderByDescending(sp => sp.CreatedAt)
                    .Take(5)
                    .ToListAsync();
            }
        }
    }
}
