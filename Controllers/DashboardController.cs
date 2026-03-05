using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace QuanLyDanhGiaCongViec.Controllers
{
    [Authorize] // Bat buoc phai dang nhap moi vao duoc
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            // Lay thong tin nguoi dung tu Claims
            ViewBag.HoTen = User.Identity?.Name;
            ViewBag.VaiTro = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            ViewBag.Email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

            return View();
        }
    }
}