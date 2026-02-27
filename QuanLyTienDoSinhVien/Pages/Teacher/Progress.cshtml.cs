using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using QuanLyTienDoSinhVien.Data;
using QuanLyTienDoSinhVien.Models;
using System.Security.Claims;
using System.Text.Json;

namespace QuanLyTienDoSinhVien.Pages.Teacher
{
    [Authorize(Roles = "Teacher")]
    public class ProgressModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public ProgressModel(ApplicationDbContext context) { _context = context; }

        public List<ClassProgressInfo> ClassProgresses { get; set; } = new();
        public int? SelectedClassId { get; set; }
        public List<Class> AssignedClasses { get; set; } = new();
        public string ChartLabelsJson { get; set; } = "[]";
        public string ChartAvgJson { get; set; } = "[]";

        public async Task<IActionResult> OnGetAsync(int? classId)
        {
            var lecturer = await GetCurrentLecturerAsync();
            if (lecturer == null) return RedirectToPage("/Auth/Login");

            SelectedClassId = classId;

            var assignedClassIds = await _context.LecturerAssignments
                .Where(la => la.LecturerId == lecturer.Id)
                .Select(la => la.ClassId).Distinct().ToListAsync();

            AssignedClasses = await _context.Classes
                .Where(c => assignedClassIds.Contains(c.Id)).ToListAsync();

            var classFilter = classId.HasValue ? new List<int> { classId.Value } : assignedClassIds;

            var students = await _context.Students
                .Include(s => s.Class)
                .Include(s => s.Enrollments).ThenInclude(e => e.Subject)
                .Include(s => s.Enrollments).ThenInclude(e => e.StudyProgresses)
                .Where(s => s.ClassId != null && classFilter.Contains(s.ClassId.Value))
                .ToListAsync();

            var labels = new List<string>();
            var avgs = new List<double>();

            foreach (var s in students)
            {
                var scored = s.Enrollments.SelectMany(e => e.StudyProgresses).Where(p => p.Score.HasValue).ToList();
                var avgScore = scored.Any() ? scored.Average(p => p.Score!.Value) : 0;
                var totalEnr = s.Enrollments.Count;
                var completed = s.Enrollments.Count(e => e.Status == "Completed");

                ClassProgresses.Add(new ClassProgressInfo
                {
                    StudentCode = s.StudentCode,
                    FullName = s.FullName ?? "N/A",
                    ClassName = s.Class?.Name ?? "N/A",
                    TotalSubjects = totalEnr,
                    CompletedSubjects = completed,
                    AvgScore = avgScore,
                    CompletionPercent = totalEnr > 0 ? (completed * 100 / totalEnr) : 0
                });

                labels.Add(s.StudentCode);
                avgs.Add(Math.Round(avgScore, 2));
            }

            ChartLabelsJson = JsonSerializer.Serialize(labels);
            ChartAvgJson = JsonSerializer.Serialize(avgs);
            return Page();
        }

        private async Task<Lecturer?> GetCurrentLecturerAsync()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return null;
            var userId = int.Parse(userIdClaim);
            return await _context.Lecturers.FirstOrDefaultAsync(l => l.UserId == userId);
        }

        public class ClassProgressInfo
        {
            public string StudentCode { get; set; } = "";
            public string FullName { get; set; } = "";
            public string ClassName { get; set; } = "";
            public int TotalSubjects { get; set; }
            public int CompletedSubjects { get; set; }
            public double AvgScore { get; set; }
            public int CompletionPercent { get; set; }
        }
    }
}
