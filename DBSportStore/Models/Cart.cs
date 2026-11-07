using System.Collections.Generic;
using System.Linq;

namespace DBSportStore.Models
{
    public class CartItem
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public string ImageUrl { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public int StockQuantity { get; set; }
        public decimal LineTotal { get { return UnitPrice * Quantity; } }
    }

    public class Cart
    {
        public List<CartItem> Items { get; private set; }

        public Cart()
        {
            Items = new List<CartItem>();
        }

        public bool Add(Product product)
        {
            if (product == null || !product.IsActive || product.StockQuantity <= 0)
                return false;

            CartItem item = Items.FirstOrDefault(x => x.ProductID == product.ProductID);
            if (item != null)
            {
                if (item.Quantity >= product.StockQuantity)
                    return false;

                item.Quantity++;
                item.StockQuantity = product.StockQuantity;
                return true;
            }

            Items.Add(new CartItem
            {
                ProductID = product.ProductID,
                ProductName = product.ProductName,
                ImageUrl = product.ImageUrl,
                UnitPrice = product.Price,
                Quantity = 1,
                StockQuantity = product.StockQuantity
            });
            return true;
        }

        public bool Update(int productId, int quantity, int currentStock)
        {
            CartItem item = Items.FirstOrDefault(x => x.ProductID == productId);
            if (item == null || quantity <= 0 || quantity > currentStock)
                return false;

            item.Quantity = quantity;
            item.StockQuantity = currentStock;
            return true;
        }

        public void Remove(int productId)
        {
            Items.RemoveAll(x => x.ProductID == productId);
        }

        public int TotalQuantity()
        {
            return Items.Sum(x => x.Quantity);
        }

        public decimal TotalMoney()
        {
            return Items.Sum(x => x.LineTotal);
        }

        public void Clear()
        {
            Items.Clear();
        }
    }
}