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
    public class ProfileModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ProfileModel(ApplicationDbContext context)
        {
            _context = context;
        }

        // Statistics
        public decimal GPA { get; set; }
        public int AttendanceRate { get; set; }

        // Student Info
        public int UserId { get; set; }
        public string StudentCode { get; set; } = null!;
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public int? ClassId { get; set; }
        public bool? IsPrivate { get; set; }
        public virtual Class? Class { get; set; }
        public string? MajorName { get; set; }
        public string? AcademicYear { get; set; }
        public Models.Student? CurrentStudent { get; set; }

        // Collections
        public List<Enrollment> Enrollments { get; set; } = new();
        public List<StudyPlan> StudyPlans { get; set; } = new();

        // Form binding for Edit Profile
        [BindProperty]
        public string? EditFullName { get; set; }
        [BindProperty]
        public string? EditEmail { get; set; }
        [BindProperty]
        public string? EditPhone { get; set; }
        [BindProperty]
        public string? EditAddress { get; set; }
        [BindProperty]
        public bool? EditIsPrivate { get; set; }

        // Share Profile Link
        public string ShareProfileLink { get; set; } = null!;

        public async Task OnGetAsync()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return;

            var userId = int.Parse(userIdClaim);
            
            // Get current student with all related data
            CurrentStudent = await _context.Students
                .Include(s => s.Class)
                .ThenInclude(c => c!.Major)
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (CurrentStudent != null)
            {
                // Map student personal info
                UserId = userId;
                StudentCode = CurrentStudent.StudentCode;
                FullName = CurrentStudent.FullName ?? "N/A";
                Email = CurrentStudent.Email ?? "N/A";
                Phone = CurrentStudent.Phone ?? "N/A";
                Address = CurrentStudent.Address ?? "N/A";
                ClassId = CurrentStudent.ClassId;
                IsPrivate = CurrentStudent.IsPrivate ?? false;
                Class = CurrentStudent.Class;
                MajorName = CurrentStudent.Class?.Major?.Name ?? "N/A";

                // Get ALL enrollments with progress data
                var allEnrollments = await _context.Enrollments
                    .Include(e => e.Subject)
                    .Include(e => e.Semester)
                    .Include(e => e.StudyProgresses)
                    .Where(e => e.StudentId == CurrentStudent.Id)
                    .OrderByDescending(e => e.Semester!.StartDate)
                    .ToListAsync();

                Enrollments = allEnrollments;

                AcademicYear = CurrentStudent.Class != null && Enrollments.Any()
    ? $"{Enrollments.First().Semester.StartDate?.Year} - {Enrollments.First().Semester.StartDate?.Year + 1}"
    : "N/A";

                // Calculate GPA from all enrollments with scores
                var enrollmentsWithScores = allEnrollments
                    .Where(e => e.StudyProgresses.Any(sp => sp.Score.HasValue))
                    .ToList();

                if (enrollmentsWithScores.Any())
                {
                    GPA = (decimal)enrollmentsWithScores
                        .SelectMany(e => e.StudyProgresses)
                        .Where(sp => sp.Score.HasValue)
                        .Average(sp => sp.Score!.Value);
                }

                // Calculate attendance rate (completion percent average)
                // If no completion data available, use enrollment count completion
                var progressWithCompletion = allEnrollments
                    .SelectMany(e => e.StudyProgresses)
                    .Where(sp => sp.CompletionPercent.HasValue)
                    .ToList();

                if (progressWithCompletion.Any())
                {
                    AttendanceRate = (int)progressWithCompletion
                        .Average(sp => sp.CompletionPercent!.Value);
                }
                else
                {
                    // Fallback: use completion status
                    var totalEnrollments = allEnrollments.Count;
                    var completedCount = allEnrollments.Count(e => e.Status == "Completed");
                    AttendanceRate = totalEnrollments > 0 ? (completedCount * 100 / totalEnrollments) : 0;
                }

                // Get study plans
                StudyPlans = await _context.StudyPlans
                    .Include(sp => sp.StudyPlanReviews)
                    .Where(sp => sp.StudentId == CurrentStudent.Id)
                    .OrderByDescending(sp => sp.CreatedAt)
                    .Take(5)
                    .ToListAsync();

                // Generate share link
                var scheme = Request.Scheme;
                var host = Request.Host;
                ShareProfileLink = $"{scheme}://{host}/Student/PublicProfile?studentCode={StudentCode}";

                // Initialize edit form with current data
                EditFullName = FullName;
                EditEmail = Email;
                EditPhone = Phone;
                EditAddress = Address;
                EditIsPrivate = IsPrivate ?? false;
            }
        }

        // POST handler for editing profile
        public async Task<IActionResult> OnPostEditProfileAsync()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return BadRequest();

            var userId = int.Parse(userIdClaim);

            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (student == null)
                return BadRequest();

            // Update student information
            if (!string.IsNullOrEmpty(EditFullName))
                student.FullName = EditFullName;
            if (!string.IsNullOrEmpty(EditEmail))
                student.Email = EditEmail;
            if (!string.IsNullOrEmpty(EditPhone))
                student.Phone = EditPhone;
            if (!string.IsNullOrEmpty(EditAddress))
                student.Address = EditAddress;

            student.IsPrivate = EditIsPrivate ?? false;

            try
            {
                _context.Students.Update(student);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Cập nhật hồ sơ thành công!";
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi cập nhật: {ex.Message}";
                return RedirectToPage();
            }
        }
    }
}