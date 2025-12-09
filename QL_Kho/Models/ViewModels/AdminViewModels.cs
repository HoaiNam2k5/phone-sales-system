using System;

namespace QL_Kho.ViewModels
{
    // ViewModel cho Dashboard
    public class DashboardViewModel
    {
        // Đơn hàng
        public int TongDonHang { get; set; }
        public int DonHangChoXacNhan { get; set; }
        public int DonHangDangGiao { get; set; }
        public int DonHangHoanThanh { get; set; }

        // Sản phẩm
        public int TongSanPham { get; set; }
        public int SanPhamHoatDong { get; set; }
        public int SanPhamHetHang { get; set; }

        // Người dùng
        public int TongNguoiDung { get; set; }
        public int NguoiDungMoi { get; set; }

        // Doanh thu
        public decimal DoanhThuHomNay { get; set; }
        public decimal DoanhThuThangNay { get; set; }
        public decimal DoanhThuNamNay { get; set; }
    }

    // ViewModel cho Chi tiết đơn hàng
    public class ChiTietDonHangViewModel
    {
        public int MaCTDH { get; set; }
        public string MaSP { get; set; }
        public string TenSP { get; set; }
        public string HinhAnh { get; set; }
        public int SoLuong { get; set; }
        public decimal DonGia { get; set; }
        public decimal? ThanhTien { get; set; }
    }

    // ViewModel cho Sản phẩm bán chạy
    public class SanPhamBanChayViewModel
    {
        public string MaSP { get; set; }
        public string TenSP { get; set; }
        public string HinhAnh { get; set; }
        public decimal? DonGia { get; set; }
        public int SoLuongBan { get; set; }
    }

    // ViewModel cho Báo cáo doanh thu
    public class BaoCaoDoanhThuViewModel
    {
        public DateTime Ngay { get; set; }
        public int SoLuongHoaDon { get; set; }
        public decimal TongDoanhThu { get; set; }
        public decimal DoanhThuTrungBinh { get; set; }
    }
}