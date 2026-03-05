using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDanhGiaCongViec.Models
{
    [Table("PhieuDanhGia")]
    public class PhieuDanhGia
    {
        [Key]
        public int MaPhieu { get; set; }

        public int? MaNguoiDuocDG { get; set; }
        public int? MaNguoiDanhGia { get; set; }
        public int? MaKy { get; set; }

        public double DiemKPI { get; set; } = 0;
        public double DiemOKR { get; set; } = 0;
        public double DiemNangLuc { get; set; } = 0;
        public double DiemTongHop { get; set; } = 0;

        [StringLength(20)]
        public string? XepLoai { get; set; }

        public string? NhanXetBanThan { get; set; }
        public string? NhanXetQuanLy { get; set; }

        [StringLength(20)]
        public string TrangThai { get; set; } = "NhapLieu";

        public DateTime NgayTao { get; set; } = DateTime.Now;
        public DateTime? NgayDuyet { get; set; }

        // Navigation
        [ForeignKey("MaNguoiDuocDG")]
        public NguoiDung? NguoiDuocDanhGia { get; set; }

        [ForeignKey("MaNguoiDanhGia")]
        public NguoiDung? NguoiDanhGia { get; set; }

        [ForeignKey("MaKy")]
        public KyDanhGia? KyDanhGia { get; set; }

        public ICollection<ChiTietDanhGiaNangLuc> ChiTietDanhGiaNangLucs { get; set; } = new List<ChiTietDanhGiaNangLuc>();
    }
}