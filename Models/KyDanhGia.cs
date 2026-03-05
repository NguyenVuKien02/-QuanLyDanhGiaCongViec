using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDanhGiaCongViec.Models
{
    [Table("KyDanhGia")]
    public class KyDanhGia
    {
        [Key]
        public int MaKy { get; set; }

        [Required]
        [StringLength(100)]
        public string TenKy { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string LoaiKy { get; set; } = string.Empty;

        public DateOnly NgayBatDau { get; set; }
        public DateOnly NgayKetThuc { get; set; }

        [StringLength(20)]
        public string TrangThai { get; set; } = "DangMo";

        public DateTime NgayTao { get; set; } = DateTime.Now;

        // Navigation
        public ICollection<CongViec> CongViecs { get; set; } = new List<CongViec>();
        public ICollection<KPINguoiDung> KPINguoiDungs { get; set; } = new List<KPINguoiDung>();
        public ICollection<MucTieuOKR> MucTieuOKRs { get; set; } = new List<MucTieuOKR>();
        public ICollection<PhieuDanhGia> PhieuDanhGias { get; set; } = new List<PhieuDanhGia>();
    }
}