using System;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;
using QL_Kho.Models;
using QL_Kho.ViewModels;

namespace QL_Kho.Controllers
{
    public class AccountController : Controller
    {
        private Model1 db = new Model1();

        // GET: Account/Login
        public ActionResult Login()
        {
            if (Session["UserID"] != null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string username, string password, string returnUrl)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ thông tin";
                return View();
            }

            try
            {
                // Gọi function fun_check_account từ SQL
                var userParam = new SqlParameter("@user", username);
                var passParam = new SqlParameter("@pass", password);

                var result = db.Database.SqlQuery<int>(
                    "SELECT dbo.fun_check_account(@user, @pass)",
                    userParam, passParam
                ).FirstOrDefault();

                if (result == 1)
                {
                    var user = db.NGUOIDUNGs
                        .Where(u => u.TenNguoiDung == username && u.TrangThai == "HoatDong")
                        .FirstOrDefault();

                    if (user != null)
                    {
                        // Lưu session
                        Session["UserID"] = user.MaUser;
                        Session["UserName"] = user.TenNguoiDung;
                        Session["UserRole"] = user.Role;
                        Session["Email"] = user.Email;
                        Session["CartCount"] = 0;

                        TempData["Success"] = $"Chào mừng {user.TenNguoiDung}!";

                        // Chuyển hướng theo role
                        if (user.Role == "admin" || user.Role == "quanly")
                        {
                            return RedirectToAction("Index", "Admin");
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                            {
                                return Redirect(returnUrl);
                            }
                            return RedirectToAction("Index", "Home");
                        }
                    }
                }

                ViewBag.Error = "Tên đăng nhập hoặc mật khẩu không đúng";
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Lỗi đăng nhập: " + ex.Message;
                return View();
            }
        }

        // GET: Account/Register
        public ActionResult Register()
        {
            if (Session["UserID"] != null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Kiểm tra email đã tồn tại
                    var existingUser = db.NGUOIDUNGs.FirstOrDefault(u => u.Email == model.Email);
                    if (existingUser != null)
                    {
                        ViewBag.Error = "Email đã được sử dụng";
                        return View(model);
                    }

                    // Gọi stored procedure proc_create_user
                    var usernameParam = new SqlParameter("@username", model.TenNguoiDung);
                    var passParam = new SqlParameter("@pass", model.MatKhau);
                    var emailParam = new SqlParameter("@email", model.Email);
                    var sdtParam = new SqlParameter("@sdt", string.IsNullOrEmpty(model.SDT) ? (object)DBNull.Value : model.SDT);
                    var diachiParam = new SqlParameter("@diachi", string.IsNullOrEmpty(model.DiaChi) ? (object)DBNull.Value : model.DiaChi);
                    var roleParam = new SqlParameter("@role", "khach");

                    db.Database.ExecuteSqlCommand(
                        "EXEC proc_create_user @username, @pass, @email, @sdt, @diachi, @role",
                        usernameParam, passParam, emailParam, sdtParam, diachiParam, roleParam
                    );

                    TempData["Success"] = "Đăng ký thành công!  Vui lòng đăng nhập.";
                    return RedirectToAction("Login");
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("Email đã tồn tại"))
                    {
                        ViewBag.Error = "Email đã được sử dụng";
                    }
                    else
                    {
                        ViewBag.Error = "Lỗi đăng ký: " + ex.Message;
                    }
                }
            }
            return View(model);
        }

        // GET: Account/Logout
        public ActionResult Logout()
        {
            Session.Clear();
            TempData["Info"] = "Đã đăng xuất thành công";
            return RedirectToAction("Login");
        }

        // GET: Account/ThongTinCaNhan
        public ActionResult ThongTinCaNhan()
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Login");
            }

            var userId = Session["UserID"].ToString();
            var user = db.NGUOIDUNGs.Find(userId);

            if (user == null)
            {
                return HttpNotFound();
            }

            var model = new ThongTinCaNhanViewModel
            {
                MaUser = user.MaUser,
                TenNguoiDung = user.TenNguoiDung,
                Email = user.Email,
                SDT = user.SDT,
                DiaChi = user.DiaChi
            };

            return View(model);
        }

        // ✅ POST: Account/ThongTinCaNhan (THÊM MỚI)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ThongTinCaNhan(ThongTinCaNhanViewModel model)
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Login");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var userId = Session["UserID"].ToString();
                    var user = db.NGUOIDUNGs.Find(userId);

                    if (user == null)
                    {
                        TempData["Error"] = "Không tìm thấy thông tin người dùng";
                        return View(model);
                    }

                    // Kiểm tra email trùng (nếu thay đổi)
                    if (user.Email != model.Email)
                    {
                        var existingEmail = db.NGUOIDUNGs.Any(u => u.Email == model.Email && u.MaUser != userId);
                        if (existingEmail)
                        {
                            ViewBag.Error = "Email đã được sử dụng bởi tài khoản khác";
                            return View(model);
                        }
                    }

                    // Cập nhật thông tin
                    user.TenNguoiDung = model.TenNguoiDung;
                    user.Email = model.Email;
                    user.SDT = model.SDT;
                    user.DiaChi = model.DiaChi;
                    user.NgayCapNhat = DateTime.Now;

                    db.SaveChanges();

                    // Cập nhật lại Session
                    Session["UserName"] = user.TenNguoiDung;
                    Session["Email"] = user.Email;

                    TempData["Success"] = "Cập nhật thông tin thành công! ";
                    return RedirectToAction("ThongTinCaNhan");
                }
                catch (Exception ex)
                {
                    ViewBag.Error = "Lỗi: " + ex.Message;
                }
            }

            return View(model);
        }

        // GET: Account/DoiMatKhau
        public ActionResult DoiMatKhau()
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Login");
            }
            return View();
        }

        // ✅ POST: Account/DoiMatKhau (THÊM MỚI)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DoiMatKhau(DoiMatKhauViewModel model)
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Login");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var userId = Session["UserID"].ToString();
                    var user = db.NGUOIDUNGs.Find(userId);

                    if (user == null)
                    {
                        TempData["Error"] = "Không tìm thấy thông tin người dùng";
                        return View(model);
                    }

                    // Kiểm tra mật khẩu cũ
                    if (user.MatKhau != model.MatKhauCu)
                    {
                        ViewBag.Error = "Mật khẩu cũ không đúng";
                        return View(model);
                    }

                    // Cập nhật mật khẩu mới
                    user.MatKhau = model.MatKhauMoi;
                    user.NgayCapNhat = DateTime.Now;

                    db.SaveChanges();

                    TempData["Success"] = "Đổi mật khẩu thành công!  Vui lòng đăng nhập lại.";
                    Session.Clear();
                    return RedirectToAction("Login");
                }
                catch (Exception ex)
                {
                    ViewBag.Error = "Lỗi: " + ex.Message;
                }
            }

            return View(model);
        }

        // GET: Account/DonHangCuaToi
        public ActionResult DonHangCuaToi()
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Login");
            }

            var userId = Session["UserID"].ToString().Trim();
            var orders = db.DONHANGs
                .Where(d => d.MaUser == userId)
                .OrderByDescending(d => d.NgayDat)
                .ToList();

            return View(orders);
        }

        // GET: Account/ChiTietDonHang
        public ActionResult ChiTietDonHang(string id)
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Login");
            }

            if (string.IsNullOrEmpty(id))
            {
                TempData["Error"] = "Mã đơn hàng không hợp lệ";
                return RedirectToAction("DonHangCuaToi");
            }

            var userId = Session["UserID"].ToString().Trim();
            id = id.Trim();

            // Lấy đơn hàng
            var donHang = db.DONHANGs.Find(id);

            if (donHang == null)
            {
                TempData["Error"] = "Không tìm thấy đơn hàng";
                return RedirectToAction("DonHangCuaToi");
            }

            // Kiểm tra quyền
            if (donHang.MaUser.Trim() != userId)
            {
                TempData["Error"] = "Bạn không có quyền xem đơn hàng này";
                return RedirectToAction("DonHangCuaToi");
            }

            var chiTiet = (from ct in db.CHITIETDONHANGs
                           join sp in db.SANPHAMs on ct.MaSP.Trim() equals sp.MaSP.Trim()
                           where ct.MaDH.Trim() == id
                           select new ChiTietDonHangViewModel
                           {
                               MaCTDH = ct.MaCTDH,
                               MaSP = ct.MaSP,
                               TenSP = sp.TenSP,
                               HinhAnh = sp.HinhAnh,
                               SoLuong = ct.SoLuong,
                               DonGia = ct.DonGia,
                               ThanhTien = ct.ThanhTien
                           }).ToList();

            ViewBag.DonHang = donHang;
            return View(chiTiet);
        }

        // POST: Account/HuyDonHang
        [HttpPost]
        public JsonResult HuyDonHang(string maDH)
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return Json(new { success = false, message = "Vui lòng đăng nhập" });
                }

                var userId = Session["UserID"].ToString().Trim();
                maDH = maDH?.Trim();

                var donHang = db.DONHANGs.Find(maDH);

                if (donHang == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy đơn hàng" });
                }

                if (donHang.MaUser.Trim() != userId)
                {
                    return Json(new { success = false, message = "Bạn không có quyền hủy đơn hàng này" });
                }

                if (donHang.TrangThai != "Chờ xác nhận")
                {
                    return Json(new { success = false, message = "Chỉ có thể hủy đơn hàng đang chờ xác nhận" });
                }

                // Hoàn trả số lượng tồn kho
                var chiTiet = db.CHITIETDONHANGs.Where(ct => ct.MaDH.Trim() == maDH).ToList();
                foreach (var item in chiTiet)
                {
                    var sp = db.SANPHAMs.Find(item.MaSP.Trim());
                    if (sp != null)
                    {
                        sp.SoLuongTon += item.SoLuong;
                        if (sp.TrangThai == "HetHang" && sp.SoLuongTon > 0)
                        {
                            sp.TrangThai = "HoatDong";
                        }
                    }
                }

                donHang.TrangThai = "Đã hủy";
                donHang.NgayCapNhat = DateTime.Now;
                db.SaveChanges();

                return Json(new { success = true, message = "Đã hủy đơn hàng thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}