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
using System.Web.Mvc;
using System.Threading.Tasks;
using System.Web;
using FashionShop;
using FashionShop.Helpper;
using FashionShop.Models;
using FashionShop.Shared;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using PagedList;
using ClosedXML.Excel;
using FashionShop.ViewModel;
using FashionShop.Extension;
using Newtonsoft.Json;
using Microsoft.AspNet.Identity;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Web.UI;
using System.Diagnostics;
using FashionShop.Areas.Admin.Controllers.AuthorizeAttributeConfig;
using System.Data.Entity.Core;
using System.Net.Mail;

namespace FashionShop.Areas.Admin.Controllers
{
    public class AccountController : BaseController
    {
        // GET: Account/Login
        public ActionResult Login()
        {
            //ClaimsPrincipal claimUser = (ClaimsPrincipal)HttpContext.User;
            //if (claimUser.Identity.IsAuthenticated)
            //{
            //    return RedirectToAction("Index", "Dashboard");

            //}
            if (!string.IsNullOrEmpty(Session[Const.ADMINIDSESSION]?.ToString()))
            {
                return RedirectToAction("Index", "Dashboard");
            }
            return View();
        }
  
        [HttpPost]
        
        public ActionResult Login(LoginViewModel usr)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }
            string pass = usr.Password.ToMD5();
            try
            {
                var u = _context.Accounts.SingleOrDefault(m => m.UserName.ToString() == usr.UserName.ToString() && m.Password.ToString() == pass.ToString());
                if (u != null)
                {
                    string admin = JsonConvert.SerializeObject(u); //json object->string
                    Session[Const.ADMINSESSION] = admin;
                    Session[Const.ADMINNAMESESSION] = u.Name;
                    Session[Const.ADMINIDSESSION] = u.Id.ToString();
                    Session[Const.ROLE] = u.Role.ToString();

                    TempData["success"] = "Đăng nhập thành công";
                    return RedirectToAction("Index", "Dashboard");
                }
            }
            catch (EntityCommandExecutionException ex)
            {
                // Xử lý ngoại lệ và xem thông báo lỗi chi tiết
                var innerException = ex.InnerException;
                while (innerException != null)
                {
                    Console.WriteLine(innerException.Message);
                    innerException = innerException.InnerException;
                }

                // Nếu cần, bạn có thể throw hoặc xử lý ngoại lệ tại đây
            }

          
            TempData["error"] = "Tên tài khoản hoặc mật khẩu không chính xác!";
            return View();
        }


        // GET: Account/Logout

        public ActionResult Logout()
        {
            Session[Const.ADMINSESSION] = "";
            Session[Const.ADMINIDSESSION] = "";
            Session[Const.ROLE] = "";
            TempData["success"] = "Đăng xuất thành công";
            return RedirectToAction("Login", "Account");
        }


        // GET: Account/Index
        [Admin]
        public ActionResult Index(int? page, string role = "", string searchkey = "")
        {
            var pageNumber = page == null || page <= 0 ? 1 : page.Value;
            var pageSize = 8;
            /* string id = HttpContext.User.Claims.FirstOrDefault(c => c.ValueType == "Id").Value;*/
            string id = Session[Const.ADMINIDSESSION]?.ToString();
            List<Account> lsAccount = _context.Accounts.AsNoTracking().Where(x => x.Id != id).OrderBy(x => x.CreatedAt).ToList();
            if (role != "")
            {
                lsAccount = lsAccount.Where(a => a.Role == role).ToList();
            }
            if (!string.IsNullOrEmpty(searchkey))
            {
                ViewBag.SearchKey = searchkey;
                lsAccount = lsAccount.Where(a => a.Name.ToLower().Contains(searchkey.ToLower()) || a.UserName.ToLower().Contains(searchkey.ToLower())).ToList();
            }
            PagedList<Account> models = new PagedList<Account>(lsAccount.AsQueryable(), pageNumber, pageSize);
            ViewBag.CurrentPage = pageNumber;
            return View(models);
        }

        // GET: Profile
        [User]
        [Route("profile")]
        public async Task<ActionResult> Profile()
        {
            if (_context.Accounts == null)
            {
                return HttpNotFound();
            }
            var admin = new Account();
            try
            {
                admin = JsonConvert.DeserializeObject<Account>(Session[Const.ADMINSESSION].ToString());
            }
            catch(Exception ex)
            {
                var ex1 = ex;
            }
            

            if (admin == null)
            {
                return HttpNotFound();
            }
            return View(admin);
        }
		static string GenerateRandomString(int length)
		{
			const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
			Random random = new Random();
			return new string(Enumerable.Repeat(chars, length)
			  .Select(s => s[random.Next(s.Length)]).ToArray());
		}
		public ActionResult ResetPassword()
		{
			return View();
		}
		[HttpPost]
		public ActionResult ResetPassword(ResetPasswordModel model)
		{
            var account = _context.Accounts.FirstOrDefault(a => a.UserName == model.UserName);
            if(account== null)
            {
				TempData["error"] = "Tài khoản không tồn tại!";
                return View();
            }
			try
			{
				MailMessage mail = new MailMessage();
				mail.From = new MailAddress("195221742gm.uit.edu.vn");
				mail.To.Add("tannguyenlu1203@gmail.com");
				mail.Subject = "Cấp lại mật khẩu mới";
                string randPass = GenerateRandomString(6);
				string body = $@"<p>Xin chào,</p>
                        <p>Mật khẩu mới của bạn là:</p>
                        <p style='font-size=20px;font-weight:bold'>{randPass}</p>  
                        <p>Nếu bạn không yêu cầu cấp lại mật khẩu, vui lòng bỏ qua email này.</p>
                        <p>Trân trọng,</p>
                        <p>Đội ngũ hỗ trợ của chúng tôi</p>";

				mail.Body = body;
				mail.IsBodyHtml = true;
				mail.Priority = MailPriority.High;

				SmtpClient smtp = new SmtpClient();
				smtp.Host = "smtp.gmail.com";
				smtp.Port = 587;
				smtp.Credentials = new NetworkCredential("195221742gm.uit.edu.vn", "1119893666");
				smtp.EnableSsl = true;

				smtp.Send(mail);

                TempData["success"] = "Email sent successfully!";
			}
			catch (Exception ex)
			{
				TempData["error"] = $"Error: {ex.Message}";
			}

			return View();
		}
		// GET: Account/ChangePassword
		[User]
        public ActionResult ChangePassword()
        {
            ViewBag.UserName = JsonConvert.DeserializeObject<Account>(Session[Const.ADMINSESSION]?.ToString())?.UserName;
			return View();
		}
		[User]
		[HttpPost]
        public ActionResult ChangePassword(ChangePassword model)
        {
            try
            {
                string id = Session[Const.ADMINIDSESSION]?.ToString();
                if (ModelState.IsValid)
                {
					var taikhoan = _context.Accounts.Find(id);
					if (taikhoan == null)
                    {
						TempData["error"] = "Tài khoản không tồn tại";
						ViewBag.UserName = JsonConvert.DeserializeObject<Account>(Session[Const.ADMINSESSION]?.ToString())?.UserName;
						return View();
					}
                    else
                    {
						var pass = model.PasswordNow.Trim().ToMD5();
						if (taikhoan.Password == pass)
						{
							string passnew = model.Password.Trim().ToMD5();
							taikhoan.Password = passnew;
							_context.Accounts.Attach(taikhoan);
							_context.Entry(taikhoan).State = EntityState.Modified;
							_context.SaveChanges();
							TempData["success"] = "Đổi mật khẩu thành công";
							return RedirectToAction("Login", "Account");
						}
						else
						{
							TempData["error"] = "Mật khẩu hiện tại không đúng";
							ViewBag.UserName = JsonConvert.DeserializeObject<Account>(Session[Const.ADMINSESSION]?.ToString())?.UserName;
							return View();
						}				
                    }
                    
                }
                else
                {
					if (model.Password != model.ConfirmPassword)
					{
						TempData["error"] = "Xác nhận mật khẩu không khớp";
						ViewBag.UserName = JsonConvert.DeserializeObject<Account>(Session[Const.ADMINSESSION]?.ToString())?.UserName;
						return View();
					}

					TempData["error"] = "Thông tin nhập sai";
					ViewBag.UserName = JsonConvert.DeserializeObject<Account>(Session[Const.ADMINSESSION]?.ToString())?.UserName;
					return View();
				}
            }
            catch
            {
				TempData["error"] = "Thử lại";
				ViewBag.UserName = JsonConvert.DeserializeObject<Account>(Session[Const.ADMINSESSION]?.ToString())?.UserName;
				return View();
			}
        }

        // GET: Account/Create
        [Admin]
        public ActionResult Create()
        {
			ViewBag.Role = new SelectList(new List<SelectListItem>
			{
				new SelectListItem()
				{
					Value= "Admin",
					Text="Admin"
				},
				new SelectListItem()
				{
					Value= "User",
					Text="User"
				},

			}, "Value", "Text");
			return View();
        }

        // POST: Account/Create
        [Admin]
        [HttpPost]
        
        public async Task<ActionResult> Create([Bind(Include ="Name,Email,Phone,UserName,Password,Role,Id,CreatedAt,UpdatedAt,DeletedAt,UpdateUserId,CreateUserId")] Account account)
        {
			if (ModelState.IsValid)
            {

                if(_context.Accounts.Any(a => a.UserName == account.UserName))
				{
					TempData["error"] = "Tên đăng nhập đã tồn tại!";
					return View(account);
				}
				if (_context.Accounts.Any(a => a.Email == account.Email))
				{
					TempData["error"] = "Email đã được sử dụng!";
					return View(account);
				}
				account.Id = Guid.NewGuid().ToString();
                account.Password = account.Password.ToMD5();
                account.CreatedAt = DateTime.Now;
                account.CreateUserId = Session[Const.ADMINIDSESSION]?.ToString();
				_context.Accounts.Add(account);
                await _context.SaveChangesAsync();
                TempData["success"] = "Thêm tài khoản thành công!";
                return RedirectToAction("Index", "Account");
            }
			ViewBag.Role = new SelectList(new List<SelectListItem>
			{
				new SelectListItem()
				{
					Value= "Admin",
					Text="Admin"
				},
				new SelectListItem()
				{
					Value= "User",
					Text="User"
				},

			}, "Value", "Text", account.Role);
			return View(account);
        }

        // GET: Account/Detail/Id
        [Admin]
        public async Task<ActionResult> Details(string id)
        {
            if (id == null || _context.Accounts == null)
            {
                return HttpNotFound();
            }

            var account = await _context.Accounts.FirstOrDefaultAsync(m => m.Id == id);
            if (account == null)
            {
                return HttpNotFound();
            }

            return View(account);
        }

        // GET: Account/Edit/id
        [User]
        public async Task<ActionResult> Edit(string id)
        {
			if (id == null || _context.Accounts == null)
            {
                return HttpNotFound();
            }

            var account = await _context.Accounts.FindAsync(id);

            if (account == null)
            {
                return HttpNotFound();
            }
            if (Session[Const.ROLE].ToString() == Const.ADMIN.ToString())
            {
                if (account.Id == Session[Const.ADMINIDSESSION]?.ToString())
                {
                    ViewData["EditAccount"] = "EditProfileAdmin";
                }
                else
                {
                    ViewData["EditAccount"] = "EditAccountAdmin";
                }
            }
            else
            {
                ViewData["EditAccount"] = "EditProfileUser";
            }
			ViewBag.Role = new SelectList(new List<SelectListItem>
			{
				new SelectListItem()
				{
					Value= "Admin",
					Text="Admin"
				},
				new SelectListItem()
				{
					Value= "User",
					Text="User"
				},

			}, "Value", "Text", account.Role);
			return View(account);
        }
        [User]
        [HttpPost]
        public async Task<ActionResult> Edit(string id, [Bind(Include = "Name,Email,Phone,UserName,Role,Id,CreatedAt,UpdatedAt,DeletedAt,UpdateUserId,CreateUserId")] Account account)
        {
			if (id != account.Id.ToString())
			{
				return HttpNotFound();
			}
			if (ModelState.IsValid)
            {
				account.UpdatedAt = DateTime.Now;
				account.UpdateUserId = Session[Const.ADMINIDSESSION]?.ToString();
				_context.Accounts.Attach(account);
				_context.Entry(account).State = EntityState.Modified;
				TempData["success"] = "Đã cập nhật tài khoản thành công!";
				await _context.SaveChangesAsync();
				return RedirectToAction("Index", "Account");
            }
			ViewBag.Role = new SelectList(new List<SelectListItem>
			{
				new SelectListItem()
				{
					Value= "Admin",
					Text="Admin"
				},
				new SelectListItem()
				{
					Value= "User",
					Text="User"
				},

			}, "Value", "Text", account.Role);
			return View(account);
        }
        // GET: Account/Delete/Id
        [Admin]
        public async Task<ActionResult> Delete(string id)
        {
            if (id == null || _context.Accounts == null)
            {
                return HttpNotFound();
            }

			var account = await _context.Accounts
                .FirstOrDefaultAsync(m => m.Id == id);
            if (account == null)
            {
                return HttpNotFound();
            }
			ViewBag.Role = new SelectList(new List<SelectListItem>
			{
				new SelectListItem()
				{
					Value= "Admin",
					Text="Admin"
				},
				new SelectListItem()
				{
					Value= "User",
					Text="User"
				},

			}, "Value", "Text", account.Role);
			return View(account);
        }

        // POST: Account/Delete/Id
        [Admin]
        [HttpPost, ActionName("Delete")]
        
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            if (_context.Accounts == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Account is null!");

            }
            var account = _context.Accounts.Find(id);
            _context.Accounts.Remove(account);
            await _context.SaveChangesAsync();
            TempData["success"] = "Đã xóa tài khoản thành công!";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [User]
        public ActionResult ValidatePhone(Account acc)
        {
            try
            {
                var khachhang = _context.Accounts.AsNoTracking().SingleOrDefault(x => x.UserName.ToLower() == acc.UserName.ToLower());
                if (khachhang != null)
                    return Json(data: "Tên ĐN : " + acc.UserName + "đã được sử dụng");

                return Json(data: true);

            }
            catch
            {
                return Json(data: true);
            }
        }
        private bool AccountExists(string id)
        {
            return _context.Accounts.Any(e => e.Id.ToString() == id);
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
