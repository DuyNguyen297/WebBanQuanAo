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
using FashionShop.Extension;
using DocumentFormat.OpenXml.Office2010.Excel;
using FashionShop.ViewModel;
using System.Web.UI;
using FashionShop.Areas.Admin.Controllers.AuthorizeAttributeConfig;

namespace FashionShop.Areas.Admin.Controllers
{
	[User]
	public class CustomerController : BaseController
    {
        // GET: Customer/Index
        public ActionResult Index(int? page, string role = "", string searchkey = "")
        {
            var pageNumber = page == null || page <= 0 ? 1 : page.Value;
            var pageSize = 8;
            /* string id = HttpContext.User.Claims.FirstOrDefault(c => c.ValueType == "Id").Value;*/
            List<Customer> lsCustomer = _context.Customers.AsNoTracking().OrderByDescending(x => x.CreatedAt).ToList();
            
            PagedList<Customer> models = new PagedList<Customer>(lsCustomer.AsQueryable(), pageNumber, pageSize);
            ViewBag.CurrentPage = pageNumber;
            return View(models);
        }

        // GET: Customer/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Customer/Create
        [HttpPost]
        
        public async Task<ActionResult> Create([Bind(Include ="Name,Email,Phone,Address,Password,Id,CreatedAt,UpdatedAt,DeletedAt,UpdateUserId,CreateUserId")] Customer account)
        {
            if (ModelState.IsValid)
            {
                account.Id = Guid.NewGuid().ToString();
                account.Password = account.Password.ToMD5();
                account.CreatedAt = DateTime.Now;
                account.CreateUserId = Session[Const.ADMINIDSESSION]?.ToString();
				_context.Customers.Add(account);
                await _context.SaveChangesAsync();
                TempData["success"] = "Thêm tài khoản thành công!";
                return RedirectToAction("Index", "Customer");
            }
            return View(account);
        }

        // GET: Customer/Detail/Id
        
        public async Task<ActionResult> Details(string id)
        {
            if (id == null || _context.Customers == null)
            {
                return HttpNotFound();
            }

            var account = await _context.Customers.FirstOrDefaultAsync(m => m.Id == id);
            if (account == null)
            {
                return HttpNotFound();
            }

            return View(account);
        }

        // GET: Customer/Edit/id
        public async Task<ActionResult> Edit(string id)
        {
            if (id == null || _context.Customers == null)
            {
                return HttpNotFound();
            }

            var account = await _context.Customers.FindAsync(id);

            if (account == null)
            {
                return HttpNotFound();
            }
           
            return View(account);
        }
        [HttpPost]
        public async Task<ActionResult> Edit(string id, [Bind(Include ="Name,Email,Phone,Address,Id,CreatedAt,UpdatedAt,DeletedAt,UpdateUserId,CreateUserId")] Customer account)
        {
            if (id != account.Id.ToString())
            {
                return HttpNotFound();
            }

            if (ModelState.IsValid)
            {
				account.Password = account.Password.ToMD5();
				account.UpdatedAt = DateTime.Now;
				account.UpdateUserId = Session[Const.ADMINIDSESSION]?.ToString();
				_context.Customers.Attach(account);
				_context.Entry(account).State = EntityState.Modified;
				TempData["success"] = "Đã cập nhật tài khoản thành công!";
				await _context.SaveChangesAsync();
				return RedirectToAction("Index", "Customer");
            }
            return View(account);
        }
        // GET: Customer/Delete/Id
        
        public async Task<ActionResult> Delete(string id)
        {
            if (id == null || _context.Customers == null)
            {
                return HttpNotFound();
            }

            var account = await _context.Customers
                .FirstOrDefaultAsync(m => m.Id == id);
            if (account == null)
            {
                return HttpNotFound();
            }

            return View(account);
        }

        // POST: Customer/Delete/Id
        
        [HttpPost, ActionName("Delete")]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            if (_context.Customers == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Custumers is null!");
            }
            var account = _context.Customers.Find(id);
            var check = _context.Orders.Any(o=>o.CustomerId == account.Id && o.Status != StatusConst.DELIVERED && o.Status != StatusConst.CANCEL && o.Status != StatusConst.RETURN);
            if (check)
            {
                TempData["error"] = "Tài khoản này đang tồn tại đơn hàng chưa xử lí!";
                return View(account);

			}
            _context.Customers.Remove(account);
            await _context.SaveChangesAsync();
            TempData["success"] = "Xóa tài khoản thành công!";
            return RedirectToAction(nameof(Index));
        }

        private bool CustomerExists(string id)
        {
            return _context.Customers.Any(e => e.Id.ToString() == id);
        }

        [OutputCache(Duration = 0, Location = OutputCacheLocation.None, NoStore = true)]
        public ActionResult Error()
        {
            string requestId = System.Web.HttpContext.Current.Items["TraceIdentifier"] as string;

            if (string.IsNullOrEmpty(requestId))
            {
                requestId = Guid.NewGuid().ToString();
                System.Web.HttpContext.Current.Items["TraceIdentifier"] = requestId;
            }

            return View(new ErrorViewModel { RequestId = requestId });
        }

    }
}
