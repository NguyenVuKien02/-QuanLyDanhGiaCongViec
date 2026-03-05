using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyDanhGiaCongViec.Data;
using QuanLyDanhGiaCongViec.Models;
using System.Security.Cryptography;
using System.Text;

namespace QuanLyDanhGiaCongViec.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    public class NguoiDungController : Controller
    {
        private readonly AppDbContext _context;

        public NguoiDungController(AppDbContext context)
        {
            _context = context;
        }

        // Danh sach nhan vien
        public IActionResult Index(int? maPhongBan, string? tuKhoa)
        {
            var query = _context.NguoiDungs
                .Include(n => n.PhongBan)
                .Include(n => n.ChucVu)
                .Include(n => n.VaiTro)
                .AsQueryable();

            if (maPhongBan.HasValue)
                query = query.Where(n => n.MaPhongBan == maPhongBan);

            if (!string.IsNullOrEmpty(tuKhoa))
                query = query.Where(n => n.HoTen.Contains(tuKhoa) || n.Email.Contains(tuKhoa));

            ViewBag.DsNguoiDung = query.OrderBy(n => n.MaPhongBan).ToList();
            ViewBag.DsPhongBan = _context.PhongBans.ToList();
            ViewBag.DsChucVu = _context.ChucVus.ToList();
            ViewBag.DsVaiTro = _context.VaiTros.ToList();
            ViewBag.DsQuanLy = _context.NguoiDungs
                                     .Where(n => n.MaVaiTro != 3)
                                     .ToList();
            ViewBag.MaPhongBanFilter = maPhongBan;
            ViewBag.TuKhoa = tuKhoa;

            return View();
        }

        // Them nhan vien
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult Them(string hoTen, string email, string matKhau,
                                  string? soDienThoai, int? maPhongBan,
                                  int? maChucVu, int? maVaiTro, int? maQuanLy)
        {
            if (_context.NguoiDungs.Any(n => n.Email == email))
            {
                TempData["Error"] = "Email nay da ton tai trong he thong!";
                return RedirectToAction("Index");
            }

            var nguoiDung = new NguoiDung
            {
                HoTen = hoTen,
                Email = email,
                MatKhauHash = HashMD5(matKhau),
                SoDienThoai = soDienThoai,
                MaPhongBan = maPhongBan,
                MaChucVu = maChucVu,
                MaVaiTro = maVaiTro,
                MaQuanLy = maQuanLy,
                TrangThai = true,
                NgayTao = DateTime.Now
            };

            _context.NguoiDungs.Add(nguoiDung);
            _context.SaveChanges();

            TempData["Success"] = "Them nhan vien thanh cong!";
            return RedirectToAction("Index");
        }

        // Lay thong tin de sua
        [HttpGet]
        public IActionResult GetById(int id)
        {
            var nd = _context.NguoiDungs.Find(id);
            if (nd == null) return NotFound();
            return Json(new
            {
                nd.MaNguoiDung,
                nd.HoTen,
                nd.Email,
                nd.SoDienThoai,
                nd.MaPhongBan,
                nd.MaChucVu,
                nd.MaVaiTro,
                nd.MaQuanLy
            });
        }

        // Cap nhat nhan vien
        [HttpPost]
        public IActionResult CapNhat(int maNguoiDung, string hoTen, string email,
                                     string? soDienThoai, int? maPhongBan,
                                     int? maChucVu, int? maVaiTro, int? maQuanLy)
        {
            var nd = _context.NguoiDungs.Find(maNguoiDung);
            if (nd == null)
            {
                TempData["Error"] = "Khong tim thay nhan vien!";
                return RedirectToAction("Index");
            }

            nd.HoTen = hoTen;
            nd.Email = email;
            nd.SoDienThoai = soDienThoai;
            nd.MaPhongBan = maPhongBan;
            nd.MaChucVu = maChucVu;
            nd.MaVaiTro = maVaiTro;
            nd.MaQuanLy = maQuanLy;

            _context.SaveChanges();

            TempData["Success"] = "Cap nhat nhan vien thanh cong!";
            return RedirectToAction("Index");
        }

        // Khoa / Mo khoa tai khoan
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult DoiTrangThai(int id)
        {
            var nd = _context.NguoiDungs.Find(id);
            if (nd == null)
            {
                TempData["Error"] = "Khong tim thay nhan vien!";
                return RedirectToAction("Index");
            }

            nd.TrangThai = !nd.TrangThai;
            _context.SaveChanges();

            TempData["Success"] = nd.TrangThai
                ? "Da mo khoa tai khoan!"
                : "Da khoa tai khoan!";

            return RedirectToAction("Index");
        }

        // Dat lai mat khau ve "123456"
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult DatLaiMatKhau(int id)
        {
            var nd = _context.NguoiDungs.Find(id);
            if (nd == null)
            {
                TempData["Error"] = "Khong tim thay nhan vien!";
                return RedirectToAction("Index");
            }

            nd.MatKhauHash = HashMD5("123456");
            _context.SaveChanges();

            TempData["Success"] = "Da dat lai mat khau ve 123456!";
            return RedirectToAction("Index");
        }

        private string HashMD5(string input)
        {
            using var md5 = MD5.Create();
            byte[] inputBytes = Encoding.ASCII.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);
            return Convert.ToHexString(hashBytes).ToLower();
        }
    }
}