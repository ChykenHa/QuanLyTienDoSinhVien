using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using QuanLyTienDoSinhVien.Data;
using QuanLyTienDoSinhVien.Models;

namespace QuanLyTienDoSinhVien.Pages.Admin.Users
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<User> Users { get; set; } = new List<User>();
        public List<Role> Roles { get; set; } = new List<Role>();

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? RoleFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? StatusFilter { get; set; }

        [TempData]
        public string? SuccessMessage { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            // Get all roles for filter dropdown
            Roles = await _context.Roles.OrderBy(r => r.Name).ToListAsync();

            // Build query
            var query = _context.Users.Include(u => u.Role).AsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                query = query.Where(u => u.Username.Contains(SearchTerm));
            }

            // Apply role filter
            if (RoleFilter.HasValue)
            {
                query = query.Where(u => u.RoleId == RoleFilter.Value);
            }

            // Apply status filter
            if (!string.IsNullOrWhiteSpace(StatusFilter))
            {
                if (StatusFilter == "active")
                {
                    query = query.Where(u => u.IsActive == true);
                }
                else if (StatusFilter == "inactive")
                {
                    query = query.Where(u => u.IsActive == false);
                }
            }

            // Get users
            Users = await query.OrderByDescending(u => u.CreatedAt).ToListAsync();
        }
    }
}
