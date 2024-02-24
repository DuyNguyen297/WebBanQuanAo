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
	[Admin]
	public class ImportController : BaseController
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
		
		public ActionResult History(int page = 1)
		{
			var pageNumber = page;
			var pageSize = 8;
			var listImports = _context.Imports
							.Include(i => i.Account)
							.Include(i => i.ImportDetails)
							.OrderByDescending(i => i.CreatedAt)
							.ToList();
			foreach (var import in listImports)
			{
				import.ImportDetails = import.ImportDetails?.OrderBy(p => p.Product?.Code).ToList();
			}
			IPagedList<Import> models = listImports.AsQueryable().ToPagedList(pageNumber, pageSize);
			ViewBag.CurrentPage = pageNumber;
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
		// lưu import vào session
		void SaveProductToSession(List<ImportDetail> imports)
		{
			if(imports == null || imports.Count == 0) 
			{
				ClearProductSession();
			}
			else
			{
				string stringImport = JsonConvert.SerializeObject(imports);
				ViewBag.Sell = imports;
				Session[Const.IMPORTSESSION] = stringImport;
			}
		}

		// Lấy Import từ Session (danh sách ImportItem)
		List<ImportDetail> GetProductSession()
		{
			string stringImport = Session[Const.IMPORTSESSION]?.ToString();
			if (!string.IsNullOrEmpty(stringImport))
			{
				return JsonConvert.DeserializeObject<List<ImportDetail>>(stringImport).Where(x => x.Quantity > 0 && x.Product.Effective == true).ToList();
			}
			return new List<ImportDetail>();
		}

		// Xóa Import khỏi session
		void ClearProductSession()
		{
			Session[Const.IMPORTSESSION] = "";
		}

		// Lưu product vào session
		// truyền vào 3 đối số
		public ActionResult AddProduct(string code, string colorId, string sizeId)
		{
			if (!string.IsNullOrEmpty(Session[Const.IMPORTID]?.ToString()))
			{
				Session[Const.IMPORTID] = "";
			}
			var product = _context.Products
								.FirstOrDefault(x => x.Code == code && x.ColorId == colorId && x.SizeId == sizeId);

			// Xử lý đưa vào Session ...
			var importDetails = GetProductSession();

			var checkExist = importDetails.Any(o => o.ProductId == product.Id);
			if (checkExist)
			{
				TempData["error"] = "Sản phẩm đã được thêm trước đó!";
			}
			else
			{
				var importDetail = new ImportDetail()
				{
					Id = Guid.NewGuid().ToString(),
					PriceIn = product.Price,
					Quantity = 1,
					ProductId = product.Id,
					Product = product,
					Total = (product.Price - product.Discount),
				};
				// thêm thuộc tính product
				importDetail.Product = product;
				//  Thêm mới
				importDetails.Add(importDetail);
				TempData["success"] = "Đã thêm sản phẩm!";

			}
			// Lưu Import vào Session
			SaveProductToSession(importDetails);
			// Chuyển đến trang hiện thị Import đã lưu vào Session
			return RedirectToAction(nameof(Index));
		}
		public void UpdateTotal(string productid)
		{
			var importDetails = GetProductSession();
			var import = importDetails.FirstOrDefault(p => p.ProductId == productid);
			importDetails.FirstOrDefault(p => p.ProductId == productid).Total = import.PriceIn * import.Quantity;
			SaveProductToSession(importDetails);
		}
		
		[HttpPost]
		public ActionResult UpdateQuantity(string productid, int quantity)
		{
			// Cập nhật Cart thay đổi số lượng quantity ...
			var importDetails = GetProductSession();
			var checkExist = importDetails.Any(p => p.ProductId == productid);
			if (checkExist)
			{
				// Đã tồn tại
				importDetails.FirstOrDefault(p => p.ProductId == productid).Quantity = quantity;
				SaveProductToSession(importDetails);
				UpdateTotal(productid);

				return Json(new { success = true, status = "success", message = "Đã cập nhật số lượng" });
			}
			else
			{
				return Json(new { success = false, status = "error", message = "Không tìm thấy sản phẩm!" });
			}
		}
		[HttpPost]
		public ActionResult UpdatePriceIn(string productid, decimal? pricein)
		{
			// Cập nhật Cart thay đổi số lượng quantity ...
			var importDetails = GetProductSession();
			var checkExist = importDetails.Any(p => p.ProductId == productid);
			if (checkExist)
			{
				// Đã tồn tại
				importDetails.FirstOrDefault(p => p.ProductId == productid).PriceIn = pricein;
				SaveProductToSession(importDetails);
				UpdateTotal(productid);

				return Json(new { success = true, status = "success", message = "Đã cập nhật giá nhập" });
			}
			else
			{
				return Json(new { success = false, status = "error", message = "Không tìm thấy sản phẩm!" });
			}
		}

		// Xóa ImportSession
		public ActionResult RemoveProduct(string productid)
		{
			var importDetails = GetProductSession();
			var checkExist = importDetails.Any(p => p.ProductId == productid);
			if (checkExist)
			{
				importDetails.Remove(importDetails.FirstOrDefault(x => x.ProductId == productid));
				SaveProductToSession(importDetails);
				TempData["success"] = "Đã xóa sản phẩm khỏi đơn hàng!";
			}
			else
			{
				TempData["error"] = "Xin vui lòng thử lại!";
			}
			return RedirectToAction(nameof(Index));
		}


		public async Task<ActionResult> AddImport()
		{
			//// thêm đơn hàng
			decimal? totalImport = 0;
			string importId = Guid.NewGuid().ToString();
			string code = $"Import-{Utilities.GenerateCode("")}";

			Import import = new Import()
			{
				Id = importId,
				Code = code,
				CreateUserId = Session[Const.ADMINIDSESSION]?.ToString(),
			};
			_context.Imports.Add(import);
			await _context.SaveChangesAsync();
			// Import Details
			var importDetails = GetProductSession();
			foreach (var item in importDetails)
			{
				try
				{
					var totaldetail = (item.PriceIn * item.Quantity);
					totalImport += totaldetail;
					ImportDetail importDetail = new ImportDetail()
					{
						Id = Guid.NewGuid().ToString(),
						Quantity = item.Quantity,
						PriceIn = item.PriceIn,
						ProductId = item.ProductId,
						Total = totaldetail,
						ImportId = importId,
						CreatedAt = DateTime.Now,
						CreateUserId = Session[Const.ADMINIDSESSION]?.ToString(),
					};
					_context.ImportDetails.Add(importDetail);

					// cập nhật số lượng
					var product = _context.Products.Find(item.ProductId);
					product.Quantity += item.Quantity;
					_context.Products.Attach(product);
					_context.Entry(product).State = EntityState.Modified;

					await _context.SaveChangesAsync();
				}
				catch (Exception e)
				{
					throw;
				}
			}

			// Import
			import.Total = totalImport;
			import.CreatedAt = DateTime.Now;

			_context.Imports.Attach(import);
			_context.Entry(import).State = EntityState.Modified;
			await _context.SaveChangesAsync();
			// Lưu session 
			Session[Const.IMPORTID] = importId;
			//
			TempData["success"] = "Đã nhập kho hàng thành công! Bạn có thể in hóa đơn";
			ClearProductSession();
			return Redirect("Index");
		}

		// POST: Sell/ConfirmPay
		[HttpPost]
		public ActionResult ConfirmPay()
		{
			return RedirectToAction("AddImport");
		}
		public ActionResult Done()
		{
			Session[Const.IMPORTID] = "";
			return Redirect("Index");
		}
		// Sell/GetBill -> export bill
		[HttpPost]
		public ActionResult GetBill()
		{
			if (string.IsNullOrEmpty(Session[Const.IMPORTID]?.ToString()))
			{
				return Json(new { error = false, message = "Nhập hàng đã hoàn thành! Vui lòng kiểm tra lại" });
			}
			else
			{
				var id = Session[Const.IMPORTID].ToString();
				var import = _context.Imports.Include(i => i.Account).FirstOrDefault(i => i.Id == id);
				import.ImportDetails = _context.ImportDetails
					.Include(o => o.Product)
					.Include(o => o.Product.Branch)
					.Include(o => o.Product.Category)
					.Include(o => o.Product.Color)
					.Include(o => o.Product.Size)
					.OrderBy(o => o.Product.Code)
					.Where(o => o.ImportId == import.Id)
					.ToList();
				return PartialView("_BillPartial", import);
			}
		}
		[HttpPost]
		public ActionResult GetBillById(string id)
		{
			var import = _context.Imports.Include(i => i.Account).FirstOrDefault(i => i.Id == id);
			if(import == null)
			{
				return Json(new { error = false, message = "Không tìm thấy đơn hàng! Vui lòng kiểm tra lại" });
			}
			import.ImportDetails = _context.ImportDetails
				.Include(o => o.Product)
				.Include(o => o.Product.Branch)
				.Include(o => o.Product.Category)
				.Include(o => o.Product.Color)
				.Include(o => o.Product.Size)
				.OrderBy(o => o.Product.Code)
				.Where(o => o.ImportId == import.Id)
				.ToList();
			return PartialView("_BillPartial", import);
		}

		// MoMo
		public ActionResult Payment(string total = "0")
		{
			//request params need to request to MoMo system
			string endpoint = "https://test-payment.momo.vn/gw_payment/transactionProcessor";
			string partnerCode = "MOMOOJOI20210710";
			string accessKey = "iPXneGmrJH0G8FOP";
			string serectkey = "sFcbSGRSJjwGxwhhcEktCHWYUuTuPNDB";
			string importInfo = "Quét mã QR để thanh toán";
			string returnUrl = "https://localhost:44333/Admin/Sell/ConfirmPaymentClient";
			string notifyurl = "https://4c8d-2001-ee0-5045-50-58c1-b2ec-3123-740d.ap.ngrok.io/AdminSell/SavePayment"; //lưu ý: notifyurl không được sử dụng localhost, có thể sử dụng ngrok để public localhost trong quá trình test

			string amount = total;
			string importid = DateTime.Now.Ticks.ToString(); //mã đơn hàng
			string requestId = DateTime.Now.Ticks.ToString();
			string extraData = "";

			//Before sign HMAC SHA256 signature
			string rawHash = "partnerCode=" +
				partnerCode + "&accessKey=" +
				accessKey + "&requestId=" +
				requestId + "&amount=" +
				amount + "&importId=" +
				importid + "&importInfo=" +
				importInfo + "&returnUrl=" +
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
				{ "importId", importid },
				{ "importInfo", importInfo },
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
				return RedirectToAction(nameof(AddImport));
			}
			TempData["error"] = "Thanh toán không thành công! Vui lòng thử lại";
			return RedirectToAction("Index");
		}
		public ActionResult ExportBill(string id, string option = "excel")
		{
			var import = _context.Imports.Include(i => i.Account).FirstOrDefault(i => i.Id == id);
			string timestamp = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture).ToUpper().Replace(':', '_').Replace('.', '_').Replace(' ', '_').Trim();
			var templateFileInfo = new FileInfo(Path.Combine(HostingEnvironment.MapPath("~/Template"), "ImportTemplate.xlsx")); //template excel
			var importDetails = _context.ImportDetails
				.Include(x => x.Product)
				.Where(x => x.ImportId == import.Id)
				.ToList();
			var stream = ExportToExcelHelper.UpdateDataIntoExcelImportTemplate(importDetails, import, templateFileInfo);

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
				return File(buffer, "application/pdf", "Import-" + timestamp + ".pdf");
			}
			//excel
			return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Import-" + timestamp + ".xlsx");
		}
		[HttpPost]
		public void SavePayment()
		{
			//cập nhật dữ liệu vào db

		}
	}
}
