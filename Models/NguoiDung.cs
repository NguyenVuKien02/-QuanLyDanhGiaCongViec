using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDanhGiaCongViec.Models
{
    [Table("NguoiDung")]
    public class NguoiDung
    {
        [Key]
        public int MaNguoiDung { get; set; }

        [Required]
        [StringLength(100)]
        public string HoTen { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string MatKhauHash { get; set; } = string.Empty;

        [StringLength(20)]
        public string? SoDienThoai { get; set; }

        [StringLength(255)]
        public string? AnhDaiDien { get; set; }

        public int? MaPhongBan { get; set; }
        public int? MaChucVu { get; set; }
        public int? MaVaiTro { get; set; }
        public int? MaQuanLy { get; set; }

        public bool TrangThai { get; set; } = true;
        public DateTime NgayTao { get; set; } = DateTime.Now;

        // Navigation
        [ForeignKey("MaPhongBan")]
        public PhongBan? PhongBan { get; set; }

        [ForeignKey("MaChucVu")]
        public ChucVu? ChucVu { get; set; }

        [ForeignKey("MaVaiTro")]
        public VaiTro? VaiTro { get; set; }

        [ForeignKey("MaQuanLy")]
        public NguoiDung? QuanLy { get; set; }

        public ICollection<NguoiDung> NhanVienCap { get; set; } = new List<NguoiDung>();
        public ICollection<CongViec> CongViecDuocGiao { get; set; } = new List<CongViec>();
        public ICollection<CongViec> CongViecDaGiao { get; set; } = new List<CongViec>();
        public ICollection<ThongBao> ThongBaos { get; set; } = new List<ThongBao>();
    }
}