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
        private const int TRANG_KICh_THUOC = 4;

        public CongViecController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(string? trangThai, int? thang, int? nam,
                                   string? tuKhoa, int trangChuaBatDau = 1,
                                   int trangDangLam = 1, int trangHoanThanh = 1,
                                   int trangHuyBo = 1)
        {
            int maNguoiDung = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            string vaiTro = User.FindFirst(ClaimTypes.Role)!.Value;

            var query = _context.CongViecs
                .Include(c => c.NguoiDungThucHien)
                .Include(c => c.NguoiDungGiao)
                .Include(c => c.KyDanhGia)
                .AsQueryable();

            if (vaiTro == "Employee")
                query = query.Where(c => c.NguoiThucHien == maNguoiDung);
            if (vaiTro == "Manager")
                query = query.Where(c => c.NguoiGiao == maNguoiDung
                                      || c.NguoiThucHien == maNguoiDung);

            // Tim kiem theo ten cong viec VA ten nguoi thuc hien
            if (!string.IsNullOrEmpty(tuKhoa))
                query = query.Where(c =>
                    c.TenCongViec.Contains(tuKhoa) ||
                    (c.NguoiDungThucHien != null &&
                     c.NguoiDungThucHien.HoTen.Contains(tuKhoa)));

            // Loc theo thang
            if (thang.HasValue && thang > 0)
            {
                int namLoc = nam ?? DateTime.Now.Year;
                query = query.Where(c =>
                    c.NgayBatDau.HasValue &&
                    c.NgayBatDau.Value.Month == thang &&
                    c.NgayBatDau.Value.Year == namLoc);
            }

            // Loc theo hoan thanh hay chua
            if (!string.IsNullOrEmpty(trangThai))
                query = query.Where(c => c.TrangThai == trangThai);

            var dsTatCa = query.OrderByDescending(c => c.NgayTao).ToList();

            // Phan trang tung cot
            var chuaBatDauList = dsTatCa.Where(c => c.TrangThai == "ChuaBatDau").ToList();
            var dangLamList = dsTatCa.Where(c => c.TrangThai == "DangLam").ToList();
            var hoanThanhList = dsTatCa.Where(c => c.TrangThai == "HoanThanh").ToList();
            var huyBoList = dsTatCa.Where(c => c.TrangThai == "HuyBo").ToList();

            ViewBag.ChuaBatDauList = chuaBatDauList
                .Skip((trangChuaBatDau - 1) * TRANG_KICh_THUOC)
                .Take(TRANG_KICh_THUOC).ToList();
            ViewBag.DangLamList = dangLamList
                .Skip((trangDangLam - 1) * TRANG_KICh_THUOC)
                .Take(TRANG_KICh_THUOC).ToList();
            ViewBag.HoanThanhList = hoanThanhList
                .Skip((trangHoanThanh - 1) * TRANG_KICh_THUOC)
                .Take(TRANG_KICh_THUOC).ToList();
            ViewBag.HuyBoList = huyBoList
                .Skip((trangHuyBo - 1) * TRANG_KICh_THUOC)
                .Take(TRANG_KICh_THUOC).ToList();

            // So trang tung cot
            ViewBag.TongTrangCBD = (int)Math.Ceiling(chuaBatDauList.Count / (double)TRANG_KICh_THUOC);
            ViewBag.TongTrangDL = (int)Math.Ceiling(dangLamList.Count / (double)TRANG_KICh_THUOC);
            ViewBag.TongTrangHT = (int)Math.Ceiling(hoanThanhList.Count / (double)TRANG_KICh_THUOC);
            ViewBag.TongTrangHB = (int)Math.Ceiling(huyBoList.Count / (double)TRANG_KICh_THUOC);

            ViewBag.TrangCBD = trangChuaBatDau;
            ViewBag.TrangDL = trangDangLam;
            ViewBag.TrangHT = trangHoanThanh;
            ViewBag.TrangHB = trangHuyBo;

            // Thong ke
            ViewBag.ChuaBatDau = chuaBatDauList.Count;
            ViewBag.DangLam = dangLamList.Count;
            ViewBag.HoanThanh = hoanThanhList.Count;
            ViewBag.HuyBo = huyBoList.Count;

            ViewBag.DsNguoiDung = _context.NguoiDungs.Where(n => n.TrangThai == true).ToList();
            ViewBag.DsKy = _context.KyDanhGias.ToList();
            ViewBag.TrangThaiFilter = trangThai;
            ViewBag.ThangFilter = thang;
            ViewBag.NamFilter = nam ?? DateTime.Now.Year;
            ViewBag.TuKhoa = tuKhoa;
            ViewBag.VaiTro = vaiTro;
            ViewBag.MaNguoiDung = maNguoiDung;
            ViewBag.NamHienTai = DateTime.Now.Year;

            return View();
        }

        // Chi tiet cong viec
        public IActionResult ChiTiet(int id)
        {
            var cv = _context.CongViecs
                .Include(c => c.NguoiDungThucHien)
                .Include(c => c.NguoiDungGiao)
                .Include(c => c.KyDanhGia)
                .FirstOrDefault(c => c.MaCongViec == id);

            if (cv == null) return NotFound();
            return View(cv);
        }

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

            _context.ThongBaos.Add(new ThongBao
            {
                MaNguoiDung = nguoiThucHien,
                TieuDe = "Bạn có công việc mới!",
                NoiDung = $"Bạn được giao công việc: {tenCongViec}",
                LoaiThongBao = "GiaoCongViec",
                DaDoc = false,
                NgayTao = DateTime.Now
            });
            _context.SaveChanges();

            TempData["Success"] = "Thêm công việc thành công!";
            return RedirectToAction("Index");
        }

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

        [HttpPost]
        public IActionResult CapNhat(int maCongViec, string tenCongViec, string? moTa,
                                     int? nguoiThucHien, int? maKy, string uuTien,
                                     string trangThai, string? ngayBatDau,
                                     string? hanHoanThanh, string? ghiChu)
        {
            var cv = _context.CongViecs.Find(maCongViec);
            if (cv == null)
            {
                TempData["Error"] = "Không tìm thấy công việc!";
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

            TempData["Success"] = "Cập nhật công việc thành công!";
            return RedirectToAction("Index");
        }

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
                cv.TrangThai = "DangLam";

            _context.SaveChanges();
            return Json(new { success = true });
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Xoa(int id)
        {
            var cv = _context.CongViecs.Find(id);
            if (cv == null)
            {
                TempData["Error"] = "Không tìm thấy công việc!";
                return RedirectToAction("Index");
            }

            _context.CongViecs.Remove(cv);
            _context.SaveChanges();

            TempData["Success"] = "Xóa công việc thành công!";
            return RedirectToAction("Index");
        }
    }
}