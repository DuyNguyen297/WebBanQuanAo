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
using FashionShop.AreaModels;
using System.Web.Hosting;
using Microsoft.Ajax.Utilities;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Spreadsheet;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TreeView;
using DocumentFormat.OpenXml.Office2010.Excel;
using Newtonsoft.Json;

namespace FashionShop.Areas.Admin.Controllers
{
    [Admin]
    public class ProductController : BaseController
    {
        // GET: Product/Index
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
               .DistinctBy(p => p.Code)
               .OrderByDescending(x => x.CreatedAt)
               .ToList();

            foreach(var item in lsProducts)
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
        public ActionResult Filter(string catid = "0", string branchid = "0")
        {
            var url = "/Product";
            if (catid != "0")
            {
                url += $"?catid={catid}";
            }
            if (branchid != "0")
            {
                url += $"&branchid={branchid}";
            }

            return Redirect(url);
        }
        // GET: Product/Detail/Id
        public ActionResult Details(string id)
        {
            ClearColorSession();

            if (!ProductExists(id))
            {
                return HttpNotFound();
            }
            string code = _context.Products.Find(id).Code;
            var productDel = ConvertProductToCrudModel(code);
            ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "Name", productDel.CategoryId);
            ViewBag.BranchId = new SelectList(_context.Branchs, "Id", "Name", productDel.BranchId);
            ViewBag.ColorId = _context.Colors.ToList();
            ViewBag.SizeId = _context.Sizes.ToList();
            return View(productDel);
        }
        // Color session
        void SaveColorToSession(List<CreateColorModel> colors)
        {
            string stringColor = JsonConvert.SerializeObject(colors);
            ViewBag.ColorSize = colors;
            Session[Const.COLOR] = stringColor;
        }
        List<CreateColorModel> GetColorSession()
        {
            string stringColor = Session[Const.COLOR]?.ToString();
            if (!string.IsNullOrEmpty(stringColor))
            {
                return JsonConvert.DeserializeObject<List<CreateColorModel>>(stringColor).ToList();
            }
            return new List<CreateColorModel>();
        }
        // Xóa color khỏi session
        void ClearColorSession()
        {
            Session[Const.COLOR] = "";
        }

        [HttpPost]
        public ActionResult UpdateColor(string colorId, int index)
        {
            var colors = GetColorSession();
            if(colors.Any(c => c.Color == colorId))
            {
                // Tìm ra màu đã có
                string colorOld = colors.FirstOrDefault(c => c.Index == index).Color;
                if(colorOld == null)
                {
                    return Json(new { success = true, colorOld = "undefined" });
                }
                return Json(new { success = true, colorOld = colorOld });
            }
            colors.FirstOrDefault(c => c.Index == index).Color = colorId;
            SaveColorToSession(colors);
            return Json(new { success = true, colorOld = "" });
        }
        [HttpPost]
        public ActionResult UpdateSize(string[] sizes, int index)
        {
            var colors = GetColorSession();
            colors.FirstOrDefault(c => c.Index == index).Size = sizes;
            SaveColorToSession(colors);
            return Content("Ok");
        }
        [HttpPost]
        public ActionResult AddColorSize(int nextColor)
        {
            var colors = GetColorSession();
            colors.Add(new CreateColorModel()
            {
                Index = nextColor,
            });
            SaveColorToSession(colors);

            ViewBag.ColorId = new SelectList(_context.Colors
                                .Select(c => new { Value = c.Id, Text = c.Code + "_" + c.Name }), "Value", "Text");
            ViewBag.SizeId = new MultiSelectList(_context.Sizes, "Id", "Code");
            
            return PartialView("_ColorSize", new CreateColorModel()
            {
                Index = nextColor,
            });
        }

        [HttpPost]
        public ActionResult ResetColorSize()
        {
            ClearColorSession();
            var colorSizeInit = new CreateColorModel()
            {
                Index = 0,
            };
            SaveColorToSession(new List<CreateColorModel>()
            {
                colorSizeInit
            });
            ViewBag.ColorId = new SelectList(_context.Colors
                                .Select(c => new { Value = c.Id, Text = c.Code + "_" + c.Name }), "Value", "Text");
            ViewBag.SizeId = new MultiSelectList(_context.Sizes, "Id", "Code");

            return PartialView("_ColorSize", new CreateColorModel()
            {
                Color = "color_0"
            });
        }
        // GET: Product/Create
        public ActionResult Create()
        {
            ClearColorSession();
            var colorSizeInit = new CreateColorModel()
            {
                Index = 0,
            };
            SaveColorToSession(new List<CreateColorModel>() 
            {
                colorSizeInit
            });
            ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "Name");
            ViewBag.BranchId = new SelectList(_context.Branchs, "Id", "Name");     
            ViewBag.ColorId = new SelectList(_context.Colors
                                .Select(c => new { Value = c.Id, Text = c.Code + "_" + c.Name }), "Value", "Text");
            ViewBag.SizeId = new MultiSelectList(_context.Sizes, "Id", "Code");
            return View();
        }

        // POST: Product/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost, ValidateInput(false)]
        public async Task<ActionResult> Create([Bind(Include = "Description,Image,Price,Discount,BranchId,CategoryId,Id,Name,Code,Effective,Outstanding")] CreateProductModel product, HttpPostedFileBase fThumb)
        {
            // check code
            string codeNew = Utilities.SEOUrl(product.Code).ToLower();
            if(_context.Products.Any(p => p.Code == codeNew))
            {
                // lấy lại giá trị màu + size
                var colors1 = GetColorSession();
                SaveColorToSession(colors1);
                ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
                ViewBag.BranchId = new SelectList(_context.Branchs, "Id", "Name", product.BranchId);
                ViewBag.ColorId = new SelectList(_context.Colors
                                .Select(c => new { Value = c.Id, Text = c.Code + "_" + c.Name }), "Value", "Text");
                ViewBag.SizeId = new MultiSelectList(_context.Sizes, "Id", "Code");

                TempData["error"] = "Mã sản phẩm đã tồn tại!";
                return View(product);
            }
            if (ModelState.IsValid)
            {
				var productTemp = new Product();
				productTemp.Name = Utilities.ToTitleCase(product.Name);
				productTemp.Code = codeNew;
				if (fThumb != null)
				{
					string extension = Path.GetExtension(fThumb.FileName);
					productTemp.Image = Utilities.SEOUrl(productTemp.Name) + $"-{productTemp.Code}" + extension;
					await Utilities.UploadFile(fThumb, @"product", productTemp.Image);
				}
				else productTemp.Image = "default.jpg";

				var listCreateColors = GetColorSession();

                foreach(var item in listCreateColors)
                {
                    foreach(var size in item.Size)
                    {
                        var productCr = new Product();
                        productCr.Id = Guid.NewGuid().ToString();
                        productCr.Name = productTemp.Name;
                        productCr.Code = productTemp.Code;
                        productCr.Effective = product.Effective;
                        productCr.Outstanding = product.Outstanding;
                        productCr.BranchId = product.BranchId;
                        productCr.CategoryId = product.CategoryId;
                        productCr.Discount = product.Discount;
                        productCr.Price = product.Price;
                        productCr.Description = product.Description;
                        // số lương là 0
                        productCr.Quantity = 0;
                        // màu + size
                        productCr.ColorId = item.Color;
                        productCr.SizeId = size;
                        //image
                        productCr.Image = productTemp.Image;

						productCr.CreateUserId = Session[Const.ADMINIDSESSION]?.ToString();
                        productCr.CreatedAt = DateTime.Now;

                        _context.Products.Add(productCr);
                    }
                }
                await _context.SaveChangesAsync();
                TempData["success"] = "Thêm sản phẩm thành công!";
                return RedirectToAction(nameof(Index));
            }

            // lấy lại giá trị màu + size
            var colors2 = GetColorSession();
            SaveColorToSession(colors2);
            ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            ViewBag.BranchId = new SelectList(_context.Branchs, "Id", "Name", product.BranchId);
            ViewBag.ColorId = new SelectList(_context.Colors
                            .Select(c => new { Value = c.Id, Text = c.Code + "_" + c.Name }), "Value", "Text");
            ViewBag.SizeId = new MultiSelectList(_context.Sizes, "Id", "Code");

            return View(product);
        }


        // POST: Product/ChangeEffective/Id
        public async Task<ActionResult> ChangeEffective(string id)
        {
            if (!ProductExists(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Product is null!");
            }
            string code = _context.Products.Find(id).Code;
            if (code == null)
            {
                TempData["error"] = "Vui lòng thử lại!";
            }
            else
            {
                var products = await _context.Products.Where(p => p.Code == code).ToListAsync();
                foreach(var item in products)
                {
                    item.Effective = !item.Effective;
                    _context.Products.Attach(item);
                    _context.Entry(item).State = EntityState.Modified;
                }
                
                _context.SaveChanges();
                TempData["success"] = "Đổi hiệu lực hành công!";
            }
            
            return RedirectToAction("Index");
        }
        // GET: Product/Edit/Id
        public async Task<ActionResult> Edit(string id)
        {
            ClearColorSession();

            if (!ProductExists(id))
            {
                return HttpNotFound();
            }
            string code = _context.Products.Find(id).Code;
            var productEdit = ConvertProductToCrudModel(code);
            var colors = GetColorSession();
            int i = 0;
            foreach(var item in productEdit.ColorSizes)
            {
                colors.Add(new CreateColorModel()
                {
                    Index = i,
                    Color = item.Color,
                    Size = item.Size
                });
                i += 1;
            }
            SaveColorToSession(colors);
            ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "Name", productEdit.CategoryId);
            ViewBag.BranchId = new SelectList(_context.Branchs, "Id", "Name", productEdit.BranchId);
            ViewBag.ColorId = new SelectList(_context.Colors
                            .Select(c => new { Value = c.Id, Text = c.Code + "_" + c.Name }), "Value", "Text");
            ViewBag.SizeId = new MultiSelectList(_context.Sizes, "Id", "Code");
            return View(productEdit);
        }

        // POST: Product/Edit/Id
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost, ValidateInput(false)]
        
        public async Task<ActionResult> Edit(string id, [Bind(Include = "Description,Image,Price,Discount,BranchId,CategoryId,Outstanding,Id,Name,Code,Effective")] CreateProductModel product, HttpPostedFileBase fThumb)
        {
            if (!ProductExists(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Product is null!");
            }
			string codeNew = Utilities.SEOUrl(product.Code).ToUpper();
			string codeOld = _context.Products.Find(id).Code;

            if (ModelState.IsValid)
            {
                var products = await _context.Products.Where(p => p.Code == codeOld).ToListAsync();
                var productSame = new List<Product>();
                var productAdd = new List<Product>();
                if (products.Count == 0)
                {
                    return HttpNotFound();
                }
                var listCreateColors = GetColorSession();

                var listColorSelected = listCreateColors.Select(s => s.Color);

                // image
                var image_update = products[0].Image;
                if (fThumb != null)
                {
                    string pathOld = Path.Combine(HostingEnvironment.MapPath("~/Assets"), "images", "product", products[0].Image);
                    if (System.IO.File.Exists(pathOld))
                    {
                        System.IO.File.Delete(pathOld);
                    }
                    string extension = Path.GetExtension(fThumb.FileName);
                    image_update = Utilities.SEOUrl(products[0].Name) + $"-{codeNew}" + extension;
                    await Utilities.UploadFile(fThumb, @"product", image_update);

                }
                // thêm lại
                foreach (var item in listCreateColors)
                {
                    foreach (var size in item.Size)
                    {
                        var productOld = products.FirstOrDefault(p => p.Code == codeOld && p.ColorId == item.Color && p.SizeId == size);
                        //product same
                        if(productOld != null)
                        {
                            productSame.Add(productOld);
						}
                        else //product add
                        {
							productAdd.Add(new Product { ColorId = item.Color, SizeId = size});
						}
                    }
                }
                foreach(var item in productSame)
                {
                    var productUpd = _context.Products.Find(item.Id);
					productUpd.Name = Utilities.ToTitleCase(product.Name);
					productUpd.Code = codeNew;
					productUpd.Effective = product.Effective;
					productUpd.Outstanding = product.Outstanding;
					productUpd.BranchId = product.BranchId;
					productUpd.CategoryId = product.CategoryId;
					productUpd.Discount = product.Discount;
					productUpd.Price = product.Price;
					productUpd.Description = product.Description;

					//// image
					productUpd.Image = image_update;

					// auth
					productUpd.UpdateUserId = Session[Const.ADMINIDSESSION]?.ToString();
					productUpd.UpdatedAt = DateTime.Now;

					_context.Products.Attach(productUpd);
					_context.Entry(productUpd).State = EntityState.Modified;
				}
				foreach (var item in productAdd)
				{
                    item.Id = Guid.NewGuid().ToString();
					item.Name = Utilities.ToTitleCase(product.Name);
					item.Code = codeNew;
					item.Effective = product.Effective;
					item.Outstanding = product.Outstanding;
					item.BranchId = product.BranchId;
					item.CategoryId = product.CategoryId;
					item.Discount = product.Discount;
					item.Price = product.Price;
					item.Description = product.Description;

					//// image
					item.Image = image_update;

					// auth
					item.CreateUserId = Session[Const.ADMINIDSESSION]?.ToString();
					item.CreatedAt = DateTime.Now;

                    _context.Products.Add(item);
					
				}
				//// xóa các sản phẩm ngoại lệ
				var productExcept = products.Except(productSame);
				_context.Products.RemoveRange(productExcept);
                ////
				await _context.SaveChangesAsync();
               
                TempData["success"] = "Cập nhật sản phẩm thành công!";
                return RedirectToAction(nameof(Index));
            }

            // lấy lại giá trị màu + size
            var colors2 = GetColorSession();
            SaveColorToSession(colors2);
            ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            ViewBag.BranchId = new SelectList(_context.Branchs, "Id", "Name", product.BranchId);
            ViewBag.ColorId = new SelectList(_context.Colors
                            .Select(c => new { Value = c.Id, Text = c.Code + "_" + c.Name }), "Value", "Text");
            ViewBag.SizeId = new MultiSelectList(_context.Sizes, "Id", "Code");

            return View(product);
        }

        // GET: Product/Delete/Id
        public async Task<ActionResult> Delete(string id)
        {
            if (!ProductExists(id))
            {
                return HttpNotFound();
            }
            string code = _context.Products.Find(id).Code;
            var productDel = ConvertProductToCrudModel(code);

            ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "Name", productDel.CategoryId);
            ViewBag.BranchId = new SelectList(_context.Branchs, "Id", "Name", productDel.BranchId);
            ViewBag.ColorId = _context.Colors.ToList();
            ViewBag.SizeId = _context.Sizes.ToList();

            return View(productDel);
        }

        // POST: Product/Delete/Id
        [HttpPost, ActionName("Delete")]
        
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            if (!ProductExists(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Product is null!");
            }
            string code = _context.Products.Find(id).Code;
            var products = await _context.Products.Where(p => p.Code == code).ToListAsync();
            
            if (!string.IsNullOrEmpty(products[0].Image))
            {
                string path = Path.Combine(HostingEnvironment.MapPath("~/Assets"), "images", "product", products[0].Image);
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }
            }

            _context.Products.RemoveRange(products);
            await _context.SaveChangesAsync();
            TempData["success"] = "Xóa sản phẩm thành công!";

            return RedirectToAction(nameof(Index));
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
                Outstanding = listProducts[0].Outstanding,
                Quantity = quantity,
                ColorSizes = createColors
            };
        }
        private bool ProductExists(string id)
        {
            return _context.Products.Any(p => p.Id == id);
        }
        ///////////////////////////////////////////
        //[HttpPost]
        //public FileResult ExportToExcel(string catid = "0", string branchid = "0", string limit = "all")
        //{
        //    DataTable dt = new DataTable("product");
        //    dt.Columns.AddRange(new DataColumn[] {
        //        new DataColumn("Ma"),
        //        new DataColumn("Ten"),
        //        new DataColumn("So luong"),
        //        new DataColumn("Gia"),
        //        new DataColumn("Gia giam"),
        //        new DataColumn("Danh muc"),
        //        new DataColumn("Thuong hieu"),
        //        new DataColumn("Mo ta"),
        //        new DataColumn("Ngay nhap"),
        //    });

        //    var insuranceCertificate = _context.Products
        //                                .Include(p => p.Category)
        //                                .Include(p => p.Branch)
        //                                .ToList();

        //    insuranceCertificate = catid != "0" ? insuranceCertificate.Where(p => p.CategoryId == catid).ToList() : insuranceCertificate;
        //    insuranceCertificate = branchid != "0" ? insuranceCertificate.Where(p => p.BranchId == branchid).ToList() : insuranceCertificate;
        //    if (limit != "all")
        //    {
        //        insuranceCertificate = limit == StatusConst.COMINGEND ? insuranceCertificate.Where(p => p.Quantity <= 5).ToList() : insuranceCertificate.Where(p => p.Quantity > 5).ToList();
        //    }

        //    foreach (var insurance in insuranceCertificate)
        //    {
        //        dt.Rows.Add(insurance.Code, insurance.Name, insurance.Quantity, insurance.Price, insurance.Discount, insurance.Category.Name, insurance.Branch.Name, insurance.Description, insurance.CreatedAt);
        //    }

        //    using (XLWorkbook wb = new XLWorkbook())
        //    {
        //        wb.Worksheets.Add(dt);
        //        using (MemoryStream stream = new MemoryStream())
        //        {
        //            wb.SaveAs(stream);
        //            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ExcelFile.xlsx");
        //        }
        //    }
        //}

    }
    /////////////////////////////////////////////
}

