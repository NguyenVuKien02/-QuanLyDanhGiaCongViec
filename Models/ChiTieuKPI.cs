using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDanhGiaCongViec.Models
{
    [Table("ChiTieuKPI")]
    public class ChiTieuKPI
    {
        [Key]
        public int MaKPI { get; set; }

        [Required]
        [StringLength(200)]
        public string TenKPI { get; set; } = string.Empty;

        public int? MaDanhMuc { get; set; }

        [StringLength(50)]
        public string? DonVi { get; set; }

        public double GiaTriMucTieu { get; set; }
        public double TrongSo { get; set; } = 1.0;

        [StringLength(255)]
        public string? MoTa { get; set; }

        // Navigation
        [ForeignKey("MaDanhMuc")]
        public DanhMucKPI? DanhMucKPI { get; set; }

        public ICollection<KPINguoiDung> KPINguoiDungs { get; set; } = new List<KPINguoiDung>();
    }
}