/*
    SportStore - cập nhật cấu trúc phục vụ Lab 07-08.
    Chạy script này trên database DBSportStore trước khi chạy project.
*/
IF COL_LENGTH('dbo.OrderPro', 'RecipientName') IS NULL
    ALTER TABLE dbo.OrderPro ADD RecipientName NVARCHAR(100) NULL;
GO

IF COL_LENGTH('dbo.OrderPro', 'ShippingPhone') IS NULL
    ALTER TABLE dbo.OrderPro ADD ShippingPhone NVARCHAR(20) NULL;
GO

-- Bổ sung dữ liệu cho các đơn hàng đã tồn tại trước khi chuyển sang NOT NULL.
UPDATE op
SET RecipientName = COALESCE(NULLIF(op.RecipientName, N''), c.FullName),
    ShippingPhone = COALESCE(op.ShippingPhone, c.Phone, N'')
FROM dbo.OrderPro AS op
INNER JOIN dbo.Customer AS c ON c.CustomerID = op.CustomerID;
GO

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.OrderPro')
           AND name = 'RecipientName' AND is_nullable = 1)
    ALTER TABLE dbo.OrderPro ALTER COLUMN RecipientName NVARCHAR(100) NOT NULL;
GO

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.OrderPro')
           AND name = 'ShippingPhone' AND is_nullable = 1)
    ALTER TABLE dbo.OrderPro ALTER COLUMN ShippingPhone NVARCHAR(20) NOT NULL;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Role WHERE RoleName = N'Admin')
    INSERT INTO dbo.Role(RoleName) VALUES (N'Admin');
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Role WHERE RoleName = N'Customer')
    INSERT INTO dbo.Role(RoleName) VALUES (N'Customer');
GO
