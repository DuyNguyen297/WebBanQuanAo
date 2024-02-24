using System.Web.Mvc;

namespace FashionShop.Areas.Admin
{
    public class AdminAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Admin";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
               "AdminLogin",
               "Admin/login",
               new { Controller = "Account", action = "Login", id = UrlParameter.Optional }
            );
            context.MapRoute(
               "Admin_dashboard",
               "Admin",
               new { controller = "Dashboard", action = "Index", id = UrlParameter.Optional }
           );
            context.MapRoute(
                "Admin_default",
                    "Admin/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}