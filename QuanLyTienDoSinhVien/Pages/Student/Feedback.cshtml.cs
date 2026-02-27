using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using QuanLyTienDoSinhVien.Data;
using QuanLyTienDoSinhVien.Models;
using System.Security.Claims;

namespace QuanLyTienDoSinhVien.Pages.Student
{
    [Authorize(Roles = "Student")]
    public class FeedbackModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public FeedbackModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public Models.Student? CurrentStudent { get; set; }
        public List<ReviewInfo> Reviews { get; set; } = new();
        public List<Notification> Notifications { get; set; } = new();
        public int UnreadCount { get; set; }
        public string? SuccessMessage { get; set; }
        public string ActiveTab { get; set; } = "reviews";

        public async Task<IActionResult> OnGetAsync(string? tab)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return RedirectToPage("/Auth/Login");

            var userId = int.Parse(userIdClaim);
            CurrentStudent = await _context.Students.FirstOrDefaultAsync(s => s.UserId == userId);
            if (CurrentStudent == null) return RedirectToPage("/Auth/Login");

            ActiveTab = tab ?? "reviews";

            // Load reviews through StudyPlans
            var studyPlans = await _context.StudyPlans
                .Include(sp => sp.StudyPlanReviews)
                    .ThenInclude(r => r.Lecturer)
                .Where(sp => sp.StudentId == CurrentStudent.Id)
                .ToListAsync();

            Reviews = studyPlans
                .SelectMany(sp => sp.StudyPlanReviews.Select(r => new ReviewInfo
                {
                    PlanId = sp.Id,
                    PlanStatus = sp.Status ?? "Unknown",
                    LecturerName = r.Lecturer.FullName ?? "N/A",
                    Comment = r.Comment ?? "",
                    ReviewedAt = r.ReviewedAt
                }))
                .OrderByDescending(r => r.ReviewedAt)
                .ToList();

            // Load notifications
            Notifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            UnreadCount = Notifications.Count(n => n.IsRead != true);

            return Page();
        }

        public async Task<IActionResult> OnPostMarkReadAsync(int notificationId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return RedirectToPage("/Auth/Login");

            var userId = int.Parse(userIdClaim);
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

            if (notification != null)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
            }

            return RedirectToPage(new { tab = "notifications" });
        }

        public async Task<IActionResult> OnPostMarkAllReadAsync()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return RedirectToPage("/Auth/Login");

            var userId = int.Parse(userIdClaim);
            var unread = await _context.Notifications
                .Where(n => n.UserId == userId && n.IsRead != true)
                .ToListAsync();

            foreach (var n in unread)
            {
                n.IsRead = true;
            }
            await _context.SaveChangesAsync();

            return RedirectToPage(new { tab = "notifications" });
        }

        public class ReviewInfo
        {
            public int PlanId { get; set; }
            public string PlanStatus { get; set; } = "";
            public string LecturerName { get; set; } = "";
            public string Comment { get; set; } = "";
            public DateTime? ReviewedAt { get; set; }
        }
    }
}
