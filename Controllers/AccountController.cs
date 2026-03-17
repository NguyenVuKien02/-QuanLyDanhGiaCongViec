using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyDanhGiaCongViec.Data;
using QuanLyDanhGiaCongViec.Models;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace QuanLyDanhGiaCongViec.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        // ===================== LOGIN =====================
        [HttpGet]
        public IActionResult Login()
        {
            // Neu da dang nhap roi thi chuyen thang vao Dashboard
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Dashboard");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string matKhau)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(matKhau))
            {
                ViewBag.Error = "Vui long nhap day du email va mat khau!";
                return View();
            }

            // Hash mat khau MD5 de so sanh
            string matKhauHash = HashMD5(matKhau);

            // Tim nguoi dung trong DB
            var nguoiDung = _context.NguoiDungs
                .FirstOrDefault(n => n.Email == email
                                  && n.MatKhauHash == matKhauHash
                                  && n.TrangThai == true);

            if (nguoiDung == null)
            {
                ViewBag.Error = "Email hoac mat khau khong dung!";
                return View();
            }

            // Lay ten vai tro
            var vaiTro = _context.VaiTros
                .FirstOrDefault(v => v.MaVaiTro == nguoiDung.MaVaiTro);

            string tenVaiTro = vaiTro?.TenVaiTro ?? "Employee";

            // Tao Claims (thong tin luu trong cookie)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, nguoiDung.MaNguoiDung.ToString()),
                new Claim(ClaimTypes.Name, nguoiDung.HoTen),
                new Claim(ClaimTypes.Email, nguoiDung.Email),
                new Claim(ClaimTypes.Role, tenVaiTro),
                new Claim("MaVaiTro", nguoiDung.MaVaiTro.ToString() ?? "0"),
                new Claim("MaPhongBan", nguoiDung.MaPhongBan.ToString() ?? "0"),
                new Claim("AnhDaiDien", nguoiDung.AnhDaiDien ?? "/images/default-avatar.png")
            };

            var claimsIdentity = new ClaimsIdentity(claims, "CookieAuth");
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            await HttpContext.SignInAsync("CookieAuth", claimsPrincipal);

            // Redirect theo vai tro
            return tenVaiTro switch
            {
                "Admin" => RedirectToAction("Index", "Dashboard"),
                "Manager" => RedirectToAction("Index", "Dashboard"),
                "Employee" => RedirectToAction("Index", "Dashboard"),
                _ => RedirectToAction("Index", "Dashboard")
            };
        }

        // ===================== LOGOUT =====================
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("CookieAuth");
            return RedirectToAction("Login");
        }

        // ===================== ACCESS DENIED =====================
        public IActionResult AccessDenied()
        {
            return View();
        }

        // ===================== HELPER =====================
        private string HashMD5(string input)
        {
            using var md5 = MD5.Create();
            byte[] inputBytes = Encoding.ASCII.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);
            return Convert.ToHexString(hashBytes).ToLower();
        }

        // GET - Hien thi form doi mat khau
        [HttpGet]
        [Authorize]
        public IActionResult DoiMatKhau()
        {
            return View();
        }

        // POST - Xu ly doi mat khau
        [HttpPost]
        [Authorize]
        public IActionResult DoiMatKhau(string matKhauCu, string matKhauMoi, string xacNhanMatKhau)
        {
            int maNguoiDung = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            if (string.IsNullOrEmpty(matKhauCu) ||
                string.IsNullOrEmpty(matKhauMoi) ||
                string.IsNullOrEmpty(xacNhanMatKhau))
            {
                ViewBag.Error = "Vui long nhap day du thong tin!";
                return View();
            }

            if (matKhauMoi != xacNhanMatKhau)
            {
                ViewBag.Error = "Mat khau moi va xac nhan khong khop!";
                return View();
            }

            if (matKhauMoi.Length < 6)
            {
                ViewBag.Error = "Mat khau moi phai co it nhat 6 ky tu!";
                return View();
            }

            var nguoiDung = _context.NguoiDungs.Find(maNguoiDung);
            if (nguoiDung == null)
            {
                ViewBag.Error = "Khong tim thay tai khoan!";
                return View();
            }

            // Kiem tra mat khau cu
            if (nguoiDung.MatKhauHash != HashMD5(matKhauCu))
            {
                ViewBag.Error = "Mat khau cu khong dung!";
                return View();
            }

            // Cap nhat mat khau moi
            nguoiDung.MatKhauHash = HashMD5(matKhauMoi);
            _context.SaveChanges();

            ViewBag.Success = "Doi mat khau thanh cong! Vui long dang nhap lai.";
            return View();
        }
    }
}