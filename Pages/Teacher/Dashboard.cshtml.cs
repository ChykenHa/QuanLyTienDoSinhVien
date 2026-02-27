using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using QuanLyTienDoSinhVien.Data;
using QuanLyTienDoSinhVien.Models;
using System.Collections.Generic;
using System;

namespace QuanLyTienDoSinhVien.Pages.Teacher
{
    [Authorize(Roles = "Teacher")]
    public class DashboardModel : PageModel
    {
        private readonly ApplicationDbContext dbContext;
        public DashboardModel(ApplicationDbContext context)
        {
            dbContext = context;
        }

        public List<StudyPlan> StudentsWaiting { get; set; } = new();
        public StudyPlan? SelectedPlan { get; set; }

        [BindProperty]
        public string TeacherComment { get; set; } = "";

        public async Task OnGetAsync(int? id)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return;

            int userId = int.Parse(userIdClaim);

            // Load all pending study plans with student info
            StudentsWaiting = await dbContext.StudyPlans
                .Where(p => p.Status == "Pending")
                .Include(p => p.Student)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            if (id.HasValue)
            {
                SelectedPlan = await dbContext.StudyPlans
                    .Include(p => p.Student)
                    .Include(p => p.StudyPlanReviews)
                        .ThenInclude(r => r.Lecturer)
                    .FirstOrDefaultAsync(p => p.Id == id);
            }
        }

        public async Task<IActionResult> OnPostApproveAsync(int id, string newStatus)
        {
            var plan = await dbContext.StudyPlans.FindAsync(id);
            if (plan != null)
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!string.IsNullOrEmpty(userIdClaim))
                {
                    int userId = int.Parse(userIdClaim);

                    // Find the lecturer record by UserId
                    var lecturer = await dbContext.Lecturers
                        .FirstOrDefaultAsync(l => l.UserId == userId);

                    if (lecturer != null)
                    {
                        // Map numeric status to text
                        plan.Status = newStatus switch
                        {
                            "2" => "Approved",
                            "3" => "NeedsRevision",
                            _ => plan.Status
                        };

                        var review = new StudyPlanReview
                        {
                            StudyPlanId = id,
                            LecturerId = lecturer.Id,
                            Comment = TeacherComment,
                            ReviewedAt = DateTime.Now
                        };
                        dbContext.StudyPlanReviews.Add(review);
                        await dbContext.SaveChangesAsync();
                    }
                }
            }
            return RedirectToPage(new { id });
        }
    }
}