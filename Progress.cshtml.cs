using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using QuanLyTienDoSinhVien.Data;
using QuanLyTienDoSinhVien.Models;
using System.Security.Claims;

namespace QuanLyTienDoSinhVien.Pages.Student
{
    // DTO for GPA trend by semester
    public class GPATrendDto
    {
        public string SemesterName { get; set; } = null!;
        public decimal GPA { get; set; }
        public DateOnly? StartDate { get; set; }
    }

    // DTO for grade distribution (A, B, C, D, F)
    public class GradeDistributionDto
    {
        public string Grade { get; set; } = null!;
        public int Count { get; set; }
        public string Color { get; set; } = null!;
    }

    // DTO for enrollment details to simplify data presentation
    public class EnrollmentDetailDto
    {
        public int Id { get; set; }
        public string SubjectName { get; set; } = null!;
        public string SubjectCode { get; set; } = null!;
        public double Score { get; set; } // -1 means no score yet, >= 0 is valid score
        public int? CompletionPercent { get; set; }
        public int Credits { get; set; }
        public string? Status { get; set; }
        public string? SemesterName { get; set; }
        public int SemesterId { get; set; }
        public string LecturerNames { get; set; } = "N/A"; // Lecturer names comma-separated
    }

    [Authorize(Roles = "Student")]
    public class ProgressModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ProgressModel(ApplicationDbContext context)
        {
            _context = context;
        }

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
        public Models.Student? CurrentStudent { get; set; }

        // Statistics
        public decimal GPA { get; set; }
        public int CompletionRate { get; set; }
        public int CumulativeCredits { get; set; }
        public int StudyPlanStatus { get; set; } // % of Approved/Completed Study Plans
        public string StudentRank { get; set; } = "Không xếp hạng"; // Student ranking based on GPA
        public string CurrentSortOrder { get; set; } = "desc"; // desc or asc for score sorting

        // GPA Trend & Grade Distribution
        public List<GPATrendDto> GPATrendData { get; set; } = new();
        public List<GradeDistributionDto> GradeDistribution { get; set; } = new();

        // Semester Filter
        public List<Semester> AvailableSemesters { get; set; } = new();
        public int? SelectedSemesterId { get; set; }
        public bool IsFilteredBySemester { get; set; } = false;

        // Collections
        public List<EnrollmentDetailDto> EnrollmentDetails { get; set; } = new();
        public List<StudyPlan> StudyPlans { get; set; } = new();

        public async Task OnGetAsync()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return;

            var userId = int.Parse(userIdClaim);
            
            // Get sort order from query string (optional)
            var sortParam = Request.Query["sort"].ToString();
            if (!string.IsNullOrEmpty(sortParam) && (sortParam == "asc" || sortParam == "desc"))
            {
                CurrentSortOrder = sortParam;
            }
            
            // Get selected semester from query string (optional)
            var semesterParam = Request.Query["semester"].ToString();
            if (!string.IsNullOrEmpty(semesterParam) && int.TryParse(semesterParam, out int semesterId))
            {
                SelectedSemesterId = semesterId;
                IsFilteredBySemester = true;
            }
            
            // Get current student
            CurrentStudent = await _context.Students
                .Include(s => s.Class)
                .ThenInclude(c => c!.Major)
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (CurrentStudent != null)
            {
                // Map student info
                StudentCode = CurrentStudent.StudentCode;
                FullName = CurrentStudent.FullName;
                Email = CurrentStudent.Email;
                Phone = CurrentStudent.Phone;
                Address = CurrentStudent.Address;
                ClassId = CurrentStudent.ClassId;
                IsPrivate = CurrentStudent.IsPrivate;
                Class = CurrentStudent.Class;
                UserId = userId;

                // Get ALL enrollments with full details (not just top 10)
                var allEnrollments = await _context.Enrollments
                    .Include(e => e.Subject)
                    .Include(e => e.Semester)
                    .Include(e => e.StudyProgresses)
                    .Where(e => e.StudentId == CurrentStudent.Id)
                    .OrderByDescending(e => e.Semester!.StartDate)
                    .ToListAsync();

                // Load LecturerAssignments for subjects
                var lecturerAssignments = await _context.LecturerAssignments
                    .Include(la => la.Lecturer)
                    .Where(la => la.ClassId == CurrentStudent.ClassId)
                    .ToListAsync();

                // DEBUG: Log the number of enrollments
                System.Diagnostics.Debug.WriteLine($"Student {CurrentStudent.StudentCode}: Found {allEnrollments.Count} enrollments");
                foreach (var enr in allEnrollments)
                {
                    System.Diagnostics.Debug.WriteLine($"  - Subject: {enr.Subject?.Name ?? "NULL"}, Semester: {enr.Semester?.Name ?? "NULL"}");
                }

                // Calculate Weighted GPA from all completed enrollments
                var enrollmentsWithScores = allEnrollments
                    .Where(e => e.StudyProgresses.Any(sp => sp.Score.HasValue) && e.Subject != null)
                    .ToList();

                if (enrollmentsWithScores.Any())    
                {
                    // Weighted GPA: (Score * Credit) / Total Credits
                    decimal totalScoreCredit = 0;
                    int totalCredit = 0;

                    foreach (var enrollment in enrollmentsWithScores)
                    {
                        var score = enrollment.StudyProgresses.FirstOrDefault()?.Score ?? 0;
                        var credit = enrollment.Subject!.Credit;
                        totalScoreCredit += (decimal)score * credit;
                        totalCredit += credit;
                    }

                    GPA = totalCredit > 0 ? Math.Round(totalScoreCredit / totalCredit, 2) : 0;
                }

                // Calculate completion rate from all enrollments
                var totalEnrollments = allEnrollments.Count;
                var completedCount = allEnrollments.Count(e => 
                    !string.IsNullOrEmpty(e.Status) && e.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase));
                CompletionRate = totalEnrollments > 0 ? (completedCount * 100 / totalEnrollments) : 0;

                System.Diagnostics.Debug.WriteLine($"Completion Rate: {completedCount}/{totalEnrollments} = {CompletionRate}%");

                // Calculate cumulative credits (sum of credits from completed enrollments)
                CumulativeCredits = allEnrollments
                    .Where(e => !string.IsNullOrEmpty(e.Status) && 
                                e.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase) && 
                                e.Subject != null)
                    .Sum(e => e.Subject!.Credit);

                System.Diagnostics.Debug.WriteLine($"Cumulative Credits: {CumulativeCredits}");

                // Build enrollment detail list (limit to 10 for display)
                // Filter out enrollments with null Subject or Semester
                var enrollmentList = allEnrollments
                    .Where(e => e.Subject != null && e.Semester != null) // Safe null check
                    .Select(e => {
                        // Get lecturer names for this subject
                        var lecturersForSubject = lecturerAssignments
                            .Where(la => la.SubjectId == e.SubjectId)
                            .Select(la => la.Lecturer.FullName ?? "Unknown")
                            .ToList();
                        var lecturerNames = lecturersForSubject.Any() ? string.Join(", ", lecturersForSubject) : "N/A";

                        return new EnrollmentDetailDto
                        {
                            Id = e.Id,
                            SubjectName = e.Subject!.Name ?? "Unknown Subject",
                            SubjectCode = e.Subject.Code ?? "N/A",
                            Score = e.StudyProgresses.FirstOrDefault()?.Score ?? -1, // -1 means no score yet
                            CompletionPercent = e.StudyProgresses.FirstOrDefault()?.CompletionPercent ?? 0,
                            Credits = e.Subject.Credit,
                            Status = e.Status ?? "Unknown",
                            SemesterName = e.Semester!.Name ?? "Unknown Semester",
                            SemesterId = e.SemesterId,
                            LecturerNames = lecturerNames
                        };
                    })
                    .ToList();

                // Get available semesters from student's enrollments
                AvailableSemesters = await _context.Semesters
                    .Where(s => allEnrollments.Select(e => e.SemesterId).Contains(s.Id))
                    .OrderByDescending(s => s.StartDate)
                    .ToListAsync();

                // Apply semester filter if selected
                if (IsFilteredBySemester && SelectedSemesterId.HasValue)
                {
                    enrollmentList = enrollmentList
                        .Where(ed => ed.SemesterId == SelectedSemesterId.Value)
                        .ToList();
                    System.Diagnostics.Debug.WriteLine($"Filtered by semester {SelectedSemesterId}: {enrollmentList.Count} enrollments");
                }

                // Apply sorting by score
                if (CurrentSortOrder == "asc")
                {
                    EnrollmentDetails = enrollmentList
                        .OrderBy(x => x.Score >= 0 ? x.Score : double.MaxValue) // Put N/A at end
                        .Take(10)
                        .ToList();
                }
                else // desc (default)
                {
                    EnrollmentDetails = enrollmentList
                        .OrderByDescending(x => x.Score >= 0 ? x.Score : -1) // Put N/A at end
                        .Take(10)
                        .ToList();
                }

                System.Diagnostics.Debug.WriteLine($"After filtering & mapping: {EnrollmentDetails.Count} enrollments to display (Sorted: {CurrentSortOrder})");

                // ===== FEATURE 1: Calculate GPA Trend by Semester =====
                var semesters = AvailableSemesters.OrderBy(s => s.StartDate).ToList();
                foreach (var semester in semesters)
                {
                    var semesterEnrollments = allEnrollments
                        .Where(e => e.SemesterId == semester.Id && 
                                    e.StudyProgresses.Any(sp => sp.Score.HasValue) &&
                                    e.Subject != null)
                        .ToList();

                    if (semesterEnrollments.Any())
                    {
                        decimal semesterTotalScoreCredit = 0;
                        int semesterTotalCredit = 0;

                        foreach (var e in semesterEnrollments)
                        {
                            var score = e.StudyProgresses.FirstOrDefault()?.Score ?? 0;
                            var credit = e.Subject!.Credit;
                            semesterTotalScoreCredit += (decimal)score * credit;
                            semesterTotalCredit += credit;
                        }

                        decimal semesterGPA = semesterTotalCredit > 0 
                            ? Math.Round(semesterTotalScoreCredit / semesterTotalCredit, 2) 
                            : 0;

                        GPATrendData.Add(new GPATrendDto
                        {
                            SemesterName = semester.Name ?? $"Semester {semester.Id}",
                            GPA = semesterGPA,
                            StartDate = semester.StartDate
                        });
                    }
                }

                System.Diagnostics.Debug.WriteLine($"GPA Trend Data: {GPATrendData.Count} semesters");

                // ===== FEATURE 2: Calculate Grade Distribution =====
                var allScores = allEnrollments
                    .Where(e => e.StudyProgresses.Any(sp => sp.Score.HasValue))
                    .Select(e => e.StudyProgresses.FirstOrDefault()?.Score ?? 0)
                    .ToList();

                if (allScores.Any())
                {
                    var gradeA = allScores.Count(s => s >= 8.0); // A: 8-10
                    var gradeB = allScores.Count(s => s >= 7.0 && s < 8.0); // B: 7-8
                    var gradeC = allScores.Count(s => s >= 6.0 && s < 7.0); // C: 6-7
                    var gradeD = allScores.Count(s => s >= 5.0 && s < 6.0); // D: 5-6
                    var gradeF = allScores.Count(s => s < 5.0); // F: <5

                    GradeDistribution = new List<GradeDistributionDto>
                    {
                        new() { Grade = "A (8-10)", Count = gradeA, Color = "#10b981" }, // Green
                        new() { Grade = "B (7-8)", Count = gradeB, Color = "#3b82f6" }, // Blue
                        new() { Grade = "C (6-7)", Count = gradeC, Color = "#f59e0b" }, // Amber
                        new() { Grade = "D (5-6)", Count = gradeD, Color = "#f97316" }, // Orange
                        new() { Grade = "F (<5)", Count = gradeF, Color = "#ef4444" } // Red
                    };

                    System.Diagnostics.Debug.WriteLine($"Grade Distribution: A={gradeA}, B={gradeB}, C={gradeC}, D={gradeD}, F={gradeF}");
                }

                // Get study plans
                var allStudyPlans = await _context.StudyPlans
                    .Include(sp => sp.StudyPlanReviews)
                    .Where(sp => sp.StudentId == CurrentStudent.Id)
                    .OrderByDescending(sp => sp.CreatedAt)
                    .ToListAsync();

                StudyPlans = allStudyPlans.Take(5).ToList();

                // Calculate Study Plan Status (% of Approved or Completed study plans)
                var totalStudyPlans = allStudyPlans.Count;
                var approvedOrCompletedPlans = allStudyPlans
                    .Count(sp => !string.IsNullOrEmpty(sp.Status) && 
                                 (sp.Status.Equals("Approved", StringComparison.OrdinalIgnoreCase) || 
                                  sp.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase)));
                StudyPlanStatus = totalStudyPlans > 0 ? (approvedOrCompletedPlans * 100 / totalStudyPlans) : 0;

                System.Diagnostics.Debug.WriteLine($"Study Plan Status: {approvedOrCompletedPlans}/{totalStudyPlans} = {StudyPlanStatus}%");

                // Calculate Student Classification based on GPA
                if (GPA >= 3.6m)
                    StudentRank = "Xuất sắc";
                else if (GPA >= 3.2m)
                    StudentRank = "Giỏi";
                else if (GPA >= 2.8m)
                    StudentRank = "Khá";
                else if (GPA >= 2.0m)
                    StudentRank = "Trung bình";
                else
                    StudentRank = "Yếu";
            }
        }
    }
}
