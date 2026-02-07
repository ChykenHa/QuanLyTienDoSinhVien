using QuanLyTienDoSinhVien.Models;
using System.Security.Cryptography;
using System.Text;

namespace QuanLyTienDoSinhVien.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            // Database is created if it doesn't exist. Data will PERSIST.
            context.Database.EnsureCreated();

            // Seed Roles if they don't exist
            string[] roleNames = { "Admin", "Lecturer", "Student", "Teacher" };
            foreach (var roleName in roleNames)
            {
                if (!context.Roles.Any(r => r.Name == roleName))
                {
                    context.Roles.Add(new Role { Name = roleName });
                }
            }
            context.SaveChanges();

            // Seed Admin User if it doesn't exist
            if (!context.Users.Any(u => u.Username == "admin"))
            {
                var adminRole = context.Roles.FirstOrDefault(r => r.Name == "Admin");
                if (adminRole != null)
                {
                    var adminUser = new User
                    {
                        Username = "admin",
                        PasswordHash = HashPassword("admin123"),
                        Role = adminRole,
                        IsActive = true,
                        FailedLogin = 0,
                        CreatedAt = DateTime.Now
                    };

                    context.Users.Add(adminUser);
                    context.SaveChanges();
                }
            }
        }

        private static string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }
}
