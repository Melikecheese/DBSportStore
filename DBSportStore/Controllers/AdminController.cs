using System.Web.Mvc;
using DBSportStore.Filters;

namespace DBSportStore.Controllers
{
    [AdminAuthorize]
    public class AdminController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}