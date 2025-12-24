using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QL_Kho.ViewModels
{
    public class HoaDonNhapViewModel
    {
        [Display(Name = "Mã hóa đơn")]
        public string MaHDN { get; set; }

        [Display(Name = "Mã nhà cung cấp")]
        public string MaNCC { get; set; }

        [Display(Name = "Tên nhà cung cấp")]
        public string TenNCC { get; set; }

        [Display(Name = "Mã người tạo")]
        public string MaNguoiTao { get; set; } // ✅ THÊM PROPERTY NÀY

        [Display(Name = "Tên người tạo")]
        public string TenNguoiTao { get; set; }

        [Display(Name = "Ngày nhập")]
        [DataType(DataType.Date)]
        public DateTime NgayNhap { get; set; }

        [Display(Name = "Tổng tiền")]
        [DataType(DataType.Currency)]
        public decimal? TongTien { get; set; }

        [Display(Name = "Trạng thái")]
        public string TrangThai { get; set; }

        [Display(Name = "Số lượng sản phẩm")]
        public int SoLuongSanPham { get; set; }

        [Display(Name = "Ghi chú")]
        public string GhiChu { get; set; }
    }
    public class ChiTietNhapKhoViewModel
    {
        public string MaHDN { get; set; }
        public string MaSP { get; set; }

        [Display(Name = "Tên sản phẩm")]
        public string TenSP { get; set; }

        [Display(Name = "Hình ảnh")]
        public string HinhAnh { get; set; }

        [Display(Name = "Số lượng")]
        public int? SoLuong { get; set; }

        [Display(Name = "Đơn giá nhập")]
        [DataType(DataType.Currency)]
        public decimal? DonGiaNhap { get; set; }

        [Display(Name = "Thành tiền")]
        [DataType(DataType.Currency)]
        public decimal? ThanhTien { get; set; }

        [Display(Name = "Tồn kho hiện tại")]
        public int? SoLuongTonHienTai { get; set; }
    }
}