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
namespace FashionShop.Areas.Admin.Controllers
{
    public class ThongKeUser : BaseController
    {
        public ActionResult Index(int? page)
        {
            var pageNumber = page == null || page <= 0 ? 1 : page.Value;
            var pageSize = 8;
            var lsOrder = _context.Orders
                .AsNoTracking()
                .OrderByDescending(x => x.CreatedAt);
            PagedList<Order> models = new PagedList<Order>(lsOrder, pageNumber, pageSize);
            ViewBag.CurrentPage = pageNumber;
            return View(models);
        }
        [HttpPost]
        public ActionResult Index(DateTime from_date, DateTime to_date)
        {
            using (_context)
            {
                ViewBag.GetBills = (from b in _context.Orders where b.CreatedAt >= from_date && b.CreatedAt <= to_date == true select b).ToList();
                ViewBag.GetQuantityOrder = (from b in _context.Orders where b.CreatedAt >= from_date && b.CreatedAt <= to_date == true select b.Id).Count();
                ViewBag.SumToTal = (from b in _context.Orders where b.CreatedAt >= from_date && b.CreatedAt <= to_date == true select b.Total).Sum();
                return View();
            }
        }
        public ActionResult Filtter(string CatID = "0")
        {
            var url = $"/Order?CatID={CatID}";
            if (CatID == "0")
            {
                url = $"/Order";
            }
            return Json(new { status = "success", redirectUrl = url });
        }

        // GET: Orders/Details/5
        public ActionResult Details(string id)
        {
            var pageNumber = 1;
            var pageSize = 8;
            var orderDetail = _context.OrderDetails
                .Where(x => x.OrderId.ToString() == id)
                .Include(x => x.Product);
            PagedList<OrderDetail> models = new PagedList<OrderDetail>(orderDetail.AsQueryable(), pageNumber, pageSize);
            ViewBag.CurrentPage = pageNumber;
            return View(models);
        }
        // GET: Orders/Delete/5
        public async Task<ActionResult> Delete(string id)
        {
            if (id == null || _context.Orders == null)
            {
                return HttpNotFound();
            }

            var order = await _context.Orders
                .FirstOrDefaultAsync(m => m.Id.ToString() == id);
            if (order == null)
            {
                return HttpNotFound();
            }

            return View(order);
        }

        // POST: Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            if (_context.Orders == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Orders is null!");
            }
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                _context.Orders.Remove(order);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrderExists(string id)
        {
            return _context.Orders.Any(e => e.Id.ToString() == id);
        }
    }
}
