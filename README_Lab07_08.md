# SportStore MVC5 - Lab 07 & Lab 08

## Cập nhật database trước khi chạy

Mở SQL Server Management Studio hoặc SQL Server Object Explorer, chọn database `DBSportStore`, mở file:

`DBSportStore/App_Data/Lab07_08_Update.sql`

Chạy toàn bộ script một lần. Script có thể chạy lại; nó bổ sung hai cột `RecipientName`, `ShippingPhone` cho `OrderPro` nếu còn thiếu và tạo hai Role `Admin`, `Customer` nếu chưa có.

## Chạy project

1. Mở `DBSportStore.sln` bằng Visual Studio 2022.
2. Restore NuGet packages.
3. Build Solution.
4. Chạy website tại `/Store/Index`.

Connection string hiện dùng LocalDB: `(localdb)\MSSQLLocalDB`, database `DBSportStore`.

## Tài khoản Admin thực hành

Khi chạy cấu hình Debug, `AdminSeedHelper` tạo tài khoản mẫu nếu chưa tồn tại:

- Username: `admin01`
- Password: `Admin123`

Không dùng tài khoản mẫu này khi triển khai thật. Helper chỉ được gọi tự động trong Debug.

## Kiểm tra Lab 07

1. Mở Trang chủ, chọn Mua ở hai sản phẩm.
2. Mở Giỏ hàng, cập nhật số lượng và xóa sản phẩm.
3. Checkout khi chưa đăng nhập.
4. Kiểm tra bảng `Customer` có Customer mới với `UserID = NULL`.
5. Kiểm tra `OrderPro`, `OrderDetail` và tồn kho `Product.StockQuantity`.
6. Kiểm tra trang OrderSuccess và giỏ hàng đã được xóa sau khi lưu đơn thành công.

## Kiểm tra Lab 08

1. Đăng ký tài khoản Customer tại `/Account/SignUp`.
2. Kiểm tra `UserAccount.PasswordHash`, `PasswordSalt` và Customer được liên kết bằng `UserID`.
3. Đăng nhập Customer, thêm sản phẩm và Checkout; hệ thống dùng Customer hiện tại, không tạo Customer mới.
4. Đăng xuất; giỏ hàng Session vẫn được giữ.
5. Đăng nhập Admin bằng tài khoản thực hành; thử truy cập quản trị Product/Category.
6. Thử truy cập Product Details khi chưa đăng nhập; trang chi tiết vẫn công khai.

## Lưu ý

Mã nguồn đã được cập nhật trên GitHub. Cần chạy script database và Build/Run trên máy có Visual Studio + SQL Server để xác nhận tích hợp với database thực tế.