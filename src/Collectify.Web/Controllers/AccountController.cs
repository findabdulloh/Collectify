using Collectify.Service.DTOs.Users;
using Collectify.Service.IServices.IUsers;
using Microsoft.AspNetCore.Mvc;

namespace Collectify.Web.Controllers;

public class AccountController : Controller
{
    private readonly IUserService userService;
    private readonly IAuthorizationService authorizationService;
    private readonly IAuthenticationService authenticationService;

    public AccountController(IUserService userService, IAuthenticationService authenticationService, IAuthorizationService authorizationService)
    {
        this.userService = userService;
        this.authorizationService = authorizationService;
        this.authenticationService = authenticationService;
    }

    [HttpGet]
    public async Task<IActionResult> Update(UserUpdateDto dto)
    {
        var user = await this.authorizationService.GetUserAsync();

        dto.Username = user.Username;
        dto.Name = user.Name;

        return View("Update", dto);
    }

    [HttpPost]
    public async Task<IActionResult> PostUpdate(UserUpdateDto dto)
    {
        if (ModelState.IsValid)
        {
            var response = await this.userService.ModifyAsync(dto);

            if (response.Result is null)
                TempData["ErrorMessage"] = response.Message;
            else
            {
                TempData["SuccessMessage"] = response.Message;
                return await Index();
            }
        }

        return await Update(dto);
    }

    [HttpGet]
    public IActionResult Email()
    {
        return View("Email");
    }

    [HttpPost]
    public async Task<IActionResult> ChangeEmail(UserEmailUpdateDto dto)
    {
        if (ModelState.IsValid)
        {
            var response = await this.userService.ChangeEmailAsync(dto);

            if (response.Result is null)
                TempData["ErrorMessage"] = response.Message;
            else
            {
                TempData["SuccessMessage"] = response.Message;
                return await Index();
            }
        }

        return Email();
    }

    [HttpGet]
    public IActionResult Password()
    {
        return View("Password");
    }

    [HttpPost]
    public async Task<IActionResult> ChangePassword(UserPasswordUpdateDto dto)
    {
        if (ModelState.IsValid)
        {
            var response = await this.userService.ChangePasswordAsync(dto);

            if (response.Result is null)
                TempData["ErrorMessage"] = response.Message;
            else
            {
                TempData["SuccessMessage"] = response.Message;
                return await Index();
            }
        }

        return Password();
    }

    [HttpPost]
    public async Task<IActionResult> SendVerificationEmail()
    {
        if (ModelState.IsValid)
        {
            var visitor = await this.authorizationService.GetUserAsync();

            if (visitor is null)
            {
                TempData["ErrorMessage"] = "Authorization error";

                return Login(new UserLoginDto());
            }

            var response = await this.authenticationService.SendVerificationMail(visitor.Id);

            if (!response.Result)
                TempData["ErrorMessage"] = response.Message;
            else
                TempData["SuccessMessage"] = response.Message;
        }

        return await Index();
    }

    [HttpGet]
    public async Task<IActionResult> Verificate(string token)
    {
        if (ModelState.IsValid)
        {
            var response = await this.authenticationService.VerificateAsync(token);

            if (!response.Result)
                TempData["ErrorMessage"] = response.Message;
            else
                TempData["SuccessMessage"] = response.Message;
        }

        return await Index();
    }

    public async Task<IActionResult> Index()
    {
        var visitor = await this.authorizationService.GetUserAsync();

        if (visitor is null)
        {
            TempData["ErrorMessage"] = "Authorization error";

            return Login(new UserLoginDto());
        }
        if (visitor.IsBlocked)
        {
            TempData["ErrorMessage"] = "Your account is blocked";
        }
        if (!visitor.Verified)
        {
            if (!(await this.authenticationService
                .CheckSentVerificationMailAsync()).Result)
                await this.authenticationService.SendVerificationMail(visitor.Id);

            TempData["Success"] = "Check your email, we have sent you verification mail.";
        }

        var visitorResultDto = (await this.userService.GetAsync(visitor.Id)).Result;

        return View("Index", visitorResultDto);
    }

    [HttpGet]
    public IActionResult Registration(UserCreationDto dto)
    {
        return View("Registration", dto);
    }

    [HttpPost]
    public async Task<IActionResult> Register(UserCreationDto dto)
    {
        if (ModelState.IsValid)
        {
            var response = await this.userService.AddAsync(dto);

            if (response.Result is not null)
            {
                TempData["SuccessMessage"] = response.Message;
                return RedirectToAction("Login", "Account");
            }

            TempData["ErrorMessage"] = response.Message;
        }

        return Registration(dto);
    }

    [HttpGet]
    public IActionResult Login(UserLoginDto dto)
    {
        return View("Login", dto);
    }

    [HttpPost]
    public async Task<IActionResult> PostLogin(UserLoginDto dto)
    {
        if (ModelState.IsValid)
        {
            var response = await this.authenticationService.AuthenticateAsync(dto);

            if (response.Result is not null)
            {
                Response.Cookies.Append("Token", response.Result);

                return RedirectToAction("Index", "Home");
            }

            TempData["ErrorMessage"] = response.Message;
        }

        return Login(dto);
    }
}
