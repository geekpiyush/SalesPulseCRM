using BCrypt.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalesPulseCRM.Application.DTOs;
using SalesPulseCRM.Application.Helpers;
using SalesPulseCRM.Application.Services;
using SalesPulseCRM.Domain.Entities;
using SalesPulseCRM.Infrastructure.DB;

namespace SalesPulseCRM.WEB.Controllers
{
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
            int num1 = rand.Next(1, 100);
            int num2 = rand.Next(1, 100);

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


            if (expected == null || captcha != expected)
            {
                ModelState.AddModelError("", "Captcha incorrect");
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
                ModelState.AddModelError("", "Please verify your email first.");
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
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                Role = registerDto.Role,
                IsActive = false,
                CreatedDate = DateTime.Now,

            };


            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            var verifyToken = Guid.NewGuid().ToString();
            user.EmailVerificationToken = verifyToken;
            user.TokenExpiry = DateTime.Now.AddMinutes(30);

            await _db.SaveChangesAsync();

            var verifyLink = Url.Action("VerifyEmail", "Auth",
            new { email = user.Email, token = user.TokenExpiry},
            Request.Scheme);

            _emailServices.SendVerificationEmail(user.Email, verifyLink);

            TempData["Success"] = "User Created Successfully";
            return RedirectToAction("Register");
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

            return Content("Email verified successfully. You can login now.");
        }


        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

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

           
            _emailServices.SendEmail(email, link);
            //SendEmail(user.Email, link);

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
    }
}
