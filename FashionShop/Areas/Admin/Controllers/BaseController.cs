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
using System.Web.Hosting;

namespace FashionShop.Areas.Admin.Controllers
{
    public class BaseController : Controller
    {
        public readonly AppDbContext _context;
        public readonly HostingEnvironment _hostingEnvironment;
        public BaseController()
        {
            _context = new AppDbContext();
            if (System.Web.HttpContext.Current.Session[Const.ADMINIDSESSION].Equals(""))
            {
                //System.Web.HttpContext.Current.Response.Redirect("~/Admin/Login");
              
            }
        }
    }
}