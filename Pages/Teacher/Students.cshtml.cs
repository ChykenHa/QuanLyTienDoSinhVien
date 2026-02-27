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
    public class StudentsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public StudentsModel(ApplicationDbContext context) { _context = context; }

        public List<StudentInfo> StudentList { get; set; } = new();
        public string? SearchQuery { get; set; }
        public Lecturer? CurrentLecturer { get; set; }

        public async Task<IActionResult> OnGetAsync(string? search)
        {
            var lecturer = await GetCurrentLecturerAsync();
            if (lecturer == null) return RedirectToPage("/Auth/Login");
            CurrentLecturer = lecturer;
            SearchQuery = search;

            // Get classes assigned to this lecturer
            var assignedClassIds = await _context.LecturerAssignments
                .Where(la => la.LecturerId == lecturer.Id)
                .Select(la => la.ClassId)
                .Distinct()
                .ToListAsync();

            var query = _context.Students
                .Include(s => s.Class).ThenInclude(c => c!.Major)
                .Where(s => s.ClassId != null && assignedClassIds.Contains(s.ClassId.Value));

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(s =>
                    (s.FullName != null && s.FullName.Contains(search)) ||
                    s.StudentCode.Contains(search));
            }

            StudentList = await query.OrderBy(s => s.Class!.Name).ThenBy(s => s.FullName)
                .Select(s => new StudentInfo
                {
                    Id = s.Id,
                    StudentCode = s.StudentCode,
                    FullName = s.FullName ?? "N/A",
                    Email = s.Email,
                    Phone = s.Phone,
                    ClassName = s.Class!.Name,
                    MajorName = s.Class.Major.Name
                }).ToListAsync();

            return Page();
        }

        private async Task<Lecturer?> GetCurrentLecturerAsync()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return null;
            var userId = int.Parse(userIdClaim);
            return await _context.Lecturers.FirstOrDefaultAsync(l => l.UserId == userId);
        }

        public class StudentInfo
        {
            public int Id { get; set; }
            public string StudentCode { get; set; } = "";
            public string FullName { get; set; } = "";
            public string? Email { get; set; }
            public string? Phone { get; set; }
            public string ClassName { get; set; } = "";
            public string MajorName { get; set; } = "";
        }
    }
}
