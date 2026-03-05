using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDanhGiaCongViec.Models
{
    [Table("ChucVu")]
    public class ChucVu
    {
        [Key]
        public int MaChucVu { get; set; }

        [Required]
        [StringLength(100)]
        public string TenChucVu { get; set; } = string.Empty;

        public int CapBac { get; set; } = 1;

        [StringLength(255)]
        public string? MoTa { get; set; }

        // Navigation
        public ICollection<NguoiDung> NguoiDungs { get; set; } = new List<NguoiDung>();
    }
}