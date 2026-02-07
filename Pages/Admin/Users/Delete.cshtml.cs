using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using QuanLyTienDoSinhVien.Data;
using QuanLyTienDoSinhVien.Models;

namespace QuanLyTienDoSinhVien.Pages.Admin.Users
{
    [Authorize(Roles = "Admin")]
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DeleteModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public int UserId { get; set; }

        public User? User { get; set; }
        public Models.Student? StudentInfo { get; set; }
        public Lecturer? LecturerInfo { get; set; }

        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            User = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (User == null)
            {
                return NotFound();
            }

            // Prevent deleting the default admin account
            if (User.Username == "admin")
            {
                ErrorMessage = "Không thể xóa tài khoản Admin mặc định.";
                return Page();
            }

            UserId = User.Id;

            // Load role-specific info
            if (User.Role.Name == "Student")
            {
                StudentInfo = await _context.Students
                    .FirstOrDefaultAsync(s => s.UserId == User.Id);
            }
            else if (User.Role.Name == "Lecturer" || User.Role.Name == "Teacher")
            {
                LecturerInfo = await _context.Lecturers
                    .FirstOrDefaultAsync(l => l.UserId == User.Id);
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == UserId);

            if (user == null)
            {
                return NotFound();
            }

            // Prevent deleting the default admin account
            if (user.Username == "admin")
            {
                TempData["ErrorMessage"] = "Không thể xóa tài khoản Admin mặc định.";
                return RedirectToPage("./Index");
            }

            try
            {
                // Delete role-specific records first (due to foreign key constraints)
                if (user.Role.Name == "Student")
                {
                    var student = await _context.Students
                        .FirstOrDefaultAsync(s => s.UserId == user.Id);

                    if (student != null)
                    {
                        // Delete related records
                        var violations = await _context.Violations
                            .Where(v => v.StudentId == student.Id)
                            .ToListAsync();
                        _context.Violations.RemoveRange(violations);

                        var studyPlans = await _context.StudyPlans
                            .Include(sp => sp.StudyPlanDetails)
                            .Include(sp => sp.StudyPlanReviews)
                            .Where(sp => sp.StudentId == student.Id)
                            .ToListAsync();

                        foreach (var plan in studyPlans)
                        {
                            _context.StudyPlanDetails.RemoveRange(plan.StudyPlanDetails);
                            _context.StudyPlanReviews.RemoveRange(plan.StudyPlanReviews);
                        }
                        _context.StudyPlans.RemoveRange(studyPlans);

                        var enrollments = await _context.Enrollments
                            .Include(e => e.StudyProgresses)
                            .Where(e => e.StudentId == student.Id)
                            .ToListAsync();

                        foreach (var enrollment in enrollments)
                        {
                            _context.StudyProgresses.RemoveRange(enrollment.StudyProgresses);
                        }
                        _context.Enrollments.RemoveRange(enrollments);

                        _context.Students.Remove(student);
                    }
                }
                else if (user.Role.Name == "Lecturer" || user.Role.Name == "Teacher")
                {
                    var lecturer = await _context.Lecturers
                        .FirstOrDefaultAsync(l => l.UserId == user.Id);

                    if (lecturer != null)
                    {
                        // Delete related records
                        var assignments = await _context.LecturerAssignments
                            .Where(la => la.LecturerId == lecturer.Id)
                            .ToListAsync();
                        _context.LecturerAssignments.RemoveRange(assignments);

                        var reviews = await _context.StudyPlanReviews
                            .Where(spr => spr.LecturerId == lecturer.Id)
                            .ToListAsync();
                        _context.StudyPlanReviews.RemoveRange(reviews);

                        _context.Lecturers.Remove(lecturer);
                    }
                }

                // Delete notifications
                var notifications = await _context.Notifications
                    .Where(n => n.UserId == user.Id)
                    .ToListAsync();
                _context.Notifications.RemoveRange(notifications);

                // Finally, delete the user
                _context.Users.Remove(user);

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Tài khoản '{user.Username}' đã được xóa thành công.";
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi xóa tài khoản: {ex.Message}";
                return RedirectToPage("./Index");
            }
        }
    }
}
