using System.Collections.Generic;

namespace DBSportStore.Models
{
    public class StoreHomeViewModel
    {
        public List<Category> Categories { get; set; }
        public List<ProductCardViewModel> Products { get; set; }
        public int? SelectedCategoryId { get; set; }
        public string SearchTerm { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
    }
}
