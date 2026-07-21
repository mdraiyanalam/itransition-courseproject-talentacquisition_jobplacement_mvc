// Licensed to the .NET Foundation under one or more agreements.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using talentacquisition_jobplacement_mvc.Models;

namespace talentacquisition_jobplacement_mvc.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly RoleManager<IdentityRole> _roleManager;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _roleManager = roleManager;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [Required]
            [Display(Name = "Full Name")]
            public string? FullName { get; set; }

            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = Input.Email,
                    Email = Input.Email,
                    FullName = Input.FullName
                };

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    // Auto-assign Candidate Role
                    const string candidateRole = "Candidate";

                    if (!await _roleManager.RoleExistsAsync(candidateRole))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(candidateRole));
                    }

                    var roleResult = await _userManager.AddToRoleAsync(user, candidateRole);
                    if (!roleResult.Succeeded)
                    {
                        _logger.LogWarning("Failed to assign Candidate role to user {Email}", Input.Email);
                    }

                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    //await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                    //    $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    // Replace the existing SendEmailAsync call with this:
                    await _emailSender.SendEmailAsync(
                        Input.Email,
                        "Welcome to TalentHub - Please Confirm Your Email",
                        $@"
                            <!DOCTYPE html>
                            <html>
                            <head>
                                <meta charset='UTF-8'>
                                <style>
                                    body {{ font-family: Arial, sans-serif; background: #f4f4f4; padding: 20px; }}
                                    .container {{ max-width: 600px; margin: 0 auto; background: white; border-radius: 10px; overflow: hidden; }}
                                    .header {{ background: linear-gradient(135deg, #1e3a8a, #3b82f6); color: white; padding: 30px; text-align: center; }}
                                    .content {{ padding: 30px; }}
                                    .btn {{ display: inline-block; background: #2563eb; color: white; padding: 14px 32px; 
                                             text-decoration: none; border-radius: 6px; font-weight: bold; margin: 20px 0; }}
                                    .footer {{ text-align: center; padding: 20px; color: #666; font-size: 14px; }}
                                </style>
                            </head>
                            <body>
                                <div class='container'>
                                    <div class='header'>
                                        <h1>TalentHub</h1>
                                        <p>Talent Acquisition Platform</p>
                                    </div>
                                    <div class='content'>
                                        <h2>Welcome, {Input.FullName ?? "there"}!</h2>
                                        <p>Thank you for registering with TalentHub. To complete your registration and activate your account, please confirm your email address by clicking the button below:</p>
                
                                        <a href='{HtmlEncoder.Default.Encode(callbackUrl)}' class='btn'>✅ Confirm My Email Address</a>
                
                                        <p>If you did not create this account, you can safely ignore this email.</p>
                                    </div>
                                    <div class='footer'>
                                        © 2026 TalentHub - Connecting Talent with Opportunity
                                    </div>
                                </div>
                            </body>
                            </html>");

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
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
            return Page();
        }

        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<ApplicationUser>)_userStore;
        }
    }
}