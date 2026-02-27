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
    public class NotificationsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public NotificationsModel(ApplicationDbContext context) { _context = context; }

        public List<StudentOption> StudentOptions { get; set; } = new();
        public List<Notification> SentNotifications { get; set; } = new();
        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var lecturer = await GetCurrentLecturerAsync();
            if (lecturer == null) return RedirectToPage("/Auth/Login");
            await LoadDataAsync(lecturer);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int[] studentUserIds, string content)
        {
            var lecturer = await GetCurrentLecturerAsync();
            if (lecturer == null) return RedirectToPage("/Auth/Login");

            if (studentUserIds == null || studentUserIds.Length == 0 || string.IsNullOrWhiteSpace(content))
            {
                ErrorMessage = "Vui lòng chọn sinh viên và nhập nội dung thông báo.";
                await LoadDataAsync(lecturer);
                return Page();
            }

            foreach (var userId in studentUserIds)
            {
                _context.Notifications.Add(new Notification
                {
                    UserId = userId,
                    Content = content,
                    IsRead = false,
                    CreatedAt = DateTime.Now
                });
            }
            await _context.SaveChangesAsync();

            SuccessMessage = $"Đã gửi thông báo đến {studentUserIds.Length} sinh viên!";
            await LoadDataAsync(lecturer);
            return Page();
        }

        private async Task LoadDataAsync(Lecturer lecturer)
        {
            var assignedClassIds = await _context.LecturerAssignments
                .Where(la => la.LecturerId == lecturer.Id)
                .Select(la => la.ClassId).Distinct().ToListAsync();

            StudentOptions = await _context.Students
                .Include(s => s.Class)
                .Where(s => s.ClassId != null && assignedClassIds.Contains(s.ClassId.Value))
                .Select(s => new StudentOption
                {
                    UserId = s.UserId,
                    DisplayName = s.StudentCode + " - " + (s.FullName ?? "N/A") + " (" + (s.Class != null ? s.Class.Name : "") + ")"
                }).ToListAsync();

            // Show recent notifications sent by looking at student user IDs
            var studentUserIdsList = StudentOptions.Select(s => s.UserId).ToList();
            SentNotifications = await _context.Notifications
                .Where(n => studentUserIdsList.Contains(n.UserId))
                .OrderByDescending(n => n.CreatedAt)
                .Take(20).ToListAsync();
        }

        private async Task<Lecturer?> GetCurrentLecturerAsync()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return null;
            var userId = int.Parse(userIdClaim);
            return await _context.Lecturers.FirstOrDefaultAsync(l => l.UserId == userId);
        }

        public class StudentOption
        {
            public int UserId { get; set; }
            public string DisplayName { get; set; } = "";
        }
    }
}
