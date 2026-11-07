using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using DBSportStore.Models;

namespace DBSportStore.Controllers
{
    public class ShoppingCartController : Controller
    {
        private readonly DBSportStoreEntities db = new DBSportStoreEntities();

        private Cart GetCart()
        {
            Cart cart = Session["Cart"] as Cart;
            if (cart == null)
            {
                cart = new Cart();
                Session["Cart"] = cart;
            }
            return cart;
        }

        [ChildActionOnly]
        public ActionResult BagCart()
        {
            return PartialView("BagCart", GetCart());
        }

        public ActionResult ShowCart()
        {
            return View(GetCart());
        }

        public ActionResult AddToCart(int id)
        {
            Product product = db.Products.FirstOrDefault(p => p.ProductID == id && p.IsActive);
            if (product == null)
            {
                TempData["CartError"] = "Sản phẩm không tồn tại hoặc đã ngừng bán.";
                return RedirectToAction("Index", "Store");
            }

            if (!GetCart().Add(product))
                TempData["CartError"] = "Không thể thêm sản phẩm. Số lượng đã đạt giới hạn tồn kho.";

            return RedirectToAction("ShowCart");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateQuantity(int productId, int quantity)
        {
            Product product = db.Products.Find(productId);
            if (product == null || !product.IsActive)
                TempData["CartError"] = "Sản phẩm không còn tồn tại hoặc đã ngừng bán.";
            else if (!GetCart().Update(productId, quantity, product.StockQuantity))
                TempData["CartError"] = "Số lượng phải lớn hơn 0 và không vượt quá tồn kho hiện tại.";
            else
                TempData["CartSuccess"] = "Đã cập nhật số lượng.";

            return RedirectToAction("ShowCart");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RemoveFromCart(int productId)
        {
            GetCart().Remove(productId);
            return RedirectToAction("ShowCart");
        }

        [HttpGet]
        public ActionResult Checkout()
        {
            Cart cart = GetCart();
            if (!cart.Items.Any())
            {
                TempData["CartError"] = "Giỏ hàng đang trống.";
                return RedirectToAction("ShowCart");
            }

            CheckoutViewModel model = new CheckoutViewModel
            {
                CartItems = cart.Items,
                TotalAmount = cart.TotalMoney()
            };

            if (Session["CustomerID"] != null)
            {
                int customerId = Convert.ToInt32(Session["CustomerID"]);
                int userId = Convert.ToInt32(Session["UserID"]);
                Customer customer = db.Customers.FirstOrDefault(c =>
                    c.CustomerID == customerId && c.UserID == userId);

                if (customer != null)
                {
                    model.FullName = customer.FullName;
                    model.Phone = customer.Phone;
                    model.Email = customer.Email;
                    model.Address = customer.Address;
                    model.ShippingAddress = customer.Address;
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Checkout(CheckoutViewModel model)
        {
            Cart cart = GetCart();
            model.CartItems = cart.Items;
            model.TotalAmount = cart.TotalMoney();

            if (!cart.Items.Any())
            {
                TempData["CartError"] = "Giỏ hàng đang trống.";
                return RedirectToAction("ShowCart");
            }

            if (!ModelState.IsValid)
                return View(model);

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    // Đọc lại tồn kho từ database để tránh dùng số liệu cũ trong Session.
                    foreach (CartItem item in cart.Items)
                    {
                        Product current = db.Products.Find(item.ProductID);
                        if (current == null || !current.IsActive ||
                            item.Quantity <= 0 || item.Quantity > current.StockQuantity)
                        {
                            ModelState.AddModelError("", "Sản phẩm " + item.ProductName +
                                " không còn đủ số lượng trong kho. Vui lòng cập nhật giỏ hàng.");
                            transaction.Rollback();
                            return View(model);
                        }
                    }

                    Customer customer;
                    if (Session["UserID"] != null && Session["CustomerID"] != null)
                    {
                        int userId = Convert.ToInt32(Session["UserID"]);
                        int customerId = Convert.ToInt32(Session["CustomerID"]);
                        customer = db.Customers.FirstOrDefault(c =>
                            c.CustomerID == customerId && c.UserID == userId);

                        if (customer == null)
                        {
                            ModelState.AddModelError("", "Không tìm thấy hồ sơ khách hàng đang đăng nhập.");
                            transaction.Rollback();
                            return View(model);
                        }
                    }
                    else
                    {
                        // Guest luôn tạo Customer mới, UserID để NULL.
                        customer = new Customer
                        {
                            UserID = null,
                            FullName = model.FullName,
                            Phone = model.Phone,
                            Email = model.Email,
                            Address = model.Address
                        };
                        db.Customers.Add(customer);
                        db.SaveChanges();
                    }

                    // Lưu snapshot người nhận tại thời điểm đặt hàng.
                    OrderPro order = new OrderPro
                    {
                        OrderDate = DateTime.Now,
                        CustomerID = customer.CustomerID,
                        RecipientName = model.FullName,
                        ShippingPhone = model.Phone,
                        ShippingAddress = model.ShippingAddress,
                        TotalAmount = cart.TotalMoney(),
                        Status = "Chờ xử lý"
                    };

                    db.OrderProes.Add(order);
                    db.SaveChanges();

                    foreach (CartItem item in cart.Items)
                    {
                        Product product = db.Products.Find(item.ProductID);
                        db.OrderDetails.Add(new OrderDetail
                        {
                            OrderID = order.OrderID,
                            ProductID = item.ProductID,
                            Quantity = item.Quantity,
                            UnitPrice = item.UnitPrice
                        });

                        product.StockQuantity -= item.Quantity;
                    }

                    db.SaveChanges();
                    transaction.Commit();

                    // Chỉ xóa Cart sau khi toàn bộ transaction đã commit thành công.
                    cart.Clear();
                    return RedirectToAction("OrderSuccess", new { id = order.OrderID });
                }
                catch
                {
                    transaction.Rollback();
                    ModelState.AddModelError("", "Không thể lưu đơn hàng. Giỏ hàng vẫn được giữ lại, vui lòng thử lại.");
                    return View(model);
                }
            }
        }

        public ActionResult OrderSuccess(int id)
        {
            OrderPro order = db.OrderProes
                .Include(o => o.Customer)
                .Include(o => o.OrderDetails.Select(d => d.Product))
                .FirstOrDefault(o => o.OrderID == id);

            if (order == null)
                return HttpNotFound();

            return View(order);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}