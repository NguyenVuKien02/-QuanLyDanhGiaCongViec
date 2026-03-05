using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDanhGiaCongViec.Models
{
    [Table("KPINguoiDung")]
    public class KPINguoiDung
    {
        [Key]
        public int MaKPINguoiDung { get; set; }

        public int? MaNguoiDung { get; set; }
        public int? MaKPI { get; set; }
        public int? MaKy { get; set; }

        public double GiaTriMucTieu { get; set; }
        public double GiaTriThucTe { get; set; } = 0;

        [StringLength(255)]
        public string? GhiChu { get; set; }

        public DateTime NgayTao { get; set; } = DateTime.Now;

        // Navigation
        [ForeignKey("MaNguoiDung")]
        public NguoiDung? NguoiDung { get; set; }

        [ForeignKey("MaKPI")]
        public ChiTieuKPI? ChiTieuKPI { get; set; }

        [ForeignKey("MaKy")]
        public KyDanhGia? KyDanhGia { get; set; }
    }
}