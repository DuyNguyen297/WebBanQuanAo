using System.Web.Mvc;
using FashionShop;
using FashionShop.Helpper;
using FashionShop.Models;
using FashionShop.Shared;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using PagedList;
using ClosedXML.Excel;
using Newtonsoft.Json;
using Irony.Parsing;
using FashionShop.ViewModel;
using Watch.OnlinePayment;
using Newtonsoft.Json.Linq;
using System.Linq;
using System;
using FashionShop.Extension;
using Microsoft.Ajax.Utilities;
using System.Data.Entity;
using OfficeOpenXml;
using System.Threading.Tasks;

namespace FashionShop.Controllers
{
    public class HomeController : GlobalController
    {
        public ActionResult Index()
        {
			ViewBag.Slide = _context.Slides
                .Where(s => s.IsActive == true)
                .OrderBy(s => s.SeqNum)
                .ToList();
			ViewBag.Branch = _context.Branchs
                .Where(s => s.Outstanding == true)
                .ToList();
            ViewBag.Banner = _context.Banners
                .Where(s => s.IsActive == true)
                .OrderBy(s => s.SeqNum)
                .ToList();
            var productOutstandings = _context.Products
                .Where(s => s.Effective && s.Outstanding)
                .DistinctBy(p => p.Code)
                .ToList();
            productOutstandings.ForEach(p =>
            {
                var productIds = _context.Products.Where(pp => pp.Code == p.Code).Select(pp => pp.Id).ToList();
                var feedbacks = _context.Feedbacks.Where(f => productIds.Contains(f.ProductId)).ToList();
                p.Feedbacks = feedbacks;
            });
            ViewBag.ProductOutstanding = productOutstandings;
			return View();
        }

        public void addAddressData()
        {
			string excelFilePath = "D:\\dataprovince.xlsx";
			ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
			using (ExcelPackage package = new ExcelPackage(new System.IO.FileInfo(excelFilePath)))
            {
                // Lấy Sheet đầu tiên từ Excel
                ExcelWorksheet worksheet = package.Workbook.Worksheets[0];

                int rowCount = worksheet.Dimension.Rows;
                int colCount = worksheet.Dimension.Columns;
                for (int row = 2; row <= rowCount; row++)
                {
                    string provinceCode = worksheet.Cells[row, 2].Text;
                    string provinceName = worksheet.Cells[row, 1].Text;

                    string districtCode = worksheet.Cells[row, 4].Text;
                    string districtName = worksheet.Cells[row, 3].Text;

                    string wardCode = worksheet.Cells[row, 6].Text;
                    string wardName = worksheet.Cells[row, 5].Text;

                    AddOrUpdateProvince( provinceCode, provinceName);
                    AddOrUpdateDistrict( districtCode, districtName, provinceCode);
                    AddOrUpdateWard( wardCode, wardName, districtCode);
                }


            }
		}
		void AddOrUpdateProvince(string code, string name)
		{
			Province province = _context.Provinces.FirstOrDefault(p => p.Code == code);

			if (province == null)
			{
				_context.Provinces.Add(new Province { Id = Guid.NewGuid().ToString(), Code = code, Name = name });
			}
			_context.SaveChanges();
		}

		void AddOrUpdateDistrict(string code, string name, string provinceCode)
		{
			District district = _context.Districts.FirstOrDefault(d => d.Code == code);

			if (district == null)
			{
				Province province = _context.Provinces.FirstOrDefault(p => p.Code == provinceCode);
				_context.Districts.Add(new District { Id = Guid.NewGuid().ToString(), Code = code, Name = name, ProvinceId = province.Id });
			}
			_context.SaveChanges();
		}

		void AddOrUpdateWard(string code, string name, string districtCode)
		{
			Ward ward = _context.Wards.FirstOrDefault(w => w.Code == code);

			if (ward == null)
			{
				District district = _context.Districts.FirstOrDefault(d => d.Code == districtCode);
				_context.Wards.Add(new Ward { Id = Guid.NewGuid().ToString(), Code = code, Name = name, DistrictId = district.Id });
			}
			_context.SaveChanges();

		}
		public ActionResult About()
        {
            return View();
        }

        public ActionResult Contact()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Contact(Contact contact)
        {
            contact.Id = Guid.NewGuid().ToString();
            contact.CreatedAt = DateTime.Now;
            
            _context.Contacts.Add(contact);
            _context.SaveChanges();
            TempData["success"] = "Cảm ơn bạn đã gửi thông tin liên hệ đến chúng tôi";
            return View();
        }
        public ActionResult Login()
        {
            if (!string.IsNullOrEmpty(Session[Const.USERIDSESSION]?.ToString()))
            {
                return Redirect("~/");
            }
            ViewBag.Error = "";
            return View();
        }
        [HttpPost]
        public ActionResult Login(LoginViewModel login)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }
            string phone = login.UserName;
            string pass = login.Password.ToString().ToMD5();
            Customer acc = _context.Customers.FirstOrDefault(x => x.Phone == phone && x.Password != null);
            if (acc == null)
            {
                TempData["error"] = "Tài khoản không tồn tại";
            }
            else
            {
                if (acc.Password.Equals(pass))
                {
                    // đăng nhập đúng
                    string cus = JsonConvert.SerializeObject(acc); //json object->string

                    Session[Const.USERIDSESSION] = acc.Id.ToString();
                    Session[Const.USERNAMESESSION] = acc.Name.ToString();
                    Session[Const.USERPHONESESSION] = acc.Phone.ToString();
                    Session[Const.USERSESSION] = cus;
                    Session[Const.CARTSESSION] = acc.Id.ToString();

                    TempData["success"] = "Đăng nhập thành công!";
                    return Redirect("~/");
                }
                else
                {
                    TempData["error"] = "Mật khẩu không chính xác";
                }
            }
            return View();
        }
        public ActionResult Logout()
        {
            Session[Const.USERIDSESSION] =  "";
            Session[Const.USERSESSION] = "";
            Session[Const.CARTSESSION] = Guid.NewGuid().ToString();
            TempData["success"] = "Đăng xuất thành công";
            return Redirect("~/");
        }
        public ActionResult Register()
        {
            if (!string.IsNullOrEmpty(Session[Const.USERIDSESSION]?.ToString()))
            {
                return Redirect("~/");
            }
            return View();
        }
        [HttpPost]
        public ActionResult Register(RegisterModel register)
        {
			if (ModelState.IsValid)
            {
                var customer = new Customer()
                {
                    Name = register.Name,
                    Email = register.Email,
                    Phone = register.Phone,
                    Address = register.Address,
                    CreatedAt = DateTime.Now,
                };
				customer.Password = register.Password.ToMD5();
				Session[Const.CARTSESSION] = string.IsNullOrEmpty(Session[Const.CARTSESSION]?.ToString()) ? Guid.NewGuid().ToString() : Session[Const.CARTSESSION]?.ToString();
				customer.Id = Session[Const.CARTSESSION]?.ToString();
				var check_phone = _context.Customers.FirstOrDefault(x => x.Phone == register.Phone && x.Password != null);
				if (check_phone != null)
				{
					TempData["error"] = "Số điện thoại đã được đăng ký!";
					return View(register);
				}
				else
				{
                    var account = _context.Customers.FirstOrDefault(x => x.Phone == register.Phone);
                    if(account != null)
                    {
                        account.Name = customer.Name;
                        account.Email = customer.Email;
                        account.Phone = customer.Phone;
                        account.Address = customer.Address;
                        account.Password = customer.Password;
                        account.UpdatedAt = DateTime.Now;

						_context.Customers.Attach(account);
						_context.Entry(account).State = EntityState.Modified;
						_context.SaveChanges();
						TempData["info"] = "Số điện thoại đã được sử dụng trước đó!";
						TempData["subinfo"] = "Chúng tôi đã đồng bộ dữ liệu cho bạn";
						TempData["success"] = "Đăng kí thành công!";
						return RedirectToAction("Login", "Home");
					}
                    else
                    {
						_context.Customers.Add(customer);
						_context.SaveChanges();
						TempData["success"] = "Đăng ký thành công!";
						return RedirectToAction("Login", "Home");
					}
				}
			}
            else
            {
                if(register.Password != register.ConfirmPassword)
                {
					TempData["error"] = "Xác nhận mật khẩu không khớp";
					return View(register);
				}
				TempData["error"] = "Thông tin nhập sai. Vui lòng thử lại!";
				return View(register);
			}
			

        }
        [ActionName("Profile")]
        public ActionResult ProfileView()
        {
            if (string.IsNullOrEmpty(Session[Const.USERIDSESSION]?.ToString()))
            {
                return Redirect("~/");
            }
            Customer cus = JsonConvert.DeserializeObject<Customer>(Session[Const.USERSESSION]?.ToString());
            return View(cus);
        }

        /// <summary>
        /// Menu render ở đây
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpGet]
        public ActionResult GetMenu()
        {
            var categories = _context.Categories.ToList();
            var brands = _context.Branchs.ToList();
            var menu = new Menu()
            {
                Categories = categories,
                Branchs = brands,
            };
            return PartialView("_MenuBar", menu);
        }
        public ActionResult Error()
        {
            return View();
        }
    }

}

