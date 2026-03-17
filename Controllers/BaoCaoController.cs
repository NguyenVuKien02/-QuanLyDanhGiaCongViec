using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyDanhGiaCongViec.Data;

namespace QuanLyDanhGiaCongViec.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    public class BaoCaoController : Controller
    {
        private readonly AppDbContext _context;

        public BaoCaoController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(int? maKy)
        {
            // Mac dinh lay ky dau tien
            if (!maKy.HasValue)
                maKy = _context.KyDanhGias.FirstOrDefault()?.MaKy;

            // Tong quan
            ViewBag.TongNhanVien = _context.NguoiDungs
                .Count(n => n.TrangThai == true);
            ViewBag.TongCongViec = _context.CongViecs.Count();
            ViewBag.CvHoanThanh = _context.CongViecs
                .Count(c => c.TrangThai == "HoanThanh");
            ViewBag.TiLeHT = ViewBag.TongCongViec > 0
                ? Math.Round((double)ViewBag.CvHoanThanh
                    / ViewBag.TongCongViec * 100, 1)
                : 0;

            // Bao cao theo ky
            ViewBag.DsKy = _context.KyDanhGias
                .OrderByDescending(k => k.NgayBatDau).ToList();
            ViewBag.MaKy = maKy;
            ViewBag.TenKy = _context.KyDanhGias
                .FirstOrDefault(k => k.MaKy == maKy)?.TenKy;

            // Danh gia theo ky
            var dsPhieu = _context.PhieuDanhGias
                .Include(p => p.NguoiDuocDanhGia)
                    .ThenInclude(n => n!.PhongBan)
                .Where(p => p.MaKy == maKy && p.TrangThai == "DaDuyet")
                .ToList();

            ViewBag.DsPhieu = dsPhieu;

            // Xep loai
            ViewBag.SoXuatSac = dsPhieu.Count(p => p.XepLoai == "XuatSac");
            ViewBag.SoTot = dsPhieu.Count(p => p.XepLoai == "Tot");
            ViewBag.SoTrungBinh = dsPhieu.Count(p => p.XepLoai == "TrungBinh");
            ViewBag.SoYeu = dsPhieu.Count(p => p.XepLoai == "Yeu");

            ViewBag.DiemTBKy = dsPhieu.Any()
                ? Math.Round(dsPhieu.Average(p => p.DiemTongHop), 1) : 0;

            // Thong ke theo phong ban - tai du lieu truoc roi tinh toan trong bo nho
            var dsPhongBan = _context.PhongBans.ToList();
            var dsNguoiDung = _context.NguoiDungs.ToList();
            var dsCongViec = _context.CongViecs.ToList();

            ViewBag.ThongKePhongBan = dsPhongBan.Select(pb => {
                var nvTrongPB = dsNguoiDung
                    .Where(n => n.MaPhongBan == pb.MaPhongBan)
                    .Select(n => n.MaNguoiDung)
                    .ToList();

                var phieuTrongPB = dsPhieu
                    .Where(p => p.NguoiDuocDanhGia != null
                             && nvTrongPB.Contains(p.NguoiDuocDanhGia.MaNguoiDung))
                    .ToList();

                return new
                {
                    pb.TenPhongBan,
                    SoNV = nvTrongPB.Count,
                    SoCvHT = dsCongViec
                        .Count(c => c.TrangThai == "HoanThanh"
                                 && c.NguoiThucHien.HasValue
                                 && nvTrongPB.Contains(c.NguoiThucHien.Value)),
                    DiemTB = phieuTrongPB.Any()
                        ? Math.Round(phieuTrongPB.Average(p => p.DiemTongHop), 1)
                        : 0.0
                };
            }).ToList();

            return View();
        }
    }
}