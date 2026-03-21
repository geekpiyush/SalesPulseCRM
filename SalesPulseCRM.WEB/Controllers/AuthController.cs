using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalesPulseCRM.Application.DTOs;
using SalesPulseCRM.Application.Helpers;
using SalesPulseCRM.Domain.Entities;
using SalesPulseCRM.Infrastructure.DB;

namespace SalesPulseCRM.WEB.Controllers
{
    public class AuthController : Controller
    {
        private readonly CrmDbContext _db;
        public AuthController(CrmDbContext crmDbContext)
        {
            _db = crmDbContext;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            if(!ModelState.IsValid)
            {
                return View(loginDto);
            }
            //find user using email 
          var user =  await _db.Users.FirstOrDefaultAsync(temp => temp.Email == loginDto.Email);

            if(user == null)
            {
                ModelState.AddModelError("Email", "User Not Found");
                return View(loginDto);
            }

            var hash = PasswordHashHelper.Hash(loginDto.Password);

            if(user.PasswordHash != hash)
            {
                ModelState.AddModelError("Password", "Invalid Password");
                return View(loginDto);
            }
            HttpContext.Session.SetString("UserName", user.Name);
            return RedirectToAction("Index","Home");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterDto registerDto)
        {
            if(!ModelState.IsValid)
            {
                return View(registerDto);
            }

            //find user already exist

            var existingUser = await _db.Users.AnyAsync(temp => temp.Email == registerDto.Email);

            if(existingUser)
            {
                ModelState.AddModelError("Email", "Email Already Exists");
                return View(registerDto);
            }

            var user = new User
            {
                Name = registerDto.Name,
                Email = registerDto.Email,
                Phone = registerDto.Phone,
                PasswordHash = PasswordHashHelper.Hash(registerDto.Password),
                Role = registerDto.Role,
                IsActive = registerDto.IsActive,
                CreatedDate = DateTime.Now,

            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            TempData["Success"] = "User Created Successfully";
            return RedirectToAction("Register");
        }
    }
}
