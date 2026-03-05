using Microsoft.EntityFrameworkCore;
using QuanLyDanhGiaCongViec.Models;

namespace QuanLyDanhGiaCongViec.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<PhongBan> PhongBans { get; set; }
        public DbSet<ChucVu> ChucVus { get; set; }
        public DbSet<VaiTro> VaiTros { get; set; }
        public DbSet<NguoiDung> NguoiDungs { get; set; }
        public DbSet<KyDanhGia> KyDanhGias { get; set; }
        public DbSet<CongViec> CongViecs { get; set; }
        public DbSet<DanhMucKPI> DanhMucKPIs { get; set; }
        public DbSet<ChiTieuKPI> ChiTieuKPIs { get; set; }
        public DbSet<KPINguoiDung> KPINguoiDungs { get; set; }
        public DbSet<MucTieuOKR> MucTieuOKRs { get; set; }
        public DbSet<KetQuaThenChot> KetQuaThenChots { get; set; }
        public DbSet<DanhMucNangLuc> DanhMucNangLucs { get; set; }
        public DbSet<NangLuc> NangLucs { get; set; }
        public DbSet<PhieuDanhGia> PhieuDanhGias { get; set; }
        public DbSet<ChiTietDanhGiaNangLuc> ChiTietDanhGiaNangLucs { get; set; }
        public DbSet<ThongBao> ThongBaos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // NguoiDung - tu tham chieu (MaQuanLy)
            modelBuilder.Entity<NguoiDung>()
                .HasOne(n => n.QuanLy)
                .WithMany(n => n.NhanVienCap)
                .HasForeignKey(n => n.MaQuanLy)
                .OnDelete(DeleteBehavior.Restrict);

            // CongViec - 2 khoa ngoai den NguoiDung
            modelBuilder.Entity<CongViec>()
                .HasOne(c => c.NguoiDungThucHien)
                .WithMany(n => n.CongViecDuocGiao)
                .HasForeignKey(c => c.NguoiThucHien)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CongViec>()
                .HasOne(c => c.NguoiDungGiao)
                .WithMany(n => n.CongViecDaGiao)
                .HasForeignKey(c => c.NguoiGiao)
                .OnDelete(DeleteBehavior.Restrict);

            // PhieuDanhGia - 2 khoa ngoai den NguoiDung
            modelBuilder.Entity<PhieuDanhGia>()
                .HasOne(p => p.NguoiDuocDanhGia)
                .WithMany()
                .HasForeignKey(p => p.MaNguoiDuocDG)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PhieuDanhGia>()
                .HasOne(p => p.NguoiDanhGia)
                .WithMany()
                .HasForeignKey(p => p.MaNguoiDanhGia)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}