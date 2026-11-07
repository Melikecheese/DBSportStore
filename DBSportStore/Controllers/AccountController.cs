using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using DBSportStore.Filters;
using DBSportStore.Models;

namespace DBSportStore.Controllers
{
    public class AccountController : Controller
    {
        private readonly DBSportStoreEntities db = new DBSportStoreEntities();

        [HttpGet]
        public ActionResult SignUp()
        {
            if (Session["UserID"] != null)
                return RedirectToAction("Index", "Store");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SignUp(SignUpViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (db.UserAccounts.Any(u => u.UserName == model.UserName))
            {
                ModelState.AddModelError("UserName", "Tên đăng nhập đã tồn tại.");
                return View(model);
            }

            Role customerRole = db.Roles.FirstOrDefault(r => r.RoleName == "Customer");
            if (customerRole == null)
            {
                ModelState.AddModelError("", "Không tìm thấy Role Customer. Hãy chạy script cập nhật database.");
                return View(model);
            }

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    string salt = PasswordHelper.CreateSalt();
                    UserAccount account = new UserAccount
                    {
                        UserName = model.UserName.Trim(),
                        PasswordSalt = salt,
                        PasswordHash = PasswordHelper.Hash(model.Password, salt),
                        RoleID = customerRole.RoleID,
                        IsActive = true,
                        CreatedAt = DateTime.Now
                    };

                    db.UserAccounts.Add(account);
                    db.SaveChanges();

                    Customer customer = new Customer
                    {
                        UserID = account.UserID,
                        FullName = model.FullName,
                        Phone = model.Phone,
                        Email = model.Email,
                        Address = model.Address
                    };

                    db.Customers.Add(customer);
                    db.SaveChanges();
                    transaction.Commit();

                    TempData["SignUpSuccess"] = "Đăng ký thành công. Vui lòng đăng nhập.";
                    return RedirectToAction("Login");
                }
                catch
                {
                    transaction.Rollback();
                    ModelState.AddModelError("", "Có lỗi khi tạo tài khoản. Vui lòng thử lại.");
                    return View(model);
                }
            }
        }

        [HttpGet]
        public ActionResult Login()
        {
            if (Session["UserID"] != null)
                return RedirectToAction("Index", "Store");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            UserAccount account = db.UserAccounts
                .Include(u => u.Role)
                .FirstOrDefault(u => u.UserName == model.UserName && u.IsActive);

            if (account == null ||
                !PasswordHelper.Verify(model.Password, account.PasswordSalt, account.PasswordHash))
            {
                ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng.");
                return View(model);
            }

            // Không dùng Session.Clear(): giỏ hàng Session["Cart"] phải được giữ nguyên.
            Session.Remove("UserID");
            Session.Remove("UserName");
            Session.Remove("Role");
            Session.Remove("CustomerID");
            Session.Remove("FullName");

            Session["UserID"] = account.UserID;
            Session["UserName"] = account.UserName;
            Session["Role"] = account.Role.RoleName;

            Customer customer = db.Customers.FirstOrDefault(c => c.UserID == account.UserID);
            if (customer != null)
            {
                Session["CustomerID"] = customer.CustomerID;
                Session["FullName"] = customer.FullName;
            }
            else
            {
                Session["FullName"] = account.UserName;
            }

            if (account.Role.RoleName == "Admin")
                return RedirectToAction("AdminTest");

            return RedirectToAction("Index", "Store");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            // Chỉ xóa thông tin tài khoản, không xóa Session["Cart"].
            Session.Remove("UserID");
            Session.Remove("UserName");
            Session.Remove("Role");
            Session.Remove("CustomerID");
            Session.Remove("FullName");

            return RedirectToAction("Index", "Store");
        }

        [AdminAuthorize]
        public ActionResult AdminTest()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}