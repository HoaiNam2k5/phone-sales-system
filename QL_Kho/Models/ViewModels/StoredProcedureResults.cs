using System;

namespace QL_Kho.ViewModels
{
    /// <summary>
    /// Kết quả trả về từ SP_TaoHoaDonNhap
    /// </summary>
    public class SpCreateResult
    {
        public string MaHDN { get; set; }
        public string Message { get; set; }
        public int Success { get; set; }
    }

    /// <summary>
    /// Kết quả trả về từ SP_ThemSanPhamNhap, SP_DuyetHoaDonNhap, etc.
    /// </summary>
    public class SpResult
    {
        public string Message { get; set; }
        public int Success { get; set; }
    }
}