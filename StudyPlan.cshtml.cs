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

        // Student Info
        public int UserId { get; set; }
        public string StudentCode { get; set; } = null!;
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public int? ClassId { get; set; }
        public Models.Student? CurrentStudent { get; set; }

        // Study Plans
        public List<StudyPlanDto> StudyPlans { get; set; } = new();
        public List<Semester> AvailableSemesters { get; set; } = new();
        public List<Subject> AvailableSubjects { get; set; } = new();

        // For creating new study plan
        [BindProperty]
        public int SelectedSemesterId { get; set; }

        [BindProperty]
        public List<int> SelectedSubjectIds { get; set; } = new();

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
                UserId = userId;
                StudentCode = CurrentStudent.StudentCode;
                FullName = CurrentStudent.FullName;
                Email = CurrentStudent.Email;
                ClassId = CurrentStudent.ClassId;

                // Get all study plans with details
                var studyPlans = await _context.StudyPlans
                    .Include(sp => sp.StudyPlanDetails)
                    .ThenInclude(spd => spd.Subject)
                    .Include(sp => sp.StudyPlanDetails)
                    .ThenInclude(spd => spd.Semester)
                    .Include(sp => sp.StudyPlanReviews)
                    .ThenInclude(spr => spr.Lecturer)
                    .Where(sp => sp.StudentId == CurrentStudent.Id)
                    .OrderByDescending(sp => sp.CreatedAt)
                    .ToListAsync();

                // Map to DTO with detail information
                StudyPlans = studyPlans.Select(sp => new StudyPlanDto
                {
                    Id = sp.Id,
                    Status = sp.Status ?? "Chưa gửi",
                    CreatedAt = sp.CreatedAt,
                    TotalCredits = sp.StudyPlanDetails.Sum(spd => spd.Subject?.Credit ?? 0),
                    SubjectCount = sp.StudyPlanDetails.Count,
                    Details = sp.StudyPlanDetails.Select(spd => new StudyPlanDetailDto
                    {
                        Id = spd.Id,
                        SubjectId = spd.SubjectId,
                        SubjectCode = spd.Subject?.Code ?? "",
                        SubjectName = spd.Subject?.Name ?? "",
                        Credit = spd.Subject?.Credit ?? 0,
                        SemesterId = spd.SemesterId,
                        SemesterName = spd.Semester?.Name ?? "",
                        SemesterStartDate = spd.Semester?.StartDate,
                        SemesterEndDate = spd.Semester?.EndDate
                    }).OrderBy(d => d.SemesterName).ToList(),
                    Reviews = sp.StudyPlanReviews.Select(spr => new StudyPlanReviewDto
                    {
                        Id = spr.Id,
                        LecturerName = spr.Lecturer?.FullName ?? "",
                        Comment = spr.Comment,
                        ReviewedAt = spr.ReviewedAt
                    }).ToList()
                }).ToList();

                // Get available semesters
                AvailableSemesters = await _context.Semesters
                    .OrderBy(s => s.StartDate)
                    .ToListAsync();

                // Get all subjects
                AvailableSubjects = await _context.Subjects
                    .OrderBy(s => s.Code)
                    .ToListAsync();
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Page();

            var userId = int.Parse(userIdClaim);
            CurrentStudent = await _context.Students
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (CurrentStudent == null)
                return Page();

            // Create new study plan
            var newStudyPlan = new StudyPlan
            {
                StudentId = CurrentStudent.Id,
                Status = "Chưa gửi",
                CreatedAt = DateTime.Now
            };

            _context.StudyPlans.Add(newStudyPlan);
            await _context.SaveChangesAsync();

            // Add selected subjects to the study plan
            if (SelectedSubjectIds.Any())
            {
                foreach (var subjectId in SelectedSubjectIds)
                {
                    var detail = new StudyPlanDetail
                    {
                        StudyPlanId = newStudyPlan.Id,
                        SubjectId = subjectId,
                        SemesterId = SelectedSemesterId
                    };
                    _context.StudyPlanDetails.Add(detail);
                }
                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Page();

            var userId = int.Parse(userIdClaim);
            var currentStudent = await _context.Students
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (currentStudent == null)
                return Page();

            // Get the study plan and verify ownership
            var studyPlan = await _context.StudyPlans
                .FirstOrDefaultAsync(sp => sp.Id == id && sp.StudentId == currentStudent.Id);

            if (studyPlan != null && studyPlan.Status == "Chưa gửi")
            {
                // Delete related details first
                var details = await _context.StudyPlanDetails
                    .Where(spd => spd.StudyPlanId == id)
                    .ToListAsync();
                _context.StudyPlanDetails.RemoveRange(details);

                // Delete the study plan
                _context.StudyPlans.Remove(studyPlan);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRemoveSubjectAsync(int detailId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Page();

            var userId = int.Parse(userIdClaim);
            var currentStudent = await _context.Students
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (currentStudent == null)
                return Page();

            // Get the detail and verify ownership through study plan
            var detail = await _context.StudyPlanDetails
                .Include(spd => spd.StudyPlan)
                .FirstOrDefaultAsync(spd => spd.Id == detailId 
                    && spd.StudyPlan.StudentId == currentStudent.Id
                    && spd.StudyPlan.Status == "Chưa gửi");

            if (detail != null)
            {
                _context.StudyPlanDetails.Remove(detail);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostSubmitAsync(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Page();

            var userId = int.Parse(userIdClaim);
            var currentStudent = await _context.Students
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (currentStudent == null)
                return Page();

            var studyPlan = await _context.StudyPlans
                .FirstOrDefaultAsync(sp => sp.Id == id && sp.StudentId == currentStudent.Id);

            if (studyPlan != null && studyPlan.Status == "Chưa gửi")
            {
                studyPlan.Status = "Chờ duyệt";
                _context.StudyPlans.Update(studyPlan);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }
    }

    // DTOs for easier data handling
    public class StudyPlanDto
    {
        public int Id { get; set; }
        public string Status { get; set; } = null!;
        public DateTime? CreatedAt { get; set; }
        public int TotalCredits { get; set; }
        public int SubjectCount { get; set; }
        public List<StudyPlanDetailDto> Details { get; set; } = new();
        public List<StudyPlanReviewDto> Reviews { get; set; } = new();
    }

    public class StudyPlanDetailDto
    {
        public int Id { get; set; }
        public int SubjectId { get; set; }
        public string SubjectCode { get; set; } = null!;
        public string SubjectName { get; set; } = null!;
        public int Credit { get; set; }
        public int SemesterId { get; set; }
        public string SemesterName { get; set; } = null!;
        public DateOnly? SemesterStartDate { get; set; }
        public DateOnly? SemesterEndDate { get; set; }
    }

    public class StudyPlanReviewDto
    {
        public int Id { get; set; }
        public string LecturerName { get; set; } = null!;
        public string? Comment { get; set; }
        public DateTime? ReviewedAt { get; set; }
    }
}
