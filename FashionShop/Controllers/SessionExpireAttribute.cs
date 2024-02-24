using FashionShop.Shared;
using System;
using System.Web;
using System.Web.Mvc;

namespace FashionShop.Controllers
{
    public class SessionExpireAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // Kiểm tra session ở đây
            if (String.IsNullOrEmpty((string)System.Web.HttpContext.Current.Session[Const.CARTSESSION]))
            {
                System.Web.HttpContext.Current.Session[Const.CARTSESSION] = Guid.NewGuid().ToString();
                return;
            }
            base.OnActionExecuting(filterContext);
        }
    }
}
