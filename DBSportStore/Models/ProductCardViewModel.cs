namespace DBSportStore.Models
{
    public class ProductCardViewModel
    {
        public int ProductID { get; set; }
        public string NamePro { get; set; }
        public string CategoryName { get; set; }
        public decimal Price { get; set; }
        public string ImagePro { get; set; }
        public string ShortDescription { get; set; } // Bổ sung theo Bài 2
    }
}