using System.ComponentModel.DataAnnotations;

namespace QL_Kho.ViewModels
{
    // ViewModel đăng ký
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Tên người dùng không được để trống")]
        [StringLength(100)]
        [Display(Name = "Tên người dùng")]
        public string TenNguoiDung { get; set; }

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [StringLength(255, MinimumLength = 6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string MatKhau { get; set; }

        [Required(ErrorMessage = "Xác nhận mật khẩu không được để trống")]
        [DataType(DataType.Password)]
        [Compare("MatKhau", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        [Display(Name = "Xác nhận mật khẩu")]
        public string XacNhanMatKhau { get; set; }

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [Display(Name = "Số điện thoại")]
        public string SDT { get; set; }

        [Display(Name = "Địa chỉ")]
        public string DiaChi { get; set; }
    }

    // ViewModel thông tin cá nhân
    public class ThongTinCaNhanViewModel
    {
        public string MaUser { get; set; }

        [Required(ErrorMessage = "Tên người dùng không được để trống")]
        [Display(Name = "Tên người dùng")]
        public string TenNguoiDung { get; set; }

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [Display(Name = "Số điện thoại")]
        public string SDT { get; set; }

        [Display(Name = "Địa chỉ")]
        public string DiaChi { get; set; }
    }

    // ViewModel đổi mật khẩu
    public class DoiMatKhauViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu cũ")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu cũ")]
        public string MatKhauCu { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới")]
        [StringLength(255, MinimumLength = 6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu mới")]
        public string MatKhauMoi { get; set; }

        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu mới")]
        [DataType(DataType.Password)]
        [Compare("MatKhauMoi", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        [Display(Name = "Xác nhận mật khẩu mới")]
        public string XacNhanMatKhauMoi { get; set; }
    }


}