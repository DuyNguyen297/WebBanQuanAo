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
using DocumentFormat.OpenXml.Office2010.Excel;
using FashionShop.AreaModels;

namespace FashionShop.Controllers
{
    public class CartController : GlobalController
    {

        public ActionResult Index()
        {
            string sessionId = Session[Const.CARTSESSION]?.ToString();
            var carts = _context.Carts
                .Include(x => x.Product)
                .Include(x => x.Product.Color)
                .Include(x => x.Product.Size)
                .Include(x => x.Product.Branch)
                .Include(x => x.Product.Category)
                .Where(x => x.SessionId == sessionId)
                .OrderByDescending(x => x.CreatedAt)
                .ToList();
            foreach(var item in carts)
            {
                var product = _context.Products.FirstOrDefault(p=>p.Id == item.ProductId);
                if(item.Quantity > product.Quantity)
                {
                    item.Quantity = product.Quantity;

                    _context.Carts.Attach(item);
                    _context.Entry(item).State = EntityState.Modified;

                    _context.SaveChanges();
                    TempData["warning"] = "Giỏ hàng của bạn chứa một số sản phẩm không đủ số lượng kho để cung cấp!";
                    TempData["warning"] = "Số lượng đã được cập nhật về mức tối đa!";
                }
            }
            return View(GetCart());
        }

        public ActionResult GetDistrictByProvinceId(string provinceId)
        {
            var listDistrict = _context.Districts.Where(d => d.ProvinceId == provinceId).OrderBy(d => d.Name).ToList();
            return Json(new { data = listDistrict }, JsonRequestBehavior.AllowGet);
		}
        public ActionResult GetWardByDistrictId(string districtId)
        {
            var listWard = _context.Wards.Where(d => d.DistrictId == districtId).OrderBy(d => d.Name).ToList();
            return Json(new { data = listWard}, JsonRequestBehavior.AllowGet);
		}

        [HttpPost]
		public ActionResult SelectVoucher(string voucherId, string type)
		{
            if(voucherId != "nan")
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
                if(type == "ship")
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
            string sessionId = Session[Const.CARTSESSION]?.ToString();
            var carts = _context.Carts
                .Include(x => x.Product)
                .Include(x => x.Product.Color)
                .Include(x => x.Product.Size)
                .Include(x => x.Product.Branch)
                .Include(x => x.Product.Category)
                .Where(x => x.SessionId == sessionId)
                .OrderByDescending(x => x.CreatedAt)
                .ToList();
            return carts;
        }
        // Xóa cart khỏi session
        void ClearCart()
        {
            string sessionId = Session[Const.CARTSESSION]?.ToString();
            var carts = _context.Carts.Where(x => x.SessionId == sessionId).ToList();
            foreach (var item in carts)
            {
                _context.Carts.Remove(item);
            }
            _context.SaveChanges();
        }

        public bool CheckQuantity(string productid, int quantity)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == productid);
            return product.Quantity >= quantity ? true : false;
        }
        public bool CheckCart()
        {
            var cart = GetCart();
            foreach(var item in cart)
            {
                var product = _context.Products.FirstOrDefault(p => p.Id == item.ProductId);
                if (item.Quantity > product.Quantity)
                {
                    return false;
                }
            }
            return true;
        }

        [HttpPost]
        public ActionResult UpdateCart(string productid, int quantity)
        {
            if(CheckQuantity(productid, quantity))
            {
                // Cập nhật Cart thay đổi số lượng quantity ...
                var carts = GetCart();
                var cartitem = carts.FirstOrDefault(p => p.ProductId == productid);
                if (cartitem != null)
                {
                    if(cartitem.Quantity == 1 && quantity == 1)
                    {
						return Json(new { success = false, status = "error", message = "Số lượng phải lớn hơn hoặc bằng 1" });
					}
                    else
                    {
                        cartitem.Quantity = quantity;
                        cartitem.UpdatedAt = DateTime.Now;
                        _context.Carts.Attach(cartitem);
                        _context.Entry(cartitem).State = EntityState.Modified;

                        _context.SaveChanges();
						return Json(new { success = true, status = "success", message = "Đã cập nhật số lượng" });
					}
                }
                else
                {
					return Json(new { success = false, status = "error", message = "Không tìm thấy sản phẩm!" });
				}
                // Trả về mã thành công (không có nội dung gì - chỉ để Ajax gọi)
            }
            else
            {
				return Json(new { success = false, status = "error", message = "Số lượng sản phẩm không đủ!" });
			}
        }

        [HttpPost]
        public ActionResult AddToCart(string productid,string colorid, string sizeid, int quantity)
        {
			if (string.IsNullOrEmpty(productid) || _context.Products == null)
			{
				return HttpNotFound();
			}
            string message = "";
            var productCode = _context.Products.FirstOrDefault(p => p.Id == productid)?.Code;
            var productFind = _context.Products.FirstOrDefault(p => p.Code == productCode && p.ColorId == colorid && p.SizeId == sizeid);
			if (CheckQuantity(productFind.Id, quantity))
            {
                // Xử lý đưa vào Cart ...
                var carts = GetCart();
                var cartitem = carts.FirstOrDefault(p => p.ProductId == productFind.Id);
                if (cartitem != null)
                {
                    // Đã tồn tại, tăng thêm số lượng
                    cartitem.Quantity += productFind.Quantity - cartitem.Quantity >= quantity ? quantity : productFind.Quantity - cartitem.Quantity; // Đảm bảo ko vượt quá quantity + quantity có sẵn trong cart
					cartitem.UpdatedAt = DateTime.Now;
                    _context.Carts.Attach(cartitem);
                    _context.Entry(cartitem).State = EntityState.Modified;
					message = "Sản phầm đã được thêm vào giỏ hàng trước đó, số lượng cập nhật đã được cập nhật";
                }
                else
                {
                    //  Thêm mới
                    _context.Carts.Add(new Cart()
                    {
                        Id = Guid.NewGuid().ToString(),
                        ProductId = productFind.Id,
                        Quantity = quantity,
                        SessionId = Session[Const.CARTSESSION]?.ToString(),
                        CreatedAt = DateTime.Now,
                    });
					message = "Đã thêm sản phẩm vào giỏ hàng";
                }
                // Lưu cart
                _context.SaveChanges();

                // Chuyển đến trang hiện thị Cart
                return Json(new { success = true, message = message });
			}
            else
            {
				message = "Số lượng sản phẩm không đủ!";
                return Json(new { success = false, message = message });
			}
        }

        public ActionResult RemoveCart(string productid)
        {
            var carts = GetCart();
            var cartitem = carts.FirstOrDefault(p => p.ProductId == productid);
            if (cartitem != null)
            {
                _context.Carts.Remove(cartitem);
                _context.SaveChanges();
                TempData["success"] = "Đã xóa sản phẩm khỏi giỏ hàng";
            }
            else
            {
                TempData["error"] = "Vui lòng thử lại";
            }
            return RedirectToAction("Index");
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
                    Discount= item.Discount,
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

		public ActionResult CheckOut()
        {
            ViewBag.Name = "";
            ViewBag.Phone = "";
            ViewBag.Address = "";
            if (!string.IsNullOrEmpty(Session[Const.USERIDSESSION]?.ToString()))
            {
                Customer cus = JsonConvert.DeserializeObject<Customer>(Session[Const.USERSESSION]?.ToString());
                ViewBag.Name = cus.Name;
                ViewBag.Phone = cus.Phone;
                ViewBag.Address = cus.Address;
            }
            else
            {
				TempData["warning"] = "Bạn chưa đăng nhập!";
				TempData["info"] = "Để lưu trữ thông tin lâu dài, bạn nên đăng nhập trước.";
			}
			ViewBag.Province = _context.Provinces.OrderBy(p => p.Name).ToList();
			ViewBag.District = _context.Districts.OrderBy(p => p.Name).ToList();
            if(GetCart().Count == 0)
            {
				TempData["error"] = "Chưa có sản phẩm nào được chọn!";
				return RedirectToAction("Index", "Home");
			}
            SetViewBagVoucher();
			return View(GetCart());
        }

        public string CustomAddress(string province, string district, string ward)
        {
            return " " +
                _context.Wards.FirstOrDefault(w => w.Id == ward)?.Name +
                ", " +
                _context.Districts.FirstOrDefault(w => w.Id == district)?.Name +
                ", " +
                _context.Provinces.FirstOrDefault(w => w.Id == province)?.Name;
				
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
		public ActionResult CheckOut(Checkout checkout)
		{
            checkout.Address += ", " + CustomAddress(checkout.Province, checkout.District, checkout.Ward);
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
					return Redirect(nameof(CheckOut));
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
            if(!CheckCart())
            {
                TempData["error"] = "Đặt hàng không thành công! Số lượng trong kho không đủ\nVui lòng kiểm tra lại";
                SetViewBagVoucher();
				return Redirect(nameof(CheckOut));
            }
            Checkout checkout = JsonConvert.DeserializeObject<Checkout>(Session[Const.CHECKOUTSESSION]?.ToString());
            var name = checkout.Name.Trim();
            var phone = checkout.Phone.Trim();
            var address = checkout.Address.Trim();
            if (string.IsNullOrEmpty(Session[Const.USERIDSESSION]?.ToString()))
            {
				string cus_id = Session[Const.CARTSESSION]?.ToString();
				if (!_context.Customers.Any(c => c.Id == cus_id))
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
			catch(Exception ex)
            {
                TempData["error"] = $"Message:{ex}";
				SetViewBagVoucher();
				return Redirect("CheckOut");
            }

            // thêm sản phẩm vào đơn hàng
            foreach (var item in carts)
            {
                try
                {
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
					return Redirect("CheckOut");
				}

            }

            //voucher
			decimal? voucherDiscount = UpdateVoucher();
			totalOrder -= voucherDiscount;

			order.Total = totalOrder - order.Discount + 30000;

            _context.Orders.Attach(order);
            _context.Entry(order).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            TempData["success"] = "Đã đặt hàng thành công! Cảm ơn quý khách hàng";
            ClearCart();
            return Redirect("Index");
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
            if(errorCode == "0")
            {
                Session[Const.PAYSTATUS] = PayStatusConst.DONE;
                return RedirectToAction(nameof(AddOrder));
            }
            TempData["error"] = "Thanh toán không thành công! Vui lòng thử lại";
            return RedirectToAction("Checkout", "Cart");
        }

        [HttpPost]
        public void SavePayment()
        {
            //cập nhật dữ liệu vào db
            
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
