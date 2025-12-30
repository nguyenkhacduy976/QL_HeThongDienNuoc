using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QL_HethongDiennuoc.Models.DTOs;
using System.Security.Claims;

namespace QL_HethongDiennuoc.Controllers;

public class AuthController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;
    private string API_URL => _configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5058/api";

    public AuthController(
        IHttpClientFactory httpClientFactory, 
        IConfiguration configuration,
        ILogger<AuthController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    [AllowAnonymous]
    public IActionResult Login()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginDto model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var client = _httpClientFactory.CreateClient();
            
            // Gọi API /api/auth/login
            var response = await client.PostAsJsonAsync($"{API_URL}/auth/login", model);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
                
                if (result != null)
                {
                    // Tạo Cookie Authentication từ JWT response
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, result.UserId.ToString()), // User ID
                        new Claim(ClaimTypes.Name, result.Username),
                        new Claim(ClaimTypes.Email, result.Email),
                        new Claim(ClaimTypes.GivenName, result.FullName),
                        new Claim(ClaimTypes.Role, result.Role),
                        new Claim("JwtToken", result.Token) // Lưu token để call API sau
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = true,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddHours(24)
                    };

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);

                    _logger.LogInformation("User {Username} logged in", result.Username);
                    TempData["Success"] = "Đăng nhập thành công!";
                    return RedirectToAction("Index", "Home");
                }
            }

            ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login error");
            ModelState.AddModelError("", "Lỗi kết nối: " + ex.Message);
        }

        return View(model);
    }

    [Authorize(Roles = "Admin")]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterDto model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var client = _httpClientFactory.CreateClient();
            
            // Add JWT token to request header for authentication
            var token = User.FindFirst("JwtToken")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            // Call API /api/auth/register
            var response = await client.PostAsJsonAsync($"{API_URL}/auth/register", model);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("User {Username} registered successfully", model.Username);
                TempData["Success"] = "Tạo tài khoản thành công!";
                return RedirectToAction("Index", "Home");
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError("", $"Lỗi: {errorContent}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Register error");
            ModelState.AddModelError("", "Lỗi kết nối: " + ex.Message);
        }

        return View(model);
    }

    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        TempData["Success"] = "Đã đăng xuất!";
        return RedirectToAction("Login");
    }

    public IActionResult AccessDenied()
    {
        return View();
    }
}
