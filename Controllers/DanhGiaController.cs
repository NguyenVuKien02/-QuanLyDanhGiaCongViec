using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyDanhGiaCongViec.Data;
using QuanLyDanhGiaCongViec.Models;
using System.Security.Claims;

namespace QuanLyDanhGiaCongViec.Controllers
{
    [Authorize]
    public class DanhGiaController : Controller
    {
        private readonly AppDbContext _context;

        public DanhGiaController(AppDbContext context)
        {
            _context = context;
        }

        // Danh sach phieu danh gia
        public IActionResult Index(int? maKy, int? maNguoiDung)
        {
            int nguoiDungHT = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            string vaiTro = User.FindFirst(ClaimTypes.Role)!.Value;

            var query = _context.PhieuDanhGias
                .Include(p => p.NguoiDuocDanhGia)
                .Include(p => p.NguoiDanhGia)
                .Include(p => p.KyDanhGia)
                .AsQueryable();

            if (vaiTro == "Employee")
                query = query.Where(p => p.MaNguoiDuocDG == nguoiDungHT);

            if (vaiTro == "Manager")
                query = query.Where(p => p.MaNguoiDanhGia == nguoiDungHT
                                      || p.MaNguoiDuocDG == nguoiDungHT);

            if (maKy.HasValue)
                query = query.Where(p => p.MaKy == maKy);

            if (maNguoiDung.HasValue && vaiTro != "Employee")
                query = query.Where(p => p.MaNguoiDuocDG == maNguoiDung);

            ViewBag.DsPhieu = query.OrderByDescending(p => p.NgayTao).ToList();
            ViewBag.DsKy = _context.KyDanhGias.ToList();
            ViewBag.DsNguoiDung = _context.NguoiDungs.Where(n => n.TrangThai == true).ToList();
            ViewBag.MaKyFilter = maKy;
            ViewBag.MaNDFilter = maNguoiDung;
            ViewBag.VaiTro = vaiTro;
            ViewBag.NguoiDungHT = nguoiDungHT;

            return View();
        }

        // Tao phieu danh gia moi
        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult TaoPhieu(int maNguoiDuocDG, int maKy)
        {
            int nguoiDungHT = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            bool daTonTai = _context.PhieuDanhGias.Any(p =>
                p.MaNguoiDuocDG == maNguoiDuocDG && p.MaKy == maKy);

            if (daTonTai)
            {
                TempData["Error"] = "Nhan vien nay da co phieu danh gia trong ky nay!";
                return RedirectToAction("Index");
            }

            var phieu = new PhieuDanhGia
            {
                MaNguoiDuocDG = maNguoiDuocDG,
                MaNguoiDanhGia = nguoiDungHT,
                MaKy = maKy,
                TrangThai = "NhapLieu",
                NgayTao = DateTime.Now
            };

            _context.PhieuDanhGias.Add(phieu);
            _context.SaveChanges();

            // Tao chi tiet danh gia nang luc tu dong
            var dsNangLuc = _context.NangLucs.ToList();
            foreach (var nl in dsNangLuc)
            {
                _context.ChiTietDanhGiaNangLucs.Add(new ChiTietDanhGiaNangLuc
                {
                    MaPhieu = phieu.MaPhieu,
                    MaNangLuc = nl.MaNangLuc
                });
            }
            _context.SaveChanges();

            TempData["Success"] = "Tao phieu danh gia thanh cong!";
            return RedirectToAction("ChiTiet", new { id = phieu.MaPhieu });
        }

        // Chi tiet phieu danh gia
        public IActionResult ChiTiet(int id)
        {
            int nguoiDungHT = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            string vaiTro = User.FindFirst(ClaimTypes.Role)!.Value;

            var phieu = _context.PhieuDanhGias
                .Include(p => p.NguoiDuocDanhGia)
                .Include(p => p.NguoiDanhGia)
                .Include(p => p.KyDanhGia)
                .Include(p => p.ChiTietDanhGiaNangLucs)
                    .ThenInclude(c => c.NangLuc)
                        .ThenInclude(n => n!.DanhMucNangLuc)
                .FirstOrDefault(p => p.MaPhieu == id);

            if (phieu == null) return NotFound();

            // Lay KPI cua nguoi duoc danh gia trong ky
            var dsKPI = _context.KPINguoiDungs
                .Include(k => k.ChiTieuKPI)
                .Where(k => k.MaNguoiDung == phieu.MaNguoiDuocDG
                         && k.MaKy == phieu.MaKy)
                .ToList();

            // Lay OKR cua nguoi duoc danh gia trong ky
            var dsOKR = _context.MucTieuOKRs
                .Include(o => o.KetQuaThenChots)
                .Where(o => o.MaNguoiSoHuu == phieu.MaNguoiDuocDG
                         && o.MaKy == phieu.MaKy)
                .ToList();

            ViewBag.Phieu = phieu;
            ViewBag.DsKPI = dsKPI;
            ViewBag.DsOKR = dsOKR;
            ViewBag.VaiTro = vaiTro;
            ViewBag.NguoiDungHT = nguoiDungHT;

            // Nhom nang luc theo danh muc
            var nhomNangLuc = phieu.ChiTietDanhGiaNangLucs
                .GroupBy(c => c.NangLuc?.DanhMucNangLuc?.TenDanhMuc)
                .ToList();
            ViewBag.NhomNangLuc = nhomNangLuc;

            return View();
        }

        // Nhan vien tu danh gia
        [HttpPost]
        public IActionResult TuDanhGia(int maPhieu, string nhanXetBanThan,
                                        IFormCollection form)
        {
            int nguoiDungHT = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var phieu = _context.PhieuDanhGias
                .Include(p => p.ChiTietDanhGiaNangLucs)
                .FirstOrDefault(p => p.MaPhieu == maPhieu);

            if (phieu == null || phieu.MaNguoiDuocDG != nguoiDungHT)
            {
                TempData["Error"] = "Khong co quyen thuc hien!";
                return RedirectToAction("Index");
            }

            phieu.NhanXetBanThan = nhanXetBanThan;

            // Cap nhat diem tu danh gia tung nang luc
            foreach (var ct in phieu.ChiTietDanhGiaNangLucs)
            {
                string key = $"self_{ct.MaChiTiet}";
                if (form.ContainsKey(key) &&
                    double.TryParse(form[key], out double diem))
                {
                    ct.DiemTuDanhGia = diem;
                }
            }

            phieu.TrangThai = "DaGui";
            _context.SaveChanges();

            TempData["Success"] = "Da nop phieu tu danh gia thanh cong!";
            return RedirectToAction("ChiTiet", new { id = maPhieu });
        }

        // Quan ly cham diem
        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult ChamDiem(int maPhieu, string nhanXetQuanLy,
                                       IFormCollection form)
        {
            var phieu = _context.PhieuDanhGias
                .Include(p => p.ChiTietDanhGiaNangLucs)
                    .ThenInclude(c => c.NangLuc)
                .FirstOrDefault(p => p.MaPhieu == maPhieu);

            if (phieu == null)
            {
                TempData["Error"] = "Khong tim thay phieu!";
                return RedirectToAction("Index");
            }

            phieu.NhanXetQuanLy = nhanXetQuanLy;

            // Cap nhat diem quan ly va diem chinh thuc
            foreach (var ct in phieu.ChiTietDanhGiaNangLucs)
            {
                string key = $"mgr_{ct.MaChiTiet}";
                if (form.ContainsKey(key) &&
                    double.TryParse(form[key], out double diem))
                {
                    ct.DiemQuanLyCham = diem;
                    ct.DiemChinhThuc = diem; // Diem quan ly la diem chinh thuc
                }
            }

            // Tinh diem nang luc tong hop (co trong so)
            double tongDiemNL = 0;
            double tongTrongSo = 0;
            foreach (var ct in phieu.ChiTietDanhGiaNangLucs)
            {
                if (ct.DiemChinhThuc.HasValue && ct.NangLuc != null)
                {
                    tongDiemNL += ct.DiemChinhThuc.Value * ct.NangLuc.TrongSo;
                    tongTrongSo += ct.NangLuc.TrongSo;
                }
            }
            phieu.DiemNangLuc = tongTrongSo > 0
                ? Math.Round(tongDiemNL / tongTrongSo, 2) : 0;

            // Tinh diem KPI trung binh
            var dsKPI = _context.KPINguoiDungs
                .Where(k => k.MaNguoiDung == phieu.MaNguoiDuocDG
                         && k.MaKy == phieu.MaKy).ToList();

            phieu.DiemKPI = dsKPI.Any()
                ? Math.Round(dsKPI.Average(k =>
                    k.GiaTriMucTieu > 0
                        ? Math.Min((k.GiaTriThucTe / k.GiaTriMucTieu) * 100, 100)
                        : 0), 2)
                : 0;

            // Tinh diem OKR trung binh
            var dsOKR = _context.MucTieuOKRs
                .Where(o => o.MaNguoiSoHuu == phieu.MaNguoiDuocDG
                         && o.MaKy == phieu.MaKy).ToList();

            phieu.DiemOKR = dsOKR.Any()
                ? Math.Round(dsOKR.Average(o => o.TienDo), 2) : 0;

            // Tinh diem tong hop: KPI 40% + OKR 30% + NangLuc 30%
            // Quy doi NangLuc ve thang 100
            double diemNLQuyDoi = (phieu.DiemNangLuc / 10) * 100;
            phieu.DiemTongHop = Math.Round(
                (phieu.DiemKPI * 0.4) +
                (phieu.DiemOKR * 0.3) +
                (diemNLQuyDoi * 0.3), 2);

            // Xep loai tu dong
            phieu.XepLoai = phieu.DiemTongHop >= 90 ? "XuatSac"
                          : phieu.DiemTongHop >= 75 ? "Tot"
                          : phieu.DiemTongHop >= 60 ? "TrungBinh"
                          : "Yeu";

            phieu.TrangThai = "DaDuyet";
            phieu.NgayDuyet = DateTime.Now;
            _context.SaveChanges();

            TempData["Success"] = "Cham diem va duyet phieu thanh cong!";
            return RedirectToAction("ChiTiet", new { id = maPhieu });
        }

        // Xoa phieu
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult Xoa(int id)
        {
            var phieu = _context.PhieuDanhGias
                .Include(p => p.ChiTietDanhGiaNangLucs)
                .FirstOrDefault(p => p.MaPhieu == id);

            if (phieu == null)
            {
                TempData["Error"] = "Khong tim thay phieu!";
                return RedirectToAction("Index");
            }

            _context.ChiTietDanhGiaNangLucs.RemoveRange(phieu.ChiTietDanhGiaNangLucs);
            _context.PhieuDanhGias.Remove(phieu);
            _context.SaveChanges();

            TempData["Success"] = "Xoa phieu danh gia thanh cong!";
            return RedirectToAction("Index");
        }
    }
}