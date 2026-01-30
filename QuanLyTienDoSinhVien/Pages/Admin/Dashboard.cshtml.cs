using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using QuanLyTienDoSinhVien.Data;
using QuanLyTienDoSinhVien.Models;

namespace QuanLyTienDoSinhVien.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class DashboardModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DashboardModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public int TotalUsers { get; set; }
        public int TotalStudents { get; set; }
        public int TotalLecturers { get; set; }
        public int InactiveUsers { get; set; }
        public List<User> RecentUsers { get; set; } = new List<User>();

        public async Task OnGetAsync()
        {
            // Get total users count
            TotalUsers = await _context.Users.CountAsync();

            // Get total students count
            TotalStudents = await _context.Users
                .Include(u => u.Role)
                .Where(u => u.Role.Name == "Student")
                .CountAsync();

            // Get total lecturers count
            TotalLecturers = await _context.Users
                .Include(u => u.Role)
                .Where(u => u.Role.Name == "Lecturer" || u.Role.Name == "Teacher")
                .CountAsync();

            // Get inactive users count
            InactiveUsers = await _context.Users
                .Where(u => u.IsActive == false)
                .CountAsync();

            // Get recent users (last 10)
            RecentUsers = await _context.Users
                .Include(u => u.Role)
                .OrderByDescending(u => u.CreatedAt)
                .Take(10)
                .ToListAsync();
        }
    }
}
