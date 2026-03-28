using BCrypt.Net;
using Hangfire;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalesPulseCRM.Application.DTOs;
using SalesPulseCRM.Application.Helpers;
using SalesPulseCRM.Application.Services;
using SalesPulseCRM.Domain.Entities;
using SalesPulseCRM.Infrastructure.DB;
using System.Security.Claims;

namespace SalesPulseCRM.WEB.Controllers
{
    [AllowAnonymous]
    public class AuthController : Controller
    {
        private readonly CrmDbContext _db;
        private readonly EmailServices _emailServices;
        public AuthController(CrmDbContext crmDbContext, EmailServices emailServices)
        {
            _db = crmDbContext;
            _emailServices = emailServices;
        }


        private void GenerateCaptcha()
        {
            var rand = new Random();
            int num1 = rand.Next(1, 10);
            int num2 = rand.Next(1, 10);

            HttpContext.Session.SetInt32("CaptchaAnswer", num1 + num2);
            ViewBag.CaptchaQuestion = $"{num1} + {num2} = ?";
        }

        [HttpGet]
        public IActionResult Login()
        {
           GenerateCaptcha();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginDto loginDto, int captcha)
        {
            if(!ModelState.IsValid)
            {

                return View(loginDto);
            }

            var expected = HttpContext.Session.GetInt32("CaptchaAnswer");


            if (expected == null || loginDto.Captcha != expected)
            {
                ModelState.AddModelError("Captcha", "Captcha incorrect");
                GenerateCaptcha();

                return View(loginDto);
            }

            //find user using email 
            var user =  await _db.Users.FirstOrDefaultAsync(temp => temp.Email == loginDto.Email);

            if(user == null)
            {
                ModelState.AddModelError("Email", "User Not Found");
                GenerateCaptcha();
                return View(loginDto);
            }


            //var hash = BCrypt.Net.BCrypt.HashPassword(loginDto.Password);
            bool isValid =  BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash);

            if(!isValid)
            {
                ModelState.AddModelError("Password", "Invalid Password");
                GenerateCaptcha();
                return View(loginDto);
            }

            if(!user.IsActive)
            {
                ModelState.AddModelError("", "⚠️ Please verify your email before login.");
                return View(loginDto);
            }

            // SESSION (optional)
            HttpContext.Session.SetString("UserName", user.Name);
            HttpContext.Session.SetString("UserRole", user.Role);
            HttpContext.Session.SetInt32("UserId", user.UserId);

            // 🔥 AUTH COOKIE (THIS WAS MISSING)
                 var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Name),
                    new Claim(ClaimTypes.Role, user.Role),
                    new Claim("UserId", user.UserId.ToString())
                };

            var identity = new ClaimsIdentity(claims, "MyCookieAuth");
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync("MyCookieAuth", principal);

            // redirect
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> RegisterAsync()
        {
            ViewBag.Managers = await _db.Users.Where(temp => temp.Role == "Manager").ToListAsync();

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterDto registerDto)
        {
            if(!ModelState.IsValid)
            {
                ViewBag.Managers = await _db.Users
                 .Where(x => x.Role == "Manager")
                 .ToListAsync();

                return View(registerDto);
            }

            if (registerDto.Role == "Employee" && registerDto.ManagerId == null)
            {
                TempData["Error"] = "Manager is required for Employee";

                ViewBag.Managers = await _db.Users
                    .Where(x => x.Role == "Manager")
                    .ToListAsync();

                return View(registerDto);
            }
            //find user already exist

            var existingUser = await _db.Users.AnyAsync(temp => temp.Email == registerDto.Email);

            if(existingUser)
            {
                TempData["Error"] = "Email Already Exists";
                ViewBag.Managers = await _db.Users
                .Where(x => x.Role == "Manager")
                .ToListAsync();

                return View(registerDto);
            }

            var user = new User
            {
                Name = registerDto.Name,
                Email = registerDto.Email,
                Phone = registerDto.Phone,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                Role = registerDto.Role,
                ManagerId = registerDto.ManagerId,
                IsActive = false,
                CreatedDate = DateTime.Now,

            };


            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            var verifyToken = Guid.NewGuid().ToString();
            user.EmailVerificationToken = verifyToken;
            user.TokenExpiry = DateTime.UtcNow.AddMinutes(30);

            await _db.SaveChangesAsync();

            var verifyLink = Url.Action("VerifyEmail", "Auth",
            new { email = user.Email, token = user.EmailVerificationToken},
            Request.Scheme);

            BackgroundJob.Enqueue(() => _emailServices.SendVerificationEmail(user.Email, verifyLink));

            TempData["Success"] = "User Created Successfully";
            return RedirectToAction("Register");
        }


        [HttpGet]
        public async Task<IActionResult> ViewEmployee()
        {
            var employees = await _db.Users
                .Where(u => u.Role != "Admin")
                .Select(u => new EmployeeViewModel
                {
                    UserId = u.UserId,
                    Name = u.Name,
                    Email = u.Email,
                    Role = u.Role,
                    IsActive = u.IsActive,

                    ManagerName = _db.Users
                    .Where(m => m.UserId == u.ManagerId)
                    .Select(m => m.Name)
                    .FirstOrDefault()
                }).ToListAsync();

            return View(employees);
        }

        [HttpGet]
        public async Task<IActionResult> VerifyEmail(string email, string token)
        {
           var user = await _db.Users.FirstOrDefaultAsync(x => x.Email == email);
            if(user == null)
            {
                return Content("Invalid Request");
            }
            if(user.TokenExpiry < DateTime.UtcNow)
            {
                return Content("Token expired");
            }
            if(user.EmailVerificationToken != token)
            {
                return Content("Invalid token");
            }

            user.IsActive = true;
            user.EmailVerificationToken = null;
            user.TokenExpiry = null; 
            await _db.SaveChangesAsync();

            TempData["Success"] = "Email verified successfully. You can login now.";
            return RedirectToAction("Login", "Auth");
        }


        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            HttpContext.SignOutAsync("MyCookieAuth");

            return RedirectToAction("Login", "Auth");
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ForgotPassword(string email)
        {
          var user =  _db.Users.FirstOrDefault(x => x.Email == email);
            if(user == null)
            {
                TempData["Msg"] = "If email exist, resent link sent";
                return View();
            }
            user.ResetToken = Guid.NewGuid().ToString();
            user.ResetTokenExpiry = DateTime.Now.AddMinutes(2);

            _db.SaveChanges();

            var link = $"http://31.97.60.7:5000/Auth/ResetPassword?token={user.ResetToken}";

            //SendEmail(user.Email, link);
            BackgroundJob.Enqueue(() => _emailServices.SendEmail(email, link));
            return View();
        }
        [HttpGet]
        public IActionResult ResetPassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ResetPassword(string token, string newPassword)
        {
            var user = _db.Users.FirstOrDefault(temp => temp.ResetToken == token);

            if(user == null || user.ResetTokenExpiry < DateTime.Now)
            {
                return Content("Invalid or expired link");
            }
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword); 
            user.ResetToken = null;
            user.ResetTokenExpiry = null;
            _db.SaveChanges();
            return RedirectToAction("Login", "Auth");
        }

        [HttpGet]
        public async Task<IActionResult> UpdateUser(int userId)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
            {
                return NotFound();
            }


            var model = new UpdateUserViewModel
            {
                UserId = user.UserId,
                Name = user.Name,
                Email = user.Email,
                Phone = user.Phone,
                Role = user.Role,
                ManagerId = user.ManagerId,

                Managers = await _db.Users.Where(u => u.Role == "Manager")
                .ToListAsync()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult>UpdateUser(UpdateUserViewModel updateUserView)
        {
            if(!ModelState.IsValid)
            {
                updateUserView.Managers = await _db.Users.Where(u => u.Role == "Manager").ToListAsync();
                return View(updateUserView);
            }

            var user = await _db.Users.FindAsync(updateUserView.UserId);

            if(user == null)
            {
                return NotFound();
            }

            user.Name = updateUserView.Name;
            user.Email = updateUserView.Email;
            user.Phone = updateUserView.Phone;
            user.Role = updateUserView.Role;
            user.ManagerId = updateUserView.ManagerId;
            user.IsActive = updateUserView.IsActive;

            await _db.SaveChangesAsync();
            return RedirectToAction("ViewEmployee");
        }
    }
}
