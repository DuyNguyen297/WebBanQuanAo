using Newtonsoft.Json;
using System.Globalization;
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
using FashionShop.ViewModel;
using FashionShop.Helper;
using System.Web.Hosting;
using FashionShop.Areas.Admin.Controllers.AuthorizeAttributeConfig;
using System.Web.Services.Description;
using Aspose.Cells;

namespace FashionShop.Areas.Admin.Controllers
{
    [User]
    public class OrderController : BaseController
    {
        // GET: Order/Index?CatId0=&BranchId=0
        public ActionResult Index(int page = 1,string active = StatusConst.WAITCONFIRM, string searchkey = "")
        {
			var pageNumber = page;
			var pageSize = 8;
			var listOrders = _context.Orders
							.Include(i => i.Account)
							.Include(i => i.Customer)
							.Include(i => i.OrderDetails)
							.OrderByDescending(i => i.CreatedAt)
							.ToList();
			foreach (var order in listOrders)
			{
				order.OrderDetails = order.OrderDetails?.OrderBy(p => p.Product?.Code).ToList();
			}
			IPagedList<Order> models = listOrders.AsQueryable().ToPagedList(pageNumber, pageSize);
			ViewBag.CurrentPage = pageNumber;
			ViewBag.ColorId = _context.Colors.ToList();
			ViewBag.SizeId = _context.Sizes.ToList();
			ViewBag.AllProduct = _context.Products.ToList();
			ViewBag.StatusListSelection = new SelectList(
                new List<SelectListItem>
                {
				    new SelectListItem { Text = "Chờ xác nhận", Value = StatusConst.WAITCONFIRM},
				    new SelectListItem { Text = "Chờ lấy hàng", Value = StatusConst.WAITSETUP},
				    new SelectListItem { Text = "Chờ giao hàng", Value = StatusConst.WAITSHIP},
				    new SelectListItem { Text = "Đang giao", Value = StatusConst.SHIPPING},
				    new SelectListItem { Text = "Đã giao", Value = StatusConst.DELIVERED},
				    new SelectListItem { Text = "Hoàn thành", Value = StatusConst.DONE},
					new SelectListItem { Text = "Đã Hủy", Value = StatusConst.CANCEL},
					new SelectListItem { Text = "Trả hàng", Value = StatusConst.RETURN},
		        }, "Value", "Text");

            ViewBag.StatusList = new Dictionary<string, string>
            {
                { StatusConst.WAITCONFIRM, "Xác nhận"},
                { StatusConst.WAITSETUP, "Lên đơn"},
                { StatusConst.WAITSHIP, "Giao hàng"},
                { StatusConst.SHIPPING, "Đã nhận"},
                { StatusConst.DELIVERED, "Xác nhận thanh toán"},
                { StatusConst.DONE, "Trả hàng"},
				{ StatusConst.CANCEL, "Xóa"},
				{ StatusConst.RETURN, "Gửi lại đơn"},
               
            };
            ViewBag.CancelStatusList = new Dictionary<string, string>
			{
				{ StatusConst.WAITCONFIRM, "Xác nhận"},
				{ StatusConst.WAITSETUP, "Đã lên đơn"},
				{ StatusConst.WAITSHIP, "Đang giao hàng"},
			};
			ViewBag.Active = !string.IsNullOrEmpty(active) ? active : StatusConst.WAITCONFIRM;
			return View(models);
        }

        public ActionResult Filter(string accid = "0", string sttcode = "0")
        {
            var url = "/Order";
            if (accid != "0")
            {
                url += $"?accid={accid}";
            }
            if (sttcode != "0")
            {
                url += $"&sttcode={sttcode}";
            }

            return Redirect(url);
        }


        ///////////////////////////////////////////////
        // GET: Order/Detail/Id
        public ActionResult Details(string id)
        {
            if (id == null || _context.OrderDetails == null)
            {
                return HttpNotFound();
            }
            var pageNumber = 1;
            var pageSize = 8;
            var orderDetail = _context.OrderDetails
                                .Include(i => i.Order)
                                .Include(i => i.Product)
                                .Include(i => i.Product.Size)
                                .Include(i => i.Product.Color)
                                .Include(i => i.Product.Branch)
                                .Include(i => i.Product.Category)
                                .Where(x => x.OrderId == id);
            ViewBag.ProductList = new SelectList(_context.Products, "Id", "Name");
            PagedList<OrderDetail> models = new PagedList<OrderDetail>(orderDetail.AsQueryable(), pageNumber, pageSize);
            ViewBag.CurrentPage = pageNumber;
            ViewBag.Id = id;
            ViewBag.Status = _context.Orders.Find(id).Status;
            return View(models);
        }
       
        // POST: Order/Edit/id
        [HttpPost, ActionName("Edit")]
        public async Task<ActionResult> EditConfirm([Bind(Include ="Id,Code,Name,Phone,Address")] Order order)
        {
            if (order.Id == null || _context.Orders == null)
            {
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Order is null!");
			}
            var orderedit = _context.Orders.Find(order.Id);
            string active = orderedit.Status;
            if (ModelState.IsValid)
            {
                orderedit.Name = order.Name;
                orderedit.Phone = order.Phone;
                orderedit.Address = order.Address;
                orderedit.UpdatedAt = DateTime.Now;
                orderedit.UpdateUserId = Session[Const.ADMINIDSESSION]?.ToString();
                _context.Orders.Attach(orderedit);
                _context.Entry(orderedit).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                TempData["success"] = "Cập nhật thành công";
                return RedirectToAction(nameof(Index), new { active = active });
            }
			TempData["error"] = "Thông tin không chính xác!";
			return RedirectToAction(nameof(Index), new { active = active });
		}

        // POST: Order/EditDetail/id
        [HttpPost]
        public async Task<ActionResult> EditDetail(string id, [Bind(Include ="Id,Quantity,ProductId")] OrderDetail orderDetail)
        {
            if (id != orderDetail.Id)
            {
                return HttpNotFound();
            }

            if (ModelState.IsValid)
            {
                var orderDetailEdit = _context.OrderDetails.Find(id);
                orderDetailEdit.UpdatedAt = DateTime.Now;
                orderDetailEdit.UpdateUserId = Session[Const.ADMINIDSESSION]?.ToString();
                
                orderDetailEdit.Quantity = orderDetail.Quantity;
                orderDetailEdit.ProductId = orderDetail.ProductId;

                var product = _context.Products.Find(orderDetail.ProductId);

                orderDetailEdit.Total = (product.Price - (product.Discount ?? 0)) * orderDetail.Quantity;
                _context.OrderDetails.Attach(orderDetailEdit);
                _context.Entry(orderDetailEdit).State = EntityState.Modified;

                Order order = await _context.Orders.FindAsync(orderDetailEdit.OrderId);
                order.Total = await _context.OrderDetails.Where(x => x.OrderId == order.Id && x.Id != id).SumAsync(x => x.Total) + orderDetailEdit.Total - order.Discount + order.ShipFee;
                order.UpdatedAt = DateTime.Now;
                order.UpdateUserId = Session[Const.ADMINIDSESSION]?.ToString();
                _context.Orders.Attach(order);
                _context.Entry(order).State = EntityState.Modified;

                await _context.SaveChangesAsync();
                TempData["success"] = "Cập nhật thành công";
                return RedirectToAction("Details", new {id = order.Id});
            }
            return View(orderDetail);
        }

        // POST: Order/Cancel/Id
        [HttpPost, ActionName("Cancel")]
        public async Task<ActionResult> CancelConfirm(ReasonView rs)
        {
            if (rs.Id == null ||_context.Orders == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Order is null!");
            }
            var order = _context.Orders.Find(rs.Id);
            var active = order.Status;
            order.Status = StatusConst.CANCEL;
            if (rs.Reason == Const.REFUSEREASON)
            {
                order.Reason = rs.RefuseReason;
            }
            else
            {
                order.Reason = rs.Reason;
            }
            order.UpdatedAt = DateTime.Now;
            order.UpdateUserId = Session[Const.ADMINIDSESSION]?.ToString();

            _context.Orders.Attach(order);
            _context.Entry(order).State = EntityState.Modified;

            await _context.SaveChangesAsync();

            TempData["success"] = "Hủy đơn hàng thành công";
			return RedirectToAction(nameof(Index), new { active = active });
		}
		// POST: Order/Cancel/Id
		[HttpPost, ActionName("Reject")]
		public async Task<ActionResult> RejectConfirm(ReasonView rs)
		{
			if (rs.Id == null || _context.Orders == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Order is null!");
			}
			var order = _context.Orders.Find(rs.Id);
			var active = order.Status;
			order.Status = StatusConst.RETURN;
			if (rs.Reason == Const.REFUSEREASON)
			{
				order.Reason = rs.RefuseReason;
			}
			else
			{
				order.Reason = rs.Reason;
			}
			order.UpdatedAt = DateTime.Now;
			order.UpdateUserId = Session[Const.ADMINIDSESSION]?.ToString();

			_context.Orders.Attach(order);
			_context.Entry(order).State = EntityState.Modified;

			await _context.SaveChangesAsync();

			TempData["success"] = "Trả đơn hàng thành công";
			return RedirectToAction(nameof(Index), new { active = active });
		}
		// POST: Order/Cancel/Id
		[HttpPost, ActionName("Return")]
		public async Task<ActionResult> ReturnConfirm(ReasonView rs)
		{
			if (rs.Id == null || _context.Orders == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Order is null!");
			}
			var order = _context.Orders.Find(rs.Id);
			var active = order.Status;
			order.Status = StatusConst.RETURN;
			if (rs.Reason == Const.REFUSEREASON)
			{
				order.Reason = rs.RefuseReason;
			}
			else
			{
				order.Reason = rs.Reason;
			}
			order.UpdatedAt = DateTime.Now;
			order.UpdateUserId = Session[Const.ADMINIDSESSION]?.ToString();

			_context.Orders.Attach(order);
			_context.Entry(order).State = EntityState.Modified;

			await _context.SaveChangesAsync();

			TempData["success"] = "Trả đơn hàng thành công";
			return RedirectToAction(nameof(Index), new { active = active });
		}
		// POST: Order/Cancel/Id
		[HttpPost, ActionName("Import")]
		public async Task<ActionResult> ImportConfirm(string id)
		{
			if (id == null || _context.Orders == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Order is null!");
			}
			var order = _context.Orders.Find(id);
			var active = order.Status;
			    
            var orderDetails = _context.OrderDetails.Where(o => o.OrderId == id).ToList();
            foreach (var item in orderDetails)
            {
                var product = _context.Products.FirstOrDefault(p => p.Id == item.ProductId);
                product.Quantity += item.Quantity;
                _context.Products.Attach(product);
                _context.Entry(product).State = EntityState.Modified;
            }
			await _context.SaveChangesAsync();

            // đơn hàng rác
            order.IsBlackOrder = true;
			_context.Orders.Attach(order);
			_context.Entry(order).State = EntityState.Modified;
			await _context.SaveChangesAsync();

			TempData["success"] = "Thành công!";
			TempData["info"] = "Các sản phẩm trong đơn hàng đã được cập nhật lại";
			return RedirectToAction(nameof(Index), new { active = active });
		}
		// POST: Order/Cancel/Id
		[HttpPost, ActionName("ReSetup")]
		public async Task<ActionResult> ReSetupConfirm(string id)
		{
			if (id == null || _context.Orders == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Order is null!");
			}
			var order = _context.Orders.Find(id);
			var active = order.Status;
			order.Status = StatusConst.WAITSETUP;
			
			order.UpdatedAt = DateTime.Now;
			order.UpdateUserId = Session[Const.ADMINIDSESSION]?.ToString();

			_context.Orders.Attach(order);
			_context.Entry(order).State = EntityState.Modified;

			await _context.SaveChangesAsync();

			TempData["success"] = "Đơn hàng đã được chuyển sang trạng thái chờ lấy hàng thành công";
			return RedirectToAction(nameof(Index), new { active = active });
		}

		// POST: Order/Delete/Id
		[HttpPost, ActionName("Delete")]
        public async Task<ActionResult> DeleteConfirm(string id)
        {
            if (id == null || _context.Orders == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Orders is null!");
            }
			var order = _context.Orders.Find(id);
			var active = order.Status;

			var orderDetails = _context.OrderDetails.Where(o => o.OrderId == id).ToList();

			_context.OrderDetails.RemoveRange(orderDetails);
			await _context.SaveChangesAsync();

			_context.Orders.Remove(order);
			await _context.SaveChangesAsync();

			TempData["success"] = "Xóa đơn hàng thành công";
			return RedirectToAction(nameof(Index), new {active = active });
        }

        // POST: Order/DeleteOrderDetail/Id
        [HttpPost, ActionName("DeleteDetail")]
        public async Task<ActionResult> DeleteDetail(string deleteid)
        {
            if (_context.OrderDetails == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "OrderDetails is null!");
            }
            var orderdetail = await _context.OrderDetails.FindAsync(deleteid);
            _context.OrderDetails.Remove(orderdetail);
            Order order = await _context.Orders.FindAsync(orderdetail.OrderId);
            order.Total = await _context.OrderDetails.Where(x => x.OrderId == order.Id && x.Id != deleteid).SumAsync(x => x.Total) - order.Discount + order.ShipFee;
            order.UpdatedAt = DateTime.Now;
            order.UpdateUserId = Session[Const.ADMINIDSESSION]?.ToString();
            string notify = "";
            if(order.Total == 0)
            {
                await _context.SaveChangesAsync();
                order.Status = Shared.StatusConst.CANCEL;
                _context.Orders.Attach(order);
                _context.Entry(order).State = EntityState.Modified;
                notify = "Đơn hàng trống! Đã hủy đơn hàng";
            }
            else
            {
                _context.Orders.Attach(order);
                _context.Entry(order).State = EntityState.Modified;
                notify = "Xóa thành công! Đơn hàng đã được cập nhật";
            }
          
            await _context.SaveChangesAsync();

            TempData["success"] = notify;
            return RedirectToAction("Details", new {id= order.Id});
        }

        // Xử lí đơn hàng
        public async Task<ActionResult> Skip(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var order = await _context.Orders.FindAsync(id);
            string notify = "";
			string active = order.Status;
			switch (order.Status)
            {
                case Shared.StatusConst.WAITCONFIRM:
                    order.Status = Shared.StatusConst.WAITSETUP;
                    notify = "Đã xác nhận đơn hàng!";
                    break;
                case (Shared.StatusConst.WAITSETUP):
                    order.Status = Shared.StatusConst.WAITSHIP;
                    notify = "Đã chuẩn bị đơn hàng";
                    break;
                case (Shared.StatusConst.WAITSHIP):
					order.Status = Shared.StatusConst.SHIPPING;
					order.ShipDate = DateTime.Now;
					notify = "Đã gửi đơn hàng. Đơn hàng đang được giao";
					break;
                case Shared.StatusConst.SHIPPING:
                    order.Status = Shared.StatusConst.DELIVERED;
					order.ReceiveDate = DateTime.Now;
					notify = "Đơn hàng đã được giao. Vui lòng xác nhận thanh toán!";
                    break;
                case Shared.StatusConst.DELIVERED:
					order.Status = Shared.StatusConst.DONE;
					order.PayStatus = Shared.PayStatusConst.DONE;
					if (order.PayWay == Shared.PayConst.ONLINE)
					{
						notify = "Xác nhận thanh toán MoMo thành công";
						break;
					}
					else
					{
						notify = "Xác nhận thanh toán thành công";
						break;
					}
				case Shared.StatusConst.DONE:
					order.Status = Shared.StatusConst.DONE;
                    break;
				case Shared.StatusConst.RETURN:
                    order.Status = Shared.StatusConst.SHIPPING;
					order.ShipDate = DateTime.Now;
					order.PayStatus = Shared.PayStatusConst.NODONE;
					notify = "Đã gửi đơn hàng. Đơn hàng đang được giao";
					break;
                default:
                    break;

            }
            order.UpdatedAt = DateTime.Now;
            order.UpdateUserId = Session[Const.ADMINIDSESSION]?.ToString();
            _context.Orders.Attach(order);
            _context.Entry(order).State = EntityState.Modified;

            await _context.SaveChangesAsync();
            TempData["success"] = notify;

			return RedirectToAction("Index", new {active = active });
        }

		[HttpPost]
		public ActionResult GetBillById(string id)
		{
			var order = _context.Orders.Include(i => i.Account).FirstOrDefault(i => i.Id == id);
			if (order == null)
			{
				return Json(new { error = false, message = "Không tìm thấy đơn hàng! Vui lòng kiểm tra lại" });
			}
			order.OrderDetails = _context.OrderDetails
				.Include(o => o.Product)
				.Include(o => o.Product.Branch)
				.Include(o => o.Product.Category)
				.Include(o => o.Product.Color)
				.Include(o => o.Product.Size)
				.OrderBy(o => o.Product.Code)
				.Where(o => o.OrderId == order.Id)
				.ToList();
			return PartialView("_BillPartial", order);
		}

		// Xuất hóa đơn
		public ActionResult ExportBill(string id, string option = "excel")
        {
            var order = _context.Orders.Find(id);
            string timestamp = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture).ToUpper().Replace(':', '_').Replace('.', '_').Replace(' ', '_').Trim();
            var templateFileInfo = new FileInfo(HostingEnvironment.MapPath("~/Template/BillTemplate.xlsx"));
            var orderDetails = _context.OrderDetails
                .Include(o => o.Product)
                .Include(o => o.Product.Branch)
                .Include(o => o.Product.Category)
                .Include(i => i.Product.Size)
                .Include(i => i.Product.Color)
                .Where(x => x.OrderId == order.Id)
                .ToList();
			var stream = ExportToExcelHelper.UpdateDataIntoExcelSellTemplate(orderDetails, order, templateFileInfo);

			if (option == "pdf")
			{
				Workbook workbook = new Workbook(stream);

				Aspose.Cells.PdfSaveOptions opts = new Aspose.Cells.PdfSaveOptions();
				opts.AllColumnsInOnePagePerSheet = true;
				opts.OptimizationType = Aspose.Cells.Rendering.PdfOptimizationType.MinimumSize;

				MemoryStream msPdf = new MemoryStream();
				workbook.Save(msPdf, opts);
				msPdf.Seek(0, SeekOrigin.Begin);

				byte[] buffer = new byte[msPdf.Length];
				buffer = msPdf.ToArray();
				return File(buffer, "application/pdf", "Bill-" + timestamp + ".pdf");
			}
			//excel
			return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Bill-" + timestamp + ".xlsx");
		}
        private bool OrderExists(string id)
        {
            return _context.Orders.Any(e => e.Id.ToString() == id);
        }
    }
}
