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
using Color = FashionShop.Models.Color;

namespace FashionShop.Areas.Admin.Controllers
{
    [Admin]
    public class ColorController : BaseController
    {
        // GET: Color/Index
        public ActionResult Index(int? page, string searchkey = "")
        {
            var pageNumber = page == null || page <= 0 ? 1 : page.Value;
            var pageSize = 5;
            var lscategory = _context.Colors
                .AsNoTracking()
                .OrderByDescending(x => x.CreatedAt).ToList();
            if (!string.IsNullOrEmpty(searchkey))
            {
                ViewBag.SearchKey = searchkey;
                lscategory = lscategory.Where(b => b.Name.ToLower().Contains(searchkey.ToLower())).ToList();
            }
            IPagedList<Color> models = lscategory.AsQueryable().ToPagedList(pageNumber, pageSize);
            ViewBag.CurrentPage = pageNumber;
            return View(models);
        }

        // GET: Color/Detail/Id
        public async Task<ActionResult> Details(string id)
        {
            if (id == null || _context.Colors == null)
            {
                return HttpNotFound();
            }

            var Color = await _context.Colors
                .FirstOrDefaultAsync(m => m.Id == id);
            if (Color == null)
            {
                return HttpNotFound();
            }

            return View(Color);
        }

        // GET: Color/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Color/Create
        [HttpPost]
        public async Task<ActionResult> Create([Bind(Include ="Id,Name,Code,CreatedAt,UpdatedAt,DeletedAt,UpdateUserId,CreateUserId")] Color Color)
        {
            if (ModelState.IsValid)
            {
                Color.Id = Guid.NewGuid().ToString();
                Color.CreateUserId = Session[Const.ADMINIDSESSION]?.ToString();
                Color.CreatedAt = DateTime.Now;
                Color.UpdatedAt = DateTime.Now;
                _context.Colors.Add(Color);
                await _context.SaveChangesAsync();
                TempData["success"] = "Thêm thượng hiệu thành công";
                return RedirectToAction(nameof(Index));
            }
            return View(Color);
        }

        // GET: Color/Edit/Id
        public async Task<ActionResult> Edit(string id)
        {
            if (id == null || _context.Colors == null)
            {
                return HttpNotFound();
            }

            var Color = await _context.Colors.FindAsync(id);
            if (Color == null)
            {
                return HttpNotFound();
            }
            return View(Color);
        }

        // POST: Color/Edit/Id
        [HttpPost]
        
        public async Task<ActionResult> Edit(string id, [Bind(Include ="Id,Name,Code,CreatedAt,UpdatedAt,DeletedAt,UpdateUserId,CreateUserId")] Color category)
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
                    _context.Colors.Attach(category);
                    _context.Entry(category).State = EntityState.Modified;
 
                    await _context.SaveChangesAsync();
                    TempData["success"] = "Cập nhật màu sắc thành công!";
                }
                catch (Exception ex)
                {
                    if (!ColorExists(category.Id.ToString()))
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

        // GET: Color/Delete/Id
        public async Task<ActionResult> Delete(string id)
        {
            if (id == null || _context.Colors == null)
            {
                return HttpNotFound();
            }

            var publisher = await _context.Colors
                .FirstOrDefaultAsync(m => m.Id == id);
            if (publisher == null)
            {
                return HttpNotFound();
            }

            return View(publisher);
        }

        // POST: Color/Delete/Id
        
        [HttpPost, ActionName("Delete")]
        
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            if (_context.Colors == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Colors is null!");
            }
            var brand = _context.Colors.Find(id);
            _context.Colors.Remove(brand);
            await _context.SaveChangesAsync();
            TempData["success"] = "Xóa màu sắc thành công";
            return RedirectToAction(nameof(Index));
        }

        private bool ColorExists(string id)
        {
            return _context.Colors.Any(e => e.Id == id);
        }
    }
}
