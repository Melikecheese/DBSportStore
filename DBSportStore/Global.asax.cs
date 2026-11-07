using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using DBSportStore.Models;

namespace DBSportStore
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
#if DEBUG
            // Tạo tài khoản admin mẫu cho bài thực hành, chỉ khi chạy Debug.
            AdminSeedHelper.CreateAdminForLab();
#endif
        }
    }
}
