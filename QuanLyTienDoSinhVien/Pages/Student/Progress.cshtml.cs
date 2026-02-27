using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using QuanLyTienDoSinhVien.Data;
using QuanLyTienDoSinhVien.Models;
using System.Security.Claims;
using System.Text.Json;

namespace QuanLyTienDoSinhVien.Pages.Student
{
    [Authorize(Roles = "Student")]
    public class ProgressModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ProgressModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public Models.Student? CurrentStudent { get; set; }
        public decimal GPA { get; set; }
        public int TotalCredits { get; set; }
        public int CompletedSubjects { get; set; }
        public int TotalSubjects { get; set; }

        // Grouped by semester
        public List<SemesterProgress> SemesterProgresses { get; set; } = new();

        // Chart.js data
        public string ChartLabelsJson { get; set; } = "[]";
        public string ChartGpaJson { get; set; } = "[]";
        public string ChartCreditsJson { get; set; } = "[]";

        public async Task<IActionResult> OnGetAsync()
        {
            var student = await GetCurrentStudentAsync();
            if (student == null) return RedirectToPage("/Auth/Login");

            CurrentStudent = student;

            var enrollments = await _context.Enrollments
                .Include(e => e.Subject)
                .Include(e => e.Semester)
                .Include(e => e.StudyProgresses)
                .Where(e => e.StudentId == student.Id)
                .OrderBy(e => e.Semester.StartDate)
                .ToListAsync();

            TotalSubjects = enrollments.Count;
            CompletedSubjects = enrollments.Count(e => e.Status == "Completed");

            // Calculate total credits for completed subjects
            TotalCredits = enrollments
                .Where(e => e.Status == "Completed")
                .Sum(e => e.Subject.Credit);

            // Calculate overall GPA
            var scoredEnrollments = enrollments
                .Where(e => e.StudyProgresses.Any(sp => sp.Score.HasValue))
                .ToList();

            if (scoredEnrollments.Any())
            {
                var totalWeightedScore = 0.0;
                var totalCreditForGpa = 0;
                foreach (var e in scoredEnrollments)
                {
                    var score = e.StudyProgresses.Where(sp => sp.Score.HasValue).Average(sp => sp.Score!.Value);
                    totalWeightedScore += score * e.Subject.Credit;
                    totalCreditForGpa += e.Subject.Credit;
                }
                GPA = totalCreditForGpa > 0 ? (decimal)(totalWeightedScore / totalCreditForGpa) : 0;
            }

            // Group by semester
            var grouped = enrollments
                .GroupBy(e => new { e.SemesterId, e.Semester.Name, e.Semester.StartDate })
                .OrderBy(g => g.Key.StartDate)
                .ToList();

            var chartLabels = new List<string>();
            var chartGpa = new List<double>();
            var chartCredits = new List<int>();

            foreach (var group in grouped)
            {
                var semProgress = new SemesterProgress
                {
                    SemesterName = group.Key.Name ?? "N/A",
                    Enrollments = group.Select(e => new EnrollmentInfo
                    {
                        SubjectCode = e.Subject.Code,
                        SubjectName = e.Subject.Name,
                        Credit = e.Subject.Credit,
                        Score = e.StudyProgresses.FirstOrDefault()?.Score,
                        CompletionPercent = e.StudyProgresses.FirstOrDefault()?.CompletionPercent ?? 0,
                        Status = e.Status ?? "Unknown"
                    }).ToList()
                };

                SemesterProgresses.Add(semProgress);

                // Chart data
                chartLabels.Add(group.Key.Name ?? "N/A");
                var semScored = group.Where(e => e.StudyProgresses.Any(sp => sp.Score.HasValue)).ToList();
                if (semScored.Any())
                {
                    var semGpa = semScored
                        .SelectMany(e => e.StudyProgresses)
                        .Where(sp => sp.Score.HasValue)
                        .Average(sp => sp.Score!.Value);
                    chartGpa.Add(Math.Round(semGpa, 2));
                }
                else
                {
                    chartGpa.Add(0);
                }
                chartCredits.Add(group.Where(e => e.Status == "Completed").Sum(e => e.Subject.Credit));
            }

            ChartLabelsJson = JsonSerializer.Serialize(chartLabels);
            ChartGpaJson = JsonSerializer.Serialize(chartGpa);
            ChartCreditsJson = JsonSerializer.Serialize(chartCredits);

            return Page();
        }

        private async Task<Models.Student?> GetCurrentStudentAsync()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return null;
            var userId = int.Parse(userIdClaim);
            return await _context.Students.FirstOrDefaultAsync(s => s.UserId == userId);
        }

        public class SemesterProgress
        {
            public string SemesterName { get; set; } = "";
            public List<EnrollmentInfo> Enrollments { get; set; } = new();
        }

        public class EnrollmentInfo
        {
            public string SubjectCode { get; set; } = "";
            public string SubjectName { get; set; } = "";
            public int Credit { get; set; }
            public double? Score { get; set; }
            public int CompletionPercent { get; set; }
            public string Status { get; set; } = "";
        }
    }
}
