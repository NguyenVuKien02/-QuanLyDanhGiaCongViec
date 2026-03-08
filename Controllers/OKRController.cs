using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyDanhGiaCongViec.Data;
using QuanLyDanhGiaCongViec.Models;
using System.Security.Claims;

namespace QuanLyDanhGiaCongViec.Controllers
{
    [Authorize]
    public class OKRController : Controller
    {
        private readonly AppDbContext _context;

        public OKRController(AppDbContext context)
        {
            _context = context;
        }

        // Danh sach OKR
        public IActionResult Index(int? maKy, int? maNguoiDung)
        {
            int nguoiDungHienTai = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            string vaiTro = User.FindFirst(ClaimTypes.Role)!.Value;

            if (vaiTro == "Employee")
                maNguoiDung = nguoiDungHienTai;

            var query = _context.MucTieuOKRs
                .Include(o => o.NguoiSoHuu)
                .Include(o => o.KyDanhGia)
                .Include(o => o.KetQuaThenChots)
                .AsQueryable();

            if (maKy.HasValue)
                query = query.Where(o => o.MaKy == maKy);

            if (maNguoiDung.HasValue)
                query = query.Where(o => o.MaNguoiSoHuu == maNguoiDung);

            var dsMucTieu = query.OrderByDescending(o => o.NgayTao).ToList();

            // Tinh tien do tung muc tieu tu key results
            foreach (var mt in dsMucTieu)
            {
                if (mt.KetQuaThenChots.Any())
                {
                    double tongTienDo = mt.KetQuaThenChots.Average(kr =>
                        kr.GiaTriMucTieu > 0
                            ? Math.Min((kr.GiaTriHienTai / kr.GiaTriMucTieu) * 100, 100)
                            : 0);
                    mt.TienDo = (int)Math.Round(tongTienDo);
                }
            }

            ViewBag.DsMucTieu = dsMucTieu;
            ViewBag.DsKy = _context.KyDanhGias.ToList();
            ViewBag.DsNguoiDung = _context.NguoiDungs.Where(n => n.TrangThai == true).ToList();
            ViewBag.MaKyFilter = maKy;
            ViewBag.MaNguoiDungFilter = maNguoiDung;
            ViewBag.VaiTro = vaiTro;
            ViewBag.NguoiDungHT = nguoiDungHienTai;

            return View();
        }

        // Them muc tieu OKR
        [HttpPost]
        public IActionResult ThemMucTieu(string tieuDe, string? moTa,
                                          int? maKy, int? maNguoiSoHuu)
        {
            int nguoiDungHienTai = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            string vaiTro = User.FindFirst(ClaimTypes.Role)!.Value;

            // Employee tu tao OKR cho chinh minh
            if (vaiTro == "Employee")
                maNguoiSoHuu = nguoiDungHienTai;

            var mucTieu = new MucTieuOKR
            {
                TieuDe = tieuDe,
                MoTa = moTa,
                MaKy = maKy,
                MaNguoiSoHuu = maNguoiSoHuu ?? nguoiDungHienTai,
                TrangThai = "DangTot",
                TienDo = 0,
                NgayTao = DateTime.Now
            };

            _context.MucTieuOKRs.Add(mucTieu);
            _context.SaveChanges();

            TempData["Success"] = "Them muc tieu OKR thanh cong!";
            return RedirectToAction("Index");
        }

        // Cap nhat muc tieu
        [HttpPost]
        public IActionResult CapNhatMucTieu(int maMucTieu, string tieuDe,
                                             string? moTa, int? maKy, string trangThai)
        {
            var mt = _context.MucTieuOKRs.Find(maMucTieu);
            if (mt == null)
            {
                TempData["Error"] = "Khong tim thay muc tieu!";
                return RedirectToAction("Index");
            }

            mt.TieuDe = tieuDe;
            mt.MoTa = moTa;
            mt.MaKy = maKy;
            mt.TrangThai = trangThai;
            _context.SaveChanges();

            TempData["Success"] = "Cap nhat muc tieu thanh cong!";
            return RedirectToAction("Index");
        }

        // Xoa muc tieu
        [HttpPost]
        public IActionResult XoaMucTieu(int id)
        {
            var mt = _context.MucTieuOKRs
                .Include(o => o.KetQuaThenChots)
                .FirstOrDefault(o => o.MaMucTieu == id);

            if (mt == null)
            {
                TempData["Error"] = "Khong tim thay muc tieu!";
                return RedirectToAction("Index");
            }

            _context.KetQuaThenChots.RemoveRange(mt.KetQuaThenChots);
            _context.MucTieuOKRs.Remove(mt);
            _context.SaveChanges();

            TempData["Success"] = "Xoa muc tieu thanh cong!";
            return RedirectToAction("Index");
        }

        // Lay thong tin muc tieu
        [HttpGet]
        public IActionResult GetMucTieuById(int id)
        {
            var mt = _context.MucTieuOKRs.Find(id);
            if (mt == null) return NotFound();
            return Json(new
            {
                mt.MaMucTieu,
                mt.TieuDe,
                mt.MoTa,
                mt.MaKy,
                mt.TrangThai
            });
        }

        // Them Key Result
        [HttpPost]
        public IActionResult ThemKeyResult(int maMucTieu, string tieuDe,
                                            double giaTriMucTieu, string? donVi,
                                            string? hanHoanThanh)
        {
            var kr = new KetQuaThenChot
            {
                MaMucTieu = maMucTieu,
                TieuDe = tieuDe,
                GiaTriMucTieu = giaTriMucTieu,
                GiaTriHienTai = 0,
                DonVi = donVi,
                HanHoanThanh = string.IsNullOrEmpty(hanHoanThanh)
                                    ? null : DateOnly.Parse(hanHoanThanh),
                TrangThai = "DangThucHien"
            };

            _context.KetQuaThenChots.Add(kr);
            _context.SaveChanges();

            TempData["Success"] = "Them Key Result thanh cong!";
            return RedirectToAction("ChiTiet", new { id = maMucTieu });
        }

        // Cap nhat tien do Key Result
        [HttpPost]
        public IActionResult CapNhatKeyResult(int maKetQua, double giaTriHienTai)
        {
            var kr = _context.KetQuaThenChots.Find(maKetQua);
            if (kr == null) return Json(new { success = false });

            kr.GiaTriHienTai = giaTriHienTai;

            if (kr.GiaTriHienTai >= kr.GiaTriMucTieu)
                kr.TrangThai = "HoanThanh";
            else
                kr.TrangThai = "DangThucHien";

            _context.SaveChanges();

            // Cap nhat tien do muc tieu cha
            var mucTieu = _context.MucTieuOKRs
                .Include(o => o.KetQuaThenChots)
                .FirstOrDefault(o => o.MaMucTieu == kr.MaMucTieu);

            if (mucTieu != null && mucTieu.KetQuaThenChots.Any())
            {
                double tongTienDo = mucTieu.KetQuaThenChots.Average(k =>
                    k.GiaTriMucTieu > 0
                        ? Math.Min((k.GiaTriHienTai / k.GiaTriMucTieu) * 100, 100)
                        : 0);
                mucTieu.TienDo = (int)Math.Round(tongTienDo);
                _context.SaveChanges();
            }

            return Json(new { success = true });
        }

        // Xoa Key Result
        [HttpPost]
        public IActionResult XoaKeyResult(int id, int maMucTieu)
        {
            var kr = _context.KetQuaThenChots.Find(id);
            if (kr != null)
            {
                _context.KetQuaThenChots.Remove(kr);
                _context.SaveChanges();
                TempData["Success"] = "Xoa Key Result thanh cong!";
            }
            return RedirectToAction("ChiTiet", new { id = maMucTieu });
        }

        // Chi tiet 1 OKR
        public IActionResult ChiTiet(int id)
        {
            var mucTieu = _context.MucTieuOKRs
                .Include(o => o.NguoiSoHuu)
                .Include(o => o.KyDanhGia)
                .Include(o => o.KetQuaThenChots)
                .FirstOrDefault(o => o.MaMucTieu == id);

            if (mucTieu == null) return NotFound();

            // Tinh lai tien do
            if (mucTieu.KetQuaThenChots.Any())
            {
                double tongTienDo = mucTieu.KetQuaThenChots.Average(kr =>
                    kr.GiaTriMucTieu > 0
                        ? Math.Min((kr.GiaTriHienTai / kr.GiaTriMucTieu) * 100, 100)
                        : 0);
                mucTieu.TienDo = (int)Math.Round(tongTienDo);
            }

            ViewBag.MucTieu = mucTieu;
            return View();
        }
    }
}