using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using QuanLyTienDoSinhVien.Data;
using QuanLyTienDoSinhVien.Models;

namespace QuanLyTienDoSinhVien.Pages.Admin.Lecturers
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public IndexModel(ApplicationDbContext context) { _context = context; }

        public List<LecturerAssignment> Assignments { get; set; } = new();
        public List<Lecturer> AllLecturers { get; set; } = new();
        public List<Subject> AllSubjects { get; set; } = new();
        public List<Class> AllClasses { get; set; } = new();
        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            await LoadDataAsync();
        }

        public async Task<IActionResult> OnPostAddAsync(int lecturerId, int subjectId, int classId)
        {
            var exists = await _context.LecturerAssignments.AnyAsync(la =>
                la.LecturerId == lecturerId && la.SubjectId == subjectId && la.ClassId == classId);
            if (exists) { ErrorMessage = "Phân công này đã tồn tại."; await LoadDataAsync(); return Page(); }

            _context.LecturerAssignments.Add(new LecturerAssignment { LecturerId = lecturerId, SubjectId = subjectId, ClassId = classId });
            await _context.SaveChangesAsync();
            SuccessMessage = "Đã thêm phân công!";
            await LoadDataAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var la = await _context.LecturerAssignments.FindAsync(id);
            if (la != null) { _context.LecturerAssignments.Remove(la); await _context.SaveChangesAsync(); SuccessMessage = "Đã xóa phân công."; }
            await LoadDataAsync();
            return Page();
        }

        private async Task LoadDataAsync()
        {
            Assignments = await _context.LecturerAssignments
                .Include(la => la.Lecturer).Include(la => la.Subject).Include(la => la.Class)
                .OrderBy(la => la.Lecturer.FullName).ToListAsync();
            AllLecturers = await _context.Lecturers.OrderBy(l => l.FullName).ToListAsync();
            AllSubjects = await _context.Subjects.OrderBy(s => s.Code).ToListAsync();
            AllClasses = await _context.Classes.OrderBy(c => c.Name).ToListAsync();
        }
    }
}
