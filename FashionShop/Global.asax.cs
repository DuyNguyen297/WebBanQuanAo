using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using FashionShop.Shared;

namespace FashionShop
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
        protected void Session_Start()
        {
            Session[Const.ADMINSESSION] = ""; // Supper Admin
            Session[Const.ADMINIDSESSION] = ""; // Supper Admin
            Session[Const.ROLE] = ""; // Supper Admin

            Session[Const.USERSESSION] = ""; // Customer
            Session[Const.USERIDSESSION] = ""; // Customer

            Session[Const.CARTSESSION] = Guid.NewGuid().ToString(); //id cho khách lạ
        }
    }
}
