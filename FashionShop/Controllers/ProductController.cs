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
using Microsoft.Ajax.Utilities;
using FashionShop.AreaModels;

namespace FashionShop.Controllers
{
    public class ProductController : GlobalController
    {
        public ActionResult Index(int page = 1, string brandid ="", string categoryid="", string search="")
        {
            var pageNumber = page;
            var pageSize = 10;
            var products = _context.Products
                .Include(p => p.Color)
                .Include(p => p.Size)
                .Include(p => p.Branch)
                .Include(p => p.Category)
                .Include(p => p.Feedbacks)
                .Where(p => p.Effective == true)
                .DistinctBy(p => p.Code)
                .ToList();
            if (!string.IsNullOrEmpty(brandid))
            {
                products = products.Where(x => x.BranchId == brandid).ToList();
                TempData["brandid"] = brandid;
            }
            if (!string.IsNullOrEmpty(categoryid))
            {
                products = products.Where(x => x.CategoryId == categoryid).ToList();
                TempData["categoryid"] = categoryid;
            }
            if (!string.IsNullOrEmpty(search))
            {
                TempData["search"] = search;
                products = products.Where(x => x.Name.ToLower().Contains(search.ToLower())).ToList();

            }

            // Feedbacks
			products.ForEach(p =>
			{
				var productIds = _context.Products.Where(pp => pp.Code == p.Code).Select(pp => pp.Id).ToList();
				var feedbacks = _context.Feedbacks.Where(f => productIds.Contains(f.ProductId)).ToList();
				p.Feedbacks = feedbacks;
			});
            ///
			PagedList<Product> models = new PagedList<Product>(products.AsQueryable(), pageNumber, pageSize);
            ViewBag.CurrentPage = pageNumber;
			Dictionary<string, string> _dictonaryBranch = new Dictionary<string, string>();
			Dictionary<string, string> _dictonaryCategory = new Dictionary<string, string>();
			Dictionary<string, string> _dictonaryPrice = new Dictionary<string, string>()
			{
				{".price0", $"0 - {200000.ToString("n0")}"},
				{".price1", $"{200000.ToString("n0")} - {500000.ToString("n0")}"},
				{".price2", $"{500000.ToString("n0")} - {1000000.ToString("n0")}"},
				{".price3", $"> {1000000.ToString("n0")}"},
			};
			foreach (var item in _context.Branchs)
			{
				_dictonaryBranch.Add($".{item.Id}", item.Name);
			}
			foreach (var item in _context.Categories)
			{
				_dictonaryCategory.Add($".{item.Id}", item.Name);
			}
			ViewBag.PriceRate = _dictonaryPrice;
			ViewBag.Category = _dictonaryCategory;
			ViewBag.Branch = _dictonaryBranch;

			return View(models);
        }
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var product = _context.Products
                .Include(p => p.Color)
                .Include(p => p.Size)
                .Include(p => p.Branch)
                .Include(p => p.Category)
                .Include(p => p.Feedbacks)
                .FirstOrDefault(x => x.Id == id);

			if (product == null)
			{
				return HttpNotFound();
			}
            //custom
			product.Quantity = _context.Products.Where(p => p.Code == product.Code).Sum(p => p.Quantity);

			// Feedbacks
			var productIds = _context.Products.Where(pp => pp.Code == product.Code).Select(pp => pp.Id).ToList();
			var feedbacks = _context.Feedbacks.Where(f => productIds.Contains(f.ProductId)).ToList();
			product.Feedbacks = feedbacks;



            var productByBranchAndCategory = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Branch)
                .DistinctBy(p => p.Code)
                .Where(p => p.BranchId == product.BranchId && p.Id != id)
                .Take(5)
                .ToList();
            if(productByBranchAndCategory.Count < 5)
            {
                productByBranchAndCategory.AddRange(_context.Products
                    .Include(p => p.Category)
                    .Include(p => p.Branch)
                    .DistinctBy(p => p.Code)
                    .Where(p => p.CategoryId == product.CategoryId && p.Id != id && !productByBranchAndCategory.Any(x => x.Code == p.Code))
                    .Take(5 - productByBranchAndCategory.Count)
                    .ToList());
			}
            ViewBag.ProductByBranchAndCategory = productByBranchAndCategory;

			ViewBag.Rate = product.Feedbacks.Average(x => x.Rate);
            var listColorSize = new List<ViewColorSizeModel>();
            var listFullSize = _context.Products.Include(p => p.Size).Where(p => p.Code == product.Code)?.DistinctBy(p => p.SizeId)?.Select(p => p.Size).ToList();
			var productColors = _context.Products.Include(p => p.Color).Where(p => p.Code == product.Code)?.DistinctBy(p => p.ColorId)?.Select(p => p.Color).ToList();
            foreach(var item in productColors)
            {
                var sizes = _context.Products.Include(p => p.Size).Where(p => p.Code == product.Code && p.ColorId == item.Id).Select(p => p.Size).ToList();
                listColorSize.Add(new ViewColorSizeModel()
                {
                    Color = item,
                    Size = sizes
				});
			}
			ViewBag.ColorId = _context.Colors.ToList();
			ViewBag.SizeId = _context.Sizes.ToList();
			ViewBag.ColorSize = listColorSize;
			ViewBag.FullSize = listFullSize;

			return View(product);
        }

        [HttpPost]
        public ActionResult Feedback(Feedback feedback)
        {
            if (feedback.ProductId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            if (!string.IsNullOrEmpty(Session[Const.USERIDSESSION]?.ToString()))
            {
				var cusId = Session[Const.CARTSESSION]?.ToString();
				feedback.CreateUserId = cusId;
			}
            else
            {
				TempData["error"] = "Bạn cần đăng nhập để đánh giá sản phẩm!";
				return RedirectToAction("Details", new { id = feedback.ProductId });
			}
            feedback.Id = Guid.NewGuid().ToString();
            _context.Feedbacks.Add(feedback);
            _context.SaveChanges();
            TempData["success"] = "Đã gửi đánh giá thành công!";
            return RedirectToAction("Details",new {id = feedback.ProductId});
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
