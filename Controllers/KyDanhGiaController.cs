using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyDanhGiaCongViec.Data;
using QuanLyDanhGiaCongViec.Models;

namespace QuanLyDanhGiaCongViec.Controllers
{
    [Authorize(Roles = "Admin")]
    public class KyDanhGiaController : Controller
    {
        private readonly AppDbContext _context;

        public KyDanhGiaController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var ds = _context.KyDanhGias
                .OrderByDescending(k => k.NgayBatDau)
                .ToList();

            ViewBag.DsKy = ds;
            return View();
        }

        [HttpPost]
        public IActionResult Them(string tenKy, string loaiKy,
                                   string ngayBatDau, string ngayKetThuc)
        {
            var ky = new KyDanhGia
            {
                TenKy = tenKy,
                LoaiKy = loaiKy,
                NgayBatDau = DateOnly.Parse(ngayBatDau),
                NgayKetThuc = DateOnly.Parse(ngayKetThuc),
                TrangThai = "DangMo",
                NgayTao = DateTime.Now
            };

            _context.KyDanhGias.Add(ky);
            _context.SaveChanges();

            TempData["Success"] = "Them ky danh gia thanh cong!";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult GetById(int id)
        {
            var ky = _context.KyDanhGias.Find(id);
            if (ky == null) return NotFound();
            return Json(new
            {
                ky.MaKy,
                ky.TenKy,
                ky.LoaiKy,
                NgayBatDau = ky.NgayBatDau.ToString("yyyy-MM-dd"),
                NgayKetThuc = ky.NgayKetThuc.ToString("yyyy-MM-dd"),
                ky.TrangThai
            });
        }

        [HttpPost]
        public IActionResult CapNhat(int maKy, string tenKy, string loaiKy,
                                      string ngayBatDau, string ngayKetThuc,
                                      string trangThai)
        {
            var ky = _context.KyDanhGias.Find(maKy);
            if (ky == null)
            {
                TempData["Error"] = "Khong tim thay ky danh gia!";
                return RedirectToAction("Index");
            }

            ky.TenKy = tenKy;
            ky.LoaiKy = loaiKy;
            ky.NgayBatDau = DateOnly.Parse(ngayBatDau);
            ky.NgayKetThuc = DateOnly.Parse(ngayKetThuc);
            ky.TrangThai = trangThai;
            _context.SaveChanges();

            TempData["Success"] = "Cap nhat ky danh gia thanh cong!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult DoiTrangThai(int id)
        {
            var ky = _context.KyDanhGias.Find(id);
            if (ky == null)
            {
                TempData["Error"] = "Khong tim thay ky!";
                return RedirectToAction("Index");
            }

            ky.TrangThai = ky.TrangThai == "DangMo" ? "DaDong" : "DangMo";
            _context.SaveChanges();

            TempData["Success"] = ky.TrangThai == "DangMo"
                ? "Da mo lai ky danh gia!"
                : "Da dong ky danh gia!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Xoa(int id)
        {
            var ky = _context.KyDanhGias.Find(id);
            if (ky == null)
            {
                TempData["Error"] = "Khong tim thay ky!";
                return RedirectToAction("Index");
            }

            bool dangDung = _context.CongViecs.Any(c => c.MaKy == id)
                         || _context.KPINguoiDungs.Any(k => k.MaKy == id)
                         || _context.MucTieuOKRs.Any(o => o.MaKy == id)
                         || _context.PhieuDanhGias.Any(p => p.MaKy == id);

            if (dangDung)
            {
                TempData["Error"] = "Khong the xoa! Ky nay dang co du lieu lien quan.";
                return RedirectToAction("Index");
            }

            _context.KyDanhGias.Remove(ky);
            _context.SaveChanges();

            TempData["Success"] = "Xoa ky danh gia thanh cong!";
            return RedirectToAction("Index");
        }
    }
}