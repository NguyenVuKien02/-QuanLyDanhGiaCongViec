using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyDanhGiaCongViec.Data;
using QuanLyDanhGiaCongViec.Models;

namespace QuanLyDanhGiaCongViec.Controllers
{
    [Authorize(Roles = "Admin")]
    public class PhongBanController : Controller
    {
        private readonly AppDbContext _context;

        public PhongBanController(AppDbContext context)
        {
            _context = context;
        }

        // Danh sach phong ban
        public IActionResult Index()
        {
            var dsPhongBan = _context.PhongBans
                .Select(p => new
                {
                    p.MaPhongBan,
                    p.TenPhongBan,
                    p.MoTa,
                    p.NgayTao,
                    SoNhanVien = _context.NguoiDungs.Count(n => n.MaPhongBan == p.MaPhongBan)
                }).ToList();

            ViewBag.DsPhongBan = dsPhongBan;
            return View();
        }

        // Them moi
        [HttpPost]
        public IActionResult Them(string tenPhongBan, string moTa)
        {
            if (string.IsNullOrEmpty(tenPhongBan))
            {
                TempData["Error"] = "Ten phong ban khong duoc de trong!";
                return RedirectToAction("Index");
            }

            var phongBan = new PhongBan
            {
                TenPhongBan = tenPhongBan,
                MoTa = moTa,
                NgayTao = DateTime.Now
            };

            _context.PhongBans.Add(phongBan);
            _context.SaveChanges();

            TempData["Success"] = "Them phong ban thanh cong!";
            return RedirectToAction("Index");
        }

        // Lay du lieu de sua (tra ve JSON)
        [HttpGet]
        public IActionResult GetById(int id)
        {
            var pb = _context.PhongBans.Find(id);
            if (pb == null) return NotFound();
            return Json(new { pb.MaPhongBan, pb.TenPhongBan, pb.MoTa });
        }

        // Cap nhat
        [HttpPost]
        public IActionResult CapNhat(int maPhongBan, string tenPhongBan, string moTa)
        {
            var pb = _context.PhongBans.Find(maPhongBan);
            if (pb == null)
            {
                TempData["Error"] = "Khong tim thay phong ban!";
                return RedirectToAction("Index");
            }

            pb.TenPhongBan = tenPhongBan;
            pb.MoTa = moTa;
            _context.SaveChanges();

            TempData["Success"] = "Cap nhat phong ban thanh cong!";
            return RedirectToAction("Index");
        }

        // Xoa
        [HttpPost]
        public IActionResult Xoa(int id)
        {
            var pb = _context.PhongBans.Find(id);
            if (pb == null)
            {
                TempData["Error"] = "Khong tim thay phong ban!";
                return RedirectToAction("Index");
            }

            bool coNhanVien = _context.NguoiDungs.Any(n => n.MaPhongBan == id);
            if (coNhanVien)
            {
                TempData["Error"] = "Khong the xoa! Phong ban nay con nhan vien.";
                return RedirectToAction("Index");
            }

            _context.PhongBans.Remove(pb);
            _context.SaveChanges();

            TempData["Success"] = "Xoa phong ban thanh cong!";
            return RedirectToAction("Index");
        }
    }
}