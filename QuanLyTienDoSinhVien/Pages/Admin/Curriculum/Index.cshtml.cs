using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using QuanLyTienDoSinhVien.Data;
using QuanLyTienDoSinhVien.Models;

namespace QuanLyTienDoSinhVien.Pages.Admin.Curriculum
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public IndexModel(ApplicationDbContext context) { _context = context; }

        public List<MajorCurriculum> Curriculums { get; set; } = new();
        public List<Major> Majors { get; set; } = new();
        public List<Subject> AllSubjects { get; set; } = new();
        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            await LoadDataAsync();
        }

        public async Task<IActionResult> OnPostAddAsync(int majorId, int subjectId, int? semesterOrder, bool isRequired)
        {
            var exists = await _context.MajorSubjects
                .AnyAsync(ms => ms.MajorId == majorId && ms.SubjectId == subjectId);
            if (exists)
            {
                ErrorMessage = "Môn học này đã có trong chương trình đào tạo.";
                await LoadDataAsync();
                return Page();
            }

            _context.MajorSubjects.Add(new MajorSubject
            {
                MajorId = majorId,
                SubjectId = subjectId,
                SemesterOrder = semesterOrder,
                IsRequired = isRequired
            });
            await _context.SaveChangesAsync();
            SuccessMessage = "Đã thêm môn học vào chương trình đào tạo!";
            await LoadDataAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var ms = await _context.MajorSubjects.FindAsync(id);
            if (ms != null)
            {
                _context.MajorSubjects.Remove(ms);
                await _context.SaveChangesAsync();
                SuccessMessage = "Đã xóa khỏi chương trình đào tạo.";
            }
            await LoadDataAsync();
            return Page();
        }

        private async Task LoadDataAsync()
        {
            Majors = await _context.Majors.ToListAsync();
            AllSubjects = await _context.Subjects.OrderBy(s => s.Code).ToListAsync();

            var majorSubjects = await _context.MajorSubjects
                .Include(ms => ms.Major).Include(ms => ms.Subject)
                .OrderBy(ms => ms.MajorId).ThenBy(ms => ms.SemesterOrder)
                .ToListAsync();

            Curriculums = Majors.Select(m => new MajorCurriculum
            {
                Major = m,
                Subjects = majorSubjects
                    .Where(ms => ms.MajorId == m.Id)
                    .Select(ms => new CurriculumSubject
                    {
                        Id = ms.Id,
                        SubjectCode = ms.Subject.Code,
                        SubjectName = ms.Subject.Name,
                        Credit = ms.Subject.Credit,
                        SemesterOrder = ms.SemesterOrder,
                        IsRequired = ms.IsRequired ?? true
                    }).ToList()
            }).ToList();
        }

        public class MajorCurriculum
        {
            public Major Major { get; set; } = null!;
            public List<CurriculumSubject> Subjects { get; set; } = new();
        }

        public class CurriculumSubject
        {
            public int Id { get; set; }
            public string SubjectCode { get; set; } = "";
            public string SubjectName { get; set; } = "";
            public int Credit { get; set; }
            public int? SemesterOrder { get; set; }
            public bool IsRequired { get; set; }
        }
    }
}
