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
using Size = FashionShop.Models.Size;

namespace FashionShop.Areas.Admin.Controllers
{
    [Admin]
    public class SizeController : BaseController
    {
        // GET: Size/Index
        public ActionResult Index(int? page, string searchkey = "")
        {
            var pageNumber = page == null || page <= 0 ? 1 : page.Value;
            var pageSize = 5;
            var lscategory = _context.Sizes
                .AsNoTracking()
                .OrderByDescending(x => x.CreatedAt).ToList();
            if (!string.IsNullOrEmpty(searchkey))
            {
                ViewBag.SearchKey = searchkey;
                lscategory = lscategory.Where(b => b.Name.ToLower().Contains(searchkey.ToLower())).ToList();
            }
            IPagedList<Size> models = lscategory.AsQueryable().ToPagedList(pageNumber, pageSize);
            ViewBag.CurrentPage = pageNumber;
            return View(models);
        }

        // GET: Size/Detail/Id
        public async Task<ActionResult> Details(string id)
        {
            if (id == null || _context.Sizes == null)
            {
                return HttpNotFound();
            }

            var Size = await _context.Sizes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (Size == null)
            {
                return HttpNotFound();
            }

            return View(Size);
        }

        // GET: Size/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Size/Create
        [HttpPost]
        
        public async Task<ActionResult> Create([Bind(Include ="Id,Name,Code,CreatedAt,UpdatedAt,DeletedAt,UpdateUserId,CreateUserId")] Size Size)
        {
            if (ModelState.IsValid)
            {
                Size.Id = Guid.NewGuid().ToString();
                Size.CreateUserId = Session[Const.ADMINIDSESSION]?.ToString();
                Size.CreatedAt = DateTime.Now;
                Size.UpdatedAt = DateTime.Now;
                _context.Sizes.Add(Size);
                await _context.SaveChangesAsync();
                TempData["success"] = "Thêm thượng hiệu thành công";
                return RedirectToAction(nameof(Index));
            }
            return View(Size);
        }

        // GET: Size/Edit/Id
        public async Task<ActionResult> Edit(string id)
        {
            if (id == null || _context.Sizes == null)
            {
                return HttpNotFound();
            }

            var Size = await _context.Sizes.FindAsync(id);
            if (Size == null)
            {
                return HttpNotFound();
            }
            return View(Size);
        }

        // POST: Size/Edit/Id
        [HttpPost]
        
        public async Task<ActionResult> Edit(string id, [Bind(Include = "Id,Name,Code,CreatedAt,UpdatedAt,DeletedAt,UpdateUserId,CreateUserId")] Size category)
        {
            if (id != category.Id.ToString())
            {
                return HttpNotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    category.UpdatedAt = DateTime.Now;
                    category.UpdateUserId = Session[Const.ADMINIDSESSION]?.ToString();
                    _context.Sizes.Attach(category);
                    _context.Entry(category).State = EntityState.Modified;
 
                    await _context.SaveChangesAsync();
                    TempData["success"] = "Cập nhật danh mục thành công!";
                }
                catch (Exception ex)
                {
                    if (!SizeExists(category.Id.ToString()))
                    {
                        return HttpNotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // GET: Size/Delete/Id
        public async Task<ActionResult> Delete(string id)
        {
            if (id == null || _context.Sizes == null)
            {
                return HttpNotFound();
            }

            var publisher = await _context.Sizes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (publisher == null)
            {
                return HttpNotFound();
            }

            return View(publisher);
        }

        // POST: Size/Delete/Id
        
        [HttpPost, ActionName("Delete")]
        
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            if (_context.Sizes == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Sizes is null!");
            }
            var brand = _context.Sizes.Find(id);
            _context.Sizes.Remove(brand);
            await _context.SaveChangesAsync();
            TempData["success"] = "Xóa danh mục thành công";
            return RedirectToAction(nameof(Index));
        }

        private bool SizeExists(string id)
        {
            return _context.Sizes.Any(e => e.Id == id);
        }
    }
}
