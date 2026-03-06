using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyDanhGiaCongViec.Data;
using QuanLyDanhGiaCongViec.Models;
using System.Security.Claims;

namespace QuanLyDanhGiaCongViec.Controllers
{
    [Authorize]
    public class CongViecController : Controller
    {
        private readonly AppDbContext _context;

        public CongViecController(AppDbContext context)
        {
            _context = context;
        }

        // Danh sach cong viec
        public IActionResult Index(string? trangThai, int? maKy, string? tuKhoa)
        {
            int maNguoiDung = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            string vaiTro = User.FindFirst(ClaimTypes.Role)!.Value;

            var query = _context.CongViecs
                .Include(c => c.NguoiDungThucHien)
                .Include(c => c.NguoiDungGiao)
                .Include(c => c.KyDanhGia)
                .AsQueryable();

            // Nhan vien chi xem cong viec cua minh
            if (vaiTro == "Employee")
                query = query.Where(c => c.NguoiThucHien == maNguoiDung);

            // Manager xem cong viec minh giao hoac cua nhom
            if (vaiTro == "Manager")
                query = query.Where(c => c.NguoiGiao == maNguoiDung
                                      || c.NguoiThucHien == maNguoiDung);

            if (!string.IsNullOrEmpty(trangThai))
                query = query.Where(c => c.TrangThai == trangThai);

            if (maKy.HasValue)
                query = query.Where(c => c.MaKy == maKy);

            if (!string.IsNullOrEmpty(tuKhoa))
                query = query.Where(c => c.TenCongViec.Contains(tuKhoa));

            var dsCongViec = query.OrderByDescending(c => c.NgayTao).ToList();

            ViewBag.DsCongViec = dsCongViec;
            ViewBag.ChuaBatDau = dsCongViec.Count(c => c.TrangThai == "ChuaBatDau");
            ViewBag.DangLam = dsCongViec.Count(c => c.TrangThai == "DangLam");
            ViewBag.HoanThanh = dsCongViec.Count(c => c.TrangThai == "HoanThanh");
            ViewBag.HuyBo = dsCongViec.Count(c => c.TrangThai == "HuyBo");
            ViewBag.DsKy = _context.KyDanhGias.ToList();
            ViewBag.DsNguoiDung = _context.NguoiDungs
                                        .Where(n => n.TrangThai == true).ToList();
            ViewBag.TrangThaiFilter = trangThai;
            ViewBag.MaKyFilter = maKy;
            ViewBag.TuKhoa = tuKhoa;
            ViewBag.VaiTro = vaiTro;
            ViewBag.MaNguoiDung = maNguoiDung;

            return View();
        }

        // Them cong viec
        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Them(string tenCongViec, string? moTa, int nguoiThucHien,
                                  int? maKy, string uuTien, string? ngayBatDau,
                                  string? hanHoanThanh, string? ghiChu)
        {
            int nguoiGiao = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var cv = new CongViec
            {
                TenCongViec = tenCongViec,
                MoTa = moTa,
                NguoiThucHien = nguoiThucHien,
                NguoiGiao = nguoiGiao,
                MaKy = maKy,
                UuTien = uuTien,
                TrangThai = "ChuaBatDau",
                NgayBatDau = string.IsNullOrEmpty(ngayBatDau) ? null : DateOnly.Parse(ngayBatDau),
                HanHoanThanh = string.IsNullOrEmpty(hanHoanThanh) ? null : DateOnly.Parse(hanHoanThanh),
                GhiChu = ghiChu,
                NgayTao = DateTime.Now
            };

            _context.CongViecs.Add(cv);
            _context.SaveChanges();

            // Tao thong bao cho nguoi thuc hien
            var tb = new ThongBao
            {
                MaNguoiDung = nguoiThucHien,
                TieuDe = "Ban co cong viec moi!",
                NoiDung = $"Ban duoc giao cong viec: {tenCongViec}",
                LoaiThongBao = "GiaoCongViec",
                DaDoc = false,
                NgayTao = DateTime.Now
            };
            _context.ThongBaos.Add(tb);
            _context.SaveChanges();

            TempData["Success"] = "Them cong viec thanh cong!";
            return RedirectToAction("Index");
        }

        // Lay thong tin de sua
        [HttpGet]
        public IActionResult GetById(int id)
        {
            var cv = _context.CongViecs.Find(id);
            if (cv == null) return NotFound();
            return Json(new
            {
                cv.MaCongViec,
                cv.TenCongViec,
                cv.MoTa,
                cv.NguoiThucHien,
                cv.MaKy,
                cv.UuTien,
                cv.TrangThai,
                cv.GhiChu,
                NgayBatDau = cv.NgayBatDau?.ToString("yyyy-MM-dd"),
                HanHoanThanh = cv.HanHoanThanh?.ToString("yyyy-MM-dd")
            });
        }

        // Cap nhat cong viec
        [HttpPost]
        public IActionResult CapNhat(int maCongViec, string tenCongViec, string? moTa,
                                     int? nguoiThucHien, int? maKy, string uuTien,
                                     string trangThai, string? ngayBatDau,
                                     string? hanHoanThanh, string? ghiChu)
        {
            var cv = _context.CongViecs.Find(maCongViec);
            if (cv == null)
            {
                TempData["Error"] = "Khong tim thay cong viec!";
                return RedirectToAction("Index");
            }

            cv.TenCongViec = tenCongViec;
            cv.MoTa = moTa;
            cv.NguoiThucHien = nguoiThucHien;
            cv.MaKy = maKy;
            cv.UuTien = uuTien;
            cv.TrangThai = trangThai;
            cv.GhiChu = ghiChu;
            cv.NgayBatDau = string.IsNullOrEmpty(ngayBatDau) ? null : DateOnly.Parse(ngayBatDau);
            cv.HanHoanThanh = string.IsNullOrEmpty(hanHoanThanh) ? null : DateOnly.Parse(hanHoanThanh);

            if (trangThai == "HoanThanh" && cv.NgayHoanThanh == null)
                cv.NgayHoanThanh = DateOnly.FromDateTime(DateTime.Now);

            _context.SaveChanges();

            TempData["Success"] = "Cap nhat cong viec thanh cong!";
            return RedirectToAction("Index");
        }

        // Cap nhat tien do nhanh
        [HttpPost]
        public IActionResult CapNhatTienDo(int id, int tienDo)
        {
            var cv = _context.CongViecs.Find(id);
            if (cv == null) return Json(new { success = false });

            cv.TienDo = tienDo;
            if (tienDo == 100)
            {
                cv.TrangThai = "HoanThanh";
                cv.NgayHoanThanh = DateOnly.FromDateTime(DateTime.Now);
            }
            else if (tienDo > 0)
            {
                cv.TrangThai = "DangLam";
            }

            _context.SaveChanges();
            return Json(new { success = true });
        }

        // Xoa cong viec
        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Xoa(int id)
        {
            var cv = _context.CongViecs.Find(id);
            if (cv == null)
            {
                TempData["Error"] = "Khong tim thay cong viec!";
                return RedirectToAction("Index");
            }

            _context.CongViecs.Remove(cv);
            _context.SaveChanges();

            TempData["Success"] = "Xoa cong viec thanh cong!";
            return RedirectToAction("Index");
        }
    }
}