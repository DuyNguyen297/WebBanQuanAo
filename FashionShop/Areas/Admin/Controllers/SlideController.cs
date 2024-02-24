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
using Slide = FashionShop.Models.Slide;
using FashionShop.Areas.Admin.Controllers.AuthorizeAttributeConfig;
using System.Web.Hosting;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TreeView;

namespace FashionShop.Areas.Admin.Controllers
{
    [Admin]
    public class SlideController : BaseController
    {
        // GET: Slide/Index
        public ActionResult Index(int? page, string searchkey = "")
        {
            var pageNumber = page == null || page <= 0 ? 1 : page.Value;
            var pageSize = 5;
            var lsslide = _context.Slides
                .AsNoTracking()
                .OrderByDescending(x => x.CreatedAt).ToList();
            if (!string.IsNullOrEmpty(searchkey))
            {
                ViewBag.SearchKey = searchkey;
                lsslide = lsslide.Where(b => b.Name.ToLower().Contains(searchkey.ToLower())).ToList();
            }
            IPagedList<Slide> models = lsslide.AsQueryable().ToPagedList(pageNumber, pageSize);
            ViewBag.CurrentPage = pageNumber;
            return View(models);
        }

        // GET: Slide/Detail/Id
        public async Task<ActionResult> Details(string id)
        {
            if (id == null || _context.Slides == null)
            {
                return HttpNotFound();
            }

            var Slide = await _context.Slides.Include(m => m.Account)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (Slide == null)
            {
                return HttpNotFound();
            }

            return View(Slide);
        }
        public void UpdateSeqNum(string slideId, int? newSeqNum)
        {
			var slideToUpdate = _context.Slides.SingleOrDefault(b => b.Id == slideId);

			if (slideToUpdate != null)
			{
				var slideToUpdates = _context.Slides
					.Where(b => b.SeqNum >= newSeqNum && b.Id != slideId)
					.OrderBy(b => b.SeqNum)
					.ToList();
                int? seqStart = newSeqNum;
				foreach (var slide in slideToUpdates)
				{
					slide.SeqNum = seqStart + 1;
					_context.Slides.Attach(slide);
					_context.Entry(slide).State = EntityState.Modified;
                    seqStart += 1;
				}


				// Lưu các thay đổi vào cơ sở dữ liệu
				_context.SaveChanges();
			}
		}
		// GET: Slide/Create
		public ActionResult Create()
        {
            return View();
        }

        // POST: Slide/Create
        [HttpPost]
        
        public async Task<ActionResult> Create([Bind(Include = "Id,Name,Title,Content,SeqNum,IsActive, CreatedAt,UpdatedAt,DeletedAt,UpdateUserId,CreateUserId")] Slide slide, HttpPostedFileBase fThumb)
        {
            if (ModelState.IsValid)
            {
				slide.Id = Guid.NewGuid().ToString();
				slide.Name = Utilities.ToTitleCase(slide.Name);
				if (fThumb != null)
				{
					string extension = Path.GetExtension(fThumb.FileName);
					slide.Image = Utilities.SEOUrl(slide.Name) + $"-{slide.Id}" + extension;
					await Utilities.UploadFile(fThumb, @"slide", slide.Image);
				}
				else slide.Image = "default.jpg";
				slide.CreateUserId = Session[Const.ADMINIDSESSION]?.ToString();
				slide.CreatedAt = DateTime.Now;
                _context.Slides.Add(slide);
                await _context.SaveChangesAsync();
                UpdateSeqNum(slide.Id, slide.SeqNum);
                TempData["success"] = "Thêm Slide thành công";
                return RedirectToAction(nameof(Index));
            }
            return View(slide);
        }

        // GET: Slide/Edit/Id
        public async Task<ActionResult> Edit(string id)
        {
            if (id == null || _context.Slides == null)
            {
                return HttpNotFound();
            }

            var Slide = await _context.Slides.FindAsync(id);
            if (Slide == null)
            {
                return HttpNotFound();
            }
            return View(Slide);
        }

        // POST: Slide/Edit/Id
        [HttpPost]
        
        public async Task<ActionResult> Edit(string id, [Bind(Include = "Id,Name,Title,Content,SeqNum,IsActive,CreatedAt,UpdatedAt,DeletedAt,UpdateUserId,CreateUserId")] Slide slide, HttpPostedFileBase fThumb)
        {
            if (id != slide.Id.ToString())
            {
                return HttpNotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
					var slideUpd = _context.Slides.FirstOrDefault(p => p.Id == id);
					slideUpd.Name = Utilities.ToTitleCase(slide.Name);
					slideUpd.Title = slide.Title;
					slideUpd.Content = slide.Content;
					slideUpd.SeqNum = slide.SeqNum;
					slideUpd.IsActive =slide.IsActive;

					if (fThumb != null)
					{
						string pathOld = Path.Combine(HostingEnvironment.MapPath("~/Assets"), "images", "slide", slideUpd.Image);
						if (System.IO.File.Exists(pathOld))
						{
							System.IO.File.Delete(pathOld);
						}
						string extension = Path.GetExtension(fThumb.FileName);
						slideUpd.Image = Utilities.SEOUrl(slideUpd.Name) + $"-{slideUpd.Id}" + extension;
						await Utilities.UploadFile(fThumb, @"slide", slideUpd.Image);

					}
					slideUpd.UpdatedAt = DateTime.Now;
					slideUpd.UpdateUserId = Session[Const.ADMINIDSESSION]?.ToString();
                    _context.Slides.Attach(slideUpd);
                    _context.Entry(slideUpd).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
					UpdateSeqNum(slideUpd.Id, slideUpd.SeqNum);
					TempData["success"] = "Cập nhật Slide thành công!";
                }
                catch (Exception ex)
                {
					throw ex;
				}
                return RedirectToAction(nameof(Index));
            }
            return View(slide);
        }

        // GET: Slide/Delete/Id
        public async Task<ActionResult> Delete(string id)
        {
            if (id == null || _context.Slides == null)
            {
                return HttpNotFound();
            }

            var publisher = await _context.Slides
                .FirstOrDefaultAsync(m => m.Id == id);
            if (publisher == null)
            {
                return HttpNotFound();
            }

            return View(publisher);
        }

        // POST: Slide/Delete/Id
        
        [HttpPost, ActionName("Delete")]
        
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            if (_context.Slides == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Slide is null!");
            }
            var slide = _context.Slides.Find(id);
            _context.Slides.Remove(slide);
            await _context.SaveChangesAsync();
			UpdateSeqNum(slide.Id, slide.SeqNum);
			TempData["success"] = "Xóa Slide thành công";
            return RedirectToAction(nameof(Index));
        }

        private bool SlideExists(string id)
        {
            return _context.Slides.Any(e => e.Id == id);
        }
    }
}
