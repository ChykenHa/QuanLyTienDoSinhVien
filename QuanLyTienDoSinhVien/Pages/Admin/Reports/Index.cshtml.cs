using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using QuanLyTienDoSinhVien.Data;
using System.Text.Json;

namespace QuanLyTienDoSinhVien.Pages.Admin.Reports
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public IndexModel(ApplicationDbContext context) { _context = context; }

        public int TotalStudents { get; set; }
        public int TotalLecturers { get; set; }
        public int TotalClasses { get; set; }
        public int TotalMajors { get; set; }
        public int TotalSubjects { get; set; }
        public int TotalEnrollments { get; set; }
        public int CompletedEnrollments { get; set; }
        public double AvgGpa { get; set; }

        // Chart data
        public string MajorLabelsJson { get; set; } = "[]";
        public string MajorCountsJson { get; set; } = "[]";
        public string ScoreDistLabelsJson { get; set; } = "[]";
        public string ScoreDistCountsJson { get; set; } = "[]";

        public List<ClassReport> ClassReports { get; set; } = new();

        public async Task OnGetAsync()
        {
            TotalStudents = await _context.Students.CountAsync();
            TotalLecturers = await _context.Lecturers.CountAsync();
            TotalClasses = await _context.Classes.CountAsync();
            TotalMajors = await _context.Majors.CountAsync();
            TotalSubjects = await _context.Subjects.CountAsync();
            TotalEnrollments = await _context.Enrollments.CountAsync();
            CompletedEnrollments = await _context.Enrollments.CountAsync(e => e.Status == "Completed");

            // Average GPA
            var scores = await _context.StudyProgresses.Where(sp => sp.Score.HasValue).Select(sp => sp.Score!.Value).ToListAsync();
            AvgGpa = scores.Any() ? scores.Average() : 0;

            // Students by major
            var majorData = await _context.Students
                .Include(s => s.Class).ThenInclude(c => c!.Major)
                .Where(s => s.Class != null && s.Class.Major != null)
                .GroupBy(s => s.Class!.Major.Name)
                .Select(g => new { Name = g.Key, Count = g.Count() }).ToListAsync();
            MajorLabelsJson = JsonSerializer.Serialize(majorData.Select(m => m.Name ?? "N/A"));
            MajorCountsJson = JsonSerializer.Serialize(majorData.Select(m => m.Count));

            // Score distribution
            var allScores = await _context.StudyProgresses.Where(sp => sp.Score.HasValue).Select(sp => sp.Score!.Value).ToListAsync();
            var ranges = new[] { "0-2", "2-4", "4-5", "5-6.5", "6.5-8", "8-10" };
            var limits = new[] { (0.0, 2.0), (2.0, 4.0), (4.0, 5.0), (5.0, 6.5), (6.5, 8.0), (8.0, 10.01) };
            var dist = limits.Select(l => allScores.Count(s => s >= l.Item1 && s < l.Item2)).ToList();
            ScoreDistLabelsJson = JsonSerializer.Serialize(ranges);
            ScoreDistCountsJson = JsonSerializer.Serialize(dist);

            // Class reports
            ClassReports = await _context.Classes
                .Include(c => c.Major).Include(c => c.Students)
                .Select(c => new ClassReport
                {
                    ClassName = c.Name,
                    MajorName = c.Major.Name ?? "N/A",
                    StudentCount = c.Students.Count
                }).ToListAsync();
        }

        public class ClassReport
        {
            public string ClassName { get; set; } = "";
            public string MajorName { get; set; } = "";
            public int StudentCount { get; set; }
        }
    }
}
