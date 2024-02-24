using DocumentFormat.OpenXml.Drawing;
using System;
using System.Web;
using System.Web.Optimization;
using static System.Windows.Forms.LinkLabel;

namespace FashionShop
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-3.4.1.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at https://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.bundle.min.js"
                      ));
			bundles.Add(new ScriptBundle("~/bundles/toastr").Include(
		              "~/Scripts/toastr.js"
		              ));
            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/lib/bootstrap/css/bootstrap.css",
                      "~/Content/lib/bootstrap/css/bootstrap.min.css",

                      "~/Content/css/site.css",

                      "~/Content/css/PagedList.css",

                      "~/Content/lib/bootstrap/css/toastr.css",

                      "~/Content/lib/font-awesome/css/all.min.css",
                      "~/Content/lib/font-awesome/css/all.css"
                      ));

            bundles.Add(new StyleBundle("~/Content/css5").Include(
                      "~/Content/lib/bootstrap5/css/bootstrap.css",
                      "~/Content/lib/bootstrap5/css/bootstrap.min.css"
                      ));
		}
    }
}
