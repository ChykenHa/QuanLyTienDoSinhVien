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
        public DashboardModel (ApplicationDbContext context)
        {
            dbContext = context;
        }

        public List<StudyPlan> StudentsWaiting {get;set;} = new();
        public StudyPlan? SelectedPlan {get;set;}
        [BindProperty]
        public string TeacherComment{get;set;}="";
        
        public async Task OnGetAsync(int? id)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim != null)
            {
                int teacherId = int.Parse(userIdClaim);
                StudentsWaiting = await dbContext.StudyPlans.Where(p => p.Status == "pending").Include(p=>p.Student).OrderByDescending(p => p.CreatedAt).ToListAsync();
                if (id.HasValue)
                {
                    SelectedPlan = await dbContext.StudyPlans.Include(p => p.Student).Include(p => p.StudyPlanReviews).ThenInclude(r => r.Lecturer).FirstOrDefaultAsync(p => p.Id == id);
                }
            }
        }
        public async Task<IActionResult> OnPostApproveAsync(int id, string newStatus)
        {
            try 
            {
       
                var plan = await dbContext.StudyPlans.FindAsync(id);
                if (plan == null) return NotFound("Không tìm thấy Study Plan ID: " + id);
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdClaim)) return BadRequest("Chưa đăng nhập");

                int userId = int.Parse(userIdClaim);
                var lecturer = await dbContext.Lecturers.FirstOrDefaultAsync(l => l.UserId == userId);
                if (lecturer == null) return BadRequest("Tài khoản không phải Giảng viên hoặc không có ID Lecturer");

                plan.Status = newStatus == "2" ? "Approved" : "NeedsRevision";
                var review = new StudyPlanReview
                {
                    StudyPlan = plan, 
                    LecturerId = lecturer.Id,
                    Comment = TeacherComment ?? "Đã duyệt",
                    ReviewedAt = DateTime.Now
                };

                dbContext.StudyPlanReviews.Add(review);

                await dbContext.SaveChangesAsync();
                return RedirectToPage();
            }
            catch (DbUpdateException ex)
            {
       
                var mysqlError = ex.InnerException?.Message;
                return Content($"Lỗi cơ sở dữ liệu: {mysqlError}. Hãy kiểm tra ID: {id}");
            }
            catch (Exception ex)
            {
                return Content("Lỗi hệ thống: " + ex.Message);
            }
        }
        
    }
}