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
using Irony.Parsing;
using FashionShop.ViewModel;
using Watch.OnlinePayment;
using Newtonsoft.Json.Linq;
using FashionShop.Areas.Admin.Controllers.AuthorizeAttributeConfig;
using FashionShop.AreaModels;
using Microsoft.Ajax.Utilities;
using System.Web.Helpers;
using FashionShop.Helper;
using System.Globalization;
using System.Web.Hosting;
using Aspose.Cells;

namespace FashionShop.Areas.Admin.Controllers
{
    [User]
    public class SellController : BaseController
    {
		public ActionResult Index(int page = 1, string catid = "0", string branchid = "0", string searchkey = "")
		{
			var pageNumber = page;
			var pageSize = 8;

			List<CreateProductModel> lsProductsView = new List<CreateProductModel>();
			var lsProducts = _context.Products
			   .Include(p => p.Category)
			   .Include(p => p.Branch)
			   .Include(p => p.Color)
			   .Include(p => p.Size)
               .Where(p => p.Effective == true)
			   .DistinctBy(p => p.Code)
			   .OrderByDescending(x => x.CreatedAt)
			   .ToList();

			foreach (var item in lsProducts)
			{
				var product = ConvertProductToCrudModel(item.Code);
				lsProductsView.Add(product);
			};
			if (catid != "0")
			{
				ViewBag.CurrentCateId = catid;
				lsProductsView = lsProductsView.Where(x => x.CategoryId == catid).OrderByDescending(x => x.CreatedAt).ToList();
			}
			if (branchid != "0")
			{
				ViewBag.CurrentBranchId = branchid;
				lsProductsView = lsProductsView.Where(x => x.BranchId == branchid).OrderByDescending(x => x.CreatedAt).ToList();
			}
			if (!string.IsNullOrEmpty(searchkey))
			{
				ViewBag.SearchKey = searchkey;
				lsProductsView = lsProductsView
								.Where(x => x.Name.Contains(searchkey))
								.OrderByDescending(x => x.CreatedAt)
								.ToList();
			}

			IPagedList<CreateProductModel> models = lsProductsView.AsQueryable().ToPagedList(pageNumber, pageSize);
			ViewBag.CurrentPage = pageNumber;
			ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "Name");
			ViewBag.BranchId = new SelectList(_context.Branchs, "Id", "Name");
			ViewBag.ColorId = _context.Colors.ToList();
			ViewBag.SizeId = _context.Sizes.ToList();
			ViewBag.AllProduct = _context.Products.ToList();
			return View(models);
		}

		public CreateProductModel ConvertProductToCrudModel(string code)
		{
			var listProducts = _context.Products
					.Include(p => p.Category)
					.Include(p => p.Branch)
					.Include(p => p.Color)
					.Include(p => p.Size)
					.Where(p => p.Code == code)
					.DistinctBy(p => p.ColorId).ToList();
			var quantity = _context.Products.Where(p => p.Code == code).Sum(p => p.Quantity);
			if (listProducts.Count == 0)
			{
				return new CreateProductModel();
			}

			var createColors = new List<CreateColorModel>();
			int i = 0;
			foreach (var product in listProducts)
			{
				var listSizeOfColor = _context.Products.Where(p => p.Code == code && p.ColorId == product.ColorId).Select(p => p.SizeId).ToArray();
				createColors.Add(new CreateColorModel()
				{
					Index = i,
					Color = product.ColorId,
					Size = listSizeOfColor
				});
				i += 1;
			};

            return new CreateProductModel()
            {
                Id = listProducts[0].Id,
                Code = listProducts[0].Code,
                Name = listProducts[0].Name,
                Description = listProducts[0].Description,
                Price = listProducts[0].Price,
                Discount = listProducts[0].Discount,
                CategoryId = listProducts[0].CategoryId,
                BranchId = listProducts[0].BranchId,
                Category = listProducts[0].Category,
                Branch = listProducts[0].Branch,
                Image = listProducts[0].Image,
                Effective = listProducts[0].Effective,
                Quantity = quantity,
				ColorSizes = createColors
			};
		}

		/// <summary>
		/// Quầy bán hàng 
		/// </summary>
		/// 
		// lưu order vào session
		void SaveProductToSession(List<OrderDetail> orders)
        {
            if(orders == null || orders.Count == 0)
            {
                ClearProductSession();
            }
            else
            {
				string stringOrder = JsonConvert.SerializeObject(orders);
				ViewBag.Sell = orders;
				Session[Const.ORDERSESSION] = stringOrder;
			}
        }

        // Lấy Order từ Session (danh sách OrderItem)
        List<OrderDetail> GetProductSession()
        {
            string stringOrder = Session[Const.ORDERSESSION]?.ToString();
            if (!string.IsNullOrEmpty(stringOrder))
            {
                return JsonConvert.DeserializeObject<List<OrderDetail>>(stringOrder).Where(x => x.Quantity > 0 && x.Product.Effective == true).ToList();
            }
            return new List<OrderDetail>();
        }

        // Xóa Order khỏi session
        void ClearProductSession()
        {
            Session[Const.ORDERSESSION] = "";
        }

        // Lưu product vào session
        // truyền vào 3 đối số
        public ActionResult AddProduct(string code, string colorId, string sizeId)
        {
			if (!string.IsNullOrEmpty(Session[Const.SELLID]?.ToString()))
			{
				Session[Const.SELLID] = "";
			}
			var product = _context.Products
				                .FirstOrDefault(x => x.Code == code && x.ColorId == colorId && x.SizeId == sizeId);
            
            // Xử lý đưa vào Session ...
            var orderDetails = GetProductSession();

            var checkExist = orderDetails.Any(o => o.ProductId == product.Id);
            if (checkExist)
            {
				TempData["error"] = "Sản phẩm đã được thêm trước đó!";
			}
            else
            {
                var orderDetail = new OrderDetail()
                {
                    Id = Guid.NewGuid().ToString(),
                    Quantity = 1,
                    ProductId = product.Id,
                    Product = product,
					Total = (product.Price - (product.Discount ?? 0)),
                };
                // thêm thuộc tính product
                orderDetail.Product = product;
                if (CheckQuantity(product.Id, 1))
                {
                    //  Thêm mới
                    orderDetails.Add(orderDetail);
                    TempData["success"] = "Đã thêm sản phẩm!";
                }
                else
                {
                    TempData["error"] = "Số lượng sản phẩm không đủ!";
                } 
                   
            }
            // Lưu Order vào Session
            SaveProductToSession(orderDetails);
            // Chuyển đến trang hiện thị Order đã lưu vào Session
            return RedirectToAction(nameof(Index));
        }
        public void UpdateTotal(string productid)
        {
            var orderDetails = GetProductSession();
            var order = orderDetails.FirstOrDefault(p => p.ProductId == productid);
            orderDetails.FirstOrDefault(p => p.ProductId == productid).Total = (order.Product?.Price - (order.Product?.Discount ?? 0)) * order.Quantity;
            SaveProductToSession(orderDetails);
        }
        public bool CheckQuantity(string productid, int quantity)
        {
            var product = _context.Products.FirstOrDefault(x => x.Id == productid);
            return product.Quantity >= quantity;
        }
        public bool CheckOrder()
        {
            var orderDetails = GetProductSession();
            foreach (var item in orderDetails)
            {
                var product = _context.Products.FirstOrDefault(x => x.Id == item.ProductId);
                if (item.Quantity > product.Quantity || product.Effective == false)
                {
                    return false;
                }
            }
            return true;
        }
        [HttpPost]
        public ActionResult UpdateQuantity(string productid, int quantity)
        {
            if (CheckQuantity(productid, quantity))
            {
                // Cập nhật Cart thay đổi số lượng quantity ...
                var orderDetails = GetProductSession();
                var checkExist = orderDetails.Any(p => p.ProductId == productid);
                if (checkExist)
                {
                    var orderDetail = orderDetails.FirstOrDefault(p => p.ProductId == productid);
                    if (orderDetail.Quantity == 1 && quantity == 1)
                    {
						return Json(new { success = false, status = "error", message = "Số lượng phải lớn hơn hoặc bằng 1" });
					}
                    else
                    {
                        // Đã tồn tại
                        orderDetails.FirstOrDefault(p => p.ProductId == productid).Quantity = quantity;
                        SaveProductToSession(orderDetails);
                        UpdateTotal(productid);

						return Json(new { success = true, status = "success", message = "Đã cập nhật số lượng" });
					}
                }
                else
                {
					return Json(new { success = false, status = "error", message = "Không tìm thấy sản phẩm!" });
				}
              
            }
            else
            {
				return Json(new { success = false, status = "error", message = "Số lượng sản phẩm không đủ!" });
			}
            // Trả về mã thành công (không có nội dung gì - chỉ để Ajax gọi)
            
        }

        // Xóa OrderSession
        public ActionResult RemoveProduct(string productid)
        {
            var orderDetails = GetProductSession();
            var checkExist = orderDetails.Any(p => p.ProductId == productid);
            if (checkExist)
            {
                orderDetails.Remove(orderDetails.FirstOrDefault(x => x.ProductId == productid));
                SaveProductToSession(orderDetails);
                TempData["success"] = "Đã xóa sản phẩm khỏi đơn hàng!";
            }
            else
            {
                TempData["error"] = "Xin vui lòng thử lại!";
            }
            return RedirectToAction(nameof(Index));
        }


        // GET: Sell/Detail/Id
        public ActionResult Details(string id)
        {
            if (id == null || _context.Products == null)
            {
                return HttpNotFound();
            }

            var product = _context.Products
                .Include(b => b.Category)
                .Include(b => b.Branch)
                .Include(b => b.Color)
                .Include(b => b.Size)
                .FirstOrDefault(m => m.Id == id);
            if (product == null)
            {
                return HttpNotFound();
            }

            return View(product);
        }


        public async Task<ActionResult> AddOrder()
        {
            if (!CheckOrder())
            {
                TempData["error"] = "Đặt hàng không thành công! Số lượng trong kho không đủ hoặc sản phẩm đã hết hiệu lực\nVui lòng kiểm tra lại";
                return Redirect(nameof(Index));
            }
            Checkout checkout = JsonConvert.DeserializeObject<Checkout>(Session[Const.SELL]?.ToString());
            var orderId = checkout.Id;
            var name = checkout.Name.Trim();
            var phone = checkout.Phone.Trim();
            var email = checkout.Email.Trim();
            var address = checkout.Address.Trim();
            var discount = checkout.Discount == null ? 0 : checkout.Discount;
            Customer cus = new Customer()
            {
                Id = Guid.NewGuid().ToString(),
                Name = checkout.Name,
                Phone = checkout.Phone,
                Email = checkout.Email,
                Address = checkout.Address
            };
			_context.Customers.Add(cus);
			await _context.SaveChangesAsync();
			//// thêm đơn hàng
			decimal? totalOrder = 0;
            string code = Utilities.GenerateCode(name);
			Order order = new Order()
            {
                Id = orderId,
                Name = name,
                Code = code,
                Phone = phone,
                Address = address,
                Discount = discount,
                ShipFee = 0,
                PayWay = Session[Const.PAYWAY].ToString(),
                PayStatus = PayStatusConst.DONE,
                CustomerId = cus.Id,
                ReceiveDate = DateTime.Now,
                CreatedAt = DateTime.Now,
                CreateUserId = Session[Const.ADMINIDSESSION]?.ToString(),
                Status = StatusConst.DONE
            };
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            // thêm sản phẩm vào đơn hàng
            var orderDetails = GetProductSession();
            foreach (var item in orderDetails)
            {
                try
                {
                    decimal? discountt = (item.Product?.Discount ?? 0) * item.Quantity;
                    var totaldetail = (item.Product?.Price * item.Quantity) - discountt;
                    totalOrder += totaldetail;
                    OrderDetail orderDetail = new OrderDetail()
                    {
                        Id = Guid.NewGuid().ToString(),
                        Quantity = item.Quantity,
                        ProductId = item.ProductId,
                        Total = totaldetail,
                        OrderId = orderId,
                        CreatedAt = DateTime.Now,
                        CreateUserId = Session[Const.ADMINIDSESSION]?.ToString(),
                    };
                    _context.OrderDetails.Add(orderDetail);

                    // cập nhật số lượng
                    var product = _context.Products.Find(item.ProductId);
                    product.Quantity -= item.Quantity;
                    _context.Products.Attach(product);
                    _context.Entry(product).State = EntityState.Modified;

                    await _context.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    throw;
                }

            }
            order.Total = totalOrder - order.Discount + order.ShipFee;
            order.UpdatedAt = DateTime.Now;
            order.UpdateUserId = Session[Const.ADMINIDSESSION]?.ToString();
            _context.Orders.Attach(order);
            _context.Entry(order).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            // Lưu session 
            Session[Const.SELLID] = orderId;
            //
            TempData["success"] = "Đã xác nhận thanh toán! Bạn có thể in hóa đơn";
            ClearProductSession();
            return Redirect("Index");
        }

        // POST: Sell/ConfirmPay
        [HttpPost]
        public ActionResult ConfirmPay(Checkout checkout)
        {
            checkout.Id = Guid.NewGuid().ToString();
            string checkoutString = JsonConvert.SerializeObject(checkout);
            Session[Const.SELL] = checkoutString;
            if (checkout.PayOption == "ship")
            {
                Session[Const.PAYWAY] = PayConst.OFFLINE;
                return RedirectToAction("AddOrder");
            }
            else
            {
                Session[Const.PAYWAY] = PayConst.ONLINE;
                var orderDetails = GetProductSession();
                decimal? totalOrder = 0;
                foreach (var item in orderDetails)
                {
                    decimal? discount = (item.Product?.Discount ?? 0) * item.Quantity;
                    var totaldetail = (item.Product?.Price * item.Quantity) - discount;
                    totalOrder += totaldetail;
                }
                if (totalOrder > 50000000)
                {
                    TempData["error"] = "Số tiền thanh toán quá lớn! Vui lòng chọn thanh toán khi nhận hàng";
                    return Redirect(nameof(Index));
                }
                else
                {
                    var amount = ((int?)totalOrder - (int?)checkout.Discount).ToString();
                    return Redirect($"Payment?total={amount}");
                }

            }
            
        }
        public ActionResult Done()
        {
            Session[Const.SELLID] = "";
            return Redirect("Index");
        }
        // Sell/GetBill -> export bill
        [HttpPost]
        public ActionResult GetBill()
        {   
            if(string.IsNullOrEmpty(Session[Const.SELLID]?.ToString()))
            {
				return Json(new { error = false, message = "Đơn hàng đã hoàn thành! Vui lòng kiểm tra lại" });
			}
            else
            {
                var id = Session[Const.SELLID].ToString();
                var order = _context.Orders.Find(id);
                order.OrderDetails = _context.OrderDetails
                    .Include(o => o.Product)
                    .Include(o => o.Product.Branch)
                    .Include(o => o.Product.Category)
                    .Include(o => o.Product.Color)
                    .Include(o => o.Product.Size)
                    .Where(o => o.OrderId == order.Id)
                    .OrderBy(o => o.Product.Code)
                    .ToList();
                return PartialView("_BillPartial", order);
            }
            
        }

        // MoMo
        public ActionResult Payment(string total = "0")
        {
            //request params need to request to MoMo system
            string endpoint = "https://test-payment.momo.vn/gw_payment/transactionProcessor";
            string partnerCode = "MOMOOJOI20210710";
            string accessKey = "iPXneGmrJH0G8FOP";
            string serectkey = "sFcbSGRSJjwGxwhhcEktCHWYUuTuPNDB";
            string orderInfo = "Quét mã QR để thanh toán";
            string returnUrl = "https://localhost:44333/Admin/Sell/ConfirmPaymentClient";
            string notifyurl = "https://4c8d-2001-ee0-5045-50-58c1-b2ec-3123-740d.ap.ngrok.io/AdminSell/SavePayment"; //lưu ý: notifyurl không được sử dụng localhost, có thể sử dụng ngrok để public localhost trong quá trình test

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
                return RedirectToAction(nameof(AddOrder));
            }
            TempData["error"] = "Thanh toán không thành công! Vui lòng thử lại";
            return RedirectToAction("Index");
        }
		public ActionResult ExportBill(string id, string option = "excel")
		{
            var order = _context.Orders.Find(id);
			string timestamp = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture).ToUpper().Replace(':', '_').Replace('.', '_').Replace(' ', '_').Trim();
			var templateFileInfo = new FileInfo(Path.Combine(HostingEnvironment.MapPath("~/Template"), "BillTemplate.xlsx"));
			var orderDetails = _context.OrderDetails
                .Include(x => x.Product)
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
		[HttpPost]
        public void SavePayment()
        {
            //cập nhật dữ liệu vào db

        }
    }
}
