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
using Banner = FashionShop.Models.Banner;
using FashionShop.Areas.Admin.Controllers.AuthorizeAttributeConfig;
using System.Web.Hosting;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TreeView;
using DocumentFormat.OpenXml.Presentation;

namespace FashionShop.Areas.Admin.Controllers
{
    [Admin]
    public class BannerController : BaseController
    {
        // GET: Banner/Index
        public ActionResult Index(int? page, string searchkey = "")
        {
            var pageNumber = page == null || page <= 0 ? 1 : page.Value;
            var pageSize = 5;
            var lsbanner = _context.Banners
                .AsNoTracking()
                .OrderByDescending(x => x.CreatedAt).ToList();
            if (!string.IsNullOrEmpty(searchkey))
            {
                ViewBag.SearchKey = searchkey;
                lsbanner = lsbanner.Where(b => b.Name.ToLower().Contains(searchkey.ToLower())).ToList();
            }
            IPagedList<Banner> models = lsbanner.AsQueryable().ToPagedList(pageNumber, pageSize);
            ViewBag.CurrentPage = pageNumber;
            return View(models);
        }

        // GET: Banner/Detail/Id
        public async Task<ActionResult> Details(string id)
        {
            if (id == null || _context.Banners == null)
            {
                return HttpNotFound();
            }

            var Banner = await _context.Banners.Include(m => m.Account)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (Banner == null)
            {
                return HttpNotFound();
            }

            return View(Banner);
        }
        public void UpdateSeqNum(string bannerId, int? newSeqNum)
        {
			var bannerToUpdate = _context.Banners.SingleOrDefault(b => b.Id == bannerId);

			if (bannerToUpdate != null)
			{
                var bannerToUpdates = _context.Banners
					.Where(b => b.SeqNum >= newSeqNum && b.Id != bannerId)
					.OrderBy(b => b.SeqNum)
					.ToList();
                int? seqStart = newSeqNum;
				foreach (var banner in bannerToUpdates)
				{
					banner.SeqNum = seqStart + 1;
					_context.Banners.Attach(banner);
					_context.Entry(banner).State = EntityState.Modified;
                    seqStart += 1;
				}


				// Lưu các thay đổi vào cơ sở dữ liệu
				_context.SaveChanges();
			}
		}
		// GET: Banner/Create
		public ActionResult Create()
        {
            return View();
        }

        // POST: Banner/Create
        [HttpPost]
        
        public async Task<ActionResult> Create([Bind(Include = "Id,Name,Title,Content,SeqNum,IsActive, CreatedAt,UpdatedAt,DeletedAt,UpdateUserId,CreateUserId")] Banner banner, HttpPostedFileBase fThumb)
        {
            if (ModelState.IsValid)
            {
				banner.Id = Guid.NewGuid().ToString();
				banner.Name = Utilities.ToTitleCase(banner.Name);
				if (fThumb != null)
				{
					string extension = Path.GetExtension(fThumb.FileName);
					banner.Image = Utilities.SEOUrl(banner.Name) + $"-{banner.Id}" + extension;
					await Utilities.UploadFile(fThumb, @"banner", banner.Image);
				}
				else banner.Image = "default.jpg";
				banner.CreateUserId = Session[Const.ADMINIDSESSION]?.ToString();
				banner.CreatedAt = DateTime.Now;
                _context.Banners.Add(banner);
                await _context.SaveChangesAsync();
                UpdateSeqNum(banner.Id, banner.SeqNum);
                TempData["success"] = "Thêm Banner thành công";
                return RedirectToAction(nameof(Index));
            }
            return View(banner);
        }

        // GET: Banner/Edit/Id
        public async Task<ActionResult> Edit(string id)
        {
            if (id == null || _context.Banners == null)
            {
                return HttpNotFound();
            }

            var Banner = await _context.Banners.FindAsync(id);
            if (Banner == null)
            {
                return HttpNotFound();
            }
            return View(Banner);
        }

        // POST: Banner/Edit/Id
        [HttpPost]
        
        public async Task<ActionResult> Edit(string id, [Bind(Include = "Id,Name,Title,Content,SeqNum,IsActive,CreatedAt,UpdatedAt,DeletedAt,UpdateUserId,CreateUserId")] Banner banner, HttpPostedFileBase fThumb)
        {
            if (id != banner.Id.ToString())
            {
                return HttpNotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
					var bannerUpd = _context.Banners.FirstOrDefault(p => p.Id == id);
					bannerUpd.Name = Utilities.ToTitleCase(banner.Name);
					bannerUpd.Title = banner.Title;
					bannerUpd.Content = banner.Content;
					bannerUpd.SeqNum = banner.SeqNum;
					bannerUpd.IsActive = banner.IsActive;

					if (fThumb != null)
					{
						string pathOld = Path.Combine(HostingEnvironment.MapPath("~/Assets"), "images", "banner", bannerUpd.Image);
						if (System.IO.File.Exists(pathOld))
						{
							System.IO.File.Delete(pathOld);
						}
						string extension = Path.GetExtension(fThumb.FileName);
						bannerUpd.Image = Utilities.SEOUrl(bannerUpd.Name) + $"-{bannerUpd.Id}" + extension;
						await Utilities.UploadFile(fThumb, @"banner", bannerUpd.Image);

					}
					bannerUpd.UpdatedAt = DateTime.Now;
					bannerUpd.UpdateUserId = Session[Const.ADMINIDSESSION]?.ToString();
                    _context.Banners.Attach(bannerUpd);
                    _context.Entry(bannerUpd).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
					UpdateSeqNum(bannerUpd.Id, bannerUpd.SeqNum);
					TempData["success"] = "Cập nhật Banner thành công!";
                }
                catch (Exception ex)
                {
					throw ex;
				}
                return RedirectToAction(nameof(Index));
            }
            return View(banner);
        }

        // GET: Banner/Delete/Id
        public async Task<ActionResult> Delete(string id)
        {
            if (id == null || _context.Banners == null)
            {
                return HttpNotFound();
            }

            var publisher = await _context.Banners
                .FirstOrDefaultAsync(m => m.Id == id);
            if (publisher == null)
            {
                return HttpNotFound();
            }

            return View(publisher);
        }

        // POST: Banner/Delete/Id
        
        [HttpPost, ActionName("Delete")]
        
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            if (_context.Banners == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Banner is null!");
            }
            var banner = _context.Banners.Find(id);
            _context.Banners.Remove(banner);
            await _context.SaveChangesAsync();
			UpdateSeqNum(banner.Id, banner.SeqNum);
			TempData["success"] = "Xóa Banner thành công";
            return RedirectToAction(nameof(Index));
        }

        private bool BannerExists(string id)
        {
            return _context.Banners.Any(e => e.Id == id);
        }
    }
}
