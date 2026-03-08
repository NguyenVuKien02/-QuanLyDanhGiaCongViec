using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyDanhGiaCongViec.Data;
using System.Security.Claims;

namespace QuanLyDanhGiaCongViec.Controllers
{
    [Authorize]
    public class ThongBaoController : Controller
    {
        private readonly AppDbContext _context;

        public ThongBaoController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            int maNguoiDung = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var ds = _context.ThongBaos
                .Where(t => t.MaNguoiDung == maNguoiDung)
                .OrderByDescending(t => t.NgayTao)
                .ToList();

            // Danh dau tat ca da doc
            foreach (var tb in ds.Where(t => !t.DaDoc))
                tb.DaDoc = true;
            _context.SaveChanges();

            ViewBag.DsThongBao = ds;
            return View();
        }

        // Xoa thong bao
        [HttpPost]
        public IActionResult Xoa(int id)
        {
            var tb = _context.ThongBaos.Find(id);
            if (tb != null)
            {
                _context.ThongBaos.Remove(tb);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        // Xoa tat ca
        [HttpPost]
        public IActionResult XoaTatCa()
        {
            int maNguoiDung = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var ds = _context.ThongBaos
                .Where(t => t.MaNguoiDung == maNguoiDung).ToList();

            _context.ThongBaos.RemoveRange(ds);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        // API lay so thong bao chua doc (dung cho badge tren navbar)
        [HttpGet]
        public IActionResult SoChuaDoc()
        {
            int maNguoiDung = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            int so = _context.ThongBaos
                .Count(t => t.MaNguoiDung == maNguoiDung && t.DaDoc == false);

            return Json(new { so });
        }
    }
}