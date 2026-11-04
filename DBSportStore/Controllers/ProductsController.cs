using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using DBSportStore.Models;

namespace DBSportStore.Controllers
{
    public class ProductsController : Controller
    {
        private readonly DBSportStoreEntities db = new DBSportStoreEntities();
        private const int MaxImageSize = 2 * 1024 * 1024;
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png" };

        public ActionResult Index()
        {
            return View(db.Products.Include(p => p.Category).ToList());
        }

        public ActionResult Details(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            Product product = db.Products.Include(p => p.Category).FirstOrDefault(p => p.ProductID == id.Value);
            if (product == null) return HttpNotFound();
            return View(product);
        }

        public ActionResult Create()
        {
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName");
            return View(new Product { IsActive = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ProductName,Description,CategoryID,Price,StockQuantity,IsActive")] Product product, HttpPostedFileBase imageFile)
        {
            string imageName = null;
            if (imageFile != null && imageFile.ContentLength > 0)
            {
                imageName = SaveImage(imageFile);
                if (imageName == null) ModelState.AddModelError("ImageUrl", "Ảnh chỉ được là JPG, JPEG hoặc PNG và dung lượng tối đa 2 MB.");
            }

            if (ModelState.IsValid)
            {
                product.ImageUrl = imageName;
                db.Products.Add(product);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            // Nếu dữ liệu khác không hợp lệ, xóa ảnh vừa tải để tránh file mồ côi.
            if (!string.IsNullOrEmpty(imageName)) DeleteImageFile(imageName);
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName", product.CategoryID);
            return View(product);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            Product product = db.Products.Find(id);
            if (product == null) return HttpNotFound();
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName", product.CategoryID);
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ProductID,ProductName,Description,CategoryID,Price,StockQuantity,IsActive")] Product product, HttpPostedFileBase imageFile)
        {
            Product existing = db.Products.Find(product.ProductID);
            if (existing == null) return HttpNotFound();

            string newImageName = null;
            if (imageFile != null && imageFile.ContentLength > 0)
            {
                newImageName = SaveImage(imageFile);
                if (newImageName == null) ModelState.AddModelError("ImageUrl", "Ảnh chỉ được là JPG, JPEG hoặc PNG và dung lượng tối đa 2 MB.");
            }

            if (ModelState.IsValid)
            {
                existing.ProductName = product.ProductName;
                existing.Description = product.Description;
                existing.CategoryID = product.CategoryID;
                existing.Price = product.Price;
                existing.StockQuantity = product.StockQuantity;
                existing.IsActive = product.IsActive;
                // Không chọn ảnh mới thì giữ nguyên ImageUrl cũ.
                if (!string.IsNullOrEmpty(newImageName)) existing.ImageUrl = newImageName;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            if (!string.IsNullOrEmpty(newImageName)) DeleteImageFile(newImageName);
            product.ImageUrl = existing.ImageUrl;
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName", product.CategoryID);
            return View(product);
        }

        public ActionResult Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            Product product = db.Products.Include(p => p.Category).FirstOrDefault(p => p.ProductID == id.Value);
            if (product == null) return HttpNotFound();
            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Product product = db.Products.Find(id);
            if (product == null) return HttpNotFound();
            db.Products.Remove(product);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        private string SaveImage(HttpPostedFileBase imageFile)
        {
            string extension = Path.GetExtension(Path.GetFileName(imageFile.FileName));
            if (string.IsNullOrEmpty(extension) || !AllowedExtensions.Contains(extension.ToLowerInvariant())) return null;
            if (imageFile.ContentLength > MaxImageSize) return null;

            string folder = Server.MapPath("~/Content/images");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            string baseName = Path.GetFileNameWithoutExtension(imageFile.FileName);
            // Loại ký tự không hợp lệ để dùng an toàn làm tên file.
            foreach (char c in Path.GetInvalidFileNameChars()) baseName = baseName.Replace(c, '_');
            if (string.IsNullOrWhiteSpace(baseName)) baseName = "image";
            string fileName = baseName + extension.ToLowerInvariant();
            string path = Path.Combine(folder, fileName);
            int suffix = 1;
            while (System.IO.File.Exists(path))
            {
                fileName = baseName + "-" + suffix + extension.ToLowerInvariant();
                path = Path.Combine(folder, fileName);
                suffix++;
            }
            imageFile.SaveAs(path);
            return fileName;
        }

        private void DeleteImageFile(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName)) return;
            string path = Path.Combine(Server.MapPath("~/Content/images"), Path.GetFileName(fileName));
            if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
