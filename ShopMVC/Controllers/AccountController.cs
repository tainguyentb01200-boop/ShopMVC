using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopMVC.Helpers;
using ShopMVC.Models;
using ShopMVC.Services.Interfaces;
using ShopMVC.ViewModels;
using System.Security.Claims;

namespace ShopMVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ICartService _cartService;

        public AccountController(
            IAuthService authService,
            ICartService cartService)
        {
            _authService = authService;
            _cartService = cartService;
        }

        /// <summary>
        /// Trang đăng nhập
        /// GET: /Account/Login
        /// </summary>
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            // Nếu đã đăng nhập thì redirect về trang chủ
            if (HttpContext.Session.IsLoggedIn())
            {
                return RedirectToAction("Index", "Home");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        /// <summary>
        /// Xử lý đăng nhập
        /// POST: /Account/Login
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // 1. Kiểm tra thông tin đăng nhập
                var user = await _authService.LoginAsync(model.Username, model.Password);

                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Tên đăng nhập hoặc mật khẩu không đúng");
                    return View(model);
                }

                // 2. Tạo danh sách Claims (Thông tin lưu trong Cookie)
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, user.Role), // Quan trọng để phân quyền [Authorize(Roles="...")]
                    new Claim("UserId", user.UserId.ToString()),
                    new Claim("FullName", user.FullName)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                // 3. Cấu hình thời gian sống của Cookie (Remember Me)
                var authProperties = new AuthenticationProperties
                {
                    // Nếu người dùng tích chọn RememberMe => Cookie lưu ổ cứng (Persistent)
                    // Nếu không => Cookie chỉ sống trong phiên (Session cookie)
                    IsPersistent = model.RememberMe,
                    ExpiresUtc = DateTime.UtcNow.AddDays(30) // Hết hạn sau 30 ngày
                };

                // 4. Ghi Cookie xuống trình duyệt (Đăng nhập hệ thống)
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    claimsPrincipal,
                    authProperties);


                // 5. Lưu thông tin vào Session (Giữ lại code cũ để tương thích với view _LoginPartial, Cart...)
                HttpContext.Session.SetUserSession(
                    user.UserId,
                    user.Username,
                    user.FullName,
                    user.Role);

                // Cập nhật số lượng giỏ hàng vào Session
                var cartCount = await _cartService.GetCartItemCountAsync(user.UserId);
                HttpContext.Session.SetCartCount(cartCount);

                TempData["Success"] = $"Chào mừng {user.FullName}!";

                // 6. Điều hướng (Redirect)
                // Ưu tiên ReturnUrl nếu có
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                // Phân quyền điều hướng
                if (user.Role == "Admin" || user.Role == "Staff")
                {
                    return RedirectToAction("Index", "Admin");
                }

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Lỗi: {ex.Message}");
                return View(model);
            }
        }

        /// <summary>
        /// Trang đăng ký
        /// GET: /Account/Register
        /// </summary>
        [HttpGet]
        public IActionResult Register()
        {
            if (HttpContext.Session.IsLoggedIn())
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        /// <summary>
        /// Xử lý đăng ký
        /// POST: /Account/Register
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Kiểm tra username đã tồn tại
                if (await _authService.UsernameExistsAsync(model.Username))
                {
                    ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại");
                    return View(model);
                }

                // Kiểm tra email đã tồn tại
                if (await _authService.EmailExistsAsync(model.Email))
                {
                    ModelState.AddModelError("Email", "Email đã được sử dụng");
                    return View(model);
                }

                // Tạo user mới
                var user = new User
                {
                    Username = model.Username,
                    FullName = model.FullName,
                    Email = model.Email,
                    Phone = model.Phone,
                    Address = model.Address,
                    Role = "Customer" // Mặc định là Customer
                };

                var result = await _authService.RegisterAsync(user, model.Password);

                if (result)
                {
                    TempData["Success"] = "Đăng ký thành công! Vui lòng đăng nhập";
                    return RedirectToAction(nameof(Login));
                }

                ModelState.AddModelError(string.Empty, "Không thể tạo tài khoản. Vui lòng thử lại");
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Lỗi: {ex.Message}");
                return View(model);
            }
        }

        /// <summary>
        /// Đăng xuất
        /// POST: /Account/Logout
        /// </summary>
        

        // HOẶC nếu muốn hỗ trợ cả GET và POST:
        [HttpGet]
        [HttpPost]
        public IActionResult Logout()
        {
            // Xóa tất cả session
            HttpContext.Session.Clear();

            // Xóa cookie đăng nhập
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme).Wait();

            TempData["Success"] = "Đăng xuất thành công!";
            return RedirectToAction("Login");
        }

        /// <summary>
        /// Trang thông tin cá nhân
        /// GET: /Account/Profile
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            if (!HttpContext.Session.IsLoggedIn())
            {
                return RedirectToAction(nameof(Login));
            }

            var userId = HttpContext.Session.GetUserId()!.Value;
            var user = await _authService.GetUserByIdAsync(userId);

            if (user == null)
            {
                TempData["Error"] = "Không tìm thấy thông tin người dùng";
                return RedirectToAction("Index", "Home");
            }

            return View(user);
        }

        /// <summary>
        /// Cập nhật thông tin cá nhân
        /// POST: /Account/Profile
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(User model)
        {
            if (!HttpContext.Session.IsLoggedIn())
            {
                return RedirectToAction(nameof(Login));
            }

            var userId = HttpContext.Session.GetUserId()!.Value;

            // Chỉ cho phép update một số field
            ModelState.Remove("Password");
            ModelState.Remove("Username");
            ModelState.Remove("Role");

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                model.UserId = userId;
                var result = await _authService.UpdateProfileAsync(model);

                if (result)
                {
                    // Cập nhật FullName trong session
                    HttpContext.Session.SetString(SessionHelper.FULL_NAME, model.FullName);

                    TempData["Success"] = "Cập nhật thông tin thành công";
                    return RedirectToAction(nameof(Profile));
                }

                TempData["Error"] = "Không thể cập nhật thông tin";
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi: {ex.Message}";
                return View(model);
            }
        }

        /// <summary>
        /// Trang đổi mật khẩu
        /// GET: /Account/ChangePassword
        /// </summary>
        [HttpGet]
        public IActionResult ChangePassword()
        {
            if (!HttpContext.Session.IsLoggedIn())
            {
                return RedirectToAction(nameof(Login));
            }

            return View();
        }

        /// <summary>
        /// Xử lý đổi mật khẩu
        /// POST: /Account/ChangePassword
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string oldPassword, string newPassword, string confirmPassword)
        {
            if (!HttpContext.Session.IsLoggedIn())
            {
                return RedirectToAction(nameof(Login));
            }

            if (string.IsNullOrEmpty(oldPassword) || string.IsNullOrEmpty(newPassword))
            {
                TempData["Error"] = "Vui lòng nhập đầy đủ thông tin";
                return View();
            }

            if (newPassword != confirmPassword)
            {
                TempData["Error"] = "Mật khẩu mới không khớp";
                return View();
            }

            if (newPassword.Length < 6)
            {
                TempData["Error"] = "Mật khẩu mới phải có ít nhất 6 ký tự";
                return View();
            }

            try
            {
                var userId = HttpContext.Session.GetUserId()!.Value;
                var result = await _authService.ChangePasswordAsync(userId, oldPassword, newPassword);

                if (result)
                {
                    TempData["Success"] = "Đổi mật khẩu thành công";
                    return RedirectToAction(nameof(Profile));
                }

                TempData["Error"] = "Mật khẩu cũ không đúng";
                return View();
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi: {ex.Message}";
                return View();
            }
        }

        // POST: Account/UpdateProfile (NẾU CHƯA CÓ)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(User model)
        {
            if (!HttpContext.Session.IsLoggedIn())
            {
                return RedirectToAction(nameof(Login));
            }

            if (string.IsNullOrEmpty(model.FullName) || string.IsNullOrEmpty(model.Email))
            {
                TempData["Error"] = "Vui lòng nhập đầy đủ thông tin";
                return RedirectToAction(nameof(Profile));
            }

            try
            {
                var userId = HttpContext.Session.GetUserId()!.Value;
                model.UserId = userId; // Set UserId vào model

                var result = await _authService.UpdateProfileAsync(model);

                if (result)
                {
                    // Update session
                    HttpContext.Session.SetString("FullName", model.FullName);

                    TempData["Success"] = "Cập nhật thông tin thành công";
                    return RedirectToAction(nameof(Profile));
                }

                TempData["Error"] = "Không thể cập nhật thông tin";
                return RedirectToAction(nameof(Profile));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi: {ex.Message}";
                return RedirectToAction(nameof(Profile));
            }
        }

        /// <summary>
        /// Kiểm tra username có tồn tại không (AJAX)
        /// GET: /Account/CheckUsername?username=abc
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> CheckUsername(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                return Json(new { available = false });
            }

            var exists = await _authService.UsernameExistsAsync(username);
            return Json(new { available = !exists });
        }

        /// <summary>
        /// Kiểm tra email có tồn tại không (AJAX)
        /// GET: /Account/CheckEmail?email=test@mail.com
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> CheckEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return Json(new { available = false });
            }

            var exists = await _authService.EmailExistsAsync(email);
            return Json(new { available = !exists });
        }
    }
}