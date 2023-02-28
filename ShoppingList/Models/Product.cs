using System;
using System.Collections.Generic;
using System.Text;

namespace ShoppingList.Models
{
    internal class Product
    {
        public int ProductId { get; set; }

        public string ProductName { get; set; }

        public int CategoryId { get; set; }

        public string PictureLink { get; set; }
    }
}
