﻿using AspNetCoreHero.ToastNotification.Abstractions;
using Funiture_Project.Models;
using Funiture_Project.ModelViews;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Funiture_Project.Controllers
{
    public class ProfileController : Controller
    {
        private FurnitureContext context;
        public INotyfService notyfService { get; }
        public ProfileController(FurnitureContext context, INotyfService notyfService)
        {
            this.context = context;
            this.notyfService = notyfService;
        }
        [Route("Profile", Name = "Dashboard")]
        public IActionResult Index()
        {
            var taikhoanID = HttpContext.Session.GetString("MaKH");
            if (taikhoanID != null)
            {
                var khachhang = context.KhachHang.AsNoTracking().SingleOrDefault(x => x.MaKh == Convert.ToInt32(taikhoanID));
                if (khachhang != null)
                {
                    //lấy các hóa đơn của khách hàng
                    var lsDonHang = context.HoaDon
                        .AsNoTracking().Where(x => x.MaKh == khachhang.MaKh)
                        .OrderByDescending(x => x.NgayHd).ToList();
                    ViewBag.DonHang = lsDonHang;

                    //lấy các sản phẩm của các hóa đơn
                    List<DSSanPham> lsSP = new List<DSSanPham>();
                    foreach (var itemDH in lsDonHang)
                    {
                        foreach (var itemCT in context.Cthd)
                        {
                            if (itemDH.MaHd == itemCT.MaHd)
                            {
                                foreach (var itemSP in context.SanPham)
                                {
                                    if (itemCT.MaSp == itemSP.MaSp)
                                        lsSP.Add(new DSSanPham(itemCT.MaHd, itemCT.MaSp, itemCT.SoLuong, itemCT.DonGia, itemSP.TenSp, itemSP.HinhAnh, itemSP.Nsx));
                                }
                            }
                        }
                    }
                    ViewBag.KhachHang = context.KhachHang.Find(Convert.ToInt32(taikhoanID));
                    ViewBag.dsSanPham = lsSP;
                    return View(khachhang);
                }
            }
            return RedirectToAction("Login");
        }
        [HttpPost]
        public IActionResult ChangePassword(ChangePasswordVM model)
        {
            if (ModelState.IsValid)
            {
                var taikhoanID = HttpContext.Session.GetString("MaKH");
                if (taikhoanID == null)
                {
                    return RedirectToAction("Index", "Login");
                }
                var taikhoan = context.KhachHang.Find(Convert.ToInt32(taikhoanID));
                if (taikhoan == null)
                {
                    return RedirectToAction("Index", "Login");
                }
                if (model.OldPassword == taikhoan.Password)
                {
                    taikhoan.Password = model.NewPassword;
                    context.Update(taikhoan);
                    context.SaveChanges();
                    notyfService.Success("Thay đổi mật khẩu thành công");
                    return RedirectToAction("Index", "Profile");
                }
                else
                {
                    notyfService.Error("Mật khẩu không đúng");
                    return RedirectToAction("Index", "Profile");
                }
            }
            return RedirectToAction("Index", "Profile");
        }
        [HttpPost]
        public IActionResult ChangeProfile(ChangeProfileVM profile)
        {
            if (ModelState.IsValid)
            {
                var taikhoanID = HttpContext.Session.Get("MaKH");
                if (taikhoanID != null)
                {
                    var taikhoan = context.KhachHang.Find(Convert.ToInt32(taikhoanID));
                    if (taikhoan == null)
                    {
                        return RedirectToAction("Index", "Profile");
                    }
                    taikhoan.HoTen = profile.HoTen;
                    taikhoan.Email = profile.Email;
                    taikhoan.Sdt = profile.SDT;
                    taikhoan.DiaChi = profile.DiaChi;
                    context.Update(taikhoan);
                    context.SaveChanges();
                    notyfService.Success("Thay đổi thông tin tài khoản thành công");
                    return RedirectToAction("Index", "Profile");
                }
            }
            return RedirectToAction("Index", "Profile");
        }
    }
}
