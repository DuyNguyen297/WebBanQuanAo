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
using Microsoft.Ajax.Utilities;

namespace FashionShop.Areas.Admin.Controllers
{
    [User]
    public class DashboardController : BaseController

    {

        // GET: Dashboard/Index
        public ActionResult Index(int? page)
        {
            using (_context)
            {
                ViewBag.Orders = _context.Orders.OrderBy(o => o.CreatedAt).ToList();//đơn hàng
                ViewBag.Categories = _context.Categories.OrderBy(o => o.CreatedAt).ToList();//danh mục
                ViewBag.Branchs = _context.Branchs.OrderBy(o => o.CreatedAt).ToList();//thương hiệu
                ViewBag.Customers = _context.Customers.OrderBy(o => o.CreatedAt).ToList();//khách hàng
                ViewBag.Products = _context.Products.OrderBy(o => o.CreatedAt).DistinctBy(p => p.Code).ToList();//sản phẩm

                ViewBag.SumOrToTal = (from b in _context.Orders select b.Total).Sum(); //tổng tiền bán
                ViewBag.SumInTotal = (from b in _context.Imports select b.Total).Sum(); // tổng tiền nhập
                ViewBag.Revenue = ViewBag.SumOrToTal - ViewBag.SumInTotal; // lợi nhuận


                var TopOrder = _context.OrderDetails
                    .Include(o => o.Product)
                    .Include(o => o.Product.Branch)
                    .Include(o => o.Product.Category)
                    .Include(o => o.Product.Color)
                    .Include(o => o.Product.Size)
                    .OrderByDescending(o => o.Quantity)
					.DistinctBy(o => o.Product.Code)
                    .ToList();

                var model = new List<Product>(); // sản phẩm bán chạy
                foreach (var order in TopOrder)
                {
                    if(!model.Any(m => m.Id == order.Id))
                    {
                        var productFullQtyById = _context.Products.Where(p => p.Id == order.ProductId).FirstOrDefault();
                        productFullQtyById.Quantity = _context.Products.Where(p => p.Code == order.Product.Code).Sum(p => p.Quantity);

						model.Add(productFullQtyById);
                    }
                    if(model.Count == 5) { break;}
                }
                return View(model);
            }

        }

    }
}
