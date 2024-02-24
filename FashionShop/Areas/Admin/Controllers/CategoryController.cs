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

namespace FashionShop.Areas.Admin.Controllers
{
    [Admin]
    public class CategoryController : BaseController
    {
        // GET: Category/Index
        public ActionResult Index(int? page, string searchkey = "")
        {
            var pageNumber = page == null || page <= 0 ? 1 : page.Value;
            var pageSize = 5;
            var lscategory = _context.Categories
                .AsNoTracking()
                .OrderByDescending(x => x.CreatedAt).ToList();
            if (!string.IsNullOrEmpty(searchkey))
            {
                ViewBag.SearchKey = searchkey;
                lscategory = lscategory.Where(b => b.Name.ToLower().Contains(searchkey.ToLower())).ToList();
            }
            IPagedList<Category> models = lscategory.AsQueryable().ToPagedList(pageNumber, pageSize);
            ViewBag.CurrentPage = pageNumber;
            return View(models);
        }

        // GET: Category/Detail/Id
        public async Task<ActionResult> Details(string id)
        {
            if (id == null || _context.Categories == null)
            {
                return HttpNotFound();
            }

            var Category = await _context.Categories
                .FirstOrDefaultAsync(m => m.Id == id);
            if (Category == null)
            {
                return HttpNotFound();
            }

            return View(Category);
        }

        // GET: Category/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Category/Create
        [HttpPost]
        
        public async Task<ActionResult> Create([Bind(Include ="Description,Id,Code,Name,Outstanding,CreatedAt,UpdatedAt,DeletedAt,UpdateUserId,CreateUserId")] Category Category)
        {
            if (ModelState.IsValid)
            {
                Category.Id = Guid.NewGuid().ToString();
                Category.CreateUserId = Session[Const.ADMINIDSESSION]?.ToString();
                Category.CreatedAt = DateTime.Now;
                _context.Categories.Add(Category);
                await _context.SaveChangesAsync();
                TempData["success"] = "Thêm thượng hiệu thành công";
                return RedirectToAction(nameof(Index));
            }
            return View(Category);
        }

        // GET: Category/Edit/Id
        public async Task<ActionResult> Edit(string id)
        {
            if (id == null || _context.Categories == null)
            {
                return HttpNotFound();
            }

            var Category = await _context.Categories.FindAsync(id);
            if (Category == null)
            {
                return HttpNotFound();
            }
            return View(Category);
        }

        // POST: Category/Edit/Id
        [HttpPost]
        
        public async Task<ActionResult> Edit(string id, [Bind(Include = "Description,Id,Name,Code,Outstanding,CreatedAt,UpdatedAt,DeletedAt,UpdateUserId,CreateUserId")] Category category)
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
                    _context.Categories.Attach(category);
                    _context.Entry(category).State = EntityState.Modified;
 
                    await _context.SaveChangesAsync();
                    TempData["success"] = "Cập nhật danh mục thành công!";
                }
                catch (Exception ex)
                {
                    if (!CategoryExists(category.Id.ToString()))
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

        // GET: Category/Delete/Id
        public async Task<ActionResult> Delete(string id)
        {
            if (id == null || _context.Categories == null)
            {
                return HttpNotFound();
            }

            var publisher = await _context.Categories
                .FirstOrDefaultAsync(m => m.Id == id);
            if (publisher == null)
            {
                return HttpNotFound();
            }

            return View(publisher);
        }

        // POST: Category/Delete/Id
        
        [HttpPost, ActionName("Delete")]
        
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            if (_context.Categories == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Categories is null!");
            }
            var brand = _context.Categories.Find(id);
            _context.Categories.Remove(brand);
            await _context.SaveChangesAsync();
            TempData["success"] = "Xóa danh mục thành công";
            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(string id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }
    }
}
