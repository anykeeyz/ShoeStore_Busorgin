using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShoeStore_Бусоргин.Models
{
    public class ProductInfo
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string ProdName { get; set; }
        public string Producer { get; set; }
        public string Supplier { get; set; }
        public decimal Price { get; set; }
        public string Unit { get; set; }
        public int OnStock { get; set; }
        public byte? Discount { get; set; }
        public string PhotoPath { get; set; }
        public bool IsOnSale => Discount > 0;
        public bool IsHighDiscount => Discount > 15;
        public bool IsOutOfStock => OnStock <= 0;
        public decimal? DiscountedPrice
        {
            get
            {
                return Price - (Price * Discount / 100);
            }
        }
    }
}
