using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDanhGiaCongViec.Models
{
    [Table("MucTieuOKR")]
    public class MucTieuOKR
    {
        [Key]
        public int MaMucTieu { get; set; }

        [Required]
        [StringLength(200)]
        public string TieuDe { get; set; } = string.Empty;

        public string? MoTa { get; set; }

        public int? MaNguoiSoHuu { get; set; }
        public int? MaKy { get; set; }

        [StringLength(20)]
        public string TrangThai { get; set; } = "DangTot";

        public int TienDo { get; set; } = 0;
        public DateTime NgayTao { get; set; } = DateTime.Now;

        // Navigation
        [ForeignKey("MaNguoiSoHuu")]
        public NguoiDung? NguoiSoHuu { get; set; }

        [ForeignKey("MaKy")]
        public KyDanhGia? KyDanhGia { get; set; }

        public ICollection<KetQuaThenChot> KetQuaThenChots { get; set; } = new List<KetQuaThenChot>();
    }
}