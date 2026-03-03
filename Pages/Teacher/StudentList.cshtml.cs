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
    public class StudentListModel : PageModel
    {
        private readonly ApplicationDbContext dbContext;
        public StudentListModel (ApplicationDbContext context)
        {
            dbContext = context;
        }
        public List<Models.Student> Students { get; set; }

        public async Task OnGetAsync(string search)
        {
            var query = dbContext.Students.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(s => s.FullName.Contains(search)
                                      || s.Email.Contains(search)
                                      || s.StudentCode.Contains(search));
            }
            Students = await query.Include(s => s.Class).ToListAsync();
        }
    }
}