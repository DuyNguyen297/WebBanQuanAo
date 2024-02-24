using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using FashionShop;
using FashionShop.Helpper;
using FashionShop.Models;
using FashionShop.Shared;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using PagedList;
using ClosedXML.Excel;
using Newtonsoft.Json;
using FashionShop.ViewModel;
using Watch.OnlinePayment;
using Newtonsoft.Json.Linq;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Ajax.Utilities;
using DocumentFormat.OpenXml.Office2010.Excel;
using Irony.Parsing;
using FashionShop.AreaModels;

namespace FashionShop.Controllers
{
    public class VoucherController : GlobalController
    {
		public ActionResult Index(int page = 1)
		{
			var now = DateTime.Now;
			var voucherShips = _context.VoucherShips
				.Where(v => v.Quantity > 0 && now > v.StartDate)
				.OrderByDescending(p => p.CreatedAt)
				.ToList()
				.Select(item => new CreateVoucherModel
				{
					Id = item.Id,
					Name = item.Name,
					Discount = item.Discount,
					Quantity = item.Quantity,
					TotalCondition = item.TotalCondition,
					StartDate = item.StartDate.HasValue ? item.StartDate.Value.ToString("dd/MM/yyyy") : DateTime.Now.ToString("dd/MM/yyyy"),
					EndDate = item.EndDate.HasValue ? item.EndDate.Value.ToString("dd/MM/yyyy") : DateTime.Now.ToString("dd/MM/yyyy"),
					VoucherType = VoucherTypeConst.VOUCHERSHIP
				})
				.ToList();
			var voucherProducts = _context.VoucherProducts
				.Where(v => v.Quantity > 0 && now > v.StartDate)
				.OrderByDescending(p => p.CreatedAt)
				.ToList()
				.Select(item => new CreateVoucherModel
				{
					Id = item.Id,
					Name = item.Name,
					Discount = item.Discount,
					Quantity = item.Quantity,
					TotalCondition = item.TotalCondition,
					StartDate = item.StartDate.HasValue ? item.StartDate.Value.ToString("dd/MM/yyyy") : DateTime.Now.ToString("dd/MM/yyyy"),
					EndDate = item.EndDate.HasValue ? item.EndDate.Value.ToString("dd/MM/yyyy") : DateTime.Now.ToString("dd/MM/yyyy"),
					VoucherType = VoucherTypeConst.VOUCHERPRODUCT
				})
				.ToList();

			var voucherCategories = _context.VoucherCategories
				.Where(v => v.Quantity > 0 && now > v.StartDate)
				.OrderByDescending(p => p.CreatedAt)
				.ToList()
				.Select(item => new CreateVoucherModel
				{
					Id = item.Id,
					Name = item.Name,
					Discount = item.Discount,
					Quantity = item.Quantity,
					TotalCondition = item.TotalCondition,
					StartDate = item.StartDate.HasValue ? item.StartDate.Value.ToString("dd/MM/yyyy") : DateTime.Now.ToString("dd/MM/yyyy"),
					EndDate = item.EndDate.HasValue ? item.EndDate.Value.ToString("dd/MM/yyyy") : DateTime.Now.ToString("dd/MM/yyyy"),
					VoucherType = VoucherTypeConst.VOUCHERCATEGORY
				})
				.ToList();

			////
			string userId = Session[Const.USERIDSESSION]?.ToString();
			var voucherCustomers = _context.VoucherCustomers
				.Where(v => v.Quantity > 0 && now > v.StartDate && v.CustomerId == userId)
				.OrderByDescending(p => p.CreatedAt)
				.ToList()
				.Select(item => new CreateVoucherModel
				{
					Id = item.Id,
					Name = item.Name,
					Discount = item.Discount,
					Quantity = item.Quantity,
					TotalCondition = item.TotalCondition,
					StartDate = item.StartDate.HasValue ? item.StartDate.Value.ToString("dd/MM/yyyy") : DateTime.Now.ToString("dd/MM/yyyy"),
					EndDate = item.EndDate.HasValue ? item.EndDate.Value.ToString("dd/MM/yyyy") : DateTime.Now.ToString("dd/MM/yyyy"),
					VoucherType = VoucherTypeConst.VOUCHERCUSTOMER
				})
				.ToList();

			var vouchers = voucherShips
				.Concat(voucherProducts)
				.Concat(voucherCategories)
				.Concat(voucherCustomers)
				.ToList();

			ViewBag.Voucher = vouchers;
			return View();
		}

		
		protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
