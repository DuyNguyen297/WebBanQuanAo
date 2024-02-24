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
using Branch = FashionShop.Models.Branch;
using FashionShop.Areas.Admin.Controllers.AuthorizeAttributeConfig;
using System.Web.Hosting;

namespace FashionShop.Areas.Admin.Controllers
{
    [Admin]
    public class BranchController : BaseController
    {
        // GET: Branch/Index
        public ActionResult Index(int? page, string searchkey = "")
        {
            var pageNumber = page == null || page <= 0 ? 1 : page.Value;
            var pageSize = 5;
            var lsbrand = _context.Branchs
                .AsNoTracking()
                .OrderByDescending(x => x.CreatedAt).ToList();
            if (!string.IsNullOrEmpty(searchkey))
            {
                ViewBag.SearchKey = searchkey;
                lsbrand = lsbrand.Where(b => b.Name.ToLower().Contains(searchkey.ToLower())).ToList();
            }
            IPagedList<Branch> models = lsbrand.AsQueryable().ToPagedList(pageNumber, pageSize);
            ViewBag.CurrentPage = pageNumber;
            return View(models);
        }

        // GET: Branch/Detail/Id
        public async Task<ActionResult> Details(string id)
        {
            if (id == null || _context.Branchs == null)
            {
                return HttpNotFound();
            }

            var Branch = await _context.Branchs
                .FirstOrDefaultAsync(m => m.Id == id);
            if (Branch == null)
            {
                return HttpNotFound();
            }

            return View(Branch);
        }

        // GET: Branch/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Branch/Create
        [HttpPost]
        
        public async Task<ActionResult> Create([Bind(Include = "Description,Id,Name,Code,Outstanding, CreatedAt,UpdatedAt,DeletedAt,UpdateUserId,CreateUserId")] Branch branch, HttpPostedFileBase fThumb)
        {
            if (ModelState.IsValid)
            {
				branch.Id = Guid.NewGuid().ToString();
				branch.Name = Utilities.ToTitleCase(branch.Name);
				branch.Code = Utilities.SEOUrl(branch.Code).ToUpper();
				if (fThumb != null)
				{
					string extension = Path.GetExtension(fThumb.FileName);
					branch.Image = Utilities.SEOUrl(branch.Name) + $"-{branch.Id}" + extension;
					await Utilities.UploadFile(fThumb, @"branch", branch.Image);
				}
				else branch.Image = "default.jpg";


				branch.CreateUserId = Session[Const.ADMINIDSESSION]?.ToString();
				branch.CreatedAt = DateTime.Now;
                _context.Branchs.Add(branch);
                await _context.SaveChangesAsync();
                TempData["success"] = "Thêm thượng hiệu thành công";
                return RedirectToAction(nameof(Index));
            }
            return View(branch);
        }

        // GET: Branch/Edit/Id
        public async Task<ActionResult> Edit(string id)
        {
            if (id == null || _context.Branchs == null)
            {
                return HttpNotFound();
            }

            var Branch = await _context.Branchs.FindAsync(id);
            if (Branch == null)
            {
                return HttpNotFound();
            }
            return View(Branch);
        }

        // POST: Branch/Edit/Id
        [HttpPost]
   
        public async Task<ActionResult> Edit(string id, [Bind(Include = "Description,Id,Code,Name,Outstanding,CreatedAt,UpdatedAt,DeletedAt,UpdateUserId,CreateUserId")] Branch branch, HttpPostedFileBase fThumb)
        {
            if (id != branch.Id.ToString())
            {
                return HttpNotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
					var branchUpd = _context.Branchs.FirstOrDefault(p => p.Id == id);
					branchUpd.Name = Utilities.ToTitleCase(branch.Name);
					branchUpd.Code = Utilities.SEOUrl(branch.Code).ToUpper();
					branchUpd.Description = branch.Description;
					branchUpd.Outstanding = branch.Outstanding;

					if (fThumb != null)
					{
						string pathOld = Path.Combine(HostingEnvironment.MapPath("~/Assets"), "images", "branch", branchUpd.Image ?? "");
						if (System.IO.File.Exists(pathOld))
						{
							System.IO.File.Delete(pathOld);
						}
						string extension = Path.GetExtension(fThumb.FileName);
						branchUpd.Image = Utilities.SEOUrl(branchUpd.Name) + $"-{branchUpd.Id}" + extension;
						await Utilities.UploadFile(fThumb, @"branch", branchUpd.Image);

					}
					branchUpd.UpdatedAt = DateTime.Now;
					branchUpd.UpdateUserId = Session[Const.ADMINIDSESSION]?.ToString();
                    _context.Branchs.Attach(branchUpd);
                    _context.Entry(branchUpd).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                    TempData["success"] = "Cập nhật thương hiệu thành công!";
                }
                catch (Exception ex)
                {
					throw ex;
				}
                return RedirectToAction(nameof(Index));
            }
            return View(branch);
        }

        // GET: Branch/Delete/Id
        public async Task<ActionResult> Delete(string id)
        {
            if (id == null || _context.Branchs == null)
            {
                return HttpNotFound();
            }

            var publisher = await _context.Branchs
                .FirstOrDefaultAsync(m => m.Id == id);
            if (publisher == null)
            {
                return HttpNotFound();
            }

            return View(publisher);
        }

        // POST: Branch/Delete/Id
        
        [HttpPost, ActionName("Delete")]
        
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            if (_context.Branchs == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Branch is null!");
            }
            var brand = _context.Branchs.Find(id);
            _context.Branchs.Remove(brand);
            await _context.SaveChangesAsync();
            TempData["success"] = "Xóa thương hiệu thành công";
            return RedirectToAction(nameof(Index));
        }

        private bool BranchExists(string id)
        {
            return _context.Branchs.Any(e => e.Id == id);
        }
    }
}
