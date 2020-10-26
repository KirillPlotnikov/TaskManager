using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using NETCore.MailKit.Core;
using Org.BouncyCastle.Bcpg;
using TaskManager.ViewModels;

namespace TaskManager.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<AccountController> _logger;
        private readonly IEmailService _emailService;


        public AccountController(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager, ILogger<AccountController> logger, IEmailService emailService)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
            this._emailService = emailService;
        }

        

        [HttpGet]
        public IActionResult Register(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            var model = new RegisterViewModel
                { returnUrl =  returnUrl};
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model, string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            
            if (ModelState.IsValid)
            {
                var user = new IdentityUser { UserName = model.UserName, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    _logger.LogInformation($"User created a new account with username {user.UserName}.");

                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Action("ConfirmEmail", "Account", new {userId = user.Id, code = code},
                        Request.Scheme, Request.Host.ToString());
                      

                    await _emailService.SendAsync(user.Email, "Task Manager - Email confirmation", $"Please <a href={callbackUrl}>click here</a> to confirm your account", isHtml: true);
                    

                    if (_userManager.Options.SignIn.RequireConfirmedEmail)
                    {
                        return RedirectToAction("RegisterConfirmation");
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpGet]
        public IActionResult RegisterConfirmation()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return BadRequest();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{userId}'.");
            }

            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            var result = await _userManager.ConfirmEmailAsync(user, code);
            ViewData["Message"] = result.Succeeded ? "Thank you for confirming your email." : "Error confirming your email. Please contact our support";
            return View();
        }


        [HttpGet]
        public IActionResult ExternalRegister(string provider, string returnUrl)
        {
            returnUrl ??= Url.Content("~/");

            var redirectUrl = Url.Action("ExternalRegisterCallback", "Account",
                new { returnUrl = returnUrl });
            var properties = _signInManager
                .ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }

        
        public async Task<IActionResult>
            ExternalRegisterCallback(string returnUrl = null, string remoteError = null)
        {
            returnUrl ??= Url.Content("~/");

            RegisterViewModel registerViewModel = new RegisterViewModel()
            {
                returnUrl = returnUrl
            };

            if (remoteError != null)
            {
                ModelState
                    .AddModelError(string.Empty, $"Error from external provider: {remoteError}");

                return View("Register", registerViewModel);
            }

            // Get the login information about the user from the external login provider
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ModelState
                    .AddModelError(string.Empty, "Error loading external login information.");

                return View("Register", registerViewModel);
            }

            var result = _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
            
            if (result.Result != null)
            {
                ModelState
                    .AddModelError(string.Empty, "There is a user associated with this account already. Try to log in");

                return View("Register", registerViewModel);
            }

            var email = info.Principal.FindFirstValue(ClaimTypes.Email);

            if (email != null)
            {
                var user = new IdentityUser
                {
                    UserName = info.Principal.FindFirstValue(ClaimTypes.Email),
                    Email = info.Principal.FindFirstValue(ClaimTypes.Email)
                };

                await _userManager.CreateAsync(user);
                await _userManager.AddLoginAsync(user, info);

                _logger.LogInformation($"User created a new account with username {user.UserName}.");

                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code },
                    Request.Scheme, Request.Host.ToString());


                await _emailService.SendAsync(user.Email, "Task Manager - Email confirmation", $"Please <a href={callbackUrl}>click here</a> to confirm your account", isHtml: true);


                if (_userManager.Options.SignIn.RequireConfirmedEmail)
                {
                    return RedirectToAction("RegisterConfirmation");
                }
                else
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return LocalRedirect(returnUrl);
                }

            }


            return View("Error");
            
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            var model = new LoginViewModel
                { returnUrl = returnUrl ??= Url.Content("~/") };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            if (ModelState.IsValid)
            {
                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User logged in.");
                    return LocalRedirect(returnUrl);
                }

                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out.");
                    return RedirectToPage("./Lockout");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return View();
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult ExternalLogin(string provider, string returnUrl)
        {
            returnUrl ??= Url.Content("~/");

            var redirectUrl = Url.Action("ExternalLoginCallback", "Account",
                new {returnUrl });
            var properties = _signInManager
                .ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }

        [AllowAnonymous]
        public async Task<IActionResult>
            ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            returnUrl ??= Url.Content("~/");

            LoginViewModel loginViewModel = new LoginViewModel
            {
                returnUrl = returnUrl
            };

            if (remoteError != null)
            {
                ModelState
                    .AddModelError(string.Empty, $"Error from external provider: {remoteError}");

                return View("Login", loginViewModel);
            }

            // Get the login information about the user from the external login provider
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ModelState
                    .AddModelError(string.Empty, "Error loading external login information.");

                return View("Login", loginViewModel);
            }

            // If the user already has a login (i.e if there is a record in AspNetUserLogins
            // table) then sign-in the user with this external login provider
            var signInResult = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider,
                info.ProviderKey, isPersistent: false, bypassTwoFactor: true);

            if (signInResult.Succeeded)
            {
                return LocalRedirect(returnUrl);
            }
            else if (signInResult.IsNotAllowed)
            {
                ModelState.AddModelError(string.Empty, "Email is not confirmed yet. Please check your inbox and spam messages.");
                return View("Login", loginViewModel);
            }
            else
            {

                // Get the email claim value
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);

                if (email != null)
                {
                    ModelState
                        .AddModelError("UserNotRegisteredError", string.Empty);
                    return View("Login", loginViewModel);
                }

              

                return View("Error");
            }
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return RedirectToAction("ForgotPasswordConfirmation");
                }

                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code },
                    Request.Scheme, Request.Host.ToString());


                await _emailService.SendAsync(user.Email, "Task Manager - Password reset", $"Please <a href={callbackUrl}>click here</a> to reset your password", isHtml: true);

            }
        }

        [HttpGet]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword()
        {
            return View();
        }
    }
}
