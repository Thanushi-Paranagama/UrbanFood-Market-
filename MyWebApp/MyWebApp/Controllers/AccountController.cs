using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

public class AccountController : Controller
{
    // Mock user database with hashed passwords
    private static List<User> users = new List<User>
    {
    new User { Id = 1, Username = "admin", PasswordHash = HashPassword("admin123"), Role = "Admin" },
    new User { Id = 2, Username = "farmer", PasswordHash = HashPassword("farmer123"), Role = "Farmer" }
    };

    public IActionResult Login() => View();

    [HttpPost]
    public async Task<IActionResult> Login(string username, string password)
    {
        try
        {
            // Add a debug line to check if this method is being hit
            // System.Diagnostics.Debug.WriteLine($"Login attempt: {username}");

            var user = users.FirstOrDefault(u => u.Username == username);
            if (user == null || !VerifyPassword(password, user.PasswordHash))
            {
                ViewBag.Error = "Invalid username or password";
                return View();
            }

            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, user.Username),
        new Claim(ClaimTypes.Role, user.Role)
    };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            // Debug the role
            // System.Diagnostics.Debug.WriteLine($"User role: {user.Role}");

            // Proper redirection based on role
            if (user.Role == "Admin")
            {
                return RedirectToAction("Dashboard", "Admin");
            }
            else if (user.Role == "Farmer")
            {
                return RedirectToAction("Dashboard", "Farmer");
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }
        catch (Exception ex)
        {
            // Log the exception
            System.Diagnostics.Debug.WriteLine($"Login error: {ex.Message}");
            ViewBag.Error = "An error occurred during login";
            return View();
        }
    }

    public async Task<IActionResult> Logout()
	{
		await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
		return RedirectToAction("Login");
	}

   

    public IActionResult AccessDenied() => View();

    public IActionResult Guest()
    {
        return View("~/Views/Home/Guest.cshtml"); // Full path to the view
    }



    // Password Hashing
    private static string HashPassword(string password)
	{
		using (SHA256 sha256 = SHA256.Create())
		{
			byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
			return Convert.ToBase64String(bytes);
		}
	}

	private static bool VerifyPassword(string enteredPassword, string storedHash)
	{
		return HashPassword(enteredPassword) == storedHash;
	}
}

// Secure User Model
public class User
{
	public int Id { get; set; }
	public string Username { get; set; }
	public string PasswordHash { get; set; }
	public string Role { get; set; }

}

