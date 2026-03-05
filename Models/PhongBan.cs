using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDanhGiaCongViec.Models
{
    [Table("PhongBan")]
    public class PhongBan
    {
        [Key]
        public int MaPhongBan { get; set; }

        [Required]
        [StringLength(100)]
        public string TenPhongBan { get; set; } = string.Empty;

        [StringLength(255)]
        public string? MoTa { get; set; }

        public DateTime NgayTao { get; set; } = DateTime.Now;

        // Navigation
        public ICollection<NguoiDung> NguoiDungs { get; set; } = new List<NguoiDung>();
    }
}