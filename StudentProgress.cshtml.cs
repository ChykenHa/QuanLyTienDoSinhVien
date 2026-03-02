using Microsoft.AspNetCore.Mvc.RazorPages;
using QuanLyTienDoSinhVien.Data;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace QuanLyTienDoSinhVien.Pages.Teacher
{
    public class StudentProgressModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public StudentProgressModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<StudentViewModel> Students { get; set; } = new();
        public int TotalStudents { get; set; }
        public int NormalCount { get; set; }
        public int WarningCount { get; set; }
        public int DangerCount { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Search { get; set; }
        [BindProperty(SupportsGet = true)]
        public string? ClassFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? MajorFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? StatusFilter { get; set; }

        public List<string> ClassList { get; set; } = new();
        public List<string> MajorList { get; set; } = new();

        public void OnGet()
        {
            ClassList = _context.Classes
                .OrderBy(c => c.Name)
                .Select(c => c.Name)
                .Distinct()
                .ToList();

            MajorList = _context.Majors
                .OrderBy(m => m.Name)
                .Select(m => m.Name)
                .Distinct()
                .ToList();

            var studentsQuery = from s in _context.Students
                                join c in _context.Classes on s.ClassId equals c.Id
                                join m in _context.Majors on c.MajorId equals m.Id
                                select new
                                {
                                    Student = s,
                                    ClassName = c.Name,
                                    MajorName = m.Name
                                };

            if (!string.IsNullOrEmpty(Search))
            {
                studentsQuery = studentsQuery
                    .Where(x => x.Student.FullName.Contains(Search)
                            || x.Student.StudentCode.Contains(Search));
            }

            if (!string.IsNullOrEmpty(ClassFilter))
                {
                    studentsQuery = studentsQuery
                        .Where(x => x.ClassName == ClassFilter);
                }

                if (!string.IsNullOrEmpty(MajorFilter))
                {
                    studentsQuery = studentsQuery
                        .Where(x => x.MajorName == MajorFilter);
                }
            var data = from x in studentsQuery
                    select new StudentViewModel
                    {
                        FullName = x.Student.FullName,
                        StudentCode = x.Student.StudentCode,
                        ClassName = x.ClassName,
                        MajorName = x.MajorName,
                        Courses = (
                            from e in _context.Enrollments
                            join sub in _context.Subjects on e.SubjectId equals sub.Id
                            where e.StudentId == x.Student.Id
                            select new CourseViewModel
                            {
                                SubjectName = sub.Name,
                                Credit = sub.Credit,
                                Score = _context.StudyProgresses
                                            .Where(sp => sp.EnrollmentId == e.Id)
                                            .OrderByDescending(sp => sp.Id)   // lấy record mới nhất
                                            .Select(sp => sp.Score)
                                            .FirstOrDefault(),

                                Completion = _context.StudyProgresses
                                            .Where(sp => sp.EnrollmentId == e.Id)
                                            .OrderByDescending(sp => sp.Id)
                                            .Select(sp => sp.CompletionPercent)
                                            .FirstOrDefault()
                            }
                        ).ToList()
                    };

            Students = data.ToList();

            foreach (var s in Students)
            {
                var totalCredit = s.Courses.Sum(c => c.Credit);
                var totalScore = s.Courses.Sum(c => (c.Score ?? 0) * c.Credit);
                var gpa = totalCredit > 0 ? totalScore / totalCredit : 0;
                s.GPA = Math.Round(gpa, 2);
            }

            if (!string.IsNullOrEmpty(StatusFilter))
            {
                Students = Students.Where(s =>
                    (StatusFilter == "normal" && s.GPA >= 8) ||
                    (StatusFilter == "warning" && s.GPA >= 5 && s.GPA < 8) ||
                    (StatusFilter == "danger" && s.GPA < 5)
                ).ToList();
            }
            
            TotalStudents = Students.Count;
            NormalCount = Students.Count(s => s.GPA >= 8);
            WarningCount = Students.Count(s => s.GPA >= 5 && s.GPA < 8);
            DangerCount = Students.Count(s => s.GPA < 5);
        }
        public class StudentViewModel
        {
            public string FullName { get; set; } = "";
            public string StudentCode { get; set; } = "";
            public string ClassName { get; set; } = "";
            public string MajorName { get; set; } = "";
            public double GPA { get; set; }
            public List<CourseViewModel> Courses { get; set; } = new();
        }

        public class CourseViewModel
        {
            public string SubjectName { get; set; } = "";
            public int Credit { get; set; }
            public double? Score { get; set; }
            public int? Completion { get; set; }
        }
    }
}