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
    public class OrderController : GlobalController
    {
        public ActionResult Index(int page = 1, string active = "all")
        {
			var pageNumber = page;
			var pageSize = 8;
            string cusId = !string.IsNullOrEmpty(Session[Const.USERIDSESSION]?.ToString()) ? Session[Const.USERIDSESSION]?.ToString() : Session[Const.CARTSESSION]?.ToString();
			var listOrders = _context.Orders
							.Include(o => o.Account)
							.Include(o => o.Customer)
							.Include(o => o.OrderDetails)
							.Include(o => o.OrderDetails.Select(od => od.Product).Select(odd => odd.Category))
							.Include(o => o.OrderDetails.Select(od => od.Product).Select(odd => odd.Branch))
							.Include(o => o.OrderDetails.Select(od => od.Product).Select(odd => odd.Color))
							.Include(o => o.OrderDetails.Select(od => od.Product).Select(odd => odd.Size))
							.Where(o => o.CustomerId == cusId)
							.OrderByDescending(o => o.CreatedAt)
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
					new SelectListItem { Text = "Chờ lên đơn", Value = StatusConst.WAITSETUP},
					new SelectListItem { Text = "Chờ giao hàng", Value = StatusConst.WAITSHIP},
					new SelectListItem { Text = "Đang giao", Value = StatusConst.SHIPPING},
					new SelectListItem { Text = "Đã giao", Value = StatusConst.DELIVERED},
					new SelectListItem { Text = "Hoàn thành", Value = StatusConst.DONE},
					new SelectListItem { Text = "Trả hàng", Value = StatusConst.RETURN},
					new SelectListItem { Text = "Đã Hủy", Value = StatusConst.CANCEL},
				}, "Value", "Text");
			ViewBag.StatusList = new Dictionary<string, string>
			{
				{ StatusConst.WAITCONFIRM, "Chờ xác nhận"},
				{ StatusConst.WAITSETUP, "Vận chuyển"},
				{ StatusConst.SHIPPING, "Chờ giao hàng"},
				{ StatusConst.DONE, "Hoàn thành"},
				{ StatusConst.CANCEL, "Đã hủy"},
				{ StatusConst.RETURN, "Trả hàng"},
			};
			ViewBag.CancelStatusList = new Dictionary<string, string>
			{
				{ StatusConst.WAITCONFIRM, "Xác nhận"},
				{ StatusConst.WAITSETUP, "Đã lên đơn"},
				{ StatusConst.WAITSHIP, "Đang giao hàng"},
            };
			ViewBag.ShipStatusList = new Dictionary<string, string>
			{
				{ StatusConst.SHIPPING, "Đang giao"},
				{ StatusConst.DELIVERED, "Đã giao"},
			};
			ViewBag.Active = !string.IsNullOrEmpty(active) ? active : "all";
            return View(models);
        }
		// Render Status

		// GET: Order/Details/5
		public ActionResult Details(string id)
        {
			if (id == null || _context.Orders == null)
			{
				return HttpNotFound();
			}
			var order = _context.Orders
								.Include(i => i.OrderDetails)
							    .Include(o => o.OrderDetails.Select(od => od.Product).Select(odd => odd.Category))
							    .Include(o => o.OrderDetails.Select(od => od.Product).Select(odd => odd.Branch))
							    .Include(o => o.OrderDetails.Select(od => od.Product).Select(odd => odd.Color))
							    .Include(o => o.OrderDetails.Select(od => od.Product).Select(odd => odd.Size))
								.Where(x => x.Id == id)
                                .FirstOrDefault();
			ViewBag.StatusListSelection = new SelectList(
				new List<SelectListItem>
				{
					new SelectListItem { Text = "Chờ xác nhận", Value = StatusConst.WAITCONFIRM},
					new SelectListItem { Text = "Chờ lên đơn", Value = StatusConst.WAITSETUP},
					new SelectListItem { Text = "Chờ giao hàng", Value = StatusConst.WAITSHIP},
					new SelectListItem { Text = "Đang giao", Value = StatusConst.SHIPPING},
					new SelectListItem { Text = "Đã giao", Value = StatusConst.DELIVERED},
					new SelectListItem { Text = "Hoàn thành", Value = StatusConst.DONE},
					new SelectListItem { Text = "Trả hàng", Value = StatusConst.RETURN},
					new SelectListItem { Text = "Đã Hủy", Value = StatusConst.CANCEL},
				}, "Value", "Text");
			ViewBag.StatusList = new Dictionary<string, string>
			{
				{ StatusConst.WAITCONFIRM, "Chờ xác nhận"},
				{ StatusConst.WAITSETUP, "Vận chuyển"},
				{ StatusConst.SHIPPING, "Chờ giao hàng"},
				{ StatusConst.DONE, "Hoàn thành"},
				{ StatusConst.CANCEL, "Đã hủy"},
				{ StatusConst.RETURN, "Trả hàng/Hoàn tiền"},
			};
			ViewBag.StatusListProgress = new Dictionary<string, string>
			{
				{ StatusConst.WAITCONFIRM, "Chờ xác nhận"},
				{ StatusConst.WAITSETUP, "Vận chuyển"},
				{ StatusConst.SHIPPING, "Chờ giao hàng"},
				{ StatusConst.DONE, "Hoàn thành"}
			};
			ViewBag.CancelStatusList = new Dictionary<string, string>
			{
				{ StatusConst.WAITCONFIRM, "Xác nhận"},
				{ StatusConst.WAITSETUP, "Đã lên đơn"},
				{ StatusConst.WAITSHIP, "Đang giao hàng"},
			};
			ViewBag.ShipStatusList = new Dictionary<string, string>
			{
				{ StatusConst.SHIPPING, "Đang giao"},
				{ StatusConst.DELIVERED, "Đã giao"},
			};
			ViewBag.ExceptStatusList = new Dictionary<string, string>
			{
				{ StatusConst.CANCEL, "Đã hủy"},
				{ StatusConst.RETURN, "Trả hàng/Hoàn tiền"},
			};
			return PartialView("_OrderDetailModelPartial", order);
		}

		
		[HttpPost]
		public async Task<ActionResult> ConfirmDelivered(string id)
		{
			if (id == null || _context.Orders == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Orders is null!");
			}
			var order = _context.Orders.Find(id);
            order.Status = StatusConst.DONE;
            order.ReceiveDate = DateTime.Now;
			order.UpdatedAt = DateTime.Now;
			_context.Orders.Attach(order);
		    _context.Entry(order).State = EntityState.Modified;
		    await _context.SaveChangesAsync();

		    TempData["success"] = "Xác nhận nhận hàng thành công!";
		    return RedirectToAction(nameof(Index));	
		}
		
		// POST: Order/Cancel
		[HttpPost]
        public async Task<ActionResult> Cancel(ReasonView rs)
        {
            if (rs.Id == null || _context.Orders == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Orders is null!");
            }
            var order = _context.Orders.Find(rs.Id);
            var listCancel = new List<string>()
                {
                    StatusConst.WAITCONFIRM,
                    StatusConst.WAITSETUP,
                    StatusConst.WAITSHIP,
                };
            if (listCancel.Contains(order.Status))
            {
                order.Status = StatusConst.CANCEL;
                order.UpdatedAt = DateTime.Now;
                if(rs.Reason == Const.REFUSEREASON)
                {
                    order.Reason = rs.RefuseReason;
                }
                else
                {
                    order.Reason = rs.Reason;
                }

                _context.Orders.Attach(order);
                _context.Entry(order).State = EntityState.Modified;
               
                await _context.SaveChangesAsync();

                TempData["success"] = "Hủy đơn hàng thành công";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                TempData["error"] = "Đơn hàng của bạn hết hiệu lực được hủy, vui lòng kiểm tra lại!";
                return RedirectToAction(nameof(Index));
            }
            
        }

		public ActionResult GetDistrictByProvinceId(string provinceId)
		{
			var listDistrict = _context.Districts.Where(d => d.ProvinceId == provinceId).OrderBy(d => d.Name).ToList();
			return Json(new { data = listDistrict }, JsonRequestBehavior.AllowGet);
		}
		public ActionResult GetWardByDistrictId(string districtId)
		{
			var listWard = _context.Wards.Where(d => d.DistrictId == districtId).OrderBy(d => d.Name).ToList();
			return Json(new { data = listWard }, JsonRequestBehavior.AllowGet);
		}

		[HttpPost]
		public ActionResult SelectVoucher(string voucherId, string type)
		{
			if (voucherId != "nan")
			{
				if (type == VoucherTypeConst.VOUCHERSHIP)
				{
					var voucher = _context.VoucherShips.FirstOrDefault(v => v.Id == voucherId);
					var totalPriceCart = GetToTalPriceCart();
					if (voucher.TotalCondition <= totalPriceCart)
					{
						Session[VoucherTypeConst.VOUCHERSHIP] = voucherId;
						return Json(new { status = "success", message = "Đã áp dụng voucher phí vận chuyển thành công" });
					}
					return Json(new { status = "warning", message = "Đơn hàng chưa đạt giá trị tối thiếu để áp dụng voucher này!" });
				}
				else if (type == VoucherTypeConst.VOUCHERPRODUCT)
				{
					var voucher = _context.VoucherProducts.FirstOrDefault(v => v.Id == voucherId);
					var totalPriceCart = GetToTalPriceCart();
					if (voucher.TotalCondition <= totalPriceCart)
					{
						Session[VoucherTypeConst.VOUCHERPRODUCT] = voucherId;
						return Json(new { status = "success", message = "Đã áp dụng voucher cho sản phẩm thành công" });
					}
					return Json(new { status = "warning", message = "Đơn hàng chưa đạt giá trị tối thiếu để áp dụng voucher này!" });
				}
				else if (type == VoucherTypeConst.VOUCHERCATEGORY)
				{
					var voucher = _context.VoucherCategories.FirstOrDefault(v => v.Id == voucherId);
					var totalPriceCart = GetToTalPriceCart();
					if (voucher.TotalCondition <= totalPriceCart)
					{
						Session[VoucherTypeConst.VOUCHERCATEGORY] = voucherId;
						return Json(new { status = "success", message = "Đã áp dụng voucher cho danh mục thành công" });
					}
					return Json(new { status = "warning", message = "Đơn hàng chưa đạt giá trị tối thiếu để áp dụng voucher này!" });
				}
				else if (type == VoucherTypeConst.VOUCHERCUSTOMER)
				{
					var voucher = _context.VoucherCustomers.FirstOrDefault(v => v.Id == voucherId);
					var totalPriceCart = GetToTalPriceCart();
					if (voucher.TotalCondition <= totalPriceCart)
					{
						Session[VoucherTypeConst.VOUCHERCUSTOMER] = voucherId;
						return Json(new { status = "success", message = "Đã áp dụng voucher dành cho bạn thành công" });
					}
					return Json(new { status = "warning", message = "Đơn hàng chưa đạt giá trị tối thiếu để áp dụng voucher này!" });
				}
				return Json(new { status = "error", message = "Hệ thông không tìm thấy voucher này!" });
			}
			else
			{
				if (type == "ship")
				{
					Session[VoucherTypeConst.VOUCHERSHIP] = "";
				}
				else
				{
					Session[VoucherTypeConst.VOUCHERCUSTOMER] = "";
					Session[VoucherTypeConst.VOUCHERPRODUCT] = "";
					Session[VoucherTypeConst.VOUCHERCATEGORY] = "";
				}
				return Json(new { status = "success", message = "Đã hủy chọn voucher!" });
			}
		}

		public decimal? GetToTalPriceCart()
		{
			var cart = GetCart();
			decimal? totalOrder = 0;
			foreach (var item in cart)
			{
				decimal? discount = (item.Product?.Discount ?? 0) * item.Quantity;
				var totaldetail = (item.Product?.Price * item.Quantity) - discount;
				totalOrder += totaldetail;
			}
			return totalOrder;
		}
		List<Cart> GetCart()
		{
			if (!string.IsNullOrEmpty(Session[Const.CARTVITUALSESSION]?.ToString()))
			{
				List<Cart> carts = JsonConvert.DeserializeObject<List<Cart>>(Session[Const.CARTVITUALSESSION]?.ToString());
				carts.ForEach(c =>
				{
					c.Product = _context.Products.Find(c.ProductId);
				});
				return carts;
			}
			return new List<Cart>();
		}
		public bool CheckCart()
		{
			var carts = GetCart();
			foreach (var item in carts)
			{
				var product = _context.Products.FirstOrDefault(p => p.Id == item.ProductId);
				if (item.Quantity > product.Quantity)
				{
					return false;
				}
			}
			return true;
		}
		// Xóa cart khỏi session
		void ClearCart()
		{
			Session[Const.CARTVITUALSESSION] = "";
		}
		public void SetViewBagVoucher()
		{
			var now = DateTime.Now;

			var voucherShips = _context.VoucherShips
				.Where(v => v.Quantity > 0 && now > v.StartDate && now < v.EndDate)
				.OrderByDescending(p => p.CreatedAt)
				.Select(item => new CreateVoucherModel
				{
					Id = item.Id,
					Name = item.Name,
					Discount = item.Discount,
					VoucherType = VoucherTypeConst.VOUCHERSHIP
				})
				.ToList();
			var voucherProducts = _context.VoucherProducts
				.Where(v => v.Quantity > 0 && now > v.StartDate && now < v.EndDate)
				.OrderByDescending(p => p.CreatedAt)
				.Select(item => new CreateVoucherModel
				{
					Id = item.Id,
					Name = item.Name,
					Discount = item.Discount,
					VoucherType = VoucherTypeConst.VOUCHERPRODUCT
				})
				.ToList();

			var voucherCategories = _context.VoucherCategories
				.Where(v => v.Quantity > 0 && now > v.StartDate && now < v.EndDate)
				.OrderByDescending(p => p.CreatedAt)
				.Select(item => new CreateVoucherModel
				{
					Id = item.Id,
					Name = item.Name,
					Discount = item.Discount,
					VoucherType = VoucherTypeConst.VOUCHERCATEGORY
				})
				.ToList();

			////
			string userId = Session[Const.USERIDSESSION]?.ToString();
			var voucherCustomers = _context.VoucherCustomers
				.Where(v => v.Quantity > 0 && now > v.StartDate && now < v.EndDate && v.CustomerId == userId)
				.OrderByDescending(p => p.CreatedAt)
				.Select(item => new CreateVoucherModel
				{
					Id = item.Id,
					Name = item.Name,
					Discount = item.Discount,
					VoucherType = VoucherTypeConst.VOUCHERCUSTOMER
				})
				.ToList();

			var vouchers = voucherShips
				.Concat(voucherProducts)
				.Concat(voucherCategories)
				.Concat(voucherCustomers)
				.ToList();

			ViewBag.Voucher = vouchers;
		}
		public ActionResult ReCheckOutInit(string id)
		{
			if (id == null || _context.Orders == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Orders is null!");
			}
			Session[Const.RECHECKOUTORDERIDSESSION] = id;
			return RedirectToAction("ReCheckOut","Order");
		}
		public ActionResult ReCheckOut()
		{
			string orderReCheckoutId = Session[Const.RECHECKOUTORDERIDSESSION]?.ToString();
			if (orderReCheckoutId == null || _context.Orders == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Orders is null!");
			}

			var order = _context.Orders
						.Include(o => o.Account)
						.Include(o => o.Customer)
						.Include(o => o.OrderDetails)
						.Include(o => o.OrderDetails.Select(od => od.Product).Select(odd => odd.Category))
						.Include(o => o.OrderDetails.Select(od => od.Product).Select(odd => odd.Branch))
						.Include(o => o.OrderDetails.Select(od => od.Product).Select(odd => odd.Color))
						.Include(o => o.OrderDetails.Select(od => od.Product).Select(odd => odd.Size))
						.Where(o => o.Id == orderReCheckoutId)
						.FirstOrDefault();
			// thêm vào Cart ảo
			List<Cart> carts = order.OrderDetails.Select(o => new Cart
			{
				ProductId = o.ProductId,
				Quantity = o.Quantity,
				SessionId = Session[Const.CARTSESSION]?.ToString(),
				CreatedAt = DateTime.Now,
			}).ToList();

			// thông tin user
			ViewBag.Name = order.Name;
			ViewBag.Phone = order.Phone;
			ViewBag.Address = order.Address;
			if (!string.IsNullOrEmpty(Session[Const.USERIDSESSION]?.ToString()))
			{

			}
			else
			{
				TempData["error"] = "Bạn chưa đăng nhập!";
				TempData["warning"] = "Để lưu trữ thông tin lâu dài, bạn nên đăng nhập trước.";
			}
			Session[Const.CARTVITUALSESSION] = JsonConvert.SerializeObject(carts);

			carts.ForEach(c =>
			{
				c.Product = _context.Products.Find(c.ProductId);
			});
			if (GetCart().Count == 0)
			{
				TempData["error"] = "Chưa có sản phẩm nào được chọn!";
				return RedirectToAction("Index", "Home");
			}
			ViewBag.Province = _context.Provinces.OrderBy(p => p.Name).ToList();
			ViewBag.District = _context.Districts.OrderBy(p => p.Name).ToList();
			SetViewBagVoucher();
			return View(carts);
		}
		public decimal? UpdateVoucher()
		{
			decimal? discountAll = 0;
			if (!string.IsNullOrEmpty(Session[VoucherTypeConst.VOUCHERSHIP]?.ToString()))
			{
				string id = Session[VoucherTypeConst.VOUCHERSHIP]?.ToString();

				var voucher = _context.VoucherShips.FirstOrDefault(v => v.Id == id);
				Session[VoucherTypeConst.VOUCHERSHIP] = "";

				//update quantity
				voucher.Quantity -= 1;
				_context.VoucherShips.Attach(voucher);
				_context.Entry(voucher).State = EntityState.Modified;
				_context.SaveChanges();

				discountAll += voucher.Discount;
			}
			if (!string.IsNullOrEmpty(Session[VoucherTypeConst.VOUCHERCUSTOMER]?.ToString()))
			{
				string id = Session[VoucherTypeConst.VOUCHERCUSTOMER]?.ToString();

				var voucher = _context.VoucherCustomers.FirstOrDefault(v => v.Id == id);
				Session[VoucherTypeConst.VOUCHERCUSTOMER] = "";

				//update quantity
				voucher.Quantity -= 1;
				_context.VoucherCustomers.Attach(voucher);
				_context.Entry(voucher).State = EntityState.Modified;
				_context.SaveChanges();

				discountAll += voucher.Discount;
			}
			if (!string.IsNullOrEmpty(Session[VoucherTypeConst.VOUCHERCATEGORY]?.ToString()))
			{
				string id = Session[VoucherTypeConst.VOUCHERCATEGORY]?.ToString();

				var voucher = _context.VoucherCategories.FirstOrDefault(v => v.Id == id);
				Session[VoucherTypeConst.VOUCHERCATEGORY] = "";

				//update quantity
				voucher.Quantity -= 1;
				_context.VoucherCategories.Attach(voucher);
				_context.Entry(voucher).State = EntityState.Modified;
				_context.SaveChanges();

				discountAll += voucher.Discount;
			}
			if (!string.IsNullOrEmpty(Session[VoucherTypeConst.VOUCHERPRODUCT]?.ToString()))
			{
				string id = Session[VoucherTypeConst.VOUCHERPRODUCT]?.ToString();

				var voucher = _context.VoucherProducts.FirstOrDefault(v => v.Id == id);
				Session[VoucherTypeConst.VOUCHERPRODUCT] = "";

				//update quantity
				voucher.Quantity -= 1;
				_context.VoucherProducts.Attach(voucher);
				_context.Entry(voucher).State = EntityState.Modified;
				_context.SaveChanges();

				discountAll += voucher.Discount;
			}
			return discountAll;
		}

		[HttpPost]
		public ActionResult ReCheckOut(Checkout checkout)
		{
			string checkoutString = JsonConvert.SerializeObject(checkout);
			Session[Const.CHECKOUTSESSION] = checkoutString;
			if (checkout.PayOption == "ship")
			{
				Session[Const.PAYWAY] = PayConst.OFFLINE;
				Session[Const.PAYSTATUS] = PayStatusConst.NODONE;

				return RedirectToAction("AddOrder");
			}
			else
			{
				Session[Const.PAYWAY] = PayConst.ONLINE;

				decimal? totalOrder = GetToTalPriceCart();

				//voucher
				decimal? voucherDiscount = UpdateVoucher();
				totalOrder -= voucherDiscount;

				if (totalOrder > 50000000)
				{
					TempData["error"] = "Số tiền thanh toán quá lớn! Vui lòng chọn thanh toán khi nhận hàng";
					SetViewBagVoucher();
					return Redirect(nameof(ReCheckOut));
				}
				else
				{
					var amount = ((int?)totalOrder + 30000).ToString();
					return Redirect($"Payment?total={amount}");
				}

			}
		}
		public async Task<ActionResult> AddOrder()
		{
			if (!CheckCart())
			{
				TempData["error"] = "Đặt hàng không thành công! Số lượng trong kho không đủ\nVui lòng kiểm tra lại";
				SetViewBagVoucher();
				return RedirectToAction("ReCheckOut");
			}

			Checkout checkout = JsonConvert.DeserializeObject<Checkout>(Session[Const.CHECKOUTSESSION]?.ToString());
			var name = checkout.Name.Trim();
			var phone = checkout.Phone.Trim();
			var address = checkout.Address.Trim();
			if (string.IsNullOrEmpty(Session[Const.USERIDSESSION]?.ToString()))
			{
				string cus_id = Session[Const.CARTSESSION]?.ToString();
				if(!_context.Customers.Any(c => c.Id == cus_id))
				{
					Customer cus = new Customer()
					{
						Id = cus_id,
						Name = name,
						Phone = phone,
						Address = address
					};
					_context.Customers.Add(cus);
					await _context.SaveChangesAsync();
				}
			}
			var carts = GetCart();
			decimal? totalOrder = 0;
			string orderId = Guid.NewGuid().ToString();
			string code = Utilities.GenerateCode(name);
			Order order = new Order()
			{
				Id = orderId,
				Code = code,
				Name = name,
				Phone = phone,
				Address = address,
				Discount = 0,
				PayWay = Session[Const.PAYWAY]?.ToString(),
				PayStatus = Session[Const.PAYSTATUS]?.ToString(),
				Total = totalOrder,
				CustomerId = Session[Const.CARTSESSION]?.ToString(),
				CreatedAt = DateTime.Now,
				Status = StatusConst.WAITCONFIRM
			};
			try
			{
				_context.Orders.Add(order);
				await _context.SaveChangesAsync();
			}
			catch (Exception ex)
			{
				TempData["error"] = $"Message:{ex}";
				SetViewBagVoucher();
				return Redirect(nameof(ReCheckOut));
			}

			// thêm sản phẩm vào đơn hàng
			foreach (var item in carts)
			{
				try
				{
					item.Product = _context.Products.Find(item.ProductId);
					decimal? discount = (item.Product?.Discount ?? 0) * item.Quantity;
					var totaldetail = (item.Product?.Price * item.Quantity) - discount;
					totalOrder += totaldetail;
					OrderDetail orderDetail = new OrderDetail()
					{
						Id = Guid.NewGuid().ToString(),
						Quantity = item.Quantity,
						ProductId = item.ProductId,
						Total = totaldetail,
						OrderId = orderId,
						CreatedAt = DateTime.Now,

					};
					_context.OrderDetails.Add(orderDetail);

					// cập nhật số lượng
					var product = _context.Products.FirstOrDefault(p => p.Id == item.ProductId);
					product.Quantity -= item.Quantity;
					_context.Products.Attach(product);
					_context.Entry(product).State = EntityState.Modified;
					await _context.SaveChangesAsync();
				}
				catch (Exception ex)
				{
					TempData["error"] = $"Message:{ex}";
					SetViewBagVoucher();
					return Redirect(nameof(ReCheckOut));
				}

			}

			//voucher
			decimal? voucherDiscount = UpdateVoucher();
			totalOrder -= voucherDiscount;

			order.Total = totalOrder - order.Discount + order.ShipFee;

			_context.Orders.Attach(order);
			_context.Entry(order).State = EntityState.Modified;
			await _context.SaveChangesAsync();
			TempData["success"] = "Đã đặt hàng thành công!";
			ClearCart();
			return RedirectToAction("Index","Order");
		}

		public ActionResult Payment(string total = "0")
		{
			//request params need to request to MoMo system
			string endpoint = "https://test-payment.momo.vn/gw_payment/transactionProcessor";
			string partnerCode = "MOMOOJOI20210710";
			string accessKey = "iPXneGmrJH0G8FOP";
			string serectkey = "sFcbSGRSJjwGxwhhcEktCHWYUuTuPNDB";
			string orderInfo = "Quét mã QR để thanh toán";
			string returnUrl = "https://localhost:44333/Cart/ConfirmPaymentClient";
			string notifyurl = "https://4c8d-2001-ee0-5045-50-58c1-b2ec-3123-740d.ap.ngrok.io/Cart/SavePayment"; //lưu ý: notifyurl không được sử dụng localhost, có thể sử dụng ngrok để public localhost trong quá trình test

			string amount = total;
			string orderid = DateTime.Now.Ticks.ToString(); //mã đơn hàng
			string requestId = DateTime.Now.Ticks.ToString();
			string extraData = "";

			//Before sign HMAC SHA256 signature
			string rawHash = "partnerCode=" +
				partnerCode + "&accessKey=" +
				accessKey + "&requestId=" +
				requestId + "&amount=" +
				amount + "&orderId=" +
				orderid + "&orderInfo=" +
				orderInfo + "&returnUrl=" +
				returnUrl + "&notifyUrl=" +
				notifyurl + "&extraData=" +
				extraData;

			MoMoSecurity crypto = new MoMoSecurity();
			//sign signature SHA256
			string signature = crypto.signSHA256(rawHash, serectkey);

			//build body json request
			JObject message = new JObject
			{
				{ "partnerCode", partnerCode },
				{ "accessKey", accessKey },
				{ "requestId", requestId },
				{ "amount", amount },
				{ "orderId", orderid },
				{ "orderInfo", orderInfo },
				{ "returnUrl", returnUrl },
				{ "notifyUrl", notifyurl },
				{ "extraData", extraData },
				{ "requestType", "captureMoMoWallet" },
				{ "signature", signature }

			};

			string responseFromMomo = PaymentRequest.sendPaymentRequest(endpoint, message.ToString());

			JObject jmessage = JObject.Parse(responseFromMomo);

			return Redirect(jmessage.GetValue("payUrl").ToString());
		}

		//Khi thanh toán xong ở cổng thanh toán Momo, Momo sẽ trả về một số thông tin, trong đó có errorCode để check thông tin thanh toán
		//errorCode = 0 : thanh toán thành công (Request.QueryString["errorCode"])
		//Tham khảo bảng mã lỗi tại: https://developers.momo.vn/#/docs/aio/?id=b%e1%ba%a3ng-m%c3%a3-l%e1%bb%97i
		public ActionResult ConfirmPaymentClient(string errorCode)
		{
			//lấy kết quả Momo trả về và hiển thị thông báo cho người dùng (có thể lấy dữ liệu ở đây cập nhật xuống db)
			if (errorCode == "0")
			{
				Session[Const.PAYSTATUS] = PayStatusConst.DONE;
				return RedirectToAction(nameof(AddOrder));
			}
			TempData["error"] = "Thanh toán không thành công! Vui lòng thử lại";
			return RedirectToAction(nameof(ReCheckOut));
		}

		[HttpPost]
		public void SavePayment()
		{
			//cập nhật dữ liệu vào db

		}

		/*[HttpPost]
        
        public ActionResult Delete(string orderid)
        {
            if (orderid == null)
            {
                return BadRequest();
            }
            Order order = _context.Orders.Find(orderid);
            if (order == null)
            {
                return NotFound();
            }
            if (order.Status == StatusConst.WAITCONFIRM || order.Status == StatusConst.CONFIRMED)
            {
                _context.Orders.Remove(order);
                _context.SaveChanges();
                _notyfService.Success("Đã xóa đơn hàng!");
                return RedirectToAction("Index");
            }
            else
            {
                _notyfService.Success("Đơn hàng của bạn đã được xử lí vừa song, vui lòng kiểm tra lại");
                return RedirectToAction("Index");
            }

        }
*/
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
