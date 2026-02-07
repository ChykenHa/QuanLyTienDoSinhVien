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
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();

        public List<Role> Roles { get; set; } = new List<Role>();
        public List<Class> Classes { get; set; } = new List<Class>();

        public string? ErrorMessage { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
            [StringLength(100, ErrorMessage = "Tên đăng nhập không được vượt quá 100 ký tự")]
            public string Username { get; set; } = string.Empty;

            [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
            [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
            [DataType(DataType.Password)]
            public string Password { get; set; } = string.Empty;

            [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu")]
            [DataType(DataType.Password)]
            [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp")]
            public string ConfirmPassword { get; set; } = string.Empty;

            [Required(ErrorMessage = "Vui lòng chọn vai trò")]
            public int RoleId { get; set; }

            public bool IsActive { get; set; } = true;

            // Student specific fields
            [StringLength(20)]
            public string? StudentCode { get; set; }

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

        public async Task OnGetAsync()
        {
            Roles = await _context.Roles.OrderBy(r => r.Name).ToListAsync();
            Classes = await _context.Classes.OrderBy(c => c.Name).ToListAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Reload roles and classes for the form
            Roles = await _context.Roles.OrderBy(r => r.Name).ToListAsync();
            Classes = await _context.Classes.OrderBy(c => c.Name).ToListAsync();

            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Check if username already exists
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == Input.Username);

            if (existingUser != null)
            {
                ErrorMessage = "Tên đăng nhập đã tồn tại trong hệ thống.";
                return Page();
            }

            // Get the selected role
            var role = await _context.Roles.FindAsync(Input.RoleId);
            if (role == null)
            {
                ErrorMessage = "Vai trò không hợp lệ.";
                return Page();
            }

            // Validate role-specific fields
            if (role.Name == "Student")
            {
                if (string.IsNullOrWhiteSpace(Input.StudentCode))
                {
                    ErrorMessage = "Mã sinh viên là bắt buộc cho tài khoản sinh viên.";
                    return Page();
                }

                if (string.IsNullOrWhiteSpace(Input.FullName))
                {
                    ErrorMessage = "Họ và tên là bắt buộc cho tài khoản sinh viên.";
                    return Page();
                }

                // Check if student code already exists
                var existingStudent = await _context.Students
                    .FirstOrDefaultAsync(s => s.StudentCode == Input.StudentCode);

                if (existingStudent != null)
                {
                    ErrorMessage = "Mã sinh viên đã tồn tại trong hệ thống.";
                    return Page();
                }
            }
            else if (role.Name == "Lecturer" || role.Name == "Teacher")
            {
                if (string.IsNullOrWhiteSpace(Input.FullName))
                {
                    ErrorMessage = "Họ và tên là bắt buộc cho tài khoản giảng viên.";
                    return Page();
                }
            }

            // Create user
            var user = new User
            {
                Username = Input.Username,
                PasswordHash = HashPassword(Input.Password),
                RoleId = Input.RoleId,
                IsActive = Input.IsActive,
                FailedLogin = 0,
                CreatedAt = DateTime.Now
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Create role-specific records
            if (role.Name == "Student")
            {
                var student = new Models.Student
                {
                    UserId = user.Id,
                    StudentCode = Input.StudentCode!,
                    FullName = Input.FullName!,
                    Email = Input.Email,
                    Phone = Input.Phone,
                    ClassId = Input.ClassId,
                    Address = Input.Address,
                    IsPrivate = false
                };

                _context.Students.Add(student);
                await _context.SaveChangesAsync();
            }
            else if (role.Name == "Lecturer" || role.Name == "Teacher")
            {
                var lecturer = new Lecturer
                {
                    UserId = user.Id,
                    FullName = Input.FullName!,
                    Email = Input.Email,
                    Phone = Input.Phone
                };

                _context.Lecturers.Add(lecturer);
                await _context.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = $"Tài khoản '{Input.Username}' đã được tạo thành công.";
            return RedirectToPage("./Index");
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
