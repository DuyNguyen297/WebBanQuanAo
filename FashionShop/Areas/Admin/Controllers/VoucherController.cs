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
using FashionShop.Areas.Admin.Controllers.AuthorizeAttributeConfig;
using System.Web.Hosting;
using FashionShop.AreaModels;
using System.Windows.Input;
using System.Globalization;

namespace FashionShop.Areas.Admin.Controllers
{
    [Admin]
    public class VoucherController : BaseController
    {
        // GET: Voucher/Index
        public ActionResult Index(int? page, string active = VoucherTypeConst.VOUCHERSHIP, string searchkey = "")
        {
            var pageNumber = page == null || page <= 0 ? 1 : page.Value;
            var pageSize = 5;
            ViewBag.VoucherType = new Dictionary<string, string>
			{
				{ VoucherTypeConst.VOUCHERSHIP, "Voucher tiền giao hàng"},
				//{ VoucherTypeConst.VOUCHERPRODUCT, "Voucher cho sản phẩm"},
				//{ VoucherTypeConst.VOUCHERCATEGORY, "Voucher theo danh mục"},
				{ VoucherTypeConst.VOUCHERCUSTOMER, "Voucher cho khách hàng"},
			};

			ViewBag.VoucherShip = new PagedList.PagedList<VoucherShip>(_context.VoucherShips.Include(v => v.Account).ToList(), pageNumber, pageSize);
			ViewBag.VoucherProduct = new PagedList.PagedList<VoucherProduct>(_context.VoucherProducts.Include(v => v.Product).ToList(), pageNumber, pageSize);
			ViewBag.VoucherCategory = new PagedList.PagedList<VoucherCategory>(_context.VoucherCategories.Include(v => v.Category).ToList(), pageNumber, pageSize);
			ViewBag.VoucherCustomer = new PagedList.PagedList<VoucherCustomer>(_context.VoucherCustomers.Include(v => v.Customer).ToList(), pageNumber, pageSize);

			ViewBag.Active = !string.IsNullOrEmpty(active) ? active : VoucherTypeConst.VOUCHERSHIP;

			ViewBag.CurrentPage = pageNumber;
            return View();
        }

		// GET: Voucher/Detail/Id
		public async Task<ActionResult> DetailVoucherShip(string id)
		{
			if (id == null || _context.VoucherShips == null)
			{
				return HttpNotFound();
			}

			var Voucher = await _context.VoucherShips
				.FirstOrDefaultAsync(m => m.Id == id);
			if (Voucher == null)
			{
				return HttpNotFound();
			}

			return View(Voucher);
		}
		// GET: Voucher/Detail/Id
		public async Task<ActionResult> DetailVoucherCustomer(string id)
		{
			if (id == null || _context.VoucherCustomers == null)
			{
				return HttpNotFound();
			}

			var Voucher = await _context.VoucherCustomers.Include(v => v.Customer)
				.FirstOrDefaultAsync(m => m.Id == id);
			if (Voucher == null)
			{
				return HttpNotFound();
			}

			return View(Voucher);
		}

		// GET: Voucher/Create
		public ActionResult CreateVoucherShip()
        {
            return View();
        }        
        public ActionResult CreateVoucherProduct()
        {
            return View();
        }        
        public ActionResult CreateVoucherCategory()
        {
            return View();
        }        
        public ActionResult CreateVoucherCustomer()
		{
            ViewBag.Customer = _context.Customers.OrderByDescending(c => c.CreatedAt).ToList();
			return View();
        }

        //POST: Voucher/Create
       [HttpPost]
        public async Task<ActionResult> CreateVoucherShip([Bind(Include = "Id,Name,StartDate,EndDate,TotalCondition,Discount,Quantity,CreatedAt,UpdatedAt,DeletedAt,UpdateUserId,CreateUserId")] CreateVoucherModel voucher)
        {
            if (ModelState.IsValid)
            {
                voucher.Id = Guid.NewGuid().ToString();
				voucher.Name = Utilities.ToTitleCase(voucher.Name);


                voucher.CreateUserId = Session[Const.ADMINIDSESSION]?.ToString();
                voucher.CreatedAt = DateTime.Now;
                _context.VoucherShips.Add(new VoucherShip()
                {
                    Id = voucher.Id,
                    Name = voucher.Name,
                    TotalCondition = voucher.TotalCondition,
                    Discount = voucher.Discount,
                    Quantity = voucher.Quantity,
                    StartDate = DateTime.ParseExact(voucher.StartDate, "MM/dd/yyyy", CultureInfo.InvariantCulture),
                    EndDate = DateTime.ParseExact(voucher.EndDate, "MM/dd/yyyy", CultureInfo.InvariantCulture),
                    CreateUserId = voucher.CreateUserId
				});
				await _context.SaveChangesAsync();
                TempData["success"] = "Thêm voucher thành công";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                TempData["error"] = "Dữ liệu không đúng định dạng";
            }
            return View(voucher);
        }
		[HttpPost]
		public async Task<ActionResult> CreateVoucherCustomer([Bind(Include = "Id,Name,StartDate,EndDate,TotalCondition,Discount,Quantity,Customer, IsForAll, CreatedAt,UpdatedAt,DeletedAt,UpdateUserId,CreateUserId")] CreateVoucherCustomerModel voucher)
		{
			if (ModelState.IsValid)
			{
				voucher.Name = Utilities.ToTitleCase(voucher.Name);

				voucher.CreateUserId = Session[Const.ADMINIDSESSION]?.ToString();
				voucher.CreatedAt = DateTime.Now;
				if (voucher.IsForAll)
				{
					var customers = _context.Customers.ToList();
					foreach (var customer in customers)
					{
						_context.VoucherCustomers.Add(new VoucherCustomer()
						{
							Id = Guid.NewGuid().ToString(),
							Name = voucher.Name,
							TotalCondition = voucher.TotalCondition,
							Discount = voucher.Discount,
							Quantity = voucher.Quantity,
							CustomerId = customer.Id,
							StartDate = DateTime.ParseExact(voucher.StartDate, "MM/dd/yyyy", CultureInfo.InvariantCulture),
							EndDate = DateTime.ParseExact(voucher.EndDate, "MM/dd/yyyy", CultureInfo.InvariantCulture),
							CreateUserId = voucher.CreateUserId
						});
					}
				}
				else
				{
					if (voucher.Customer.Count() == 0)
					{
						TempData["error"] = "Chưa có khách hàng nào được chọn";
						return View(voucher);
					}
					foreach(var item in voucher.Customer)
					{
						_context.VoucherCustomers.Add(new VoucherCustomer()
						{
							Id = Guid.NewGuid().ToString(),
							Name = voucher.Name,
							TotalCondition = voucher.TotalCondition,
							Discount = voucher.Discount,
							Quantity = voucher.Quantity,
							CustomerId = item,
							StartDate = DateTime.ParseExact(voucher.StartDate, "MM/dd/yyyy", CultureInfo.InvariantCulture),
							EndDate = DateTime.ParseExact(voucher.EndDate, "MM/dd/yyyy", CultureInfo.InvariantCulture),
							CreateUserId = voucher.CreateUserId
						});
					}
				}
				
				await _context.SaveChangesAsync();
				TempData["success"] = "Thêm voucher thành công";
				return RedirectToAction(nameof(Index));
			}
			else
			{
				TempData["error"] = "Dữ liệu không đúng định dạng";
			}
			return View(voucher);
		}

		// GET: Voucher/Edit/Id
		public async Task<ActionResult> EditVoucherShip(string id)
		{
			if (id == null || _context.VoucherShips == null)
			{
				return HttpNotFound();
			}

			var Voucher = await _context.VoucherShips.FindAsync(id);
			if (Voucher == null)
			{
				return HttpNotFound();
			}

			var voucherEdit = new CreateVoucherModel()
			{
				Id = id,
				Name = Voucher.Name,
				Discount = Voucher.Discount,
				TotalCondition = Voucher.TotalCondition,
				Quantity = Voucher.Quantity,
				StartDate = Voucher.StartDate?.ToString("MM/dd/yyyy"),
				EndDate = Voucher.EndDate?.ToString("MM/dd/yyyy"),
			};
			return View(voucherEdit);
		}

		// POST: Category/Edit/Id
		[HttpPost]

		public async Task<ActionResult> EditVoucherShip(string id, [Bind(Include = "Id,Name,StartDate,EndDate,TotalCondition,Discount,Quantity,CreatedAt,UpdatedAt,DeletedAt,UpdateUserId,CreateUserId")] CreateVoucherModel voucher)
		{
			if (id != voucher.Id.ToString())
			{
				return HttpNotFound();
			}

			if (ModelState.IsValid)
			{
				try
				{
					var voucherUpd = _context.VoucherShips.Find(id);
					voucherUpd.Name = voucher.Name;
					voucherUpd.TotalCondition = voucher.TotalCondition;
					voucherUpd.Discount = voucher.Discount;
					voucherUpd.Quantity = voucher.Quantity;
					voucherUpd.StartDate = DateTime.ParseExact(voucher.StartDate, "MM/dd/yyyy", CultureInfo.InvariantCulture);
					voucherUpd.EndDate = DateTime.ParseExact(voucher.EndDate, "MM/dd/yyyy", CultureInfo.InvariantCulture);
					voucher.UpdatedAt = DateTime.Now;
					voucher.UpdateUserId = Session[Const.ADMINIDSESSION]?.ToString();
					_context.VoucherShips.Attach(voucherUpd);
					_context.Entry(voucherUpd).State = EntityState.Modified;

					await _context.SaveChangesAsync();
					TempData["success"] = "Cập nhật voucher thành công!";
				}
				catch (Exception ex)
				{
					if (!VoucherShipExists(voucher.Id.ToString()))
					{
						return HttpNotFound();
					}
					else
					{
						throw ex;
					}
				}
				return RedirectToAction(nameof(Index));
			}
			return View(voucher);
		}
		// GET: Voucher/Edit/Id
		public async Task<ActionResult> EditVoucherCustomer(string id)
		{
			if (id == null || _context.VoucherCustomers == null)
			{
				return HttpNotFound();
			}

			var Voucher = await _context.VoucherCustomers.FindAsync(id);
			if (Voucher == null)
			{
				return HttpNotFound();
			}

			var voucherEdit = new CreateVoucherCustomerModel()
			{
				Id = id,
				Name = Voucher.Name,
				Discount = Voucher.Discount,
				TotalCondition = Voucher.TotalCondition,
				Quantity = Voucher.Quantity,
				StartDate = Voucher.StartDate?.ToString("MM/dd/yyyy"),
				EndDate = Voucher.EndDate?.ToString("MM/dd/yyyy"),
			};
			ViewBag.Customer = _context.Customers.Find(Voucher.CustomerId);
			return View(voucherEdit);
		}

		// POST: Category/Edit/Id
		[HttpPost]

		public async Task<ActionResult> EditVoucherCustomer(string id, [Bind(Include = "Id,Name,StartDate,EndDate,TotalCondition,Discount,Quantity, CreatedAt,UpdatedAt,DeletedAt,UpdateUserId,CreateUserId")] CreateVoucherCustomerModel voucher)
		{
			if (id != voucher.Id.ToString())
			{
				return HttpNotFound();
			}

			if (ModelState.IsValid)
			{
				try
				{
					var voucherUpd = _context.VoucherCustomers.Find(id);
					voucherUpd.Name = voucher.Name;
					voucherUpd.TotalCondition = voucher.TotalCondition;
					voucherUpd.Discount = voucher.Discount;
					voucherUpd.Quantity = voucher.Quantity;
					voucherUpd.StartDate = DateTime.ParseExact(voucher.StartDate, "MM/dd/yyyy", CultureInfo.InvariantCulture);
					voucherUpd.EndDate = DateTime.ParseExact(voucher.EndDate, "MM/dd/yyyy", CultureInfo.InvariantCulture);
					voucher.UpdatedAt = DateTime.Now;
					voucher.UpdateUserId = Session[Const.ADMINIDSESSION]?.ToString();
					_context.VoucherCustomers.Attach(voucherUpd);
					_context.Entry(voucherUpd).State = EntityState.Modified;

					await _context.SaveChangesAsync();
					TempData["success"] = "Cập nhật voucher thành công!";
				}
				catch (Exception ex)
				{
					if (!VoucherShipExists(voucher.Id.ToString()))
					{
						return HttpNotFound();
					}
					else
					{
						throw ex;
					}
				}
				return RedirectToAction(nameof(Index));
			}
			var voucherCus = _context.VoucherCustomers.Find(id);
			ViewBag.Customer = _context.Customers.Find(voucherCus.CustomerId);
			return View(voucher);
		}
		// GET: Voucher/Delete/Id
		public async Task<ActionResult> DeleteVoucherShip(string id)
		{
			if (id == null || _context.VoucherShips == null)
			{
				return HttpNotFound();
			}

			var publisher = await _context.VoucherShips
				.FirstOrDefaultAsync(m => m.Id == id);
			if (publisher == null)
			{
				return HttpNotFound();
			}

			return View(publisher);
		}

		// POST: Voucher/Delete/Id

		[HttpPost, ActionName("DeleteVoucherShip")]

		public async Task<ActionResult> DeleteVoucherShipConfirm(string id)
		{
			if (_context.VoucherShips == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Voucher is null!");
			}
			var voucher = _context.VoucherShips.Find(id);
			_context.VoucherShips.Remove(voucher);
			await _context.SaveChangesAsync();
			TempData["success"] = "Xóa voucher thành công";
			return RedirectToAction(nameof(Index));
		}

		public async Task<ActionResult> DeleteVoucherCustomer(string id)
		{
			if (id == null || _context.VoucherShips == null)
			{
				return HttpNotFound();
			}

			var publisher = await _context.VoucherCustomers.Include(v => v.Customer)
				.FirstOrDefaultAsync(m => m.Id == id);
			if (publisher == null)
			{
				return HttpNotFound();
			}

			return View(publisher);
		}

		// POST: Voucher/Delete/Id

		[HttpPost, ActionName("DeleteVoucherCustomer")]

		public async Task<ActionResult> DeleteVoucherCustomerConfirm(string id)
		{
			if (_context.VoucherCustomers == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Voucher is null!");
			}
			var voucher = _context.VoucherCustomers.Find(id);
			_context.VoucherCustomers.Remove(voucher);
			await _context.SaveChangesAsync();
			TempData["success"] = "Xóa voucher thành công";
			return RedirectToAction(nameof(Index));
		}
		private bool VoucherShipExists(string id)
		{
			return _context.VoucherShips.Any(e => e.Id == id);
		}
		private bool VoucherCustomerExists(string id)
		{
			return _context.VoucherCustomers.Any(e => e.Id == id);
		}
		private bool VoucherProductExists(string id)
		{
			return _context.VoucherProducts.Any(e => e.Id == id);
		}
		private bool VoucherCategoryExists(string id)
		{
			return _context.VoucherCategories.Any(e => e.Id == id);
		}
	}
}
