using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using QuanLyTienDoSinhVien.Data;
using QuanLyTienDoSinhVien.Models;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;

namespace QuanLyTienDoSinhVien.Pages.Admin.Users
{
    [Authorize(Roles = "Admin")]
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EditModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();

        public List<Class> Classes { get; set; } = new List<Class>();
        public string CurrentRole { get; set; } = string.Empty;
        public Models.Student? StudentInfo { get; set; }
        public Lecturer? LecturerInfo { get; set; }
        public int FailedLoginCount { get; set; }
        public DateTime? LastLogin { get; set; }

        public string? ErrorMessage { get; set; }

        public class InputModel
        {
            public int UserId { get; set; }

            public string Username { get; set; } = string.Empty;

            [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
            [DataType(DataType.Password)]
            public string? NewPassword { get; set; }

            [DataType(DataType.Password)]
            [Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp")]
            public string? ConfirmPassword { get; set; }

            public bool IsActive { get; set; }

            // Student/Lecturer specific fields
            [StringLength(100)]
            public string? FullName { get; set; }

            [EmailAddress(ErrorMessage = "Email không hợp lệ")]
            [StringLength(100)]
            public string? Email { get; set; }

            [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
            [StringLength(20)]
            public string? Phone { get; set; }

            public int? ClassId { get; set; }

            public string? Address { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            // Load classes for dropdown
            Classes = await _context.Classes.OrderBy(c => c.Name).ToListAsync();

            // Set basic user info
            Input.UserId = user.Id;
            Input.Username = user.Username;
            Input.IsActive = user.IsActive ?? true;
            CurrentRole = user.Role.Name;
            FailedLoginCount = user.FailedLogin ?? 0;
            LastLogin = user.LastLogin;

            // Load role-specific info
            if (user.Role.Name == "Student")
            {
                StudentInfo = await _context.Students
                    .FirstOrDefaultAsync(s => s.UserId == user.Id);

                if (StudentInfo != null)
                {
                    Input.FullName = StudentInfo.FullName;
                    Input.Email = StudentInfo.Email;
                    Input.Phone = StudentInfo.Phone;
                    Input.ClassId = StudentInfo.ClassId;
                    Input.Address = StudentInfo.Address;
                }
            }
            else if (user.Role.Name == "Lecturer" || user.Role.Name == "Teacher")
            {
                LecturerInfo = await _context.Lecturers
                    .FirstOrDefaultAsync(l => l.UserId == user.Id);

                if (LecturerInfo != null)
                {
                    Input.FullName = LecturerInfo.FullName;
                    Input.Email = LecturerInfo.Email;
                    Input.Phone = LecturerInfo.Phone;
                }
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == Input.UserId);

            if (user == null)
            {
                return NotFound();
            }

            // Reload data for the form
            Classes = await _context.Classes.OrderBy(c => c.Name).ToListAsync();
            CurrentRole = user.Role.Name;
            FailedLoginCount = user.FailedLogin ?? 0;
            LastLogin = user.LastLogin;

            // Load role-specific info for display
            if (user.Role.Name == "Student")
            {
                StudentInfo = await _context.Students
                    .FirstOrDefaultAsync(s => s.UserId == user.Id);
            }
            else if (user.Role.Name == "Lecturer" || user.Role.Name == "Teacher")
            {
                LecturerInfo = await _context.Lecturers
                    .FirstOrDefaultAsync(l => l.UserId == user.Id);
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Update password if provided
            if (!string.IsNullOrWhiteSpace(Input.NewPassword))
            {
                user.PasswordHash = HashPassword(Input.NewPassword);
            }

            // Update active status
            user.IsActive = Input.IsActive;

            // Update role-specific info
            if (user.Role.Name == "Student")
            {
                var student = await _context.Students
                    .FirstOrDefaultAsync(s => s.UserId == user.Id);

                if (student != null)
                {
                    student.FullName = Input.FullName ?? student.FullName;
                    student.Email = Input.Email;
                    student.Phone = Input.Phone;
                    student.ClassId = Input.ClassId;
                    student.Address = Input.Address;
                }
            }
            else if (user.Role.Name == "Lecturer" || user.Role.Name == "Teacher")
            {
                var lecturer = await _context.Lecturers
                    .FirstOrDefaultAsync(l => l.UserId == user.Id);

                if (lecturer != null)
                {
                    lecturer.FullName = Input.FullName ?? lecturer.FullName;
                    lecturer.Email = Input.Email;
                    lecturer.Phone = Input.Phone;
                }
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Tài khoản '{user.Username}' đã được cập nhật thành công.";
            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnPostResetFailedLoginAsync()
        {
            var user = await _context.Users.FindAsync(Input.UserId);

            if (user == null)
            {
                return NotFound();
            }

            user.FailedLogin = 0;
            user.IsActive = true;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Đã đặt lại số lần đăng nhập thất bại cho tài khoản '{user.Username}'.";
            return RedirectToPage("./Edit", new { id = Input.UserId });
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }
}
