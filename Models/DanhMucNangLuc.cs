using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDanhGiaCongViec.Models
{
    [Table("DanhMucNangLuc")]
    public class DanhMucNangLuc
    {
        [Key]
        public int MaDanhMucNL { get; set; }

        [Required]
        [StringLength(100)]
        public string TenDanhMuc { get; set; } = string.Empty;

        [StringLength(255)]
        public string? MoTa { get; set; }

        // Navigation
        public ICollection<NangLuc> NangLucs { get; set; } = new List<NangLuc>();
    }
}