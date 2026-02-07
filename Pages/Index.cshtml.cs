using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace QuanLyTienDoSinhVien.Pages
{
    public class IndexModel : PageModel
    {
        public IActionResult OnGet()
        {
            // Check if user is authenticated
            if (User.Identity?.IsAuthenticated == true)
            {
                // Get user's role from claims
                var role = User.FindFirst(ClaimTypes.Role)?.Value;

                // Redirect based on role
                return role switch
                {
                    "Admin" => RedirectToPage("/Admin/Dashboard"),
                    "Student" => RedirectToPage("/Student/Dashboard"),
                    "Lecturer" or "Teacher" => RedirectToPage("/Teacher/Dashboard"),
                    _ => RedirectToPage("/Auth/Login")
                };
            }

            // If not authenticated, redirect to login
            return RedirectToPage("/Auth/Login");
        }
    }
}
