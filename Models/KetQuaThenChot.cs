using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDanhGiaCongViec.Models
{
    [Table("KetQuaThenChot")]
    public class KetQuaThenChot
    {
        [Key]
        public int MaKetQua { get; set; }

        public int? MaMucTieu { get; set; }

        [Required]
        [StringLength(200)]
        public string TieuDe { get; set; } = string.Empty;

        public double GiaTriMucTieu { get; set; }
        public double GiaTriHienTai { get; set; } = 0;

        [StringLength(50)]
        public string? DonVi { get; set; }

        public DateOnly? HanHoanThanh { get; set; }

        [StringLength(20)]
        public string TrangThai { get; set; } = "DangThucHien";

        // Navigation
        [ForeignKey("MaMucTieu")]
        public MucTieuOKR? MucTieuOKR { get; set; }
    }
}