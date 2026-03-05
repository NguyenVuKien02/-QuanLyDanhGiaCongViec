using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDanhGiaCongViec.Models
{
    [Table("NangLuc")]
    public class NangLuc
    {
        [Key]
        public int MaNangLuc { get; set; }

        public int? MaDanhMucNL { get; set; }

        [Required]
        [StringLength(100)]
        public string TenNangLuc { get; set; } = string.Empty;

        public int DiemToiDa { get; set; } = 10;
        public double TrongSo { get; set; } = 1.0;

        [StringLength(255)]
        public string? MoTa { get; set; }

        // Navigation
        [ForeignKey("MaDanhMucNL")]
        public DanhMucNangLuc? DanhMucNangLuc { get; set; }

        public ICollection<ChiTietDanhGiaNangLuc> ChiTietDanhGias { get; set; } = new List<ChiTietDanhGiaNangLuc>();
    }
}