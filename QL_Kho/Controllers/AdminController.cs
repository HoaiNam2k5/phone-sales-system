using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using QL_Kho.Models;
using QL_Kho.ViewModels;
using SystemFile = System.IO.File;
namespace QL_Kho.Controllers
{
    public class AdminController : Controller
    {
        private Model1 db = new Model1();

        // ==========================================
        // AUTHORIZATION FILTER
        // ==========================================
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (Session["UserID"] == null)
            {
                TempData["Error"] = "Vui lòng đăng nhập! ";
                filterContext.Result = new RedirectResult("~/Account/Login");
                return;
            }

            var userRole = Session["UserRole"]?.ToString();
            if (userRole != "admin" && userRole != "quanly")
            {
                TempData["Error"] = "Bạn không có quyền truy cập trang này!";
                filterContext.Result = new RedirectResult("~/Home/Index");
                return;
            }

            // ✅ LƯU ROLE VÀO ViewBag ĐỂ VIEW SỬ DỤNG
            ViewBag.UserRole = userRole;
            ViewBag.UserName = Session["UserName"]?.ToString();
            ViewBag.UserID = Session["UserID"]?.ToString();

            base.OnActionExecuting(filterContext);
        }

        // ✅ HELPER METHODS - KIỂM TRA QUYỀN
        private bool IsAdmin()
        {
            return Session["UserRole"]?.ToString()?.ToLower() == "admin";
        }

        private bool IsQuanLy()
        {
            return Session["UserRole"]?.ToString()?.ToLower() == "quanly";
        }

        private JsonResult AdminOnly()
        {
            return Json(new { success = false, message = "⛔ Chỉ Admin mới có quyền thực hiện chức năng này!" });
        }

        // ==========================================
        // DASHBOARD - CẢ 2 ĐỀU XEM ĐƯỢC
        // ==========================================

        public ActionResult Index()
        {
            return RedirectToAction("Dashboard");
        }

        public ActionResult Dashboard()
        {
            try
            {
                var homNay = DateTime.Today;
                var dauThang = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                var dauNam = new DateTime(DateTime.Now.Year, 1, 1);
                var _30NgayTruoc = DateTime.Now.AddDays(-30);

                var stats = new DashboardViewModel
                {
                    TongDonHang = db.DONHANGs.Count(),
                    DonHangChoXacNhan = db.DONHANGs.Count(d => d.TrangThai == "Chờ xác nhận"),
                    DonHangDangGiao = db.DONHANGs.Count(d => d.TrangThai == "Đang giao"),
                    DonHangHoanThanh = db.DONHANGs.Count(d => d.TrangThai == "Đã giao"),

                    TongSanPham = db.SANPHAMs.Count(),
                    SanPhamHoatDong = db.SANPHAMs.Count(sp => sp.TrangThai == "HoatDong"),
                    SanPhamHetHang = db.SANPHAMs.Count(sp => sp.TrangThai == "HetHang" || sp.SoLuongTon == 0),

                    TongNguoiDung = db.NGUOIDUNGs.Count(),
                    NguoiDungMoi = db.NGUOIDUNGs.Count(u => u.NgayTao >= _30NgayTruoc),

                    DoanhThuHomNay = db.DONHANGs
                        .Where(d => DbFunctions.TruncateTime(d.NgayDat) == homNay && d.TrangThai == "Đã giao")
                        .Sum(d => (decimal?)d.TongTien) ?? 0,

                    DoanhThuThangNay = db.DONHANGs
                        .Where(d => d.NgayDat >= dauThang && d.TrangThai == "Đã giao")
                        .Sum(d => (decimal?)d.TongTien) ?? 0,

                    DoanhThuNamNay = db.DONHANGs
                        .Where(d => d.NgayDat >= dauNam && d.TrangThai == "Đã giao")
                        .Sum(d => (decimal?)d.TongTien) ?? 0
                };

                ViewBag.DonHangMoi = db.DONHANGs
                    .OrderByDescending(d => d.NgayDat)
                    .Take(10)
                    .ToList();

                ViewBag.SanPhamBanChay = (from ct in db.CHITIETDONHANGs
                                          join sp in db.SANPHAMs on ct.MaSP.Trim() equals sp.MaSP.Trim()
                                          join dh in db.DONHANGs on ct.MaDH.Trim() equals dh.MaDH.Trim()
                                          where dh.TrangThai == "Đã giao"
                                          group ct by new { sp.MaSP, sp.TenSP, sp.HinhAnh, sp.DonGia } into g
                                          orderby g.Sum(x => x.SoLuong) descending
                                          select new SanPhamBanChayViewModel
                                          {
                                              MaSP = g.Key.MaSP,
                                              TenSP = g.Key.TenSP,
                                              HinhAnh = g.Key.HinhAnh,
                                              DonGia = g.Key.DonGia,
                                              SoLuongBan = g.Sum(x => x.SoLuong)
                                          }).Take(5).ToList();

                return View(stats);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi: " + ex.Message;
                return View(new DashboardViewModel());
            }
        }

        // ==========================================
        // QUẢN LÝ SẢN PHẨM - CẢ 2 ĐỀU QUẢN LÝ ĐƯỢC
        // ==========================================

        public ActionResult QuanLySanPham(string search, string category, string status)
        {
            try
            {
                var query = db.SANPHAMs.AsQueryable();

                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(sp => sp.TenSP.Contains(search) || sp.MaSP.Contains(search));
                    ViewBag.Search = search;
                }

                if (!string.IsNullOrEmpty(category))
                {
                    query = query.Where(sp => sp.MaDM == category);
                    ViewBag.Category = category;
                }

                if (!string.IsNullOrEmpty(status))
                {
                    query = query.Where(sp => sp.TrangThai == status);
                    ViewBag.Status = status;
                }

                var sanPham = query.OrderByDescending(sp => sp.NgayTao).ToList();
                ViewBag.DanhMuc = db.DANHMUCs.Where(dm => dm.TrangThai == "HoatDong").ToList();

                return View(sanPham);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi: " + ex.Message;
                return View(new List<SANPHAM>());
            }
        }

        public ActionResult ThemSanPham()
        {
            ViewBag.DanhMuc = new SelectList(db.DANHMUCs.Where(dm => dm.TrangThai == "HoatDong"), "MaDM", "TenDM");
            ViewBag.NhaCungCap = new SelectList(db.NHACUNGCAPs.Where(ncc => ncc.TrangThai == "HoatDong"), "MaNCC", "TenNCC");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ThemSanPham(SANPHAM model, HttpPostedFileBase HinhAnhFile)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (HinhAnhFile != null && HinhAnhFile.ContentLength > 0)
                    {
                        var fileName = Path.GetFileName(HinhAnhFile.FileName);
                        var path = Path.Combine(Server.MapPath("~/Content/images/"), fileName);

                        var directory = Path.GetDirectoryName(path);
                        if (!Directory.Exists(directory))
                        {
                            Directory.CreateDirectory(directory);
                        }

                        HinhAnhFile.SaveAs(path);
                        model.HinhAnh = fileName;
                    }

                    var tenSP = new SqlParameter("@TenSP", model.TenSP);
                    var donGia = new SqlParameter("@DonGia", model.DonGia);
                    var donViTinh = new SqlParameter("@DonViTinh", model.DonViTinh ?? "Chiếc");
                    var soLuongTon = new SqlParameter("@SoLuongTon", model.SoLuongTon ?? 0);
                    var hinhAnh = new SqlParameter("@HinhAnh", (object)model.HinhAnh ?? DBNull.Value);
                    var moTa = new SqlParameter("@MoTa", (object)model.MoTa ?? DBNull.Value);
                    var maNCC = new SqlParameter("@MaNCC", model.MaNCC);
                    var maDM = new SqlParameter("@MaDM", model.MaDM);
                    var trangThai = new SqlParameter("@TrangThai", model.TrangThai ?? "HoatDong");

                    db.Database.ExecuteSqlCommand(
                        "EXEC SP_ThemSanPham @TenSP, @DonGia, @DonViTinh, @SoLuongTon, @HinhAnh, @MoTa, @MaNCC, @MaDM, @TrangThai",
                        tenSP, donGia, donViTinh, soLuongTon, hinhAnh, moTa, maNCC, maDM, trangThai
                    );

                    TempData["Success"] = "Thêm sản phẩm thành công!";
                    return RedirectToAction("QuanLySanPham");
                }

                ViewBag.DanhMuc = new SelectList(db.DANHMUCs.Where(dm => dm.TrangThai == "HoatDong"), "MaDM", "TenDM");
                ViewBag.NhaCungCap = new SelectList(db.NHACUNGCAPs.Where(ncc => ncc.TrangThai == "HoatDong"), "MaNCC", "TenNCC");
                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Lỗi: " + ex.Message;
                ViewBag.DanhMuc = new SelectList(db.DANHMUCs, "MaDM", "TenDM");
                ViewBag.NhaCungCap = new SelectList(db.NHACUNGCAPs, "MaNCC", "TenNCC");
                return View(model);
            }
        }

        public ActionResult SuaSanPham(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return HttpNotFound();
            }

            var sanPham = db.SANPHAMs.Find(id.Trim());
            if (sanPham == null)
            {
                return HttpNotFound();
            }

            ViewBag.DanhMuc = new SelectList(db.DANHMUCs.Where(dm => dm.TrangThai == "HoatDong"), "MaDM", "TenDM", sanPham.MaDM);
            ViewBag.NhaCungCap = new SelectList(db.NHACUNGCAPs.Where(ncc => ncc.TrangThai == "HoatDong"), "MaNCC", "TenNCC", sanPham.MaNCC);

            return View(sanPham);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SuaSanPham(SANPHAM model, HttpPostedFileBase HinhAnhFile)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var sanPham = db.SANPHAMs.Find(model.MaSP.Trim());
                    if (sanPham == null)
                    {
                        TempData["Error"] = "Không tìm thấy sản phẩm!";
                        return RedirectToAction("QuanLySanPham");
                    }

                    if (HinhAnhFile != null && HinhAnhFile.ContentLength > 0)
                    {
                        var fileName = Path.GetFileName(HinhAnhFile.FileName);
                        var path = Path.Combine(Server.MapPath("~/Content/images/"), fileName);

                        var directory = Path.GetDirectoryName(path);
                        if (!Directory.Exists(directory))
                        {
                            Directory.CreateDirectory(directory);
                        }

                        HinhAnhFile.SaveAs(path);
                        model.HinhAnh = fileName;
                    }
                    else
                    {
                        model.HinhAnh = sanPham.HinhAnh;
                    }

                    var maSP = new SqlParameter("@MaSP", model.MaSP);
                    var tenSP = new SqlParameter("@TenSP", model.TenSP);
                    var donGia = new SqlParameter("@DonGia", model.DonGia);
                    var donViTinh = new SqlParameter("@DonViTinh", model.DonViTinh ?? "Chiếc");
                    var soLuongTon = new SqlParameter("@SoLuongTon", model.SoLuongTon ?? 0);
                    var hinhAnh = new SqlParameter("@HinhAnh", (object)model.HinhAnh ?? DBNull.Value);
                    var moTa = new SqlParameter("@MoTa", (object)model.MoTa ?? DBNull.Value);
                    var maNCC = new SqlParameter("@MaNCC", model.MaNCC);
                    var maDM = new SqlParameter("@MaDM", model.MaDM);
                    var trangThai = new SqlParameter("@TrangThai", model.TrangThai ?? "HoatDong");

                    db.Database.ExecuteSqlCommand(
                        "EXEC SP_SuaSanPham @MaSP, @TenSP, @DonGia, @DonViTinh, @SoLuongTon, @HinhAnh, @MoTa, @MaNCC, @MaDM, @TrangThai",
                        maSP, tenSP, donGia, donViTinh, soLuongTon, hinhAnh, moTa, maNCC, maDM, trangThai
                    );

                    TempData["Success"] = "Cập nhật sản phẩm thành công!";
                    return RedirectToAction("QuanLySanPham");
                }

                ViewBag.DanhMuc = new SelectList(db.DANHMUCs.Where(dm => dm.TrangThai == "HoatDong"), "MaDM", "TenDM", model.MaDM);
                ViewBag.NhaCungCap = new SelectList(db.NHACUNGCAPs.Where(ncc => ncc.TrangThai == "HoatDong"), "MaNCC", "TenNCC", model.MaNCC);
                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Lỗi: " + ex.Message;
                ViewBag.DanhMuc = new SelectList(db.DANHMUCs, "MaDM", "TenDM");
                ViewBag.NhaCungCap = new SelectList(db.NHACUNGCAPs, "MaNCC", "TenNCC");
                return View(model);
            }
        }

        // ✅ XÓA SẢN PHẨM - CHỈ ADMIN
        [HttpPost]
        public JsonResult XoaSanPham(string maSP)
        {
            try
            {
                // ✅ KIỂM TRA QUYỀN ADMIN
                if (!IsAdmin())
                {
                    return Json(new
                    {
                        success = false,
                        message = "⛔ Chỉ Admin mới có quyền xóa sản phẩm!"
                    });
                }

                if (string.IsNullOrEmpty(maSP))
                {
                    return Json(new { success = false, message = "Mã sản phẩm không hợp lệ" });
                }

                var param = new SqlParameter("@MaSP", maSP.Trim());
                db.Database.ExecuteSqlCommand("EXEC SP_XoaSanPham @MaSP", param);

                return Json(new { success = true, message = "Xóa sản phẩm thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // ==========================================
        // QUẢN LÝ DANH MỤC - CẢ 2 ĐỀU QUẢN LÝ ĐƯỢC
        // ==========================================

        public ActionResult QuanLyDanhMuc()
        {
            var danhMuc = db.DANHMUCs.OrderByDescending(dm => dm.NgayTao).ToList();
            return View(danhMuc);
        }

        public ActionResult ThemDanhMuc()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ThemDanhMuc(DANHMUC model, HttpPostedFileBase HinhAnhFile)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (HinhAnhFile != null && HinhAnhFile.ContentLength > 0)
                    {
                        var fileName = Path.GetFileName(HinhAnhFile.FileName);
                        var path = Path.Combine(Server.MapPath("~/Content/images/"), fileName);
                        HinhAnhFile.SaveAs(path);
                        model.HinhAnh = fileName;
                    }

                    var tenDM = new SqlParameter("@TenDM", model.TenDM);
                    var moTa = new SqlParameter("@MoTa", (object)model.MoTa ?? DBNull.Value);
                    var hinhAnh = new SqlParameter("@HinhAnh", (object)model.HinhAnh ?? DBNull.Value);
                    var trangThai = new SqlParameter("@TrangThai", model.TrangThai ?? "HoatDong");

                    db.Database.ExecuteSqlCommand(
                        "EXEC SP_ThemDanhMuc @TenDM, @MoTa, @HinhAnh, @TrangThai",
                        tenDM, moTa, hinhAnh, trangThai
                    );

                    TempData["Success"] = "Thêm danh mục thành công!";
                    return RedirectToAction("QuanLyDanhMuc");
                }

                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Lỗi: " + ex.Message;
                return View(model);
            }
        }

        public ActionResult SuaDanhMuc(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return HttpNotFound();
            }

            var danhMuc = db.DANHMUCs.Find(id.Trim());
            if (danhMuc == null)
            {
                return HttpNotFound();
            }

            return View(danhMuc);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SuaDanhMuc(DANHMUC model, HttpPostedFileBase HinhAnhFile)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var danhMuc = db.DANHMUCs.Find(model.MaDM.Trim());
                    if (danhMuc == null)
                    {
                        TempData["Error"] = "Không tìm thấy danh mục!";
                        return RedirectToAction("QuanLyDanhMuc");
                    }

                    if (HinhAnhFile != null && HinhAnhFile.ContentLength > 0)
                    {
                        var fileName = Path.GetFileName(HinhAnhFile.FileName);
                        var path = Path.Combine(Server.MapPath("~/Content/images/"), fileName);
                        HinhAnhFile.SaveAs(path);
                        model.HinhAnh = fileName;
                    }
                    else
                    {
                        model.HinhAnh = danhMuc.HinhAnh;
                    }

                    var maDM = new SqlParameter("@MaDM", model.MaDM);
                    var tenDM = new SqlParameter("@TenDM", model.TenDM);
                    var moTa = new SqlParameter("@MoTa", (object)model.MoTa ?? DBNull.Value);
                    var hinhAnh = new SqlParameter("@HinhAnh", (object)model.HinhAnh ?? DBNull.Value);
                    var trangThai = new SqlParameter("@TrangThai", model.TrangThai ?? "HoatDong");

                    db.Database.ExecuteSqlCommand(
                        "EXEC SP_SuaDanhMuc @MaDM, @TenDM, @MoTa, @HinhAnh, @TrangThai",
                        maDM, tenDM, moTa, hinhAnh, trangThai
                    );

                    TempData["Success"] = "Cập nhật danh mục thành công!";
                    return RedirectToAction("QuanLyDanhMuc");
                }

                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Lỗi: " + ex.Message;
                return View(model);
            }
        }

        [HttpPost]
        public JsonResult XoaDanhMuc(string maDM)
        {
            try
            {
                var param = new SqlParameter("@MaDM", maDM.Trim());
                db.Database.ExecuteSqlCommand("EXEC SP_XoaDanhMuc @MaDM", param);

                return Json(new { success = true, message = "Xóa danh mục thành công!" });
            }
            catch (SqlException ex)
            {
                if (ex.Message.Contains("có sản phẩm"))
                {
                    return Json(new { success = false, message = "Không thể xóa danh mục đang có sản phẩm!" });
                }
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // ==========================================
        // QUẢN LÝ NHÀ CUNG CẤP - CẢ 2 ĐỀU QUẢN LÝ ĐƯỢC
        // ==========================================

        public ActionResult QuanLyNhaCungCap()
        {
            var nhaCungCap = db.NHACUNGCAPs.OrderByDescending(ncc => ncc.NgayTao).ToList();
            return View(nhaCungCap);
        }

        public ActionResult ThemNhaCungCap()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ThemNhaCungCap(NHACUNGCAP model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var tenNCC = new SqlParameter("@TenNCC", model.TenNCC);
                    var diaChi = new SqlParameter("@DiaChi", (object)model.DiaChi ?? DBNull.Value);
                    var sdt = new SqlParameter("@SDT", (object)model.SDT ?? DBNull.Value);
                    var email = new SqlParameter("@Email", (object)model.Email ?? DBNull.Value);
                    var trangThai = new SqlParameter("@TrangThai", model.TrangThai ?? "HoatDong");

                    db.Database.ExecuteSqlCommand(
                        "EXEC SP_ThemNhaCungCap @TenNCC, @DiaChi, @SDT, @Email, @TrangThai",
                        tenNCC, diaChi, sdt, email, trangThai
                    );

                    TempData["Success"] = "Thêm nhà cung cấp thành công!";
                    return RedirectToAction("QuanLyNhaCungCap");
                }

                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Lỗi: " + ex.Message;
                return View(model);
            }
        }

        public ActionResult SuaNhaCungCap(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return HttpNotFound();
            }

            var nhaCungCap = db.NHACUNGCAPs.Find(id.Trim());
            if (nhaCungCap == null)
            {
                return HttpNotFound();
            }

            return View(nhaCungCap);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SuaNhaCungCap(NHACUNGCAP model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var maNCC = new SqlParameter("@MaNCC", model.MaNCC);
                    var tenNCC = new SqlParameter("@TenNCC", model.TenNCC);
                    var diaChi = new SqlParameter("@DiaChi", (object)model.DiaChi ?? DBNull.Value);
                    var sdt = new SqlParameter("@SDT", (object)model.SDT ?? DBNull.Value);
                    var email = new SqlParameter("@Email", (object)model.Email ?? DBNull.Value);
                    var trangThai = new SqlParameter("@TrangThai", model.TrangThai ?? "HoatDong");

                    db.Database.ExecuteSqlCommand(
                        "EXEC SP_SuaNhaCungCap @MaNCC, @TenNCC, @DiaChi, @SDT, @Email, @TrangThai",
                        maNCC, tenNCC, diaChi, sdt, email, trangThai
                    );

                    TempData["Success"] = "Cập nhật nhà cung cấp thành công! ";
                    return RedirectToAction("QuanLyNhaCungCap");
                }

                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Lỗi: " + ex.Message;
                return View(model);
            }
        }

        [HttpPost]
        public JsonResult XoaNhaCungCap(string maNCC)
        {
            try
            {
                var param = new SqlParameter("@MaNCC", maNCC.Trim());
                db.Database.ExecuteSqlCommand("EXEC SP_XoaNhaCungCap @MaNCC", param);

                return Json(new { success = true, message = "Xóa nhà cung cấp thành công!" });
            }
            catch (SqlException ex)
            {
                if (ex.Message.Contains("có sản phẩm"))
                {
                    return Json(new { success = false, message = "Không thể xóa nhà cung cấp đang có sản phẩm!" });
                }
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // ==========================================
        // QUẢN LÝ ĐƠN HÀNG - CẢ 2 ĐỀU QUẢN LÝ ĐƯỢC
        // ==========================================

        public ActionResult QuanLyDonHang(string search, string status)
        {
            try
            {
                var query = db.DONHANGs.AsQueryable();

                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(d => d.MaDH.Contains(search) || d.MaUser.Contains(search));
                    ViewBag.Search = search;
                }

                if (!string.IsNullOrEmpty(status))
                {
                    query = query.Where(d => d.TrangThai == status);
                    ViewBag.Status = status;
                }

                var donHang = query.OrderByDescending(d => d.NgayDat).ToList();
                return View(donHang);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi: " + ex.Message;
                return View(new List<DONHANG>());
            }
        }

        public ActionResult ChiTietDonHang(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return HttpNotFound();
            }

            var donHang = db.DONHANGs.Find(id.Trim());
            if (donHang == null)
            {
                return HttpNotFound();
            }

            var chiTiet = (from ct in db.CHITIETDONHANGs
                           join sp in db.SANPHAMs on ct.MaSP.Trim() equals sp.MaSP.Trim()
                           where ct.MaDH.Trim() == id.Trim()
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

        [HttpPost]
        public JsonResult CapNhatTrangThaiDonHang(string maDH, string trangThai)
        {
            try
            {
                var donHang = db.DONHANGs.Find(maDH?.Trim());
                if (donHang == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy đơn hàng!" });
                }

                donHang.TrangThai = trangThai;
                donHang.NgayCapNhat = DateTime.Now;
                db.SaveChanges();

                return Json(new { success = true, message = "Cập nhật trạng thái thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // ==========================================
        // QUẢN LÝ NGƯỜI DÙNG - ❌ CHỈ ADMIN
        // ==========================================

        public ActionResult QuanLyNguoiDung(string search, string role)
        {
            // ✅ KIỂM TRA QUYỀN ADMIN
            if (!IsAdmin())
            {
                TempData["Error"] = "⛔ Chỉ Admin mới có quyền quản lý người dùng!";
                return RedirectToAction("Dashboard");
            }

            try
            {
                var query = db.NGUOIDUNGs.AsQueryable();

                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(u => u.TenNguoiDung.Contains(search) || u.Email.Contains(search));
                    ViewBag.Search = search;
                }

                if (!string.IsNullOrEmpty(role))
                {
                    query = query.Where(u => u.Role == role);
                    ViewBag.Role = role;
                }

                var nguoiDung = query.OrderByDescending(u => u.NgayTao).ToList();
                return View(nguoiDung);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi: " + ex.Message;
                return View(new List<NGUOIDUNG>());
            }
        }

        public ActionResult SuaNguoiDung(string id)
        {
            // ✅ KIỂM TRA QUYỀN ADMIN
            if (!IsAdmin())
            {
                TempData["Error"] = "⛔ Chỉ Admin mới có quyền sửa người dùng!";
                return RedirectToAction("Dashboard");
            }

            if (string.IsNullOrEmpty(id))
            {
                return HttpNotFound();
            }

            var user = db.NGUOIDUNGs.Find(id.Trim());
            if (user == null)
            {
                return HttpNotFound();
            }

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SuaNguoiDung(NGUOIDUNG model)
        {
            // ✅ KIỂM TRA QUYỀN ADMIN
            if (!IsAdmin())
            {
                TempData["Error"] = "⛔ Chỉ Admin mới có quyền sửa người dùng!";
                return RedirectToAction("Dashboard");
            }

            try
            {
                if (ModelState.IsValid)
                {
                    var user = db.NGUOIDUNGs.Find(model.MaUser.Trim());
                    if (user == null)
                    {
                        TempData["Error"] = "Không tìm thấy người dùng!";
                        return RedirectToAction("QuanLyNguoiDung");
                    }

                    user.TenNguoiDung = model.TenNguoiDung;
                    user.Email = model.Email;
                    user.SDT = model.SDT;
                    user.DiaChi = model.DiaChi;
                    user.Role = model.Role;
                    user.TrangThai = model.TrangThai;
                    user.NgayCapNhat = DateTime.Now;

                    db.SaveChanges();

                    TempData["Success"] = "Cập nhật thông tin người dùng thành công!";
                    return RedirectToAction("QuanLyNguoiDung");
                }

                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Lỗi: " + ex.Message;
                return View(model);
            }
        }

        [HttpPost]
        public JsonResult XoaNguoiDung(string maUser)
        {
            // ✅ KIỂM TRA QUYỀN ADMIN
            if (!IsAdmin())
            {
                return Json(new
                {
                    success = false,
                    message = "⛔ Chỉ Admin mới có quyền vô hiệu hóa người dùng!"
                });
            }

            try
            {
                var user = db.NGUOIDUNGs.Find(maUser?.Trim());
                if (user == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy người dùng!" });
                }

                user.TrangThai = "KhongHoatDong";
                user.NgayCapNhat = DateTime.Now;
                db.SaveChanges();

                return Json(new { success = true, message = "Vô hiệu hóa người dùng thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // ==========================================
        // BÁO CÁO - CẢ 2 ĐỀU XEM ĐƯỢC
        // ==========================================

        public ActionResult BaoCaoDoanhThu(DateTime? tuNgay, DateTime? denNgay)
        {
            try
            {
                if (!tuNgay.HasValue) tuNgay = DateTime.Now.AddMonths(-1);
                if (!denNgay.HasValue) denNgay = DateTime.Now;

                ViewBag.TuNgay = tuNgay.Value.ToString("yyyy-MM-dd");
                ViewBag.DenNgay = denNgay.Value.ToString("yyyy-MM-dd");

                var result = db.DONHANGs
                    .Where(d => d.NgayDat >= tuNgay && d.NgayDat <= denNgay && d.TrangThai == "Đã giao")
                    .GroupBy(d => DbFunctions.TruncateTime(d.NgayDat))
                    .Select(g => new BaoCaoDoanhThuViewModel
                    {
                        Ngay = g.Key.Value,
                        SoLuongHoaDon = g.Count(),
                        TongDoanhThu = g.Sum(d => d.TongTien),
                        DoanhThuTrungBinh = g.Average(d => d.TongTien)
                    })
                    .OrderBy(x => x.Ngay)
                    .ToList();

                return View(result);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi: " + ex.Message;
                return View(new List<BaoCaoDoanhThuViewModel>());
            }
        }

        // ==========================================
        // BACKUP DATABASE - ❌ CHỈ ADMIN
        // ==========================================

        public ActionResult BackupDatabase()
        {
            if (!IsAdmin())
            {
                TempData["Error"] = "⛔ Chỉ Admin mới có quyền backup database!";
                return RedirectToAction("Dashboard");
            }

            try
            {
                var connectionString = ConfigurationManager.ConnectionStrings["Model1"].ConnectionString;
                var builder = new SqlConnectionStringBuilder(connectionString);
                var databaseName = builder.InitialCatalog;
                var serverName = builder.DataSource;

                ViewBag.DatabaseName = databaseName;
                ViewBag.ServerName = serverName;

                var backupFolder = Server.MapPath("~/App_Data/Backups/");

                if (!Directory.Exists(backupFolder))
                {
                    Directory.CreateDirectory(backupFolder);
                }

                // ✅ ĐÚNG: Dùng System.IO.File
                var backupFiles = Directory.GetFiles(backupFolder, "*.bak")
                    .Select(f => new
                    {
                        FileName = Path.GetFileName(f),
                        FilePath = f,
                        FileSize = new FileInfo(f).Length / 1024 / 1024,
                        CreatedDate = System.IO.File.GetCreationTime(f) // ✅ ĐÚNG
                    })
                    .OrderByDescending(f => f.CreatedDate)
                    .ToList();

                ViewBag.BackupFiles = backupFiles;

                return View();
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi: " + ex.Message;
                return RedirectToAction("Dashboard");
            }
        }

        [HttpPost]
        public JsonResult ThucHienBackup(string backupName)
        {
            // ✅ KIỂM TRA QUYỀN
            if (!IsAdmin())
            {
                return Json(new { success = false, message = "⛔ Chỉ Admin mới có quyền backup!" });
            }

            try
            {
                var connectionString = ConfigurationManager.ConnectionStrings["Model1"].ConnectionString;
                var builder = new SqlConnectionStringBuilder(connectionString);
                var databaseName = builder.InitialCatalog;

                if (string.IsNullOrEmpty(backupName))
                {
                    backupName = $"Backup_{DateTime.Now:yyyyMMdd_HHmmss}";
                }
                else
                {
                    backupName = backupName.Replace(" ", "_");
                }

                var backupFolder = Server.MapPath("~/App_Data/Backups/");
                if (!Directory.Exists(backupFolder))
                {
                    Directory.CreateDirectory(backupFolder);
                }

                var backupPath = Path.Combine(backupFolder, $"{backupName}.bak");

                var sql = $@"
                    BACKUP DATABASE [{databaseName}] 
                    TO DISK = @BackupPath 
                    WITH FORMAT, INIT, 
                    NAME = @BackupName, 
                    SKIP, NOREWIND, NOUNLOAD, STATS = 10";

                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (var command = new SqlCommand(sql, connection))
                    {
                        command.CommandTimeout = 300;
                        command.Parameters.AddWithValue("@BackupPath", backupPath);
                        command.Parameters.AddWithValue("@BackupName", backupName);

                        command.ExecuteNonQuery();
                    }
                }

                var fileInfo = new FileInfo(backupPath);
                var logMessage = $"Backup thành công!  File: {backupName}. bak ({fileInfo.Length / 1024 / 1024} MB)";

                return Json(new
                {
                    success = true,
                    message = logMessage,
                    fileName = $"{backupName}.bak",
                    fileSize = fileInfo.Length / 1024 / 1024
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi backup: " + ex.Message });
            }
        }

        [HttpPost]
        public JsonResult ThucHienRestore(string fileName)
        {
            if (!IsAdmin())
            {
                return Json(new { success = false, message = "⛔ Chỉ Admin mới có quyền restore!" });
            }

            try
            {
                var backupFolder = Server.MapPath("~/App_Data/Backups/");
                var backupPath = Path.Combine(backupFolder, fileName);

                // ✅ ĐÚNG: Dùng System.IO.File
                if (!System.IO.File.Exists(backupPath))
                {
                    return Json(new { success = false, message = "File backup không tồn tại!" });
                }

                var connectionString = ConfigurationManager.ConnectionStrings["Model1"].ConnectionString;
                var builder = new SqlConnectionStringBuilder(connectionString);
                var databaseName = builder.InitialCatalog;

                builder.InitialCatalog = "master";
                var masterConnectionString = builder.ToString();

                var sql = $@"
            ALTER DATABASE [{databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
            RESTORE DATABASE [{databaseName}] 
            FROM DISK = @BackupPath 
            WITH REPLACE, STATS = 10;
            ALTER DATABASE [{databaseName}] SET MULTI_USER;";

                using (var connection = new SqlConnection(masterConnectionString))
                {
                    connection.Open();
                    using (var command = new SqlCommand(sql, connection))
                    {
                        command.CommandTimeout = 300;
                        command.Parameters.AddWithValue("@BackupPath", backupPath);
                        command.ExecuteNonQuery();
                    }
                }

                return Json(new
                {
                    success = true,
                    message = $"Restore database thành công từ file: {fileName}"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi restore: " + ex.Message });
            }
        }

        [HttpPost]
        public JsonResult XoaBackup(string fileName)
        {
            if (!IsAdmin())
            {
                return Json(new { success = false, message = "⛔ Chỉ Admin mới có quyền xóa backup!" });
            }

            try
            {
                var backupFolder = Server.MapPath("~/App_Data/Backups/");
                var backupPath = Path.Combine(backupFolder, fileName);

                // ✅ ĐÚNG: Dùng System.IO. File
                if (System.IO.File.Exists(backupPath))
                {
                    System.IO.File.Delete(backupPath); // ✅ ĐÚNG
                    return Json(new { success = true, message = $"Đã xóa file: {fileName}" });
                }
                else
                {
                    return Json(new { success = false, message = "File không tồn tại!" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }
        public ActionResult DownloadBackup(string fileName)
        {
            if (!IsAdmin())
            {
                TempData["Error"] = "⛔ Chỉ Admin mới có quyền download backup!";
                return RedirectToAction("BackupDatabase");
            }

            try
            {
                var backupFolder = Server.MapPath("~/App_Data/Backups/");
                var filePath = Path.Combine(backupFolder, fileName);

                // ✅ ĐÚNG: Dùng System.IO.File
                if (!System.IO.File.Exists(filePath))
                {
                    TempData["Error"] = "File không tồn tại!";
                    return RedirectToAction("BackupDatabase");
                }

                // ✅ ĐÚNG: Dùng System.IO.File
                var fileBytes = System.IO.File.ReadAllBytes(filePath);

                // ✅ ĐÚNG: Controller.File() method
                return File(fileBytes, "application/octet-stream", fileName);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi: " + ex.Message;
                return RedirectToAction("BackupDatabase");
            }
        }

        [HttpPost]
        public JsonResult UploadBackup(HttpPostedFileBase backupFile)
        {
            // ✅ KIỂM TRA QUYỀN
            if (!IsAdmin())
            {
                return Json(new { success = false, message = "⛔ Chỉ Admin mới có quyền upload backup!" });
            }

            try
            {
                if (backupFile == null || backupFile.ContentLength == 0)
                {
                    return Json(new { success = false, message = "Vui lòng chọn file!" });
                }

                var extension = Path.GetExtension(backupFile.FileName).ToLower();
                if (extension != ".bak")
                {
                    return Json(new { success = false, message = "Chỉ chấp nhận file .bak!" });
                }

                var backupFolder = Server.MapPath("~/App_Data/Backups/");
                if (!Directory.Exists(backupFolder))
                {
                    Directory.CreateDirectory(backupFolder);
                }

                var fileName = Path.GetFileName(backupFile.FileName);
                var filePath = Path.Combine(backupFolder, fileName);

                backupFile.SaveAs(filePath);

                var fileInfo = new FileInfo(filePath);

                return Json(new
                {
                    success = true,
                    message = $"Upload thành công: {fileName} ({fileInfo.Length / 1024 / 1024} MB)"
                });
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