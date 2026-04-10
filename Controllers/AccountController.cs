using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using MVCDHProject.Models;
using System.Security.Claims;
using System.Text;

namespace MVCDHProject.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly SignInManager<IdentityUser> signInManager;
        private readonly IConfiguration configuration;

        public AccountController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IConfiguration configuration)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.configuration = configuration;
        }

        #region Register
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(UserViewModel userModel)
        {
            if (ModelState.IsValid)
            {
                IdentityUser identityUser = new IdentityUser
                {
                    UserName = userModel.Name,
                    Email = userModel.Email,
                    PhoneNumber = userModel.Mobile
                };

                var Result = await userManager.CreateAsync(identityUser, userModel.Password);

                if (Result.Succeeded)
                {
                    var token = await userManager.GenerateEmailConfirmationTokenAsync(identityUser);
                    var confirmationUrlLink = Url.Action("ConfirmEmail", "Account",
                        new { UserId = identityUser.Id, Token = token }, Request.Scheme);
                    SendMail(identityUser, confirmationUrlLink, "Email Confirmation Link");

                    TempData["Title"] = "Email Confirmation Link";
                    TempData["Message"] = "A confirm email link has been sent to your registered mail, click on it to confirm.";
                    return View("DisplayMessages");
                }
                else
                {
                    foreach (var Error in Result.Errors)
                        ModelState.AddModelError("", Error.Description);
                }
            }
            return View(userModel);
        }
        #endregion

        #region Login-LogOut
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel loginModel)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByNameAsync(loginModel.Name);
                if (user != null && (await userManager.CheckPasswordAsync(user, loginModel.Password)) && user.EmailConfirmed == false)
                {
                    ModelState.AddModelError("", "Your email is not confirmed.");
                    return View(loginModel);
                }

                var result = await signInManager.PasswordSignInAsync(loginModel.Name, loginModel.Password, loginModel.RememberMe, false);

                if (result.Succeeded)
                {
                    if (string.IsNullOrEmpty(loginModel.ReturnUrl))
                        return RedirectToAction("Index", "Home");
                    else
                        return LocalRedirect(loginModel.ReturnUrl);
                }
                else
                {
                    ModelState.AddModelError("", "Invalid Credential.");
                }
            }
            return View();
        }

        public async Task<IActionResult> LogOut()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
        #endregion

        #region Send Mail
        public void SendMail(IdentityUser identityUser, string requestLink, string subject)
        {
            var emailSettings = configuration.GetSection("EmailSettings");

            StringBuilder mailBody = new StringBuilder();
            mailBody.Append("Hello " + identityUser.UserName + "<br/><br/>");
            if (subject == "Email Confirmation Link")
                mailBody.Append("Click on the link below to confirm your email:");
            else if (subject == "reset Password Link")
                mailBody.Append("Click on the link below to reset your Password:");
            mailBody.Append("<br/>");
            mailBody.Append(requestLink);
            mailBody.Append("<br/><br/>Regards<br/><br/>Customer Support.");

            BodyBuilder bodyBuilder = new BodyBuilder();
            bodyBuilder.HtmlBody = mailBody.ToString();

            MailboxAddress fromAddress = new MailboxAddress(emailSettings["FromName"], emailSettings["FromEmail"]);
            MailboxAddress toAddress = new MailboxAddress(identityUser.UserName, identityUser.Email);

            MimeMessage mailMessage = new MimeMessage();
            mailMessage.From.Add(fromAddress);
            mailMessage.To.Add(toAddress);
            mailMessage.Subject = subject;
            mailMessage.Body = bodyBuilder.ToMessageBody();

            using SmtpClient smtpClient = new SmtpClient();
            smtpClient.Connect(emailSettings["SmtpHost"], int.Parse(emailSettings["SmtpPort"]), true);
            smtpClient.Authenticate(emailSettings["Username"], emailSettings["AppPassword"]);
            smtpClient.Send(mailMessage);
            smtpClient.Disconnect(true);
        }
        #endregion

        #region Confirm-Email
        public async Task<IActionResult> ConfirmEmail(string UserId, string token)
        {
            if (UserId != null && token != null)
            {
                var user = await userManager.FindByIdAsync(UserId);
                if (user != null)
                {
                    var result = await userManager.ConfirmEmailAsync(user, token);
                    if (result.Succeeded)
                    {
                        TempData["Title"] = "Email Confirmation Success.";
                        TempData["Message"] = "Email confirmation is completed. You can now login into the application.";
                        return View("DisplayMessages");
                    }
                    else
                    {
                        StringBuilder Errors = new StringBuilder();
                        foreach (var error in result.Errors)
                            Errors.Append(error.Description);

                        TempData["Title"] = "Confirmation Email Failure";
                        TempData["Message"] = Errors.ToString();
                        return View("DisplayMessages");
                    }
                }
                else
                {
                    TempData["Title"] = "Invalid User Id.";
                    TempData["Message"] = "User Id which is present in confirm email link is in-valid.";
                    return View("DisplayMessages");
                }
            }
            else
            {
                TempData["Title"] = "Invalid Email Confirmation Link.";
                TempData["Message"] = "Email confirmation link is invalid, either missing the User Id or Confirmation Token.";
                return View("DisplayMessages");
            }
        }
        #endregion

        #region Resend Confirmation Email
        [HttpPost]
        public async Task<IActionResult> ResendConfirmationEmail(string userName)
        {
            var user = await userManager.FindByNameAsync(userName);

            if (user == null)
            {
                TempData["Title"] = "Error";
                TempData["Message"] = "User not found.";
                return View("DisplayMessages");
            }

            if (user.EmailConfirmed)
            {
                TempData["Title"] = "Info";
                TempData["Message"] = "Email is already confirmed.";
                return View("DisplayMessages");
            }

            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmationUrl = Url.Action("ConfirmEmail", "Account",
                new { UserId = user.Id, Token = token }, Request.Scheme);

            SendMail(user, confirmationUrl, "Email Confirmation Link");

            TempData["Title"] = "Confirmation Email Sent";
            TempData["Message"] = "A new confirmation link has been sent to your email.";
            return View("DisplayMessages");
        }
        #endregion

        #region Forgot-Password Reset-Password
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
                var user = await userManager.FindByNameAsync(model.Name);
                if (user != null && (await userManager.IsEmailConfirmedAsync(user)))
                {
                    string token = await userManager.GeneratePasswordResetTokenAsync(user);
                    string RequestUrl = Url.Action("ResetPassword", "Account",
                        new { UserId = user.Id, Token = token }, Request.Scheme);
                    SendMail(user, RequestUrl, "reset Password Link");

                    TempData["Title"] = "Reset Password Link";
                    TempData["Message"] = "Reset password link has been sent to your mail, click on it and reset password.";
                    return View("DisplayMessages");
                }
                else
                {
                    TempData["Title"] = "Reset Password Mail Generation Failed.";
                    TempData["Message"] = "Either the Username you have entered is in-valid or your email is not confirmed.";
                    return View("DisplayMessages");
                }
            }
            return View(model);
        }

        [HttpGet]
        public ViewResult ResetPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByIdAsync(model.UserId);
                if (user != null)
                {
                    var result = await userManager.ResetPasswordAsync(user, model.Token, model.Password);
                    if (result.Succeeded)
                    {
                        TempData["Title"] = "Reset Password Success";
                        TempData["Message"] = "Your password has been reset successfully.";
                        return View("DisplayMessages");
                    }
                    else
                    {
                        foreach (var Error in result.Errors)
                            ModelState.AddModelError("", Error.Description);
                        return View(model);
                    }
                }
                else
                {
                    TempData["Title"] = "Invalid User";
                    TempData["Message"] = "No user exists with the given User Id.";
                    return View("DisplayMessages");
                }
            }
            else
            {
                ModelState.AddModelError("", "Either User Id or Token is missing in the reset password link.");
                return View(model);
            }
        }
        #endregion

        #region External Login
        public IActionResult ExternalLogin(string provider, string? returnUrl = null)
        {
            var redirectUrl = Url.Action("Callback", "Account", new { ReturnUrl = returnUrl });
            var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }

        public async Task<IActionResult> CallBack(string returnUrl)
        {
            if (string.IsNullOrEmpty(returnUrl))
                returnUrl = "~/";

            var info = await signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ModelState.AddModelError("", "Error loading external login information.");
                return View("Login");
            }

            var signInResult = await signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, false, true);
            if (signInResult.Succeeded)
                return LocalRedirect(returnUrl);

            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            if (email != null)
            {
                var user = await userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    user = new IdentityUser
                    {
                        UserName = info.Principal.FindFirstValue(ClaimTypes.Email),
                        Email = info.Principal.FindFirstValue(ClaimTypes.Email),
                        PhoneNumber = info.Principal.FindFirstValue(ClaimTypes.MobilePhone),
                    };
                    await userManager.CreateAsync(user);
                }
                await userManager.AddLoginAsync(user, info);
                await signInManager.SignInAsync(user, false);
                return LocalRedirect(returnUrl);
            }

            TempData["Title"] = "Error";
            TempData["Message"] = "Email claim not received from third party provided.";
            return RedirectToAction("DisplayMessages");
        }
        #endregion
    }
}