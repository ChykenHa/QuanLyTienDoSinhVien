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
    public class AssignmentsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public AssignmentsModel(ApplicationDbContext context) { _context = context; }

        public List<Assignment> AssignmentList { get; set; } = new();
        public List<LecturerAssignment> MySubjectClasses { get; set; } = new();
        public Assignment? SelectedAssignment { get; set; }
        public List<SubmissionInfo> Submissions { get; set; } = new();
        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(int? assignmentId)
        {
            var lecturer = await GetCurrentLecturerAsync();
            if (lecturer == null) return RedirectToPage("/Auth/Login");
            await LoadDataAsync(lecturer, assignmentId);
            return Page();
        }

        // Create assignment
        public async Task<IActionResult> OnPostCreateAsync(int subjectId, int classId, string title, string? description, DateTime? dueDate, double maxScore)
        {
            var lecturer = await GetCurrentLecturerAsync();
            if (lecturer == null) return RedirectToPage("/Auth/Login");

            if (string.IsNullOrWhiteSpace(title))
            {
                ErrorMessage = "Vui lòng nhập tiêu đề bài tập.";
                await LoadDataAsync(lecturer, null);
                return Page();
            }

            var assignment = new Assignment
            {
                SubjectId = subjectId,
                ClassId = classId,
                LecturerId = lecturer.Id,
                Title = title,
                Description = description,
                DueDate = dueDate,
                MaxScore = maxScore > 0 ? maxScore : 10,
                CreatedAt = DateTime.Now
            };
            _context.Assignments.Add(assignment);
            await _context.SaveChangesAsync();

            SuccessMessage = "Đã tạo bài tập mới!";
            await LoadDataAsync(lecturer, null);
            return Page();
        }

        // Grade submissions
        public async Task<IActionResult> OnPostGradeAsync(int assignmentId, int[] studentIds, double[] scores, string[] comments)
        {
            var lecturer = await GetCurrentLecturerAsync();
            if (lecturer == null) return RedirectToPage("/Auth/Login");

            for (int i = 0; i < studentIds.Length; i++)
            {
                var existing = await _context.AssignmentSubmissions
                    .FirstOrDefaultAsync(s => s.AssignmentId == assignmentId && s.StudentId == studentIds[i]);

                if (existing != null)
                {
                    existing.Score = scores.Length > i ? scores[i] : null;
                    existing.Comment = comments.Length > i ? comments[i] : null;
                    existing.GradedAt = DateTime.Now;
                }
                else
                {
                    _context.AssignmentSubmissions.Add(new AssignmentSubmission
                    {
                        AssignmentId = assignmentId,
                        StudentId = studentIds[i],
                        Score = scores.Length > i ? scores[i] : null,
                        Comment = comments.Length > i ? comments[i] : null,
                        SubmittedAt = DateTime.Now,
                        GradedAt = DateTime.Now
                    });
                }
            }
            await _context.SaveChangesAsync();

            SuccessMessage = "Đã chấm điểm thành công!";
            await LoadDataAsync(lecturer, assignmentId);
            return Page();
        }

        private async Task LoadDataAsync(Lecturer lecturer, int? assignmentId)
        {
            MySubjectClasses = await _context.LecturerAssignments
                .Include(la => la.Subject).Include(la => la.Class)
                .Where(la => la.LecturerId == lecturer.Id).ToListAsync();

            AssignmentList = await _context.Assignments
                .Include(a => a.Subject).Include(a => a.Class)
                .Where(a => a.LecturerId == lecturer.Id)
                .OrderByDescending(a => a.CreatedAt).ToListAsync();

            if (assignmentId.HasValue)
            {
                SelectedAssignment = await _context.Assignments
                    .Include(a => a.Subject).Include(a => a.Class)
                    .FirstOrDefaultAsync(a => a.Id == assignmentId && a.LecturerId == lecturer.Id);

                if (SelectedAssignment != null)
                {
                    // Get students in the assignment's class
                    var students = await _context.Students
                        .Where(s => s.ClassId == SelectedAssignment.ClassId)
                        .ToListAsync();

                    var existingSubs = await _context.AssignmentSubmissions
                        .Where(s => s.AssignmentId == assignmentId).ToListAsync();

                    Submissions = students.Select(s =>
                    {
                        var sub = existingSubs.FirstOrDefault(x => x.StudentId == s.Id);
                        return new SubmissionInfo
                        {
                            StudentId = s.Id,
                            StudentCode = s.StudentCode,
                            FullName = s.FullName ?? "N/A",
                            Score = sub?.Score,
                            Comment = sub?.Comment,
                            GradedAt = sub?.GradedAt
                        };
                    }).ToList();
                }
            }
        }

        private async Task<Lecturer?> GetCurrentLecturerAsync()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return null;
            return await _context.Lecturers.FirstOrDefaultAsync(l => l.UserId == int.Parse(userIdClaim));
        }

        public class SubmissionInfo
        {
            public int StudentId { get; set; }
            public string StudentCode { get; set; } = "";
            public string FullName { get; set; } = "";
            public double? Score { get; set; }
            public string? Comment { get; set; }
            public DateTime? GradedAt { get; set; }
        }
    }
}
