using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDanhGiaCongViec.Models
{
    [Table("CongViec")]
    public class CongViec
    {
        [Key]
        public int MaCongViec { get; set; }

        [Required]
        [StringLength(200)]
        public string TenCongViec { get; set; } = string.Empty;

        public string? MoTa { get; set; }

        public int? NguoiThucHien { get; set; }
        public int? NguoiGiao { get; set; }
        public int? MaKy { get; set; }

        [StringLength(20)]
        public string UuTien { get; set; } = "TrungBinh";

        [StringLength(20)]
        public string TrangThai { get; set; } = "ChuaBatDau";

        public DateOnly? NgayBatDau { get; set; }
        public DateOnly? HanHoanThanh { get; set; }
        public DateOnly? NgayHoanThanh { get; set; }

        public int TienDo { get; set; } = 0;
        public string? GhiChu { get; set; }
        public DateTime NgayTao { get; set; } = DateTime.Now;

        // Navigation
        [ForeignKey("NguoiThucHien")]
        public NguoiDung? NguoiDungThucHien { get; set; }

        [ForeignKey("NguoiGiao")]
        public NguoiDung? NguoiDungGiao { get; set; }

        [ForeignKey("MaKy")]
        public KyDanhGia? KyDanhGia { get; set; }
    }
}