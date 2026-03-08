using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyDanhGiaCongViec.Data;
using QuanLyDanhGiaCongViec.Models;
using System.Security.Claims;

namespace QuanLyDanhGiaCongViec.Controllers
{
    [Authorize]
    public class KPIController : Controller
    {
        private readonly AppDbContext _context;

        public KPIController(AppDbContext context)
        {
            _context = context;
        }

        // ===================== DANH MUC KPI =====================
        public IActionResult Index(int? maKy, int? maNguoiDung)
        {
            int nguoiDungHienTai = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            string vaiTro = User.FindFirst(ClaimTypes.Role)!.Value;

            // Mac dinh loc theo nguoi dung hien tai neu la Employee
            if (vaiTro == "Employee")
                maNguoiDung = nguoiDungHienTai;

            var query = _context.KPINguoiDungs
                .Include(k => k.ChiTieuKPI)
                    .ThenInclude(c => c!.DanhMucKPI)
                .Include(k => k.NguoiDung)
                .Include(k => k.KyDanhGia)
                .AsQueryable();

            if (maKy.HasValue)
                query = query.Where(k => k.MaKy == maKy);

            if (maNguoiDung.HasValue)
                query = query.Where(k => k.MaNguoiDung == maNguoiDung);

            var dsKPINguoiDung = query.OrderBy(k => k.MaKy)
                                      .ThenBy(k => k.MaNguoiDung)
                                      .ToList();

            ViewBag.DsKPINguoiDung = dsKPINguoiDung;
            ViewBag.DsKy = _context.KyDanhGias.ToList();
            ViewBag.DsNguoiDung = _context.NguoiDungs.Where(n => n.TrangThai == true).ToList();
            ViewBag.DsChiTieuKPI = _context.ChiTieuKPIs
                                        .Include(c => c.DanhMucKPI).ToList();
            ViewBag.DsDanhMucKPI = _context.DanhMucKPIs.ToList();
            ViewBag.MaKyFilter = maKy;
            ViewBag.MaNguoiDungFilter = maNguoiDung;
            ViewBag.VaiTro = vaiTro;
            ViewBag.NguoiDungHienTai = nguoiDungHienTai;

            return View();
        }

        // Giao KPI cho nhan vien
        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult GiaoKPI(int maNguoiDung, int maKPI, int maKy,
                                     double giaTriMucTieu, string? ghiChu)
        {
            // Kiem tra da co chua
            bool daTonTai = _context.KPINguoiDungs.Any(k =>
                k.MaNguoiDung == maNguoiDung &&
                k.MaKPI == maKPI &&
                k.MaKy == maKy);

            if (daTonTai)
            {
                TempData["Error"] = "Nhan vien nay da co KPI nay trong ky do!";
                return RedirectToAction("Index");
            }

            var kpiNd = new KPINguoiDung
            {
                MaNguoiDung = maNguoiDung,
                MaKPI = maKPI,
                MaKy = maKy,
                GiaTriMucTieu = giaTriMucTieu,
                GiaTriThucTe = 0,
                GhiChu = ghiChu,
                NgayTao = DateTime.Now
            };

            _context.KPINguoiDungs.Add(kpiNd);
            _context.SaveChanges();

            TempData["Success"] = "Giao KPI thanh cong!";
            return RedirectToAction("Index");
        }

        // Nhan vien cap nhat ket qua thuc te
        [HttpPost]
        public IActionResult CapNhatKetQua(int maKPINguoiDung, double giaTriThucTe, string? ghiChu)
        {
            int nguoiDungHienTai = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            string vaiTro = User.FindFirst(ClaimTypes.Role)!.Value;

            var kpi = _context.KPINguoiDungs.Find(maKPINguoiDung);
            if (kpi == null)
            {
                TempData["Error"] = "Khong tim thay KPI!";
                return RedirectToAction("Index");
            }

            // Nhan vien chi duoc cap nhat KPI cua minh
            if (vaiTro == "Employee" && kpi.MaNguoiDung != nguoiDungHienTai)
            {
                TempData["Error"] = "Ban khong co quyen cap nhat KPI nay!";
                return RedirectToAction("Index");
            }

            kpi.GiaTriThucTe = giaTriThucTe;
            kpi.GhiChu = ghiChu;
            _context.SaveChanges();

            TempData["Success"] = "Cap nhat ket qua KPI thanh cong!";
            return RedirectToAction("Index");
        }

        // Lay thong tin KPI nguoi dung
        [HttpGet]
        public IActionResult GetKPIById(int id)
        {
            var kpi = _context.KPINguoiDungs
                .Include(k => k.ChiTieuKPI)
                .FirstOrDefault(k => k.MaKPINguoiDung == id);

            if (kpi == null) return NotFound();

            return Json(new
            {
                kpi.MaKPINguoiDung,
                kpi.MaNguoiDung,
                kpi.MaKPI,
                kpi.MaKy,
                kpi.GiaTriMucTieu,
                kpi.GiaTriThucTe,
                kpi.GhiChu,
                TenKPI = kpi.ChiTieuKPI?.TenKPI
            });
        }

        // Xoa KPI
        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Xoa(int id)
        {
            var kpi = _context.KPINguoiDungs.Find(id);
            if (kpi == null)
            {
                TempData["Error"] = "Khong tim thay KPI!";
                return RedirectToAction("Index");
            }

            _context.KPINguoiDungs.Remove(kpi);
            _context.SaveChanges();

            TempData["Success"] = "Xoa KPI thanh cong!";
            return RedirectToAction("Index");
        }

        // Quan ly chi tieu KPI (Admin)
        [Authorize(Roles = "Admin")]
        public IActionResult QuanLyChiTieu()
        {
            ViewBag.DsChiTieuKPI = _context.ChiTieuKPIs
                .Include(c => c.DanhMucKPI).ToList();
            ViewBag.DsDanhMucKPI = _context.DanhMucKPIs.ToList();
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult ThemChiTieu(string tenKPI, int maDanhMuc,
                                          string? donVi, double giaTriMucTieu,
                                          double trongSo, string? moTa)
        {
            var ct = new ChiTieuKPI
            {
                TenKPI = tenKPI,
                MaDanhMuc = maDanhMuc,
                DonVi = donVi,
                GiaTriMucTieu = giaTriMucTieu,
                TrongSo = trongSo,
                MoTa = moTa
            };

            _context.ChiTieuKPIs.Add(ct);
            _context.SaveChanges();

            TempData["Success"] = "Them chi tieu KPI thanh cong!";
            return RedirectToAction("QuanLyChiTieu");
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult XoaChiTieu(int id)
        {
            var ct = _context.ChiTieuKPIs.Find(id);
            if (ct == null)
            {
                TempData["Error"] = "Khong tim thay chi tieu!";
                return RedirectToAction("QuanLyChiTieu");
            }

            bool dangDung = _context.KPINguoiDungs.Any(k => k.MaKPI == id);
            if (dangDung)
            {
                TempData["Error"] = "Khong the xoa! Chi tieu nay dang duoc su dung.";
                return RedirectToAction("QuanLyChiTieu");
            }

            _context.ChiTieuKPIs.Remove(ct);
            _context.SaveChanges();

            TempData["Success"] = "Xoa chi tieu thanh cong!";
            return RedirectToAction("QuanLyChiTieu");
        }
    }
}