using System;
using System.Linq;

namespace DBSportStore.Models
{
    // Chỉ dùng trong môi trường thực hành để tạo tài khoản Admin mẫu.
    // Sau khi tạo xong, không gọi lại helper này trên môi trường triển khai thật.
    public static class AdminSeedHelper
    {
        public static void CreateAdminForLab()
        {
            using (var db = new DBSportStoreEntities())
            {
                Role adminRole = db.Roles.FirstOrDefault(r => r.RoleName == "Admin");
                if (adminRole == null)
                {
                    adminRole = new Role { RoleName = "Admin" };
                    db.Roles.Add(adminRole);
                    db.SaveChanges();
                }

                if (db.UserAccounts.Any(u => u.UserName == "admin01"))
                    return;

                string salt = PasswordHelper.CreateSalt();
                db.UserAccounts.Add(new UserAccount
                {
                    UserName = "admin01",
                    PasswordSalt = salt,
                    PasswordHash = PasswordHelper.Hash("Admin123", salt),
                    RoleID = adminRole.RoleID,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                });
                db.SaveChanges();
            }
        }
    }
}