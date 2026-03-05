using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDanhGiaCongViec.Models
{
    [Table("DanhMucKPI")]
    public class DanhMucKPI
    {
        [Key]
        public int MaDanhMuc { get; set; }

        [Required]
        [StringLength(100)]
        public string TenDanhMuc { get; set; } = string.Empty;

        [StringLength(255)]
        public string? MoTa { get; set; }

        // Navigation
        public ICollection<ChiTieuKPI> ChiTieuKPIs { get; set; } = new List<ChiTieuKPI>();
    }
}