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
using Newtonsoft.Json;
using Irony.Parsing;
using FashionShop.ViewModel;
using Watch.OnlinePayment;
using Newtonsoft.Json.Linq;

namespace FashionShop.Controllers
{
    [SessionExpire]
    public class GlobalController : Controller
    {
        public readonly AppDbContext _context;

        public GlobalController()
        {
            _context = new AppDbContext();    
        }

    }
}
