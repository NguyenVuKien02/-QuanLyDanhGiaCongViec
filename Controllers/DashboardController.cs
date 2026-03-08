using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyDanhGiaCongViec.Data;
using System.Security.Claims;

namespace QuanLyDanhGiaCongViec.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            int maNguoiDung = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            string vaiTro = User.FindFirst(ClaimTypes.Role)!.Value;

            // ===== THONG KE CONG VIEC =====
            var queryCv = _context.CongViecs.AsQueryable();
            if (vaiTro == "Employee")
                queryCv = queryCv.Where(c => c.NguoiThucHien == maNguoiDung);
            if (vaiTro == "Manager")
                queryCv = queryCv.Where(c => c.NguoiGiao == maNguoiDung
                                          || c.NguoiThucHien == maNguoiDung);

            ViewBag.TongCongViec = queryCv.Count();
            ViewBag.CvDangLam = queryCv.Count(c => c.TrangThai == "DangLam");
            ViewBag.CvHoanThanh = queryCv.Count(c => c.TrangThai == "HoanThanh");
            ViewBag.CvQuaHan = queryCv.Count(c =>
                c.HanHoanThanh < DateOnly.FromDateTime(DateTime.Now)
                && c.TrangThai != "HoanThanh");

            // ===== THONG KE KPI =====
            var queryKpi = _context.KPINguoiDungs.AsQueryable();
            if (vaiTro == "Employee")
                queryKpi = queryKpi.Where(k => k.MaNguoiDung == maNguoiDung);

            var dsKpi = queryKpi.ToList();
            ViewBag.KpiTrungBinh = dsKpi.Any()
                ? Math.Round(dsKpi.Average(k =>
                    k.GiaTriMucTieu > 0
                        ? Math.Min((k.GiaTriThucTe / k.GiaTriMucTieu) * 100, 100)
                        : 0), 1)
                : 0;

            // ===== THONG KE OKR =====
            var queryOkr = _context.MucTieuOKRs.AsQueryable();
            if (vaiTro == "Employee")
                queryOkr = queryOkr.Where(o => o.MaNguoiSoHuu == maNguoiDung);

            var dsOkr = queryOkr.ToList();
            ViewBag.OkrTrungBinh = dsOkr.Any()
                ? Math.Round(dsOkr.Average(o => o.TienDo), 1) : 0;

            // ===== THONG BAO CHUA DOC =====
            ViewBag.SoThongBao = _context.ThongBaos
                .Count(t => t.MaNguoiDung == maNguoiDung && t.DaDoc == false);

            // ===== DANH SACH CV SAP DEN HAN (7 ngay) =====
            var sapDenHan = DateOnly.FromDateTime(DateTime.Now.AddDays(7));
            var homNay = DateOnly.FromDateTime(DateTime.Now);

            var querySapHan = _context.CongViecs
                .Include(c => c.NguoiDungThucHien)
                .Where(c => c.HanHoanThanh >= homNay
                         && c.HanHoanThanh <= sapDenHan
                         && c.TrangThai != "HoanThanh");

            if (vaiTro == "Employee")
                querySapHan = querySapHan.Where(c => c.NguoiThucHien == maNguoiDung);
            if (vaiTro == "Manager")
                querySapHan = querySapHan.Where(c => c.NguoiGiao == maNguoiDung
                                                  || c.NguoiThucHien == maNguoiDung);

            ViewBag.DsCvSapHan = querySapHan
                .OrderBy(c => c.HanHoanThanh).Take(5).ToList();

            // ===== ADMIN: THONG KE THEO PHONG BAN =====
            if (vaiTro == "Admin" || vaiTro == "Manager")
            {
                ViewBag.TongNhanVien = _context.NguoiDungs
                    .Count(n => n.TrangThai == true);
                ViewBag.TongPhongBan = _context.PhongBans.Count();

                ViewBag.ThongKePhongBan = _context.PhongBans
                    .Select(p => new {
                        p.TenPhongBan,
                        SoNV = _context.NguoiDungs
                            .Count(n => n.MaPhongBan == p.MaPhongBan)
                    }).ToList();

                // Bieu do cong viec theo trang thai
                ViewBag.BieuDoCv = new
                {
                    ChuaBatDau = _context.CongViecs
                        .Count(c => c.TrangThai == "ChuaBatDau"),
                    DangLam = _context.CongViecs
                        .Count(c => c.TrangThai == "DangLam"),
                    HoanThanh = _context.CongViecs
                        .Count(c => c.TrangThai == "HoanThanh"),
                    HuyBo = _context.CongViecs
                        .Count(c => c.TrangThai == "HuyBo")
                };

                // Top 5 nhan vien co diem KPI cao nhat
                ViewBag.TopKPI = _context.KPINguoiDungs
                    .Include(k => k.NguoiDung)
                    .ToList()
                    .GroupBy(k => k.NguoiDung?.HoTen)
                    .Select(g => new {
                        HoTen = g.Key,
                        DiemKPI = g.Any()
                            ? Math.Round(g.Average(k =>
                                k.GiaTriMucTieu > 0
                                    ? Math.Min((k.GiaTriThucTe / k.GiaTriMucTieu) * 100, 100)
                                    : 0), 1)
                            : 0
                    })
                    .OrderByDescending(x => x.DiemKPI)
                    .Take(5).ToList();
            }

            ViewBag.VaiTro = vaiTro;
            ViewBag.HoTen = User.Identity?.Name;
            ViewBag.MaNguoiDung = maNguoiDung;

            return View();
        }
    }
}