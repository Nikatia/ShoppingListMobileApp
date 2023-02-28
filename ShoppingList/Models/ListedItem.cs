using System;
using System.Collections.Generic;
using System.Text;

namespace ShoppingList.Models
{
    internal class ListedItem
    {
        public int ListedItemId { get; set; }

        public int ProductId { get; set; }

        public string ProductName { get; set; }

        public int Amount { get; set; }
    }
}
