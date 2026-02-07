using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using QuanLyTienDoSinhVien.Data;
using QuanLyTienDoSinhVien.Models;
using System.Collections.Generic;
using System;

namespace QuanLyTienDoSinhVien.Pages.Teacher
{
    [Authorize(Roles = "Teacher")]
    public class DashboardModel : PageModel
    {
        public List<StudentSummary> StudentsWaiting { get; set; } = new List<StudentSummary>();
        
        [BindProperty] // Cho phép nhận dữ liệu từ Form gửi lên
        public StudentDetail? CurrentStudent { get; set; }

        public void OnGet(int? id)
        {
            // Tạm thời khởi tạo danh sách rỗng để không bị lỗi Build
            // Sau này bạn sẽ viết code lấy từ DbContext ở đây
            StudentsWaiting = new List<StudentSummary>();

            if (id.HasValue)
            {
                // Logic lấy chi tiết sinh viên theo ID
            }
        }

        public IActionResult OnPostApprove()
        {
            // Logic xử lý khi bấm nút phê duyệt
            return RedirectToPage();
        }
    }

    // Các class hỗ trợ (Bạn có thể để cuối file hoặc tạo file riêng trong folder Models)
    public class StudentSummary
    {
        public int Id { get; set; }
        public string FullName { get; set; } = "";
        public string StudentCode { get; set; } = "";
        public string ProjectTitle { get; set; } = "";
        public string Status { get; set; } = "Chờ duyệt";
        public string TimeAgo { get; set; } = "";
    }

    public class StudentDetail : StudentSummary
    {
        public string Semester { get; set; } = "";
        public int Progress { get; set; }
        public string ProgressSteps { get; set; } = "0/3";
        public List<RoadmapStep> Steps { get; set; } = new List<RoadmapStep>();
        public string? TeacherComment { get; set; } // Thuộc tính để nhận góp ý
    }

    public class RoadmapStep
    {
        public string StepName { get; set; } = "";
        public string Description { get; set; } = "";
        public bool IsCompleted { get; set; }
        public DateTime Deadline { get; set; }
    }
    
}