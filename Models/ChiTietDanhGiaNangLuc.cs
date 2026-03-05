using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDanhGiaCongViec.Models
{
    [Table("ChiTietDanhGiaNangLuc")]
    public class ChiTietDanhGiaNangLuc
    {
        [Key]
        public int MaChiTiet { get; set; }

        public int? MaPhieu { get; set; }
        public int? MaNangLuc { get; set; }

        public double? DiemTuDanhGia { get; set; }
        public double? DiemQuanLyCham { get; set; }
        public double? DiemChinhThuc { get; set; }

        [StringLength(255)]
        public string? NhanXet { get; set; }

        // Navigation
        [ForeignKey("MaPhieu")]
        public PhieuDanhGia? PhieuDanhGia { get; set; }

        [ForeignKey("MaNangLuc")]
        public NangLuc? NangLuc { get; set; }
    }
}