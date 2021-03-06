using Amazon.IdentityManagement.Model;
using e_commerce.Data;
using e_commerce.Data.Static;
using e_commerce.Data.ViewModels;
using e_commerce.Models;


using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace e_commerce.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ShopDbContext _context;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ShopDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context; 
        }
        public async Task<IActionResult> Users()
        {
            var users = await _context.Users.ToListAsync();
            return View(users);
        }

        public IActionResult Login() => View(new LoginVM());

		[HttpPost]
        public async Task<IActionResult> Login(LoginVM loginVM)
		{
            if(!ModelState.IsValid) return View(loginVM);
            var user = await _userManager.FindByEmailAsync(loginVM.EmailAddress);
                if(user != null)
			    {
                    var passwordCheck = await _userManager.CheckPasswordAsync(user, loginVM.Password);
                    if(passwordCheck)
				    {
                   
                        var result = await _signInManager.PasswordSignInAsync(user, loginVM.Password, false, false);
                        if (result.Succeeded)
					    {
                            return RedirectToAction("Index", "Product");
					    }

				    }
                    TempData["Error"] = "Wrong credentials. Please, try again!";
                    return View(loginVM);
				    }

			
		       
		    TempData["Error"] = "Wrong credentials. Please, try again!";
            return View(loginVM);
        }
        public IActionResult Register() => View(new RegisterVM());

		[HttpPost]
        public async Task<IActionResult> Register(RegisterVM registerVM)
		{
            if (!ModelState.IsValid) return View(registerVM);

            var user = await _userManager.FindByEmailAsync(registerVM.EmailAddress);
            if (user != null)
            {
                TempData["Error"] = "This email address is already in use";
                return View(registerVM);
            }

            
            var newUser = new ApplicationUser()
            {
                Fullname = registerVM.Fullname,
                Email = registerVM.EmailAddress,
                UserName = registerVM.EmailAddress,
                EmailConfirmed = true

            };

            var newUserResponse = await _userManager.CreateAsync(newUser, registerVM.Password);

            if (!newUserResponse.Succeeded)
            {

                foreach (var error in newUserResponse.Errors)
                {
                    TempData["Error"] = "error: " + error.Description;
                    return View(registerVM);
                }
            }
            
            await _userManager.AddToRoleAsync(newUser, UserRoles.User);
            return View("RegisterCompleted");
            
           
            

        }

		[HttpPost]
        public async Task<IActionResult> Logout()
		{
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index","Product");
		}

        public IActionResult AccessDenied(string ReturnUrl)
		{
            return View();
		}
    }
}
