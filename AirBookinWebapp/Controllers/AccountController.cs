using System.Diagnostics;
using DTO;
using Microsoft.AspNetCore.Mvc;
using Models;
using Repository.Interfaces;

namespace AirBookingApplication.Controllers
{
    [Route("[controller]")]
    public class AccountController : Controller
    {
        private readonly IAccountRepository _accountRepository;

        public AccountController(IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository;
        }

        //[Authorize]
        [Route("Index")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("TestCookie")]
        public IActionResult TestCookie()
        {
            var authToken = Request.Cookies["AuthToken"];
            if (authToken == null)
            {
                return Content("AuthToken is missing in cookies.");
            }
            return Content("AuthToken found: " + authToken);
        }


        [Route("Register")]
        public async Task<IActionResult> Register(User user)
        {
            try
            {
                bool success = await _accountRepository.Register(user);
                if (success)
                {
                    TempData["SuccessMessage"] = "User registered successfully!";
                    return RedirectToAction("Login");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Registration failed.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return View(user);
        }

        [Route("Login")]
        public async Task<IActionResult> Login(LoginDTO loginDto)
        {
            try
            {
                //Debug.WriteLine("Return URL: " + ViewBag.returnUrl);
                // Get token from API
                string token = await _accountRepository.Login(loginDto);

                if (!string.IsNullOrEmpty(token))  // If login is successful
                {
                    // Set JWT token as HttpOnly cookie before redirecting
                    Response.Cookies.Append("AuthToken", token, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,  // Change to 'false' for localhost testing
                        SameSite = SameSiteMode.Strict // Change to 'None' if cross-origin
                    });

                    TempData["SuccessMessage"] = "User logged in successfully!";

                    //Debug.WriteLine("Return URL: " + ViewBag.returnUrl);

                    if (!string.IsNullOrEmpty(loginDto.ReturnUrl) && Url.IsLocalUrl(loginDto.ReturnUrl))
                    {
                        return Redirect(loginDto.ReturnUrl);
                    }

                    return RedirectToAction("Index", "Home"); // Redirect after setting the cookie
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Unauthorized User.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return View(loginDto);
        }

        [Route("logout")]
        public ActionResult Logout()
        {
            Response.Cookies.Delete("AuthToken");

            // Optionally, show a logout success message
            TempData["SuccessMessage"] = "Logged out successfully!";

            // Redirect to the Login page or Home page
            return RedirectToAction("Login");
        }
    }
}
